using System;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Shmap.BusinessLogic.AutocompletionHelper;
using Shmap.BusinessLogic.Currency;
using Shmap.DataAccess;
using Xunit;

namespace Shmap.BusinessLogic.Invoices.Tests
{
    public class IntegrationTests // Charact. tests?
    {
        // TODO add multi-file import test

        [Fact]
        public void Autocomplete_Sku()
        {
            IntegrationTestBase("autocomplete sku", 294200489);
        }

        [Fact]
        public void Discounts_Aggregation()
        {
            IntegrationTestBase("discounts aggregation", 294200515);
        }

        [Fact]
        public void Empty_Line()
        {
            IntegrationTestBase("empty line", 294200516);
        }

        [Fact]
        public void General()
        {
            IntegrationTestBase("general", 294200518);
        }

        [Fact]
        public void Hermes_Shipping()
        {
            IntegrationTestBase("hermes shipping", 294200521);
        }

        [Fact]
        public void Multi_Item_Order()
        {
            IntegrationTestBase("multi-item order", 294200522);
        }

        [Fact]
        public void Order_Number_Zeros()
        {
            IntegrationTestBase("order number zeros", 294200529);
        }

        [Fact]
        public void Quantity()
        {
            IntegrationTestBase("quantity", 294200530);
        }

        private void IntegrationTestBase(string testCaseDataDirName, int startingOrderNumber)
        {
            string inputAmazonReportFilePath = @$"TestData\{testCaseDataDirName}\amazon.txt";
            string expectedResultFilePath = @$"TestData\{testCaseDataDirName}\converted.xml";

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string invoiceDir = "TestData\\TestSettings\\Invoice Converter";

            var _jsonManager = new JsonManager();
            var _invoiceXmlXmlManager = new InvoicesXmlManager(invoiceDir) { UserNotification = (_, _) => { } };
            var _currencyLoader = new CsvLoader(invoiceDir);
            var _autocompleteDataLoader = new AutocompleteDataLoader(_jsonManager, invoiceDir);
            var _autocompleteData = _autocompleteDataLoader.LoadSettings();
            var InvoiceConverter = new InvoiceConverter(
                _autocompleteData,
                new CurrencyConverter(),
                _currencyLoader,
                _invoiceXmlXmlManager,
                (msg, stringToChange, maxLen) => { return stringToChange; },
                _autocompleteDataLoader);

            var conversionContext = new InvoiceConversionContext()
            {
                ConvertToDate = DateTime.Parse("26.09.2021"),
                DefaultEmail = "info@czechdrawing.com", 
                ExistingInvoiceNumber = (uint)startingOrderNumber,
            };

            InvoiceConverter.LoadAmazonReports(new[] { inputAmazonReportFilePath }, conversionContext);

            string resultFile = Path.Join(Path.GetDirectoryName(expectedResultFilePath),"result.xml");
            InvoiceConverter.ProcessInvoices(resultFile, out _);

            string expectedResult = File.ReadAllText(expectedResultFilePath); // TODO save to memory
            string result = File.ReadAllText(resultFile);

            Assert.Equal(expectedResult, result);
        }
    }
}
