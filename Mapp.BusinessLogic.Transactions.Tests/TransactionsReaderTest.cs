using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Xunit;

namespace Mapp.Business
{
    public class TransactionsReaderTest
    {

        // TODO create TestExtension assembly, put extension method for fast creation of autofixture with Automock

        [Fact]
        public void Read_Given_Returns()
        {
            // ReadTransactions - musi spadnout a zahlasit vlastni typ chyby pokud nenajde jakykoliv sloupec z tech co jsou nadefinovane v Config,
            // pripadne projet cele, posbirat vsechny vyjimky a vygenerovat vysledne exception

            //var reader = new TransactionsReader(new JsonManager()); // TODO moq

            //var transactions = reader.ReadTransactions(Path.Combine(Environment.CurrentDirectory, "Data", "2019DecMonthlyTransaction au.csv"));
            //transactions = reader.ReadTransactions(Path.Combine(Environment.CurrentDirectory, "Data", "2019DecMonthlyTransaction ca.csv"));
            //transactions = reader.ReadTransactions(Path.Combine(Environment.CurrentDirectory, "Data", "2019DecMonthlyTransaction de.csv"));
            //transactions = reader.ReadTransactions(Path.Combine(Environment.CurrentDirectory, "Data", "2019DecMonthlyTransaction es.csv"));
            //transactions = reader.ReadTransactions(Path.Combine(Environment.CurrentDirectory, "Data", "2019DecMonthlyTransaction fr.csv"));
            //transactions = reader.ReadTransactions(Path.Combine(Environment.CurrentDirectory, "Data", "2019DecMonthlyTransaction gb.csv"));
            //transactions = reader.ReadTransactions(Path.Combine(Environment.CurrentDirectory, "Data", "2019DecMonthlyTransaction it.csv"));
            //transactions = reader.ReadTransactions(Path.Combine(Environment.CurrentDirectory, "Data", "2019DecMonthlyTransaction mx.csv"));
            //transactions = reader.ReadTransactions(Path.Combine(Environment.CurrentDirectory, "Data", "2019DecMonthlyTransaction jp.csv"));
            //transactions = reader.ReadTransactions(Path.Combine(Environment.CurrentDirectory, "Data", "2019DecMonthlyTransaction us.csv"));

            Assert.True(true);
        }

        //[Fact]
        //public void ParseAuDate()
        //{
        //    var reader = new TransactionsReader(new JsonManager());
        //    var config = new MarketPlaceTransactionsConfig("en-AU", "(.*) GMT", 4);
        //    var result = reader.ParseDate("18/12/2019 6:13:02 PM GMT+09:00", config);

        //    Assert.Equal(new DateTime(2019, 12, 18, 18, 13, 02), result);
        //}

        //[Fact]
        //public void ParseCaDate()
        //{
        //    var reader = new TransactionsReader(new JsonManager());

        //    var result = reader.ParseDate("2019-12-18 11:41:16 PM PST", @"(.*) (PST|PDT)", new CultureInfo("en-CA"));

        //    Assert.Equal(new DateTime(2019, 12, 18, 23, 41, 16), result);
        //}

        //[Fact]
        //public void ParseDeDate()
        //{
        //    var reader = new TransactionsReader(new JsonManager());

        //    var result = reader.ParseDate("02.12.2019 08:03:44 GMT+00:00", @"(.*) GMT", new CultureInfo("de-DE"));

        //    Assert.Equal(new DateTime(2019, 12, 2, 8, 3, 44), result);
        //}

        //[Fact]
        //public void ParseEsDate()
        //{
        //    var reader = new TransactionsReader(new JsonManager());

        //    var result = reader.ParseDate("09/12/2019 08:37:20 GMT+00:00", @"(.*) GMT", new CultureInfo("es-ES"));

        //    Assert.Equal(new DateTime(2019, 12, 9, 8, 37, 20), result);
        //}

        //[Fact]
        //public void ParseMxDate()
        //{
        //    Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("");
        //    //var result = DateTime.Parse("2 déc. 2019 08:04:58", new CultureInfo(""));
        //    //var result2 = DateTime.Parse("12 févr. 2024 07:52:56", new CultureInfo("es-MX"));
        //    //var result = DateTime.Parse("8 feb 2024 23:25:40 GMT-8", new CultureInfo(""));
        //    var result2 = DateTime.Parse("8 feb 2024 23:25:40", new CultureInfo("es-MX"));

        //    Debugger.Break();
        //    //Assert.Equal(new DateTime(2019, 12, 2, 8, 4, 58), result);
        //}

        //[Fact]
        //public void ParseBgDate()
        //{
        //    var reader = new TransactionsReader(new JsonManager());

        //    var result = reader.ParseDate("2 Dec 2019 08:06:49 GMT+00:00", @"(.*) GMT", new CultureInfo("bg-BG"));

        //    Assert.Equal(new DateTime(2019, 12, 2, 8, 6, 49), result);
        //}

        //[Fact]
        //public void ParseItDate()
        //{
        //    var reader = new TransactionsReader(new JsonManager());

        //    var x = new CultureInfo("it-IT").DateTimeFormat.TimeSeparator;

        //    CultureInfo culture = CultureInfo.CreateSpecificCulture("it-IT");
        //    culture.DateTimeFormat.TimeSeparator = ".";

        //    var result = reader.ParseDate("10/dic/2019 01.59.23 GMT+00.00", @"(.*) GMT", culture);

        //    Assert.Equal(new DateTime(2019, 12, 10, 1, 59, 23), result);
        //}

        //[Fact]
        //public void ParseJpDate()
        //{
        //    var reader = new TransactionsReader(new JsonManager());

        //    var result = reader.ParseDate("2019/12/02 17:37:04JST", @"(.*)JST", new CultureInfo("ja-JP"));

        //    Assert.Equal(new DateTime(2019, 12, 02, 17, 37, 04), result);
        //}


        //[Fact]
        //public void ParseMxDate()
        //{
        //    var reader = new TransactionsReader(new JsonManager());

        //    var result = reader.ParseDate("06/12/2019 06:20:47 PST", @"(.*) (PST|PDT)", new CultureInfo("es-MX"));

        //    Assert.Equal(new DateTime(2019, 12, 06, 6, 20, 47), result);
        //}

        //[Fact]
        //public void ParseUsDate()
        //{
        //    var reader = new TransactionsReader(new JsonManager());

        //    var result = reader.ParseDate("Dec 1, 2019 11:42:55 PM PST", @"(.*) (PST|PDT)", new CultureInfo("en-US"));

        //    Assert.Equal(new DateTime(2019, 12, 1, 23, 42, 55), result);
        //}
    }
}
