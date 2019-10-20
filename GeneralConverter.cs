using System;
using System.Collections.Generic;
using System.Linq;

namespace Martin_App
{
    public class GeneralConverter
    {
        public static string GetAmazonName(
            string amazonName,
            List<ConvertableParameterPair> CurrencyPairs)
        {
            return CurrencyPairs.Single<ConvertableParameterPair>((Func<ConvertableParameterPair, bool>)(p => p.AmazonParameterName.Equals(amazonName, StringComparison.InvariantCultureIgnoreCase))).PohodaParameterName;
        }
    }
}