﻿using System.Linq;

namespace Shmap.CommonServices
{
    public abstract class InvoiceItemBase
    {
        private string[] _taxIncludingCountries = { "GB", "AU" };

        public InvoiceItemType Type { get; protected set; }
        public decimal Quantity { get; set; }
        public string Name { get; set; }
        public decimal PercentVat { get; set; }
        public bool IsMoss => ParentInvoice.IsMoss;
        public Invoice ParentInvoice { get; }
        public Currency TotalPrice => GetTotalPrice();

        private Currency GetTotalPrice()
        {
            if (_taxIncludingCountries.Contains(ParentInvoice.ShipCountryCode))
            {
                return TotalPriceWithTax - Tax;
            }

            return TotalPriceWithTax;
        }

        public Currency UnitPrice => new(TotalPrice.AmountForeign / Quantity, ParentInvoice.TotalPrice.ForeignCurrencyName, TotalPrice.Rates);
        public Currency VatPrice => new(TotalPrice.AmountForeign * ParentInvoice.CountryVat.ReversePercentage, ParentInvoice.TotalPrice.ForeignCurrencyName, TotalPrice.Rates);
        public Currency TotalPriceWithTax { get; set; }
        public Currency Tax { get; set; }

        protected InvoiceItemBase(Invoice parentInvoice)
        {
            ParentInvoice = parentInvoice;
        }
    }
}