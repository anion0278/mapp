using System.Globalization;

namespace Martin_app
{
    public class TransactionsFileLanguageSettings
    {
        public string Name => DateCultureInfo.Name;

        public TransactionsFileLanguageSettings(CultureInfo dateCultureInfo, string dateSubstring)
        {
            DateCultureInfo = dateCultureInfo;
            DateSubstring = dateSubstring;
        }

        public int LinesToSkip { get; set; }

        public CultureInfo DateCultureInfo { get; }

        // it is decided to  use this phrase because parameter Market place can be unavailable (no transactions)
        public string DistinctionPhrase { get; set; }

        public string DateSubstring { get; }

        public string MarketplaceParameter { get; set; }

        public string OrderIdParameter { get; set; }
        public string QuantityParameter { get; set; }
        public string DateTimeParameter { get; set; }
        public string DescriptionParameter { get; set; }
        public string ShippingPriceParameter { get; set; }
        public string ProductPriceParameter { get; set; }
        public string TransactionTypeParameter { get; set; }

        // TODO solve it some different way
        public string Transfer { get; set; }
        public string Refund { get; set; }
        public string Order { get; set; }
        public string ServiceFee { get; set; }
        public string TotalPriceParameter { get; set; }
    }
}