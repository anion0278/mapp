namespace Martin_App
{
    public class InvoiceItemWithDetails
    {
        public InvoiceXML.invoiceInvoiceItem Item { get; set; }

        public InvoiceXML.invoiceInvoiceHeader Header { get; }

        public InvoiceItemWithDetails(
            InvoiceXML.invoiceInvoiceItem item,
            InvoiceXML.invoiceInvoiceHeader header)
        {
            this.Item = item;
            this.Header = header;
        }

        public decimal ItemQuantity
        {
            get { return Item.quantity; }
            set
            {
                Item.foreignCurrency.unitPrice = (Item.foreignCurrency.unitPrice * Item.quantity) / value;
                Item.homeCurrency.unitPrice = (Item.homeCurrency.unitPrice * Item.quantity) / value;
                Item.quantity = value;
            }
        }
    }
}


