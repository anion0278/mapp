using System;
using System.Collections.Generic;

namespace Martin_App
{
    public class CurrencyConverter
    {
        public static string Convert(string amazonCurrency)
        {
            List<ConvertableParameterPair> CurrencyPairs = new List<ConvertableParameterPair>()
            {
                new ConvertableParameterPair("USD", "USD"),
                new ConvertableParameterPair("CDN", "CAD"),
                new ConvertableParameterPair("GBP", "GBP"),
                new ConvertableParameterPair("EUR", "EUR"),
                new ConvertableParameterPair("MXN", "MXN")
            };
            try
            {
                return GeneralConverter.GetAmazonName(amazonCurrency, CurrencyPairs);
            }
            catch (Exception ex)
            {
                throw new ConversionException("Invalid Currency name!");
            }
        }
    }
}