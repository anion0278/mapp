using System;

namespace Shmap.CommonServices
{
    public enum InvoiceItemType
    {
        Undefined,
        Product,
        Shipping,
        Discount,
        GiftWrap
    }

    public abstract class InvoiceItemBase
    {
        public InvoiceItemType Type { get; protected set; }
        public decimal Quantity { get; set; }
        public string Name { get; set; }
        public decimal PercentVat { get; set; }
        public bool IsMoss => ParentInvoice.IsMoss;
        public Invoice ParentInvoice { get; }
        public Currency TotalPrice { get; set; }
        public Currency UnitPrice => new (TotalPrice.AmountForeign / Quantity, ParentInvoice.TotalPrice.ForeignCurrencyName, TotalPrice.Rates);
        public Currency VatPrice => new(TotalPrice.AmountForeign * ParentInvoice.CountryVat.ReversePercentage, ParentInvoice.TotalPrice.ForeignCurrencyName, TotalPrice.Rates);

        protected InvoiceItemBase(Invoice parentInvoice)
        {
            ParentInvoice = parentInvoice;
        }
    }

    // TODO composition over inheritance?
    public class InvoiceProduct: InvoiceItemBase
    {
        private uint _packQuantityMultiplier;
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string AmazonSku { get; set; }
        public uint PackQuantityMultiplier
        {
            get => _packQuantityMultiplier;
            set
            {
                // revert previous multiplier and apply the new one
                Quantity = (int)(Quantity / _packQuantityMultiplier * value);
                _packQuantityMultiplier = value;
            }
        } 

        public InvoiceProduct(Invoice parentInvoice) : base(parentInvoice)
        {
            Type = InvoiceItemType.Product;
            _packQuantityMultiplier = 1;
        }
    }

    public class InvoiceItemGeneral: InvoiceItemBase
    {
        public InvoiceItemGeneral(Invoice parentInvoice, InvoiceItemType type) : base(parentInvoice)
        {
            Type = type;
        }
    }

}