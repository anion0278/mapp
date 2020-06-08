namespace Martin_App
{
    public class InvoiceItemWithDetails
    {
        private int _packQuantityMultiplicator;
        private decimal _initialOrderQuantity;

        public InvoiceXML.invoiceInvoiceItem Item { get; set; }

        public InvoiceXML.invoiceInvoiceHeader Header { get; }

        public InvoiceItemWithDetails(
            InvoiceXML.invoiceInvoiceItem item,
            InvoiceXML.invoiceInvoiceHeader header)
        {
            _packQuantityMultiplicator = 1; // init !!
            this.Item = item;
            _initialOrderQuantity = Item.quantity;
            this.Header = header;
        }

        public int PackQuantityMultiplicator
        {
            get => _packQuantityMultiplicator;
            set
            {
                _packQuantityMultiplicator = value;
                ItemQuantity = _initialOrderQuantity * _packQuantityMultiplicator; // can I use smth else instead of init val?
                Item.foreignCurrency.unitPrice = Item.foreignCurrency.price / ItemQuantity;
                Item.homeCurrency.unitPrice = Item.homeCurrency.price / ItemQuantity;
            }
        }

        public decimal ItemQuantity
        {
            get { return Item.quantity; }
            set { Item.quantity = value; }
        }
    }
}


