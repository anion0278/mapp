using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Xunit.Categories;
using Mapp.Common;
using Mapp.DataAccess;
using Mapp.BusinessLogic.Currency;
using Mapp.BusinessLogic.Invoices;
using VerifyXunit;
using FluentAssertions;
using FluentAssertions.Extensions;

namespace Mapp.BusinessLogic.Transactions.Tests
{
    [UsesVerify]
    [IntegrationTest]
    public class IntegrationTests : VerifyBase
    {
        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_Shopify()
        {
            await IntegrationTestBase("Shopify");
        }

        //[Fact]
        //public async Task ConvertTransactions_ParsesAndConvertsTransactions_Paypal()
        //{
        //    await IntegrationTestBase("PayPal");
        //}

        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_PaypalCZ()
        {
            await IntegrationTestBase("PayPalCZ");
        }

        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_AmazonAU()
        {
            await IntegrationTestBase("AmazonAU");
        }

        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_AmazonBE()
        {
            await IntegrationTestBase("AmazonBE");
        }

        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_AmazonCA()
        {
            await IntegrationTestBase("AmazonCA");
        }

        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_AmazonDE()
        {
            await IntegrationTestBase("AmazonDE");
        }

        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_AmazonES()
        {
            await IntegrationTestBase("AmazonES");
        }

        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_AmazonFR()
        {
            await IntegrationTestBase("AmazonFR");
        }

        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_AmazonGB()
        {
            await IntegrationTestBase("AmazonGB");
        }

        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_AmazonIT()
        {
            await IntegrationTestBase("AmazonIT");
        }

        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_AmazonJP()
        {
            await IntegrationTestBase("AmazonJP");
        }

        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_AmazonMX()
        {
            await IntegrationTestBase("AmazonMX");
        }

        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_AmazonNL()
        {
            await IntegrationTestBase("AmazonNL");
        }

        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_AmazonPL()
        {
            await IntegrationTestBase("AmazonPL");
        }

        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_AmazonSE()
        {
            await IntegrationTestBase("AmazonSE");
        }

        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_AmazonSG()
        {
            await IntegrationTestBase("AmazonSG");
        }

        [Fact]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_AmazonUS()
        {
            await IntegrationTestBase("AmazonUS");
        }

        [Fact]
        [Bug]
        public async Task ConvertTransactions_ParsesAndConvertsTransactions_ZerosBug()
        {
            await IntegrationTestBase("Zeros");
        }

        private async Task IntegrationTestBase(string testCaseDataName)
        {
            // Arrange
            string inputTransactionFile = @$"../../../TestData/{testCaseDataName}.csv";

            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var configMock = new Mock<ISettingsWrapper>();
            configMock.Setup(m => m.TransactionConverterConfigsDir).Returns(@"..\..\..\..\Mapp.UI\Transactions Configs");

            string resultText = string.Empty;
            var fileManagerMock = new Mock<IFileManager>();
            fileManagerMock
                .Setup(fm => fm.WriteAllTextToFile(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((name, content) => resultText = content);

            var dateTimeManagerMock = new Mock<IDateTimeManager>();
            dateTimeManagerMock
                .Setup(m => m.Today).Returns(1.April(2024));

            var jsonManager = new JsonManager(configMock.Object, new FileManager());
            var transactionsConverter = new TransactionsReader(jsonManager);
            var gpcGenerator = new GpcGenerator(fileManagerMock.Object, dateTimeManagerMock.Object);

            // Act
            var transactions = transactionsConverter.ReadTransactionsFromMultipleFiles(new[] { inputTransactionFile });
            gpcGenerator.SaveTransactions(transactions, "IrrelevantFileName");

            await Verify(resultText);
        }

        public IntegrationTests() : base() { }
    }
}
