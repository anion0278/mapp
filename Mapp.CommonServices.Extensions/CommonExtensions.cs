using System;
using System.ComponentModel;

namespace Shmap.CommonServices
{
    public static class CommonExtensions
    {
        public static decimal DefRound(this decimal value)
        {
            return Math.Round(value, ApplicationConstants.Rounding);
        }

        public static string GetDescriptionFromEnum(this Enum enumeration, bool useEnumValueNameAsDefault = false)
        {
            System.Reflection.FieldInfo fieldInfo = enumeration.GetType().GetField(enumeration.ToString());
            var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
                return attributes[0].Description;

            if (useEnumValueNameAsDefault)
                return enumeration.ToString();

            throw new InvalidOperationException($"No description attribute found for enum : {enumeration} !");
        }

    }
}
