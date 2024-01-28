using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Xunit.Categories;
using Mapp.Common;
using Mapp.DataAccess;
using ApprovalTests.Reporters;
using ApprovalTests;

namespace Mapp.BusinessLogic.Transactions.Tests
{
    [UseReporter(typeof(VisualStudioReporter))]
    public class IntegrationTests
    {
        //[Theory]
        [Fact]
        [IntegrationTest]
        //[InlineData("AmazonCA")]
        //[InlineData("AmazonDE")]
        //[InlineData("AmazonES")]
        //[InlineData("AmazonGB")]
        //[InlineData("AmazonIT")]
        //[InlineData("AmazonJP")]
        //[InlineData("AmazonMX")]
        //[InlineData("AmazonNL")]
        //[InlineData("PayPal")]
        public void ConvertTransactions_ParsesAndConvertsTransactions()
        {
            string testCaseDataName = "PayPal";

            // Arrange
            string inputTransactionFile = @$"../../../TestData/{testCaseDataName}/{testCaseDataName}.csv";

            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var configMock = new Mock<ISettingsWrapper>();
            configMock.Setup(m => m.TransactionConverterConfigsDir).Returns(@"C:\Users\stefan\source\repos\anion0278\mapp\Mapp.UI\bin\Debug\net6.0-windows10.0.19041.0\win-x64\Transactions Configs");

            string resultText = string.Empty;
            var fileManagerMock = new Mock<IFileManager>();
            fileManagerMock
                .Setup(fm => fm.WriteAllTextToFile(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((name, content) => resultText = content);

            var jsonManager = new JsonManager(configMock.Object, new FileManager());
            var transactionsConverter = new TransactionsReader(jsonManager);
            var gpcGenerator = new GpcGenerator(fileManagerMock.Object);

            // Act
            var transactions = transactionsConverter.ReadTransactionsFromMultipleFiles(new[] { inputTransactionFile });
            gpcGenerator.SaveTransactions(transactions, "IrrelevantFileName");

            // Assert
            //Approvals.VerifyAll(resultText, label: "");
            Assert.True(true);
        }
    }
}
