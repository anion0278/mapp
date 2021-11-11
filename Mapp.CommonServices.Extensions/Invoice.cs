using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace Shmap.CommonServices
{
    public enum InvoiceVatClassification
    {
        Undefined,
        [Description("UDA5")]
        UDA5,
        [Description("RDzasEU")]
        RDzasEU,
        [Description("UVzboží")]
        UVzbozi
    }


    public class Invoice  
    {
        private readonly List<InvoiceItemBase> _invoiceItems = new();
        private Dictionary<string, decimal> _vatPercentage;

        private string[] _euCountries = // TODO from settings
        {
            "BE", "BG", "DK", "EE", "FI", "FR", "IE", "IT", "CY", "LT", "LV", "LU", "HU", "HR", "MT", "DE",
            "NL", "PL", "PT", "AT", "RO", "GR", "SK", "SI", "ES", "SE", "EU", "CZ"
        };

        private Dictionary<string, string> _mossExceptionCountryCodes = new() { { "GR", "EL" } };

        private string[] _homeCountries = { "CZ" };

        public Invoice(Dictionary<string, decimal> vatPercentage)
        {
            _vatPercentage = vatPercentage;
        }

        public IEnumerable<InvoiceItemBase> InvoiceItems => _invoiceItems.OrderBy(i => i.Type); // This is business rule - order Product-gift-discount-shipping
        public string ShipCountryCode { get; set; }
        public InvoiceVatClassification Classification => GetClassification();
        public uint Number { get; set; }

        // TODO join variable symbols into single Class with ShortVersion autoprop
        public string VariableSymbolFull { get; set; }
        public string VariableSymbolShort => GetShortVariableCode(VariableSymbolFull, out _);
        public DateTime ConversionDate { get; set; }
        public DateTime DateTax => CalculateTaxDate();
        public DateTime DateAccounting => ConversionDate;
        public DateTime DateDue => CalculateDueDate();
        public string VatType => IsNonEuCountryByClassification(Classification) ? "nonSubsume" : null; // TODO into DAL ?
        public PartnerInfo ClientInfo { get; set; }
        public string CustomsDeclaration { get; set; }
        public string SalesChannel { get; set; } = string.Empty;
        public string RelatedWarehouseName { get; set; }
        public Currency TotalPrice => AggregatePrice();

        // TODO Misto toho aby se pouzivali calulated props se muzou pouzivat nastaveni 
        public Currency TotalPriceVat => new((TotalPrice.AmountForeign * CountryVat.ReversePercentage).DefRound(), TotalPrice.ForeignCurrencyName, TotalPrice.Rates);

        public bool IsMoss => Classification == InvoiceVatClassification.RDzasEU;

        public Vat CountryVat => GetCountryVat();

        private Vat GetCountryVat()
        {
            if (!IsNonEuCountryByClassification(Classification))
            {
                return new Vat(_vatPercentage[ShipCountryCode]);
            }
            return new Vat(0);
        }

        public bool PayVat => Classification is InvoiceVatClassification.RDzasEU or InvoiceVatClassification.UDA5;

        public string CurrencyName { get; set; } // TODO get rid of that!
        public string MossCountryCode => GetMossCountryCode();

        private string GetMossCountryCode()
        {
            return _mossExceptionCountryCodes.TryGetValue(ShipCountryCode, out string mossCode) ? mossCode : ShipCountryCode;
        }

        private Currency AggregatePrice()
        {
            if (!InvoiceItems.Any()) return new Currency(0, CurrencyName, new Dictionary<string, decimal>());

            return InvoiceItems
                .Select(i => i.TotalPrice)
                .Aggregate((s, i) => s + i);
        }

        public void MergeInvoice(Invoice aggregatedInvoice)
        {
            _invoiceItems.AddRange(aggregatedInvoice.InvoiceItems.Where(item => item.Type == InvoiceItemType.Product));

            AggregateItemsOfType(aggregatedInvoice.InvoiceItems, InvoiceItemType.Discount);
            AggregateItemsOfType(aggregatedInvoice.InvoiceItems, InvoiceItemType.GiftWrap);
            AggregateItemsOfType(aggregatedInvoice.InvoiceItems, InvoiceItemType.Shipping);
        }

        private void AggregateItemsOfType(IEnumerable<InvoiceItemBase> items2, InvoiceItemType itemType)
        {
            var newItemOfType = items2.SingleOrDefault(i => i.Type == itemType);
            var existingItemOfType = _invoiceItems.SingleOrDefault(i => i.Type == itemType);

            if (existingItemOfType == null && newItemOfType == null) return;

            if (existingItemOfType == null)
            {
                _invoiceItems.Add(newItemOfType);
                return;
            }
            AggregateItems(existingItemOfType, newItemOfType);
        }

        public static string GetShortVariableCode(string fullVariableCode, out int zerosRemoved)
        {
            string filteredCode = fullVariableCode.RemoveAll("-");
            filteredCode = filteredCode.Substring(filteredCode.Length - 10, 10);

            // if short var code has zeros in the begining - they cannot be stored in Invoice, that is why we delete them
            // and give information about how many zeros were deleted to GPC generator
            var finalCode = filteredCode.TrimStart('0');  // zeros don't get correctly imported into Pohoda
            zerosRemoved = filteredCode.Length - finalCode.Length;
            return finalCode;
        }

        private void AggregateItems(InvoiceItemBase existingItem, InvoiceItemBase aggregatedItem)
        {
            if (existingItem.Type != aggregatedItem.Type)
                throw new ArgumentException("Cannot aggregate items of different type!");

            existingItem.TotalPriceWithTax += aggregatedItem.TotalPriceWithTax;
            existingItem.Tax += aggregatedItem.Tax;
        }


        public void AddInvoiceItem(InvoiceItemBase item)
        {
            _invoiceItems.Add(item);
        }

        public void AddInvoiceItems(IEnumerable<InvoiceItemBase> items)
        {
            foreach (var item in items)
            {
                _invoiceItems.Add(item);
            }
        }


        private InvoiceVatClassification GetClassification()
        {
            // UDA5 only CZ
            // UDzasEU - European Union countries, UVzboží - the rest
            if (_homeCountries.Contains(ShipCountryCode)) return InvoiceVatClassification.UDA5; // TODO make sure that lists of countries вщ not contain duplicates
            return _euCountries.Contains(ShipCountryCode) ? InvoiceVatClassification.RDzasEU : InvoiceVatClassification.UVzbozi;
        }

        private DateTime CalculateTaxDate()
        {
            return IsNonEuCountryByClassification(Classification) ? ConversionDate.AddDays(5.0) : ConversionDate;
        }

        private DateTime CalculateDueDate()
        {
            return ConversionDate.AddDays(3.0);
        }

        private bool IsNonEuCountryByClassification(InvoiceVatClassification classification)
        {
            return classification == InvoiceVatClassification.UVzbozi;
        }
    }

    public class Vat
    {
        public decimal Percentage { get; }
        public decimal ReversePercentage => Percentage / (1 + Percentage);

        public Vat(decimal percentage)
        {
            Percentage = percentage;
        }
    }

    public class PartnerInfo
    {
        public string Name { get; set; }
        public Address Address { get; set; }
        public ContactData Contact { get; set; }
    }

    public class Address
    {
        public string City { get; set; }
        public string Street { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
    }

    public class ContactData
    {
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}
