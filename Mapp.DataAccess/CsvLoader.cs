using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using Shmap.CommonServices;
using Microsoft.VisualBasic.FileIO;

namespace Shmap.DataAccess
{
    public interface ICsvLoader
    {
        Dictionary<string, decimal> LoadFixedCurrencyRates();
        Dictionary<string, decimal> LoadCountryVatRates();
    }

    public class CsvLoader : ICsvLoader
    {
        private readonly IConfigProvider _configProvider;

        public CsvLoader(IConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public Dictionary<string, decimal> LoadFixedCurrencyRates()
        {
            string fileContent;
            try
            {
                fileContent = File.ReadAllText(Path.Join(_configProvider.InvoiceConverterConfigsDir, "fixed_currency_rates.csv"));
            }
            catch (Exception ex)
            {
                throw new SettingsDataAccessException("Fixed currency rates loading error!", ex);
            }

            return ParseCurrencyRates(fileContent, 0, 0);
        }

        public Dictionary<string, decimal> LoadCountryVatRates()
        {
            string fileContent;
            try
            {
                fileContent = File.ReadAllText(Path.Join(_configProvider.InvoiceConverterConfigsDir, "vat_by_country.csv"));
            }
            catch (Exception ex)
            {
                throw new SettingsDataAccessException("VAT settings loading error!", ex);
            }
            return ParseVatRates(fileContent);
        }

        private Dictionary<string, decimal> ParseCurrencyRates(string downloadString, int skipLines, int skipColumns)
        {
            var strArrayList = new List<string[]>();
            using (TextFieldParser textFieldParser = new TextFieldParser(new StringReader(downloadString)))
            {
                textFieldParser.TextFieldType = FieldType.Delimited;
                textFieldParser.SetDelimiters("|");
                while (!textFieldParser.EndOfData)
                {
                    string[] strArray = textFieldParser.ReadFields();
                    strArrayList.Add(strArray);
                }
            }

            var ratesDict = new Dictionary<string, decimal>();
            for (int index = skipLines; index < strArrayList.Count; ++index)
            {
                decimal num = decimal.Parse(strArrayList[index][skipColumns + 2], CultureInfo.InvariantCulture) /
                              decimal.Parse(strArrayList[index][skipColumns], CultureInfo.InvariantCulture);
                ratesDict.Add(strArrayList[index][skipColumns + 1], num);
            }

            return ratesDict;
        }

        private Dictionary<string, decimal> ParseVatRates(string downloadString)
        {
            var strArrayList = new List<string[]>();
            using (TextFieldParser textFieldParser = new TextFieldParser(new StringReader(downloadString)))
            {
                textFieldParser.TextFieldType = FieldType.Delimited;
                textFieldParser.SetDelimiters("|");
                while (!textFieldParser.EndOfData)
                {
                    string[] strArray = textFieldParser.ReadFields();
                    strArrayList.Add(strArray);
                }
            }

            var ratesDict = new Dictionary<string, decimal>();
            for (int index = 0; index < strArrayList.Count; ++index)
            {
                decimal vat = decimal.Parse(strArrayList[index][1], CultureInfo.InvariantCulture) / (decimal) 100.0;
                ratesDict.Add(strArrayList[index][0], vat);
            }

            return ratesDict;
        }


        //private Dictionary<string, decimal> DownloadLatestCurrencyRates()
        //{
        //    string downloadString;
        //    using (var webClient = new WebClient() { Encoding = XmlEncoding })
        //    {
        //        string str = DateTime.Today.AddDays(-1).ToString("dd.MM.yyyy");
        //        downloadString = webClient.DownloadString("http://www.cnb.cz/cs/financni_trhy/devizovy_trh/kurzy_devizoveho_trhu/denni_kurz.txt?date=" + str);
        //    }
        //    if (string.IsNullOrEmpty(downloadString))
        //        throw new ArgumentNullException("Nepodarilo se stahnout aktualni kurzy men!");

        //    return ParseCurrencyRates(downloadString, 2, 2);
        //}

    }
}
