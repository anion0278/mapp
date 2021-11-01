using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Shmap.BusinessLogic.AutocompletionHelper;
using Shmap.BusinessLogic.Currency;
using Shmap.CommonServices;
using Shmap.DataAccess;
using Moq;
using Xunit;

namespace Shmap.BusinessLogic.Invoices.Tests
{
    public class IntegrationTests // Charact. tests?
    {
        // TODO add multi-file import test

        //[Fact]
        //public void Autocomplete_Sku()
        //{
        //    IntegrationTestBase("autocomplete sku", 300000000);
        //}

        //[Fact] // TODO example name IsNameUnique_GivenItems_ReturnsTrueIfUnique
        //public void Discounts_Aggregation()
        //{
        //    IntegrationTestBase("discounts aggregation", 300000000);
        //}

        //[Fact]
        //public void Empty_Line()
        //{
        //    IntegrationTestBase("empty line", 300000000);
        //}

        //[Fact]
        //public void General()
        //{
        //    IntegrationTestBase("general", 300000000);
        //}

        //[Fact]
        //public void Hermes_Shipping()
        //{
        //    IntegrationTestBase("hermes shipping", 300000000);
        ////}

        //[Fact]
        //public void Multi_Item_Order()
        //{
        //    IntegrationTestBase("multi-item order", 300000000);
        //}

        [Fact]
        public void Order_Number_Zeros()
        {
            IntegrationTestBase("order number zeros", 300000000);
        }

        [Fact]
        public void Quantity()
        {
            IntegrationTestBase("quantity", 300000000);
        }

        private void IntegrationTestBase(string testCaseDataDirName, int startingOrderNumber)
        {
            string inputAmazonReportFilePath = @$"TestData\{testCaseDataDirName}\amazon.txt";
            string expectedResultFilePath = @$"TestData\{testCaseDataDirName}\converted.xml";

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); 
            string invoiceDir = "TestData\\TestSettings\\Invoice Converter";

            var configMock = new Mock<IConfigProvider>();
            configMock.Setup(m => m.InvoiceConverterConfigsDir).Returns(invoiceDir);

            var dialogServiceMock = new Mock<IDialogService>();
            dialogServiceMock
                .Setup(m => m.AskToChangeLongStringIfNeeded(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns((string _, string s2, int _) => s2);

            ApplicationConstants.Rounding = 2;

            var _jsonManager = new JsonManager();
            var _invoiceXmlXmlManager = new InvoicesXmlManager(Mock.Of<IDialogService>(), configMock.Object) ;
            var _currencyLoader = new CsvLoader(configMock.Object);
            var _autocompleteDataLoader = new AutocompleteDataLoader(_jsonManager, configMock.Object);
            var _autocompleteData = _autocompleteDataLoader.LoadSettings();
            var InvoiceConverter = new InvoiceConverter(
                _autocompleteData,
                new CurrencyConverter(),
                _currencyLoader,
                _invoiceXmlXmlManager,
                Mock.Of<IAutocompleteDataLoader>(),
                dialogServiceMock.Object);

            var conversionContext = new InvoiceConversionContext()
            {
                ConvertToDate = new DateTime(2021, 10, 20),
                DefaultEmail = "info@czechdrawing.com", 
                ExistingInvoiceNumber = (uint)startingOrderNumber,
            };

            var invoices = InvoiceConverter.LoadAmazonReports(new[] { inputAmazonReportFilePath }, conversionContext);

            string resultFileName = Path.Join(Path.GetDirectoryName(expectedResultFilePath), "result.xml");
            InvoiceConverter.ProcessInvoices(invoices, resultFileName);

            string expectedResult = File.ReadAllText(expectedResultFilePath); // TODO save to memory
            string result = File.ReadAllText(resultFileName);

            Assert.Equal(expectedResult, result);
        }
    }
}
