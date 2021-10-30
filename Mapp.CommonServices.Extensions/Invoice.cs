using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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
        public IEnumerable<InvoiceItemBase> InvoiceItems { get; set; } 
        public string ShipCountryCode { get; set; }
        public InvoiceVatClassification Classification { get; set; }
        public string UserId { get; set; }
        public uint Number { get; set; }
        public string VariableSymbolFull { get; set; }
        public DateTime ConversionDate { get; set; }
        public DateTime DateTax { get; set; }
        public DateTime DateAccounting { get; set; }
        public DateTime DateDue { get; set; }
        public string VariableSymbolShort { get; set; }
        public string VatType { get; set; }
        public PartnerInfo ClientInfo { get; set; }
        public string CustomsDeclaration { get; set; }
        public string SalesChannel { get; set; }
        public string RelatedWarehouseName { get; set; }
        public Currency TotalPrice { get; set; } // TODO this should be => calcualted property, beacuse its business object
        public Currency TotalPriceVat { get; set; }
        public bool IsMoss { get; set; }
        public Vat CountryVat { get; set; }
        public bool PayVat { get; set; }

        // it is a bussiness object, it should have methods like AddDiscount, add shipping
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
