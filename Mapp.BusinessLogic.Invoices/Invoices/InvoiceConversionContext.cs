using System;

namespace Shmap.BusinessLogic.Invoices
{
    public class InvoiceConversionContext
    {
        public uint ExistingInvoiceNumber { get; init; }
        public string DefaultEmail { get; init; }
        public DateTime ConvertToDate { get; init; }
    }
}