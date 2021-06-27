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
        [Fact]
        public void Multiple_Discounts_Aggregation()
        {
            IntegrationTestBase("aggregation of discounts", 290100449);
        }

        [Fact]
        public void Autocomplete_Sku()
        {
            IntegrationTestBase("autocomplete sku", 290100450);
        }

        [Fact]
        public void Empty_Line()
        {
            IntegrationTestBase("empty line", 290100436);
        }

        [Fact]
        public void General()
        {
            IntegrationTestBase("general", 290100476);
        }

        [Fact]
        public void Hermes_Shipping()
        {
            IntegrationTestBase("hermes shipping", 290100478);
        }

        [Fact]
        public void Multi_Item_Order()
        {
            IntegrationTestBase("multi-item order", 290100442);
        }

        [Fact]
        public void Order_Number_Zeros()
        {
            IntegrationTestBase("order number zeros", 290100479);
        }

        [Fact]
        public void Quantity()
        {
            IntegrationTestBase("quantity", 290100480);
        }

        private void IntegrationTestBase(string testCaseDataDirName, int startingOrderNumber)
        {
            string inputAmazonReportFilePath = @$"TestData\{testCaseDataDirName}\amazon.txt";
            string expectedResultFilePath = @$"TestData\{testCaseDataDirName}\converted.xml";

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string invoiceDir = "TestData\\TestSettings\\Invoice Converter";

            var _jsonManager = new JsonManager();
            var _invoiceXmlXmlManager = new InvoicesXmlManager(invoiceDir) { UserNotification = (_, _) => { } };
            var _currencyLoader = new CurrencyLoader(invoiceDir);
            var _autocompleteDataLoader = new AutocompleteDataLoader(_jsonManager, invoiceDir);
            var _autocompleteData = _autocompleteDataLoader.LoadSettings();
            var InvoiceConverter = new InvoiceConverter(
                _autocompleteData,
                new CurrencyConverter(),
                _currencyLoader,
                _invoiceXmlXmlManager,
                (msg, stringToChange, maxLen) => { return stringToChange; },
                _autocompleteDataLoader);

            InvoiceConverter.ExistingInvoiceNumber = (uint) startingOrderNumber;
            InvoiceConverter.DPH = (decimal)0.1736;
            InvoiceConverter.DefaultEmail = "info@czechdrawing.com";

            InvoiceConverter.LoadAmazonReports(new[] { inputAmazonReportFilePath });

            string resultFile = Path.GetFullPath(expectedResultFilePath) + "result.xml";
            InvoiceConverter.ProcessInvoices(resultFile);

            string expectedResult = File.ReadAllText(expectedResultFilePath); // TODO save to memory
            string result = File.ReadAllText(resultFile);

            Assert.Equal(expectedResult, result);
        }
    }
}
