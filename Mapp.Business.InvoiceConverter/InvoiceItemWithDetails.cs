using System;

namespace Mapp
{
    public class InvoiceItemWithDetails
    {
        private int _packQuantityMultiplier;
        private decimal _initialOrderQuantity;

        public InvoiceXml.invoiceInvoiceItem Item { get; set; }

        public InvoiceXml.invoiceInvoiceHeader Header { get; }

        public InvoiceItemWithDetails(
            InvoiceXml.invoiceInvoiceItem item,
            InvoiceXml.invoiceInvoiceHeader header)
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


