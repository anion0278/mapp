using System.Linq;

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
                return TotalPriceWithTax - Tax; // TODO zvazit zda tady nedat vypocet zalozeny na ReversePercentage Vat, pak to bude univerzalni a bude zachovavat invarianty pro pripad zmeny zeme
                //price * (1 - invoiceItem.ParentInvoice.CountryVat.ReversePercentage)
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
            //ParentInvoice.AddInvoiceItem(this); // TODO !!!! ПРЯМО ЗДЕСЬ сам новый объект может добавлять себя к своему инвойс парэнту
        }
    }
}