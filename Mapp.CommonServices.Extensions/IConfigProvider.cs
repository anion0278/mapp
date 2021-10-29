using System.Drawing;

namespace Shmap.CommonServices
{
    public interface IConfigProvider
    {
        uint ExistingInvoiceNumber { get; set; }
        string DefaultEmail { get; set; }
        string TrackingCode { get; set; }
        bool IsMainWindowMaximized { get; set; }
        Size MainWindowSize { get; set; }
        Point MainWindowTopLeftCorner { get; set; }
        bool OpenTargetFolderAfterConversion { get; set; }
        string InvoiceConverterConfigsDir { get; }
        void SaveConfig();
    }

}