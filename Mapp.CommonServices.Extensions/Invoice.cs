using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        public IEnumerable<InvoiceItemBase> InvoiceItems => _invoiceItems;
        public string ShipCountryCode { get; set; }
        public InvoiceVatClassification Classification { get; set; }
        public string UserId { get; set; }
        public uint Number { get; set; }
        public string VariableSymbolFull { get; set; }
        public DateTime ConversionDate { get; set; }
        public DateTime DateTax => CalculateTaxDate(ConversionDate);
        public DateTime DateAccounting => ConversionDate;
        public DateTime DateDue => CalculateDueDate(ConversionDate);
        public string VariableSymbolShort { get; set; }
        public string VatType { get; set; }
        public PartnerInfo ClientInfo { get; set; }
        public string CustomsDeclaration { get; set; }
        public string SalesChannel { get; set; }
        public string RelatedWarehouseName { get; set; }
        public Currency TotalPrice => AggregatePrice();

        public Currency TotalPriceVat => new (Math.Round(TotalPrice.AmountForeign * CountryVat.ReversePercentage, 2), TotalPrice.ForeignCurrencyName, TotalPrice.Rates);

        public bool IsMoss { get; set; }

        public Vat CountryVat { get; set; } = new(0);

        public bool PayVat { get; set; }

        public string CurrencyName { get; set; } // TODO get rid of that!

        private Currency AggregatePrice()
        {
            if (!InvoiceItems.Any()) return new Currency(0, CurrencyName, new Dictionary<string, decimal>());

            return InvoiceItems
                .Select(i=>i.TotalPrice)
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

        private void AggregateItems(InvoiceItemBase existingItem, InvoiceItemBase aggregatedItem)
        {
            if (existingItem.Type != aggregatedItem.Type)
                throw new ArgumentException("Cannot aggregate items of different type!");

            existingItem.TotalPrice += aggregatedItem.TotalPrice;
        }


        public void AddInvoiceItem(InvoiceItemBase item)
        {
            _invoiceItems.Add(item);
        }

        private DateTime CalculateTaxDate(DateTime conversionDate)
        {
            if (IsNonEuCountryByClassification(Classification))
                return conversionDate.AddDays(5.0);
            return conversionDate;
        }

        private DateTime CalculateDueDate(DateTime purchaseDate)
        {
            return purchaseDate.AddDays(3.0);
        }

        private bool IsNonEuCountryByClassification(InvoiceVatClassification classification)
        {
            return classification == InvoiceVatClassification.UVzbozi; // AFWULL
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

    [DebuggerDisplay("{AmountForeign} {ForeignCurrencyName}")]
    public class Currency
    {
        public Dictionary<string, decimal> Rates { get; }
        public decimal AmountForeign { get; }
        public decimal AmountHome => Rate * AmountForeign;
        public string ForeignCurrencyName { get; }
        public decimal Rate => GetCurrencyRate(ForeignCurrencyName);

        public Currency(decimal amountForeign, string foreignCurrencyName, Dictionary<string, decimal> rates) // TODO solve. Maybe Currency Factory?
        {
            Rates = rates;
            AmountForeign = amountForeign;
            ForeignCurrencyName = foreignCurrencyName;
        }

        private decimal GetCurrencyRate(string currency)
        {
            return Rates.Single(r => r.Key.Equals(currency, StringComparison.InvariantCultureIgnoreCase)).Value;
        }

        public static Currency operator +(Currency c1, Currency c2)
        {
            if (!c1.ForeignCurrencyName.EqualsIgnoreCase(c2.ForeignCurrencyName))
                throw new ArgumentException("Aggregated price has different currency!");
            return new Currency(c1.AmountForeign + c2.AmountForeign, c1.ForeignCurrencyName, c1.Rates); ;
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
