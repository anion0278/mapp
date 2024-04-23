using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapp.DataAccess
{
    public interface IValueParser
    {
        decimal ParseDecimal(string value);
        DateTime ParseDateTime(string value);
    }

    public class CultureDependentValueParser : IValueParser
    {
        public CultureInfo CultureInfo { get; }

        public CultureDependentValueParser(CultureInfo cultureInfo)
        {
            CultureInfo = cultureInfo;
        }

        public DateTime ParseDateTime(string value)
        {
            return DateTime.Parse(value, CultureInfo);
        }

        public decimal ParseDecimal(string value)
        {
            return decimal.Parse(value, CultureInfo);
        }
    }
}
