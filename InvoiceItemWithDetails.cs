namespace Martin_App
{
    public class InvoiceItemWithDetailes
    {
        public InvoiceXML.invoiceInvoiceItem Item { get; set; }

        public InvoiceXML.invoiceInvoiceHeader Header { get; }

        public InvoiceItemWithDetailes(
            InvoiceXML.invoiceInvoiceItem item,
            InvoiceXML.invoiceInvoiceHeader header)
        {
            this.Item = item;
            this.Header = header;
        }
    }
}