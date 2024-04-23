using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using Mapp.Common;
using Mapp.DataAccess;
using Mapp.Models.Transactions;
using Microsoft.VisualBasic.FileIO;

namespace Mapp.BusinessLogic.Transactions
{
    public interface ITransactionsReader
    {
        IEnumerable<Transaction> ReadTransactionsFromMultipleFiles(IEnumerable<string> fileNames);
    }

    public class TransactionsReader : ITransactionsReader
    {
        private readonly IJsonManager _jsonManager;
        private readonly IEnumerable<MarketPlaceTransactionsConfig> _marketplaceConfigs;

        public TransactionsReader(IJsonManager jsonManager)
        {
            _jsonManager = jsonManager;
            _marketplaceConfigs = GetAvailableMarketplaceConfigs();
        }

        public IEnumerable<Transaction> ReadTransactionsFromMultipleFiles(IEnumerable<string> fileNames)
        {
            var transactions = new List<Transaction>();
            foreach (var fileName in fileNames)
            {
                transactions.AddRange(ReadTransactions(fileName));
            }

            return transactions;
        }

        private IEnumerable<MarketPlaceTransactionsConfig> GetAvailableMarketplaceConfigs()
        {
            var mapperConfiguration = new MapperConfiguration(cfg => {
                cfg.CreateMap<MarketPlaceTransactionsConfigData, MarketPlaceTransactionsConfig>();
            });
            IMapper mapper = mapperConfiguration.CreateMapper();

            var configsData = _jsonManager.LoadTransactionsConfigs();

            var configs = configsData.Select(data =>
                mapper.Map<MarketPlaceTransactionsConfigData, MarketPlaceTransactionsConfig>(data));

            var marketPlaceIds = configs.Select(s => s.MarketPlaceId).ToList();
            //if (marketPlaceIds.Distinct().Count() != marketPlaceIds.Count()) // commented because we now have PaypalCZ and paypal 
            //{
            //    throw new ArgumentException($"Chyba, duplicitni hodnota {nameof(marketPlaceIds)} v JSON konfiguracich!");
            //}

            return configs;
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

        private DateTime ParseDate(string dateString, MarketPlaceTransactionsConfig config)
        {
            // TODO use CultureDependentValueParser
            var match = Regex.Match(dateString, config.DateSubstring);
            return DateTime.Parse(match.Groups[1].Value, config.DateCultureInfo);
        }

        private IEnumerable<Transaction> ReadTransactions(string fileName)
        {
            var lines = GetFileLines(fileName);

            string japaneseCrazyEncoding = "�w��̂Ȃ��ꍇ�A�P�ʂ͉~"; //TODO solve using BOM?
            if (lines[1][0].Equals(japaneseCrazyEncoding))
                lines = GetFileLines(fileName, "Shift-JIS"); 

            var marketPlaceSetting = GetMarketPlaceSetting(lines);

            int linesToSkip;
            try
            {
                linesToSkip = lines.ToList()
                    .FindIndex(s => s.Intersect(marketPlaceSetting.DateTimeColumnNames).Any());
            }
            catch (Exception ex)
            {
                throw new Exception("Could not find start of the data columns", ex);
            }
            var validLines = lines.Skip(linesToSkip).ToList();

            var transactionsDict = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            for (int columnIndex = 0; columnIndex < validLines[0].Length; ++columnIndex)
            {
                string columnNameKey = validLines[0][columnIndex].Trim(); //tolower? 
                transactionsDict.Add(columnNameKey, validLines.Skip(1).Select(l => l[columnIndex]).ToArray());
            } 

            var transactions = new List<Transaction>();
            for (int index = 0; index < transactionsDict.First().Value.Count(); index++)
            {
                string orderId = transactionsDict[marketPlaceSetting.OrderIdColumnName][index];
                orderId = new string(orderId.ToCharArray().Where(c => char.IsDigit(c) || c == '-').ToArray()); // we only need to take numbers, since for Shoppify order is smth like #3214, however '-' should be ok
                if (string.IsNullOrEmpty(orderId))
                    orderId = "0000000000000000000";

                // PRICE

                decimal transactionTotalValue = 0;
                foreach (var compColumnName in marketPlaceSetting.ValueComponentsColumnName)
                {
                    string val = transactionsDict[compColumnName][index];
                    transactionTotalValue += decimal.Parse(val, marketPlaceSetting.DateCultureInfo);
                }

                var transactionType = ParseTransactionType(transactionsDict[marketPlaceSetting.TransactionTypeColumnName][index], marketPlaceSetting);
                if (transactionType == TransactionTypes.Transfer || transactionType == TransactionTypes.ServiceFee)
                {
                    // V priprade Service Fee a Transferu product price je total price
                    transactionTotalValue = decimal.Parse(transactionsDict[marketPlaceSetting.TotalPriceColumnName][index], marketPlaceSetting.DateCultureInfo);
                }

                // DATE 
                string dateComplete = string.Empty;
                foreach (var columnName in marketPlaceSetting.DateTimeColumnNames)
                {
                    dateComplete += transactionsDict[columnName][index] + " ";
                }

                var date = ParseDate(dateComplete, marketPlaceSetting);

                var transaction = new Transaction()
                {
                    Date = date,
                    OrderId = orderId,
                    TransactionValue = transactionTotalValue,
                    Type = transactionType,
                    MarketplaceId = marketPlaceSetting.MarketPlaceId,
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

        private TransactionTypes ParseTransactionType(string transactionType, MarketPlaceTransactionsConfig settings)
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

        private MarketPlaceTransactionsConfig GetMarketPlaceSetting(IReadOnlyList<string[]> dataLines)
        {
            // TODO handle possible exception on higher level
            // put into factory
            foreach (var marketPlace in _marketplaceConfigs)
            {
                var isFound = IsFirstLineCorrespondToConfig(dataLines, marketPlace);
                if (isFound)
                {
                    return marketPlace;
                }
            }
            throw new ArgumentException("Nerozpoznany typ souboru");
        }

        private static bool IsFirstLineCorrespondToConfig(IReadOnlyList<string[]> dataLines, MarketPlaceTransactionsConfig marketPlace)
        {
            return dataLines.Any(l => !marketPlace.DistinctionPhrases.Except(l).Any());
        }
    }
}
