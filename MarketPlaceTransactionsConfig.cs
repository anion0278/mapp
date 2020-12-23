using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace Martin_app
{
    public class MarketPlaceTransactionsConfig 
    {
        public MarketPlaceTransactionsConfig(string dateCultureInfoName, string dateSubstring, int marketPlaceId)
        {
            DateCultureInfoName = dateCultureInfoName;
            DateSubstring = dateSubstring;
            MarketPlaceId = marketPlaceId;
        }

        /// <summary>
        /// Number of lines before the column-name-line.
        /// </summary>
        public int LinesToSkipBeforeColumnNames { get; set; }

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
        public string DistinctionPhrase { get; set; }

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


    public class MarketPlaceTransactionsConfigDTO
    {
        protected MarketPlaceTransactionsConfigDTO()
        {}

        public int LinesToSkipBeforeColumnNames { get; set; }

        public int MarketPlaceId { get; set; }
        
        public string DistinctionPhrase { get; set; }

        public string DateSubstring { get; set; }

        public string TimeSeparatorOverride { get; set; }

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