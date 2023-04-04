using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Shmap.BusinessLogic.Currency;
using Shmap.CommonServices;
using Shmap.DataAccess;
using Moq;
using VerifyXunit;
using Xunit;

namespace Shmap.BusinessLogic.Invoices.Tests
{
    [UsesVerify]
    public class IntegrationTests: VerifyBase
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

        [Fact]
        public async Task Hermes_Shipping()
        {
            await IntegrationTestBase("hermes shipping", 300000000);
        }

        [Fact]
        public async Task Multi_Item_Order()
        {
            await IntegrationTestBase("multi-item order", 300000000);
        }

        [Fact]
        public async Task Order_Number_Zeros()
        {
            await IntegrationTestBase("order number zeros", 300000000);
        }

        [Fact]
        public async Task Quantity()
        {
            await IntegrationTestBase("quantity", 300000000);
        }

        private async Task IntegrationTestBase(string testCaseDataDirName, int startingOrderNumber) // TODO testCaseDataDirName - use CallerMethodName attribute, Or use Theories?
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

            ApplicationConstants.Rounding = 2; // TODO solve by intoducing settings context

            var jsonManager = new JsonManager();
            var invoiceXmlXmlManager = new InvoicesXmlManager(Mock.Of<IDialogService>(), configMock.Object) ;
            var currencyLoader = new CsvLoader(configMock.Object);
            var autocompleteDataLoader = new AutocompleteDataLoader(jsonManager, configMock.Object);
            var autocompleteData = autocompleteDataLoader.LoadSettings();
            var invoiceConverter = new InvoiceConverter(
                autocompleteData,
                new CurrencyConverter(),
                currencyLoader,
                invoiceXmlXmlManager,
                Mock.Of<IAutocompleteDataLoader>(),
                dialogServiceMock.Object);

            var conversionContext = new InvoiceConversionContext()
            {
                ConvertToDate = new DateTime(2021, 10, 20),
                DefaultEmail = "info@czechdrawing.com", 
                ExistingInvoiceNumber = (uint)startingOrderNumber,
            };
            
            var invoices = invoiceConverter.LoadAmazonReports(new[] { inputAmazonReportFilePath }, conversionContext);

            string resultFileName = Path.Join(Path.GetDirectoryName(expectedResultFilePath), "result.xml");
            invoiceConverter.ProcessInvoices(invoices, resultFileName); // TODO pass JSON-to-memory-saver

            //string expectedResult = File.ReadAllText(expectedResultFilePath); // TODO save to memory
            string result = await File.ReadAllTextAsync(resultFileName);

            await Verify(result);
            //Assert.Equal(expectedResult, result);
        }

        public IntegrationTests(): base(){ }
    }
}
