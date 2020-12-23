using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;

namespace Martin_app
{
    public class TransactionsFileLanguageSettings // TODO rename - not language settings, but MarketPlaceSettings
    {
        public string Name => DateCultureInfo.Name;

        public TransactionsFileLanguageSettings(CultureInfo dateCultureInfo, string dateSubstring)
        {
            DateCultureInfo = dateCultureInfo;
            DateSubstring = dateSubstring;
        }

        /// <summary>
        /// Number of lines before the column-name-line.
        /// </summary>
        public int LinesToSkipBeforeColumnNames { get; set; }

        public CultureInfo DateCultureInfo { get; }

        // it is decided to  use this phrase because parameter Market place can be unavailable (no transactions)
        public string DistinctionPhrase { get; set; }

        public string DateSubstring { get; }

        public string MarketplaceColumnName { get; set; } // TODO there is no need in this, since we know it beforehand by DistinctionPhrase 
        // WE ONLY NEED Bank number (currently from enum)

        public string OrderIdColumnName { get; set; }
        public IEnumerable<string> DateTimeColumnNames { get; set; } // TODO should be of type TransactionParameter: columnNames, ParseType(enum) 
        
        /// <summary>
        /// Only for debug
        /// </summary>
        public string DebugDescriptionColumnName { get; set; }
        
        //public string ShippingPriceColumnName { get; set; } // TODO into priceComponents Ienum
        //public string ProductPriceColumnName { get; set; }
        //public string PromotionRebateColumnNames { get; set; }

        public IEnumerable<string> ValueComponentsColumnName { get; set; }

        public string TransactionTypeColumnName { get; set; }

        // TODO solve it some different way
        public IEnumerable<string> TransferTypeNames { get; set; }
        public IEnumerable<string> RefundTypeNames { get; set; }
        public IEnumerable<string> OrderTypeNames { get; set; }
        public IEnumerable<string> ServiceFeeTypeNames { get; set; }
        public string TotalPriceColumnName { get; set; }
    }
}