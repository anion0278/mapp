using System;
using System.ComponentModel;

namespace Martin_app
{
    public static class StringExtensions
    {
        public static string RemoveAll(this string str, string findString)
        {
            return str.Replace(findString, string.Empty);
        }

        public static bool EqualsIgnoreCase(this string firstStr, string secondStr)
        {
            return firstStr.Equals(secondStr, StringComparison.InvariantCultureIgnoreCase);
        }


    }
}