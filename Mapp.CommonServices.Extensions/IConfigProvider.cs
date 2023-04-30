using System.Drawing;

namespace Shmap.Common
{
    public interface ISettingsWrapper // TODO move to UI.Settings
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