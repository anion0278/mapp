using System.Collections.Generic;

namespace Mapp.Models.Transactions
{
    public class MarketPlaceTransactionsConfigData
    {
        public MarketPlaceTransactionsConfigData() // TODO why cannot be protected ? (exception)
        { }

        public string Name { get; set; }
        public int MarketPlaceId { get; set; }

        public IEnumerable<string> DistinctionPhrases { get; set; }

        public string DateSubstring { get; set; }

        public string TimeSeparatorOverride { get; set; }
        public string NumericFormatSourceCultureName { get; set; }

        public string DateCultureInfoName { get; set; }

        public string OrderIdColumnName { get; set; }

        public string TransactionTypeColumnName { get; set; }

        public IEnumerable<string> DateTimeColumnNames { get; set; }
        public IEnumerable<string> ValueComponentsColumnName { get; set; }

        public IEnumerable<string> TransferTypeNames { get; set; }
        public IEnumerable<string> RefundTypeNames { get; set; }
        public IEnumerable<string> OrderTypeNames { get; set; }
        public IEnumerable<string> ServiceFeeTypeNames { get; set; }
        public string TotalPriceColumnName { get; set; }
    }
}
