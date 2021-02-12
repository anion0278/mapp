using System;

namespace Martin_App
{
    public class InvoiceItemWithDetails
    {
        private int _packQuantityMultiplier;
        private decimal _initialOrderQuantity;

        public InvoiceXML.invoiceInvoiceItem Item { get; set; }

        public InvoiceXML.invoiceInvoiceHeader Header { get; }

        public InvoiceItemWithDetails(
            InvoiceXML.invoiceInvoiceItem item,
            InvoiceXML.invoiceInvoiceHeader header)
        {
            _packQuantityMultiplier = 1; // init !!
            Item = item;
            _initialOrderQuantity = Item.quantity;
            Header = header;
        }

        public int PackQuantityMultiplier
        {
            get => _packQuantityMultiplier;
            set
            {
                Item.foreignCurrency.unitPrice = Math.Round(Item.foreignCurrency.unitPrice * _packQuantityMultiplier / value, 6);
                Item.homeCurrency.unitPrice = Math.Round(Item.homeCurrency.unitPrice * _packQuantityMultiplier / value, 6);
                Item.quantity = (int)(Item.quantity / _packQuantityMultiplier * value);
                _packQuantityMultiplier = value;
            }
        }

        public decimal ItemQuantity
        {
            get { return Item.quantity; }
            set { Item.quantity = value; }
        }

        public string AmazonSkuCode => Item.amazonSkuCode;
    }
}


