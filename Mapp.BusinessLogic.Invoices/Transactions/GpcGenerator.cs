using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mapp.Common;
using Mapp.DataAccess;

namespace Mapp.BusinessLogic.Transactions
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
        private readonly IFileManager _fileManager;
        private readonly IDateTimeManager _dateTimeManager;

        public GpcGenerator(IFileManager fileManager, IDateTimeManager dateTimeManager)
        {
            _fileManager = fileManager;
            _dateTimeManager = dateTimeManager;
        }

        public void SaveTransactions(IEnumerable<Transaction> transactions, string fileName)
        {
            DateTime endOfCurrentMonth = GetEndOfCurrentMonth();

            string firstLine = string.Format(_intitialLine, endOfCurrentMonth.ToString("ddMMyy"));

            var outputText = new StringBuilder();

            outputText.AppendLine(firstLine);
            foreach (var transaction in transactions.Where(t => !t.Type.Equals(TransactionTypes.ServiceFee)))
            {
                outputText.AppendLine(GetTransactionLine(transaction));
            }
            _fileManager.WriteAllTextToFile(fileName, outputText.ToString());
        }

        private string GetTransactionLine(Transaction transaction)
        {
            var shortVariableCode = VariableCode.GetShortVariableCode(transaction.OrderId);
            if (transaction.Type == TransactionTypes.Refund) // Refunds have short variable codes from first 10 symbols
            {
                shortVariableCode = VariableCode.GetShortVariableCode(transaction.OrderId);
            }

            shortVariableCode = shortVariableCode.PadLeft(VariableCode.ShortVariableCodeLength, '0');

            string type = ((int)transaction.Type).ToString();

            string marketPlace = transaction.MarketplaceId.ToString().PadLeft(2, '0'); // max 2

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

        private string FormatPrice(decimal price)
        {
            string priceFormatted = Math.Abs(price).ToString("N2").RemoveAll(".").RemoveAll(",").PadLeft(8, '0');
            return priceFormatted;
        }

        private DateTime GetEndOfCurrentMonth()
        {
            var today = _dateTimeManager.Today;
            return today.AddDays(1 - today.Day).AddMonths(1).AddDays(-1).Date;
        }

    }

}