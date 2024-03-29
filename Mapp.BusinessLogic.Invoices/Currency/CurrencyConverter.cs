﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mapp.Common;

namespace Mapp.BusinessLogic.Currency
{
    public interface ICurrencyConverter
    {
        string Convert(string amazonCurrency);
        string GetAmazonName(string amazonName, List<ConvertableParameterPair> CurrencyPairs);
    }

    public class CurrencyConverter : ICurrencyConverter // TODO currency factory
    {
        public string Convert(string amazonCurrency)
        {
            var CurrencyPairs = new List<ConvertableParameterPair>()
            {
                new("USD", "USD"),
                new ("CDN", "CAD"),
                new ("GBP", "GBP"),
                new ("EUR", "EUR"),
                new ("MXN", "MXN"),
                new ("AUD", "AUD")
            };
            try
            {
                return GetAmazonName(amazonCurrency, CurrencyPairs);
            }
            catch (Exception ex)
            {
                throw new ConversionException("Invalid Currency name!");
            }
        }

        public string GetAmazonName(string amazonName, List<ConvertableParameterPair> CurrencyPairs)
        {
            return CurrencyPairs.Single(p => p.AmazonParameterName.Equals(amazonName, StringComparison.InvariantCultureIgnoreCase)).PohodaParameterName;
        }
    }

    public class ConvertableParameterPair
    {
        public string AmazonParameterName { get; set; }

        public string PohodaParameterName { get; set; }

        public ConvertableParameterPair(string amazonName, string pohodaName)
        {
            this.AmazonParameterName = amazonName;
            this.PohodaParameterName = pohodaName;
        }
    }

}
