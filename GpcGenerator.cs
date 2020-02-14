using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Markup;
using Microsoft.Win32;

namespace Martin_app
{
    public class GpcGenerator
    {
        private string _intitialLine =
            "0740000002001353907Czech Goods s.r.o.  01111900000013280900+00000016514842+000000461730770000000494070190011{0}FIO           ";

        private string _transactionBase = "075000000200135390{0}000000000000000000000000000000000{1}{2}{3}00000000000000000000000000{4}000124{5}";

        public GpcGenerator()
        {
                
        }

        public void SaveTransactions(IEnumerable<Transaction> transactions, string fileName)
        {
            DateTime endOfCurrentMonth = GetEndOfCurrentMonth();

            string firstLine = string.Format(_intitialLine, endOfCurrentMonth.ToString("ddMMyy"));

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName))
            {
                file.WriteLine(firstLine);
                foreach (var transaction in transactions)
                {
                    file.WriteLine(GetTransactionLine(transaction));
                }
            }
        }

        private string GetTransactionLine(Transaction transaction)
        {
            return string.Format(_transactionBase,
                (int)transaction.Marketplace,
                FormatPrice(transaction.TotalPrice),
                (int)transaction.Type,
                TransactionsReader.GetShortVariableCode(transaction.OrderId),
                transaction.OrderId,
                transaction.Date.ToString("ddMMyy"));
        }

        private string FormatPrice(double price)
        {
            string priceFormatted = Math.Abs(price).ToString("N2").RemoveAll(".").RemoveAll(",").PadLeft(8,'0');
            return priceFormatted;
        }

        private static DateTime GetEndOfCurrentMonth()
        {
            var today = DateTime.Today;
            return today.AddDays(1 - today.Day).AddMonths(1).AddDays(-1).Date;
        }

    }


    public enum AmazonMarketplace
    {
        [Description("amazon.com")]
        AmazonCom,
        [Description("amazon.ca")]
        AmazonCa,
        [Description("amazon.com.mx")]
        AmazonComMx,
        [Description("amazon.jp")]
        AmazonJp,
        [Description("amazon.com.au")]
        AmazonComAu,
        [Description("amazon.co.uk")]
        AmazonCoUk,
        [Description("amazon.de")]
        AmazonDe,
        [Description("amazon.es")]
        AmazonEs,
        [Description("amazon.fr")]
        AmazonFr,
        [Description("amazon.it")]
        AmazonIt,
    }
}