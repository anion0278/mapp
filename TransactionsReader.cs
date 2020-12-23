using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
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
                new TransactionsFileLanguageSettings(new CultureInfo("en-AU"), @"(.*) GMT")
                {
                    DistinctionPhrase = "All amounts in AUD, unless specified",
                    DateTimeColumnNames = new []{"date/time"}, OrderIdColumnName = "order id",
                    TransactionTypeColumnName = "type", MarketplaceColumnName = "marketplace",
                    DebugDescriptionColumnName = "description", 
                    ValueComponentsColumnName = new []{"product sales", "shipping credits","promotional rebates"},
                    TotalPriceColumnName = "total",
                    OrderTypeNames = new []{"Order"}, RefundTypeNames = new []{"Refund"}, TransferTypeNames = new []{"Transfer"}, ServiceFeeTypeNames = new []{"Service Fee"},
                    LinesToSkipBeforeColumnNames = 7
                    },
                new TransactionsFileLanguageSettings(new CultureInfo("en-CA"), @"(.*) (PST|PDT)")
                {
                    DistinctionPhrase = "All amounts in CAD, unless specified",
                    DateTimeColumnNames = new []{"date/time"}, OrderIdColumnName = "order id",
                    TransactionTypeColumnName = "type", MarketplaceColumnName = "marketplace",
                    DebugDescriptionColumnName = "description",
                    ValueComponentsColumnName = new []{ "product sales", "shipping credits", "promotional rebates"},
                    TotalPriceColumnName = "total",
                    OrderTypeNames = new []{"Order"}, RefundTypeNames = new []{"Refund"}, TransferTypeNames = new []{"Transfer"},ServiceFeeTypeNames = new[] { "Service Fee" },
                    LinesToSkipBeforeColumnNames = 7
                },
                new TransactionsFileLanguageSettings(new CultureInfo("de-DE"), @"(.*) GMT")
                {
                    DistinctionPhrase = "Alle Beträge in Euro sofern nicht anders gekennzeichnet",
                    DateTimeColumnNames = new []{"Datum/Uhrzeit"}, OrderIdColumnName = "Bestellnummer",
                    TransactionTypeColumnName = "Typ", MarketplaceColumnName = "Marketplace",
                    DebugDescriptionColumnName = "Beschreibung",
                    ValueComponentsColumnName = new []{ "Umsätze", "Gutschrift für Versandkosten", "Rabatte aus Werbeaktionen"},
                    TotalPriceColumnName = "Gesamt",
                    OrderTypeNames = new []{ "Anpassung", "Bestellung" }, RefundTypeNames = new []{"Erstattung"}, TransferTypeNames = new []{"Übertrag"},ServiceFeeTypeNames = new[] { "Servicegebühr" },
                    LinesToSkipBeforeColumnNames = 6
                },
                new TransactionsFileLanguageSettings(new CultureInfo("es-ES"), @"(.*) GMT")
                {
                    DistinctionPhrase = "Todos los importes en EUR, a menos que se especifique lo contrario",
                    DateTimeColumnNames = new []{ "fecha y hora" }, OrderIdColumnName = "número de pedido",
                    TransactionTypeColumnName = "tipo", MarketplaceColumnName = "web de Amazon",
                    DebugDescriptionColumnName = "descripción",
                    ValueComponentsColumnName = new []{ "ventas de productos","abonos de envío", "devoluciones promocionales"},
                    TotalPriceColumnName = "total",
                    OrderTypeNames = new []{ "Pedido"}, RefundTypeNames = new []{ "Reembolso"}, TransferTypeNames = new []{ "Transferir"},
                    LinesToSkipBeforeColumnNames = 6
                },
                new TransactionsFileLanguageSettings(new CultureInfo("fr-FR"), @"(.*) UTC")
                {
                    DistinctionPhrase = "Tous les montants sont en EUR, sauf mention contraire.",
                    DateTimeColumnNames = new []{"date/heure"}, OrderIdColumnName = "numéro de la commande",
                    TransactionTypeColumnName = "type", MarketplaceColumnName = "Marketplace",
                    DebugDescriptionColumnName = "description",
                    ValueComponentsColumnName = new []{ "ventes de produits", "crédits d'expédition", "Rabais promotionnels"},
                    TotalPriceColumnName = "total",
                    OrderTypeNames = new []{"Commande"}, RefundTypeNames = new []{"Remboursement"}, TransferTypeNames = new []{"Transfert"},ServiceFeeTypeNames = new []{"Service Fee"},
                    LinesToSkipBeforeColumnNames = 6
                },
                new TransactionsFileLanguageSettings(new CultureInfo("en-GB"), @"(.*) GMT")
                {
                    DistinctionPhrase = "All amounts in GBP, unless specified",
                    DateTimeColumnNames = new []{"date/time"}, OrderIdColumnName = "order id",
                    TransactionTypeColumnName = "type", MarketplaceColumnName = "marketplace",
                    DebugDescriptionColumnName = "description",
                    ValueComponentsColumnName = new []{ "product sales", "postage credits", "promotional rebates"},
                    TotalPriceColumnName = "total",
                    OrderTypeNames = new []{"Order"}, RefundTypeNames = new []{"Refund"}, TransferTypeNames = new []{"Transfer"},ServiceFeeTypeNames = new []{"Service Fee"},
                    LinesToSkipBeforeColumnNames = 6
                },
                new TransactionsFileLanguageSettings(italyCulture, @"(.*) GMT")
                {
                    DistinctionPhrase = "Tutti gli importi sono espressi in EUR, se non diversamente specificato.",
                    DateTimeColumnNames = new []{"Data/Ora:"}, OrderIdColumnName = "Numero ordine",
                    TransactionTypeColumnName = "Tipo", MarketplaceColumnName = "Marketplace",
                    DebugDescriptionColumnName = "Descrizione",
                    ValueComponentsColumnName = new []{ "Vendite","Accrediti per le spedizioni", "Sconti promozionali"},
                    TotalPriceColumnName = "totale",
                    OrderTypeNames = new []{"Ordine"}, RefundTypeNames = new []{"Rimborso"}, TransferTypeNames = new []{"Trasferimento"}, ServiceFeeTypeNames = new []{"Service Fee"},
                    LinesToSkipBeforeColumnNames = 6
                },
                new TransactionsFileLanguageSettings(new CultureInfo("ja-JP"), @"(.*)JST")
                {
                    DistinctionPhrase = "指定のない場合、単位は円",
                    DateTimeColumnNames = new []{"日付/時間"}, OrderIdColumnName = "注文番号",
                    TransactionTypeColumnName = "トランザクションの種類", MarketplaceColumnName = "Amazon 出品サービス",
                    DebugDescriptionColumnName = "説明",
                    ValueComponentsColumnName = new []{ "商品売上","配送料", "プロモーション割引額", "商品の売上税", "配送料の税金"},
                    TotalPriceColumnName = "合計",
                    OrderTypeNames = new []{"注文"}, RefundTypeNames = new []{"返金"}, TransferTypeNames = new []{"振込み"}, ServiceFeeTypeNames = new []{"注文外料金"},
                    LinesToSkipBeforeColumnNames = 6
                },
                new TransactionsFileLanguageSettings(new CultureInfo("es-MX"), @"(.*) (PST|PDT)")
                {
                    DistinctionPhrase = "Todos los importes en dólares, a menos que se especifique",
                    DateTimeColumnNames = new []{"fecha/hora"}, OrderIdColumnName = "Id. del pedido",
                    TransactionTypeColumnName = "tipo", MarketplaceColumnName = "marketplace",
                    DebugDescriptionColumnName = "descripción",
                    ValueComponentsColumnName = new []{ "ventas de productos", "créditos de envío", "descuentos promocionales"},
                    TotalPriceColumnName = "total",
                    OrderTypeNames = new []{"Pedido"}, RefundTypeNames = new []{"Reembolso"}, TransferTypeNames = new []{"Trasferir"}, ServiceFeeTypeNames = new []{"Service Fee"}, // TODO Service fee
                    LinesToSkipBeforeColumnNames = 7
                },
                new TransactionsFileLanguageSettings(new CultureInfo("en-US"), @"(.*) (PST|PDT)")
                {
                    DistinctionPhrase = "All amounts in USD, unless specified",
                    DateTimeColumnNames = new []{"date/time"}, OrderIdColumnName = "order id",
                    TransactionTypeColumnName = "type", MarketplaceColumnName = "marketplace",
                    DebugDescriptionColumnName = "description",
                    ValueComponentsColumnName = new []{ "product sales", "shipping credits", "promotional rebates"},
                    TotalPriceColumnName = "total",
                    OrderTypeNames = new []{"Order"}, RefundTypeNames = new []{"Refund"}, TransferTypeNames = new []{"Transfer"},ServiceFeeTypeNames = new []{"Service Fee"},
                    LinesToSkipBeforeColumnNames = 7
                },
                new TransactionsFileLanguageSettings(new CultureInfo("nl-NL"), @"(.*) GMT")
                {
                    DistinctionPhrase = "Alle bedragen in EUR, tenzij anders vermeld",
                    DateTimeColumnNames = new []{"datum/tijd"}, OrderIdColumnName = "bestelnummer",
                    TransactionTypeColumnName = "type", MarketplaceColumnName = "marketplace",
                    DebugDescriptionColumnName = "beschrijving",
                    ValueComponentsColumnName = new []{ "verkoop van producten","Verzendtegoeden","promotiekortingen"},
                    TotalPriceColumnName = "totaal",
                    OrderTypeNames = new []{"Bestelling"}, RefundTypeNames = new []{"Terugbetaling"}, TransferTypeNames = new []{"Overdracht"},ServiceFeeTypeNames = new []{"Servicekosten"},
                    LinesToSkipBeforeColumnNames = 6
                },
                new TransactionsFileLanguageSettings(new CultureInfo("en-US"), @"(.*)")
                {
                    DistinctionPhrase = "Date",
                    DateTimeColumnNames = new []{"Date", "Time"}, OrderIdColumnName = "Transaction ID",
                    TransactionTypeColumnName = "Description", MarketplaceColumnName = "Time Zone", // TODO very wrong
                    DebugDescriptionColumnName = "From Email Address", // TODO can be none
                    
                    ValueComponentsColumnName = new []{ "Gross", "Sales Tax"},

                    TotalPriceColumnName = "Gross",

                    OrderTypeNames = new []{"eBay Auction Payment", "Express Checkout Payment", "General Payment", "Mobile Payment", "Website Payment"}, 
                    RefundTypeNames = new []{"Payment Refund"}, 
                    TransferTypeNames = new []{"General Withdrawal - Bank Account"},
                    ServiceFeeTypeNames = new []{"General Currency Conversion", "Tax collected by partner", "Cancellation of Hold for Dispute Resolution", "Hold on Balance for Dispute Investigation", "PreApproved Payment Bill User Payment"},

                    LinesToSkipBeforeColumnNames = 0
                },
            };

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

        public DateTime ParseDate(string dateString, TransactionsFileLanguageSettings settings)
        {
            var match = Regex.Match(dateString, settings.DateSubstring);
            return DateTime.Parse(match.Groups[1].Value, settings.DateCultureInfo);
        }

        public IEnumerable<Transaction> ReadTransactions(string fileName)
        {
            string japaneseCrazyEncoding = "�w��̂Ȃ��ꍇ�A�P�ʂ͉~";

            var lines = GetFileLines(fileName);

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
                AmazonMarketplace marketplace = 0;

                // FFFF!!! japanese have not decided whether its amazon.co.jp or amazon.jp
                string marketplaceStr = transactionsDict[languageSetting.MarketplaceColumnName][index];
                marketplaceStr = marketplaceStr.EqualsIgnoreCase("amazon.co.jp") ? "amazon.jp" : marketplaceStr;

                if (!string.IsNullOrEmpty(transactionsDict[languageSetting.MarketplaceColumnName][index]))
                {
                    marketplace = GetValueFromDescription<AmazonMarketplace>(marketplaceStr);
                }

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
                    ProductDescription = transactionsDict[languageSetting.DebugDescriptionColumnName][index],
                    TransactionValue = transactionTotalValue,
                    Type = transactionType,
                    Marketplace = marketplace,
                };
                transactions.Add(transaction);
            }

            var orders = transactions.Where(t => t.Type.Equals(TransactionTypes.Order)).ToList();
            if (orders.Any())
            {
                var averageMarketplace = (AmazonMarketplace)orders.Average(t => (int)t.Marketplace);
                foreach (var transaction in transactions.Except(orders))
                {
                    transaction.Marketplace = averageMarketplace;
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

        public TransactionTypes ParseTransactionType(string transactionType, TransactionsFileLanguageSettings settings)
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

        private TransactionsFileLanguageSettings GetLanguageSetting(IReadOnlyList<string[]> dataLines)
        {
            // TODO handle possible exception on higher level
            // put into factory
            foreach (var marketPlace in _languageSettings)
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
