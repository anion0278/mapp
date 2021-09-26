using System;
using System.Collections.Generic;
using System.Linq;
using Shmap.CommonServices;

namespace Shmap.BusinessLogic.Transactions
{
    public interface IGpcGenerator
    {
        void SaveTransactions(IEnumerable<Transaction> transactions, string fileName);
    }

    public class GpcGenerator : IGpcGenerator
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
                zerosRemoved = 0;
            }

            string type = ((int) transaction.Type).ToString();
            // in case that order ID contained zeros at the beginning
            type = type.PadRight(type.Length + zerosRemoved, '0');

            string marketPlace = transaction.MarketplaceId.ToString().PadLeft(2, '0');

            string orderId = transaction.OrderId.PadRight(19, '0');

            string price = FormatPrice(transaction.TransactionValue);

            string date = transaction.Date.ToString("ddMMyy");

            string formatted = string.Format(_transactionBase,
                marketPlace,
                price,
                type, 
                shortVariableCode,
                orderId,
                date);

            int assertLen = 128;
            if (formatted.Length != assertLen) throw new DataMisalignedException($"Konvertovany radek nema delku {assertLen} symbolu! Chyba!");

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

}