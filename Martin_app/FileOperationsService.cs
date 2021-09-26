using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace Mapp
{

    public interface IFileOperationService
    {
        IEnumerable<string> OpenAmazonInvoices();
        string SaveConvertedAmazonInvoices();
        IEnumerable<string> GetTransactionFileNames();
        string GetSaveFileNameForConvertedTransactions();
        void OpenFileFolder(string fileName);
    }

    public class FileOperationsService: IFileOperationService
    {
        public IEnumerable<string> OpenAmazonInvoices()// TODO rename
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Zvol Amazon report",
                Filter = "Amazon Report|*.txt"
            };
            bool? dialogResult = openFileDialog.ShowDialog();
            if (dialogResult == false) return new string[]{};

            return openFileDialog.FileNames;
        }

        public string SaveConvertedAmazonInvoices()// TODO rename
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Zvol vystupni slozku",
                FileName = "PohodaInvoices_" + DateTime.Today.ToString("dd-MM-yyyy") + ".xml",
                Filter = "Converted Amazon Report|*.xml"
            };
            bool? dialogResult = saveFileDialog.ShowDialog();
            if (dialogResult != true) return null;

            return saveFileDialog.FileName;
        }

        public IEnumerable<string> GetTransactionFileNames()
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Title = "Zvol soubor transakci",
                Filter = "Transactions|*.csv"
            };
            bool? dialogResult = openFileDialog.ShowDialog();
            if (dialogResult == false) return new string[]{};

            return openFileDialog.FileNames;
        }

        public string GetSaveFileNameForConvertedTransactions()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Zvol umisteni vystupniho souboru",
                FileName = "Transactions_" + DateTime.Today.ToString("dd-MM-yyyy") + ".gpc",
                Filter = "Converted Transactions|*.gpc"
            };
            bool? result = saveFileDialog.ShowDialog();
            if (result != true) return null;

            return saveFileDialog.FileName;
        }

        public void OpenFileFolder(string fileName)
        {
            Process.Start("explorer.exe", Path.GetDirectoryName(fileName));
        }
    }
}