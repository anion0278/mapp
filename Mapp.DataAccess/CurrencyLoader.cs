using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using Microsoft.VisualBasic.FileIO;

namespace Shmap.DataAccess
{
    public class CurrencyLoader
    {
        private string _invoiceConverterConfigsDir;

        public CurrencyLoader(string invoiceConverterConfigsDir)
        {
            _invoiceConverterConfigsDir = invoiceConverterConfigsDir;
        }

        public Dictionary<string, decimal> LoadFixedCurrencyRates()
        {
            string fileContent = File.ReadAllText(Path.Join(_invoiceConverterConfigsDir, "fixed_currency_rates.csv"));
            return ParseCurrencyRates(fileContent, 0, 0);
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
                decimal num = decimal.Parse(ToInvariantFormat(strArrayList[index][skipColumns + 2])) /
                              decimal.Parse(ToInvariantFormat(strArrayList[index][skipColumns]));
                ratesDict.Add(strArrayList[index][skipColumns + 1], num);
            }

            return ratesDict;
        }

        public string ToInvariantFormat(string input)
        {
            return input.Replace(",", CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
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
