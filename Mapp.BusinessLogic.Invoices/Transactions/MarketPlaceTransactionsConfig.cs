using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Mapp.BusinessLogic.Transactions
{
    [DebuggerDisplay("{Name}")]
    public class MarketPlaceTransactionsConfig 
    {
        public string Name { get; set; }

        public MarketPlaceTransactionsConfig(string dateCultureInfoName, string dateSubstring, int marketPlaceId)
        {
            DateCultureInfoName = dateCultureInfoName;
            DateSubstring = dateSubstring;
            MarketPlaceId = marketPlaceId;
        }

        public string DateCultureInfoName { get; }

        private string _timeSeparatorOverride = null;
        public string TimeSeparatorOverride
        {
            get { return _timeSeparatorOverride; }
            set
            {
                if (_dataCultureInfo != null)
                {
                    _dataCultureInfo.DateTimeFormat.TimeSeparator = TimeSeparatorOverride;
                }

                _timeSeparatorOverride = value;
            }
        }

        private CultureInfo _dataCultureInfo;

        public CultureInfo DateCultureInfo
        {
            get
            {
                if (_dataCultureInfo == null)
                {
                    _dataCultureInfo = new CultureInfo(DateCultureInfoName);
                    if (_timeSeparatorOverride != null)
                    {
                        _dataCultureInfo.DateTimeFormat.TimeSeparator = _timeSeparatorOverride;
                    }    
                }
                return _dataCultureInfo;
            }
        }

        // it is decided to  use this phrase because parameter Market place can be unavailable (no transactions)
        public IEnumerable<string> DistinctionPhrases { get; set; }

        public string DateSubstring { get; }

        public string OrderIdColumnName { get; set; }

        public int MarketPlaceId { get; set; }

        public string TransactionTypeColumnName { get; set; }

        public IEnumerable<string> DateTimeColumnNames { get; set; } // TODO should be of type TransactionParameter: columnNames, ParseType(enum) 
        public IEnumerable<string> ValueComponentsColumnName { get; set; }

        public IEnumerable<string> TransferTypeNames { get; set; }
        public IEnumerable<string> RefundTypeNames { get; set; }
        public IEnumerable<string> OrderTypeNames { get; set; }
        public IEnumerable<string> ServiceFeeTypeNames { get; set; }
        public string TotalPriceColumnName { get; set; }
    }
}