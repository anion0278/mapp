using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Shmap.CommonServices
{
    [DebuggerDisplay("{AmountForeign} {ForeignCurrencyName}")]
    public class Currency
    {
        public Dictionary<string, decimal> Rates { get; }
        public decimal AmountForeign { get; }
        public decimal AmountHome => Rate * AmountForeign;
        public string ForeignCurrencyName { get; }
        public decimal Rate => GetCurrencyRate(ForeignCurrencyName);

        public Currency(decimal amountForeign, string foreignCurrencyName, Dictionary<string, decimal> rates) // TODO solve. Maybe Currency Factory?
        {
            Rates = rates;
            AmountForeign = amountForeign;
            ForeignCurrencyName = foreignCurrencyName;
        }

        private decimal GetCurrencyRate(string currency)
        {
            return Rates.Single(r => r.Key.Equals(currency, StringComparison.InvariantCultureIgnoreCase)).Value;
        }

        public static Currency operator +(Currency c1, Currency c2)
        {
            if (!c1.ForeignCurrencyName.EqualsIgnoreCase(c2.ForeignCurrencyName))
                throw new ArgumentException("Different currency!");
            return new Currency(c1.AmountForeign + c2.AmountForeign, c1.ForeignCurrencyName, c1.Rates);
        }

        public static Currency operator -(Currency c1, Currency c2)
        {
            if (!c1.ForeignCurrencyName.EqualsIgnoreCase(c2.ForeignCurrencyName))
                throw new ArgumentException("Different currency!");
            return new Currency(c1.AmountForeign - c2.AmountForeign, c1.ForeignCurrencyName, c1.Rates);
        }
    }
}