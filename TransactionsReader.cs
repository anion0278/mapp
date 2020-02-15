﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;

namespace Martin_app
{
    public class TransactionsReader
    {
        private IEnumerable<TransactionsFileLanguageSettings> _languageSettings;

        public TransactionsReader()
        {
            var italyCulture = new CultureInfo("it-IT");
            italyCulture.DateTimeFormat.TimeSeparator = ".";
            _languageSettings = new List<TransactionsFileLanguageSettings>
            {
                new TransactionsFileLanguageSettings(new CultureInfo("en-AU"), "GMT+09:00")
                {
                    DistinctionPhrase = "All amounts in AUD, unless specified",
                    DateTimeParameter = "date/time", OrderIdParameter = "order id",
                    TransactionTypeParameter = "type", MarketplaceParameter = "marketplace",
                    DescriptionParameter = "description", ProductPriceParameter = "product sales",
                    ShippingPriceParameter = "shipping credits", QuantityParameter = "quantity",
                    TotalPriceParameter = "total",
                    Order = "Order", Refund = "Refund", Transfer = "Transfer", ServiceFee = "Service Fee",
                    LinesToSkip = 7
                    },
                new TransactionsFileLanguageSettings(new CultureInfo("en-CA"), "PST")
                {
                    DistinctionPhrase = "All amounts in CAD, unless specified",
                    DateTimeParameter = "date/time", OrderIdParameter = "order id",
                    TransactionTypeParameter = "type", MarketplaceParameter = "marketplace",
                    DescriptionParameter = "description", ProductPriceParameter = "product sales",
                    ShippingPriceParameter = "shipping credits", QuantityParameter = "quantity",
                    TotalPriceParameter = "total",
                    Order = "Order", Refund = "Refund", Transfer = "Transfer",ServiceFee = "Service Fee",
                    LinesToSkip = 7
                },
                new TransactionsFileLanguageSettings(new CultureInfo("de-DE"), "GMT+00:00")
                {
                    DistinctionPhrase = "Alle Beträge in Euro sofern nicht anders gekennzeichnet",
                    DateTimeParameter = "Datum/Uhrzeit", OrderIdParameter = "Bestellnummer",
                    TransactionTypeParameter = "Typ", MarketplaceParameter = "Marketplace",
                    DescriptionParameter = "Beschreibung", ProductPriceParameter = "Umsätze",
                    ShippingPriceParameter = "Gutschrift für Versandkosten", QuantityParameter = "Menge",
                    TotalPriceParameter = "Gesamt",
                    Order = "Bestellung", Refund = "Erstattung", Transfer = "Übertrag",ServiceFee = "Service Fee",
                    LinesToSkip = 6
                },
                new TransactionsFileLanguageSettings(new CultureInfo("es-ES"), "GMT+00:00")
                {
                    DistinctionPhrase = "Todos los importes en EUR, a menos que se especifique lo contrario",
                    DateTimeParameter = "fecha y hora", OrderIdParameter = "número de pedido",
                    TransactionTypeParameter = "tipo", MarketplaceParameter = "web de Amazon",
                    DescriptionParameter = "descripción", ProductPriceParameter = "ventas de productos",
                    ShippingPriceParameter = "abonos de envío", QuantityParameter = "cantidad",
                    TotalPriceParameter = "total",
                    Order = "Pedido", Refund = "Refund", Transfer = "Transferir",
                    LinesToSkip = 6
                },
                new TransactionsFileLanguageSettings(new CultureInfo("fr-FR"), "UTC+00:00")
                {
                    DistinctionPhrase = "Tous les montants sont en EUR, sauf mention contraire.",
                    DateTimeParameter = "date/heure", OrderIdParameter = "numéro de la commande",
                    TransactionTypeParameter = "type", MarketplaceParameter = "Marketplace",
                    DescriptionParameter = "description", ProductPriceParameter = "ventes de produits",
                    ShippingPriceParameter = "crédits d'expédition", QuantityParameter = "quantité",
                    TotalPriceParameter = "total",
                    Order = "Commande", Refund = "Remboursement", Transfer = "Transfert",ServiceFee = "Service Fee",
                    LinesToSkip = 6
                },
                new TransactionsFileLanguageSettings(new CultureInfo("en-GB"), "GMT+00:00")
                {
                    DistinctionPhrase = "All amounts in GBP, unless specified",
                    DateTimeParameter = "date/time", OrderIdParameter = "order id",
                    TransactionTypeParameter = "type", MarketplaceParameter = "marketplace",
                    DescriptionParameter = "description", ProductPriceParameter = "product sales",
                    ShippingPriceParameter = "postage credits", QuantityParameter = "quantity",
                    TotalPriceParameter = "total",
                    Order = "Order", Refund = "Refund", Transfer = "Transfer",ServiceFee = "Service Fee",
                    LinesToSkip = 6
                },
                new TransactionsFileLanguageSettings(italyCulture, "GMT+00.00")
                {
                    DistinctionPhrase = "Tutti gli importi sono espressi in EUR, se non diversamente specificato.",
                    DateTimeParameter = "Data/Ora:", OrderIdParameter = "Numero ordine",
                    TransactionTypeParameter = "Tipo", MarketplaceParameter = "Marketplace",
                    DescriptionParameter = "Descrizione", ProductPriceParameter = "Vendite",
                    ShippingPriceParameter = "Accrediti per le spedizioni", QuantityParameter = "Quantità",
                    TotalPriceParameter = "totale",
                    Order = "Ordine", Refund = "Refund", Transfer = "Trasferimento", // TODO refund
                    LinesToSkip = 6
                },
                new TransactionsFileLanguageSettings(new CultureInfo("ja-JP"), "JST")
                {
                    DistinctionPhrase = "指定のない場合、単位は円",
                    DateTimeParameter = "日付/時間", OrderIdParameter = "注文番号",
                    TransactionTypeParameter = "トランザクションの種類", MarketplaceParameter = "Amazon 出品サービス",
                    DescriptionParameter = "説明", ProductPriceParameter = "商品売上",
                    ShippingPriceParameter = "配送料", QuantityParameter = "数量",
                    TotalPriceParameter = "合計",
                    Order = "注文", Refund = "Refund", Transfer = "振込み", ServiceFee = "注文外料金",
                    LinesToSkip = 6
                },
                new TransactionsFileLanguageSettings(new CultureInfo("es-MX"), "PST")
                {
                    DistinctionPhrase = "Todos los importes en dólares, a menos que se especifique",
                    DateTimeParameter = "fecha/hora", OrderIdParameter = "Id. del pedido",
                    TransactionTypeParameter = "tipo", MarketplaceParameter = "marketplace",
                    DescriptionParameter = "descripción", ProductPriceParameter = "ventas de productos",
                    ShippingPriceParameter = "créditos de envío", QuantityParameter = "cantidad",
                    TotalPriceParameter = "total",
                    Order = "Pedido", Refund = "Refund", Transfer = "Trasferir",ServiceFee = "Service Fee", // TODO Refund?
                    LinesToSkip = 6
                },
                new TransactionsFileLanguageSettings(new CultureInfo("en-US"), "PST")
                {
                    DistinctionPhrase = "All amounts in USD, unless specified",
                    DateTimeParameter = "date/time", OrderIdParameter = "order id",
                    TransactionTypeParameter = "type", MarketplaceParameter = "marketplace",
                    DescriptionParameter = "description", ProductPriceParameter = "product sales",
                    ShippingPriceParameter = "shipping credits", QuantityParameter = "quantity",
                    TotalPriceParameter = "total",
                    Order = "Order", Refund = "Refund", Transfer = "Transfer",ServiceFee = "Service Fee",
                    LinesToSkip = 7
                },
            };

        }

