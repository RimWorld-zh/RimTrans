using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml;

namespace RimTrans.Lite.Util
{
    /// <summary>
    /// For Globalization/Localization of the applictation.
    /// </summary>
    public static class LocalizationHelper
    {
        public static void SelectLanguage(string languageCode)
        {
            // Get OS language code
            if (languageCode == null || languageCode.Equals("auto") || languageCode.Length < 2)
            {
                languageCode = CultureInfo.InstalledUICulture.Name;
            }

            // Get all ResourceDictionary
            var mergedDictionaries = Application.Current.Resources.MergedDictionaries.ToList();

            // Search for the requested localization dictionary
            var requestedLanguage = $"/Resources/Localizations/StringResources.{languageCode}.xaml";
            var requestedDictionary = mergedDictionaries.FirstOrDefault(d => d.Source?.OriginalString == requestedLanguage);

            // e.g. If 'zh-CN' on found, try found 'zh'.
            int splitIndex = languageCode.IndexOf('-');
            if (requestedDictionary == null && languageCode.Length > 2 && splitIndex > 1)
            {
                languageCode = languageCode.Substring(0, splitIndex);
                requestedLanguage = $"/Resources/Localizations/StringResources.{languageCode}.xaml";
                requestedDictionary = mergedDictionaries.FirstOrDefault(d => d.Source?.OriginalString == requestedLanguage);
            }

            // If still no found, use 'en'.
            if (requestedDictionary == null)
            {
                languageCode = "en";
                requestedLanguage = $"/Resources/Localizations/StringResources.en.xaml";
                requestedDictionary = mergedDictionaries.FirstOrDefault(d => d.Source?.OriginalString == requestedLanguage);
            }

            // Remove and Add, then it will take effect.
            Application.Current.Resources.MergedDictionaries.Remove(requestedDictionary);
            Application.Current.Resources.MergedDictionaries.Add(requestedDictionary);
        }
        
    }
}
