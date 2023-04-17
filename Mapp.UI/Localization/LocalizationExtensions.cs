using System.Diagnostics;
using System.Reflection;
using WPFLocalizeExtension.Extensions;

namespace Shmap.UI.Localization;

public static class LocalizationExtensions
{
    private static string GetLocalizedValue(string key)
    {
        return LocExtension.GetLocalizedValue<string>(
            Assembly.GetCallingAssembly().GetName().Name + $":{nameof(LocalizationStrings)}:" + key);
    }

    private static string TrimStart(this string target, string trimString)
    {
        if (string.IsNullOrEmpty(trimString)) return target;

        string result = target;
        while (result.StartsWith(trimString))
        {
            result = result.Substring(trimString.Length);
        }

        return result;
    }

    public static string GetLocalized(
        this object obj,
        [System.Runtime.CompilerServices.CallerArgumentExpression("obj")] string callerExp = "")
    {
        string key = callerExp.TrimStart(nameof(LocalizationStrings) + ".");
        Debug.Print(key);
        return GetLocalizedValue(key);
    }
}