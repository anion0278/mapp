using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using FlaUI.Core;
using Xunit;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using FlaUI.UIA3;

namespace Shmap.UI.UiTests
{
    public class ApplicationUiTests
    {
        //[Fact]
        //public void RunApp_LoadPredefinedInvoices_ExportConvertedInvoices()
        //{
        //    var assemblyLocation = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName.Replace(".UiTests", null);
        //    var app = FlaUI.Core.Application.Launch(Path.Join(assemblyLocation, @"win-x64\Shmap.exe"));
        //    using (var automation = new UIA3Automation())
        //    {
        //        var mainWindow = app.GetMainWindow(automation);

        //        var updateWin = GetModalWindow(mainWindow, w => w.Name.Contains("available"), TimeSpan.FromSeconds(3)); 
        //        updateWin.Close();

        //        var openInvoiceButton = mainWindow.FindFirstDescendant(cf => cf.ByText("Zvolit fakturu"))?.AsButton();
        //        openInvoiceButton.WaitUntilClickable(TimeSpan.FromSeconds(3));
        //        openInvoiceButton?.Invoke();

        //        var fileDialog = GetModalWindow(mainWindow, w => w.Name == "Zvol Amazon report", TimeSpan.FromSeconds(5));
        //        var fileNameInput = fileDialog.FindFirstDescendant(cf => cf.ByAutomationId("1148"))?.AsComboBox();
        //        var fileNameInputEdit = WaitForElement(() => fileNameInput.FindAllChildren().Single(c => c.ControlType == ControlType.Edit).AsTextBox());
        //        fileNameInputEdit.Text = @"D:\Stefan Grushko\OneDrive\My Projects\mapp\Invoices converter\Test cases\multi-item order\amazon_multi_item_order.txt";

        //        var buttonOpenFile = fileDialog.FindFirstDescendant(cf => cf.ByAutomationId("1"))?.AsButton();
        //        buttonOpenFile?.Invoke();
        //        Thread.Sleep(TimeSpan.FromSeconds(5));

        //        app.Close();
        //    }
        //}

        //https://stackoverflow.com/questions/51026119/wait-for-application-launch-without-using-thread-sleep-using-flaui
        private T WaitForElement<T>(Func<T> getter)
        {
            var retry = Retry.WhileNull<T>(
                () => getter(),
                TimeSpan.FromSeconds(3));

            if (!retry.Success)
            {
                throw new TimeoutException("Failed to get an element within a wait timeout");
            }

            return retry.Result;
        }

        public static Window GetWindow(Application app, UIA3Automation automation, Func<Window, bool> condition, TimeSpan timeout)
        {
            var result = Retry.WhileNull(() => app.GetAllTopLevelWindows(automation).Where(condition).FirstOrDefault(),
                timeout, ignoreException: true, throwOnTimeout: true);

            return result.Result;
        }


        private static Window GetParentWindow(AutomationElement element)
        {

            var automationElement2 = element;
            var controlViewWalker = element.Automation.TreeWalkerFactory.GetControlViewWalker();
            while (automationElement2.Properties.NativeWindowHandle.ValueOrDefault == new IntPtr(0))
                automationElement2 = controlViewWalker.GetParent(automationElement2);

            return automationElement2.AsWindow();
        }

        //https://github.com/jakubsuchybio/FlaUI.TestUtils/blob/master/FlaUI.TestUtils/Extensions/WindowExtensions.cs
        public static Window GetModalWindow(Window window, Func<Window, bool> condition, TimeSpan timeout)
        {
            var result = Retry.WhileNull(() => window.ModalWindows.Where(condition).FirstOrDefault(),
                timeout, ignoreException: true, throwOnTimeout: true);

            return result.Result;
        }
    }
}