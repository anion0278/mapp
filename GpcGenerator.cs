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

        private string _transactionBase = "07500000020013539{0}000000000000000000000000000000000{1}{2}{3}00000000000000000000000000{4}000124{5}";

        public void SaveTransactions(IEnumerable<Transaction> transactions, string fileName)
        {
            DateTime endOfCurrentMonth = GetEndOfCurrentMonth();

            string firstLine = string.Format(_intitialLine, endOfCurrentMonth.ToString("ddMMyy"));

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName))
            {
                file.WriteLine(firstLine);
                foreach (var transaction in transactions.Where(t => !t.Type.Equals(TransactionTypes.ServiceFee)))
                {
                    file.WriteLine(GetTransactionLine(transaction));
                }
            }
        }

        private string GetShortVariableCodeForRefund(string fullVariableCode)
        {
            // refunds are filled manually in pohoda, so there is no need to care about invoice symVar
            string filteredCode = fullVariableCode.RemoveAll("-");
            filteredCode = filteredCode.Substring(0, 10);
            return filteredCode;
        }

        private string GetTransactionLine(Transaction transaction)
        {
            var shortVariableCode = TransactionsReader.GetShortVariableCode(transaction.OrderId, out var zerosRemoved);
            if (transaction.Type == TransactionTypes.Refund) // Refunds have short variable codes from first 10 symbols
            {
                shortVariableCode = GetShortVariableCodeForRefund(transaction.OrderId);
            }

            string type = ((int) transaction.Type).ToString();

            string marketPlace = ((int) transaction.Marketplace).ToString().PadLeft(2, '0');

            string formatted = string.Format(_transactionBase,
                marketPlace,
                FormatPrice(transaction.TotalPrice),
                type.PadRight(type.Length + zerosRemoved, '0'), // in case that order ID contained zeros at the beginning
                shortVariableCode,
                transaction.OrderId,
                transaction.Date.ToString("ddMMyy"));

            int asserLen = 128;
            if (formatted.Length != asserLen) throw new DataMisalignedException($"Konvertovany radek nema delku {asserLen} symbolu! Chyba!");

            return formatted;
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
        AmazonCom = 0,
        [Description("amazon.ca")]
        AmazonCa = 1,
        [Description("amazon.com.mx")]
        AmazonComMx = 2,
        [Description("amazon.jp")]
        AmazonJp = 3,
        [Description("amazon.com.au")]
        AmazonComAu = 4,
        [Description("amazon.co.uk")]
        AmazonCoUk = 5,
        [Description("amazon.de")]
        AmazonDe = 6,
        [Description("amazon.es")]
        AmazonEs = 7,
        [Description("amazon.fr")]
        AmazonFr = 8,
        [Description("amazon.it")]
        AmazonIt = 9,
        [Description("amazon.nl")]
        AmazonNl = 10,
    }
}