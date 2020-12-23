using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using AutoMapper;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;

namespace Martin_app
{
    public class TransactionsReader
    {
        private readonly IEnumerable<MarketPlaceTransactionsConfig> _marketplaceConfigs;

        public TransactionsReader()
        {
            _marketplaceConfigs = GetAvailableMarketplaceConfigs();
        }

        private static IEnumerable<MarketPlaceTransactionsConfig> GetAvailableMarketplaceConfigs()
        {
            // TODO load only once
            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<MarketPlaceTransactionsConfigDTO, MarketPlaceTransactionsConfig>();
            });
            IMapper iMapper = mapperConfiguration.CreateMapper();

            var configs = new List<MarketPlaceTransactionsConfig>();
            var fileNames = Directory.GetFiles("Transactions Configs");
            foreach (var fileName in fileNames.Where(fn => fn.Contains("TransactionsConfig")))
            {
                string json = File.ReadAllText(fileName);
                var configDto = JsonSerializer.Deserialize<MarketPlaceTransactionsConfigDTO>(json);
                configs.Add(iMapper.Map<MarketPlaceTransactionsConfigDTO, MarketPlaceTransactionsConfig>(configDto));
            }

            var marketPlaceIds = configs.Select(s => s.MarketPlaceId).ToList();
            if (marketPlaceIds.Distinct().Count() != marketPlaceIds.Count())
            {
                throw new ArgumentException($"Chyba, duplicitni hodnota {nameof(marketPlaceIds)} v JSON konfiguracich!");
            }

            return configs;
        }

        public static string GetShortVariableCode(string fullVariableCode, out int zerosRemoved)
        {
            // TODO put somewhere else, used in two places
            zerosRemoved = 0;

            string filteredCode = fullVariableCode.RemoveAll("-");
            filteredCode = filteredCode.Substring(filteredCode.Length - 10, 10);

            // if short var code has zeros in the begining - they cannot be stored in Invoice, that is why we delete them
            // and give information about how many zeros were deleted to GPC generator
            var finalCode = filteredCode.TrimStart('0');  // zeros don't get correctly imported into Pohoda
            zerosRemoved = filteredCode.Length - finalCode.Length;

            return finalCode;
        }

        private IReadOnlyList<string[]> GetFileLines(string fileName, string encodingCode = "utf-8")
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var lineItems = new List<string[]>();
            using (var textFieldParser = new TextFieldParser(fileName, Encoding.GetEncoding(encodingCode)))
            {
                textFieldParser.TextFieldType = FieldType.Delimited;
                textFieldParser.SetDelimiters(",");
                while (!textFieldParser.EndOfData)
                {
                    string[] orderLine = textFieldParser.ReadFields();
                    lineItems.Add(orderLine);
                }
            }

            return lineItems;
        }

        // TODO REMOVE!!
        public DateTime ParseDate(string dateInput, string datePattern, CultureInfo culture)
        {
            var match = Regex.Match(dateInput, datePattern);

            return DateTime.Parse(match.Groups[1].Value, culture);
        }

        public DateTime ParseDate(string dateString, MarketPlaceTransactionsConfig settings)
        {
            var match = Regex.Match(dateString, settings.DateSubstring);
            return DateTime.Parse(match.Groups[1].Value, settings.DateCultureInfo);
        }

        public IEnumerable<Transaction> ReadTransactions(string fileName)
        {
            var lines = GetFileLines(fileName);

            string japaneseCrazyEncoding = "�w��̂Ȃ��ꍇ�A�P�ʂ͉~"; //TODO solve using BOM?
            if (lines[1][0].Equals(japaneseCrazyEncoding))
                lines = GetFileLines(fileName, "Shift-JIS"); 

            var languageSetting = GetLanguageSetting(lines);

            var validLines = lines.Skip(languageSetting.LinesToSkipBeforeColumnNames).ToList();

            var transactionsDict = new Dictionary<string, string[]>();
            for (int columnIndex = 0; columnIndex < validLines[0].Length; ++columnIndex)
            {
                string columnNameKey = validLines[0][columnIndex].Trim(); //tolower? 
                transactionsDict.Add(columnNameKey, validLines.Skip(1).Select(l => l[columnIndex]).ToArray());
            }

            var transactions = new List<Transaction>();
            for (int index = 0; index < transactionsDict.First().Value.Count(); index++)
            {
                string orderId = transactionsDict[languageSetting.OrderIdColumnName][index];
                if (string.IsNullOrEmpty(orderId))
                    orderId = "0000000000000000000";

                // PRICE

                double transactionTotalValue = 0;
                foreach (var compColumnName in languageSetting.ValueComponentsColumnName)
                {
                    transactionTotalValue += double.Parse(transactionsDict[compColumnName][index], languageSetting.DateCultureInfo);
                }

                var transactionType = ParseTransactionType(transactionsDict[languageSetting.TransactionTypeColumnName][index], languageSetting);
                if (transactionType == TransactionTypes.Transfer || transactionType == TransactionTypes.ServiceFee)
                {
                    // V priprade Service Fee a Transferu product price je total price
                    transactionTotalValue = double.Parse(transactionsDict[languageSetting.TotalPriceColumnName][index], languageSetting.DateCultureInfo);
                }

                // DATE 
                string dateComplete = String.Empty;
                foreach (var columnName in languageSetting.DateTimeColumnNames)
                {
                    dateComplete += transactionsDict[columnName][index] + " ";
                }

                var date = ParseDate(dateComplete, languageSetting);

                var transaction = new Transaction()
                {
                    Date = date,
                    OrderId = orderId,
                    TransactionValue = transactionTotalValue,
                    Type = transactionType,
                    MarketplaceId = languageSetting.MarketPlaceId,
                };
                transactions.Add(transaction);
            }

            var orders = transactions.Where(t => t.Type.Equals(TransactionTypes.Order)).ToList();
            if (orders.Any())
            {
                var averageMarketplace = (int)orders.Average(t => t.MarketplaceId);
                foreach (var transaction in transactions.Except(orders))
                {
                    transaction.MarketplaceId = averageMarketplace;
                }
            }

            return transactions;
        }


        public T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description.EqualsIgnoreCase(description))
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", nameof(description));
            // or return default(T);
        }

        public TransactionTypes ParseTransactionType(string transactionType, MarketPlaceTransactionsConfig settings)
        {
            // TODO refactoring AWFUL CODE
            if (settings.OrderTypeNames.Any(n => n.EqualsIgnoreCase(transactionType)))
                return TransactionTypes.Order;
            if (settings.TransferTypeNames.Any(n => n.EqualsIgnoreCase(transactionType)))
                return TransactionTypes.Transfer;
            if (settings.RefundTypeNames.Any(n => n.EqualsIgnoreCase(transactionType)))
                return TransactionTypes.Refund;
            if (settings.ServiceFeeTypeNames.Any(n => n.EqualsIgnoreCase(transactionType)))
                return TransactionTypes.ServiceFee;

            throw new ArgumentException($"Wrong transaction type! Name of transaction: {transactionType}");
        }

        private MarketPlaceTransactionsConfig GetLanguageSetting(IReadOnlyList<string[]> dataLines)
        {
            // TODO handle possible exception on higher level
            // put into factory
            foreach (var marketPlace in _marketplaceConfigs)
            {
                var found = dataLines.SingleOrDefault(l => l.First().EqualsIgnoreCase(marketPlace.DistinctionPhrase));
                if (found != null)
                {
                    return marketPlace;
                }
            }
            throw new ArgumentException("Nerozpoznany typ souboru");
        }

        public static string GetEnumDescription(Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());
            var attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
    }
}
