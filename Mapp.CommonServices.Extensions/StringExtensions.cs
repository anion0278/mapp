using System;

namespace Shmap.Common
{
    public static class StringExtensions
    {
        public static bool ContainsIgnoreCase(this string locationString, string soughtString)
        {
            return locationString.ToLower().Contains(soughtString.ToLower());
        }

        public static bool EqualsIgnoreCase(this string str1, string str2)
        {
            Require.NotNull(str1, nameof(str1));
            Require.NotNull(str2, nameof(str2));
            return str1.Equals(str2, StringComparison.InvariantCultureIgnoreCase);
        }

        public static string RemoveAll(this string locationString, string substringToRemove)
        {
            if (string.IsNullOrEmpty(locationString) || string.IsNullOrEmpty(substringToRemove)) return locationString;

            return locationString.Replace(substringToRemove, string.Empty);
        }

        public static string FormatWith(this string str, object arg)
        {
            return string.Format(str, arg);
        }

        public static string FormatWith(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        //public static bool IsNumber(this string str)
        //{
        //    return double.TryParse(str.Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture, out _);
        //}
    }
}