namespace Shmap.CommonServices
{
    public enum InvoiceItemType // TODO how to let know that this sequence is a business rule (order in final output)
    {
        Undefined,
        Product,
        GiftWrap,
        Discount,
        Shipping,
    }

    // TODO composition over inheritance?
    public class InvoiceProduct : InvoiceItemBase
    {
        private uint _packQuantityMultiplier;
        public string WarehouseCode { get; set; }
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

    public class InvoiceItemGeneral : InvoiceItemBase
    {
        public InvoiceItemGeneral(Invoice parentInvoice, InvoiceItemType type) : base(parentInvoice)
        {
            Type = type;
        }
    }

}