        public static string GetShortVariableCode(string fullVariableCode)
        {
            // TODO put somewhere else

            string filteredCode = fullVariableCode.RemoveAll("-");
            return filteredCode.Substring(filteredCode.Length - 10, 10);
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
        public DateTime ParseDate(string dateInput, string cutSubstring, CultureInfo culture)
        {
            return DateTime.Parse(dateInput.RemoveAll(cutSubstring), culture);
        }

        public DateTime ParseDate(string dateString, TransactionsFileLanguageSettings settings)
        {
            return DateTime.Parse(dateString.RemoveAll(settings.DateSubstring), settings.DateCultureInfo);
        }

        public IEnumerable<Transaction> ReadTransactions(string fileName)
        {
            string japaneseCrazyEncoding = "�w��̂Ȃ��ꍇ�A�P�ʂ͉~";

            var lines = GetFileLines(fileName);

            if (lines[1][0].Equals(japaneseCrazyEncoding))
                lines = GetFileLines(fileName, "Shift-JIS");

            var languageSetting = GetLanguageSetting(lines);

            var validLines = lines.Skip(languageSetting.LinesToSkip).ToList();

            var transactionsDict = new Dictionary<string, string[]>();
            for (int columnIndex = 0; columnIndex < validLines[0].Length; ++columnIndex)
            {
                string columnNameKey = validLines[0][columnIndex].Trim(); //tolower? 
                transactionsDict.Add(columnNameKey, validLines.Skip(1).Select(l => l[columnIndex]).ToArray());
            }

            var transactions = new List<Transaction>();
            for (int index = 0; index < transactionsDict.First().Value.Count(); index++)
            {
                int.TryParse(transactionsDict[languageSetting.QuantityParameter][index], out var quantity);

                AmazonMarketplace marketplace = 0;

                // FFFF!!! japanese have not decided whether its amazon.co.jp or amazon.jp
                string marketplaceStr = transactionsDict[languageSetting.MarketplaceParameter][index];
                marketplaceStr = marketplaceStr.EqualsIgnoreCase("amazon.co.jp") ? "amazon.jp" : marketplaceStr;

                if (!string.IsNullOrEmpty(transactionsDict[languageSetting.MarketplaceParameter][index]))
                    marketplace = GetValueFromDescription<AmazonMarketplace>(marketplaceStr);

                string orderId = transactionsDict[languageSetting.OrderIdParameter][index];
                if (string.IsNullOrEmpty(orderId))
                    orderId = "0000000000000000000";

                double productPrice = float.Parse(transactionsDict[languageSetting.ProductPriceParameter][index],
                    languageSetting.DateCultureInfo);

                var transactionType = ParseTransactionType(transactionsDict[languageSetting.TransactionTypeParameter][index], languageSetting);
                if (transactionType == TransactionTypes.Transfer || transactionType == TransactionTypes.ServiceFee)
                {
                    productPrice = float.Parse(transactionsDict[languageSetting.TotalPriceParameter][index], languageSetting.DateCultureInfo);
                }

                var transaction = new Transaction()
                {
                    Date = ParseDate(transactionsDict[languageSetting.DateTimeParameter][index], languageSetting),
                    OrderId = orderId,
                    ProductDescription = transactionsDict[languageSetting.DescriptionParameter][index],
                    ProductPrice = productPrice,
                    ShippingPrice = float.Parse(transactionsDict[languageSetting.ShippingPriceParameter][index], languageSetting.DateCultureInfo),
                    Quantity = quantity,
                    Type = transactionType,
                    Marketplace = marketplace
                };
                transactions.Add(transaction);
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

        public TransactionTypes ParseTransactionType(string transactionType, TransactionsFileLanguageSettings settings)
        {
            // FIXME AWFUL CODE
            if (transactionType.EqualsIgnoreCase(settings.Order)) return TransactionTypes.Order;
            if (transactionType.EqualsIgnoreCase(settings.Transfer)) return TransactionTypes.Transfer;
            if (transactionType.EqualsIgnoreCase(settings.Refund)) return TransactionTypes.Refund;
            if (transactionType.EqualsIgnoreCase(settings.ServiceFee)) return TransactionTypes.ServiceFee;

            throw new ArgumentException("Wrong transaction type!");
        }

        private TransactionsFileLanguageSettings GetLanguageSetting(IReadOnlyList<string[]> dataLines)
        {
            // TODO handle possible exception on higher level
            // put into factory
            return _languageSettings.Single(s => s.DistinctionPhrase.EqualsIgnoreCase(dataLines[1].First()));
        }


        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
    }
}