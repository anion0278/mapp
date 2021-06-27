using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Shmap.CommonServices;
using Microsoft.VisualBasic.FileIO;
using Formatting = System.Xml.Formatting;

namespace Shmap.DataAccess
{
    public class InvoicesXmlManager : IInvoicesXmlManager
    {
        private readonly string _invoiceConverterConfigsDir;

        public EventHandler<string> UserNotification { get; init; }
        public EventHandler<string> UserInteraction { get; init; }

        private Encoding XmlEncoding = CodePagesEncodingProvider.Instance.GetEncoding("windows-1250");

        public InvoicesXmlManager(string invoiceConverterConfigsDir)
        {
            _invoiceConverterConfigsDir = invoiceConverterConfigsDir;
        }

        public InvoiceXML.dataPackDataPackItem PrepareDatapackItem()
        {
            InvoiceXML.dataPack dataPack;
            using (StreamReader streamReader = new StreamReader(Path.Combine(_invoiceConverterConfigsDir, "InvoiceBasic"), XmlEncoding))
                dataPack = (InvoiceXML.dataPack)new XmlSerializer(typeof(InvoiceXML.dataPack)).Deserialize(streamReader);
            return dataPack.dataPackItem[0];
        }


        public void SerializeXmlInvoice(string fileName, InvoiceXML.dataPack invoice)
        {
            var settings = new XmlWriterSettings();
            settings.Encoding = XmlEncoding;
            settings.NamespaceHandling = NamespaceHandling.Default;
            settings.Indent = true;

            var xmlSerializer = new XmlSerializer(invoice.GetType());
            using (var xmlWriter = XmlWriter.Create(fileName, settings))
            {
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add("dat", "http://www.stormware.cz/schema/version_2/data.xsd");
                xmlSerializer.Serialize(xmlWriter, invoice, namespaces);
            }
            FixNamespaces(fileName);
        }

        public InvoiceXML.dataPack PrepareInvoice(IEnumerable<InvoiceXML.dataPackDataPackItem> dataItems)
        {
            // TODO CHECK DATA 
            return new()
            {
                ico = 5448034,
                id = "Usr01",
                key = "efd0db6a-9c08-4bb5-befb-36809eadcba1",
                programVersion = "12101.4 (7.1.2019)",
                application = "Transformace",
                note = "Uživatelský export",
                dataPackItem = dataItems.ToArray(),
                version = new decimal(20, 0, 0, false, 1)
            };
        }

        private void FixNamespaces(string resultFilePath)
        {
            File.WriteAllText(resultFilePath,
                    File.ReadAllText(resultFilePath, XmlEncoding)
                    .Replace("5448034", "05448034")
                    .Replace("<inv:invoiceItem xmlns:typ=\"http://www.stormware.cz/schema/version_2/type.xsd\">", "<inv:invoiceItem>")
                    .Replace("<inv:invoiceDetail>", "<inv:invoiceDetail xmlns:rsp=\"http://www.stormware.cz/schema/version_2/response.xsd\" xmlns:rdc=\"http://www.stormware.cz/schema/version_2/documentresponse.xsd\" xmlns:typ=\"http://www.stormware.cz/schema/version_2/type.xsd\" xmlns:lst=\"http://www.stormware.cz/schema/version_2/list.xsd\" xmlns:lStk=\"http://www.stormware.cz/schema/version_2/list_stock.xsd\" xmlns:lAdb=\"http://www.stormware.cz/schema/version_2/list_addBook.xsd\" xmlns:lCen=\"http://www.stormware.cz/schema/version_2/list_centre.xsd\" xmlns:lAcv=\"http://www.stormware.cz/schema/version_2/list_activity.xsd\" xmlns:acu=\"http://www.stormware.cz/schema/version_2/accountingunit.xsd\" xmlns:vch=\"http://www.stormware.cz/schema/version_2/voucher.xsd\" xmlns:int=\"http://www.stormware.cz/schema/version_2/intDoc.xsd\" xmlns:stk=\"http://www.stormware.cz/schema/version_2/stock.xsd\" xmlns:ord=\"http://www.stormware.cz/schema/version_2/order.xsd\" xmlns:ofr=\"http://www.stormware.cz/schema/version_2/offer.xsd\" xmlns:enq=\"http://www.stormware.cz/schema/version_2/enquiry.xsd\" xmlns:vyd=\"http://www.stormware.cz/schema/version_2/vydejka.xsd\" xmlns:pri=\"http://www.stormware.cz/schema/version_2/prijemka.xsd\" xmlns:bal=\"http://www.stormware.cz/schema/version_2/balance.xsd\" xmlns:pre=\"http://www.stormware.cz/schema/version_2/prevodka.xsd\" xmlns:vyr=\"http://www.stormware.cz/schema/version_2/vyroba.xsd\" xmlns:pro=\"http://www.stormware.cz/schema/version_2/prodejka.xsd\" xmlns:con=\"http://www.stormware.cz/schema/version_2/contract.xsd\" xmlns:adb=\"http://www.stormware.cz/schema/version_2/addressbook.xsd\" xmlns:prm=\"http://www.stormware.cz/schema/version_2/parameter.xsd\" xmlns:lCon=\"http://www.stormware.cz/schema/version_2/list_contract.xsd\" xmlns:ctg=\"http://www.stormware.cz/schema/version_2/category.xsd\" xmlns:ipm=\"http://www.stormware.cz/schema/version_2/intParam.xsd\" xmlns:str=\"http://www.stormware.cz/schema/version_2/storage.xsd\" xmlns:idp=\"http://www.stormware.cz/schema/version_2/individualPrice.xsd\" xmlns:sup=\"http://www.stormware.cz/schema/version_2/supplier.xsd\" xmlns:prn=\"http://www.stormware.cz/schema/version_2/print.xsd\" xmlns:sEET=\"http://www.stormware.cz/schema/version_2/sendEET.xsd\" xmlns:act=\"http://www.stormware.cz/schema/version_2/accountancy.xsd\" xmlns:bnk=\"http://www.stormware.cz/schema/version_2/bank.xsd\" xmlns:sto=\"http://www.stormware.cz/schema/version_2/store.xsd\" xmlns:grs=\"http://www.stormware.cz/schema/version_2/groupStocks.xsd\" xmlns:acp=\"http://www.stormware.cz/schema/version_2/actionPrice.xsd\" xmlns:csh=\"http://www.stormware.cz/schema/version_2/cashRegister.xsd\" xmlns:bka=\"http://www.stormware.cz/schema/version_2/bankAccount.xsd\" xmlns:ilt=\"http://www.stormware.cz/schema/version_2/inventoryLists.xsd\" xmlns:nms=\"http://www.stormware.cz/schema/version_2/numericalSeries.xsd\" xmlns:pay=\"http://www.stormware.cz/schema/version_2/payment.xsd\" xmlns:mKasa=\"http://www.stormware.cz/schema/version_2/mKasa.xsd\" xmlns:gdp=\"http://www.stormware.cz/schema/version_2/GDPR.xsd\" xmlns:est=\"http://www.stormware.cz/schema/version_2/establishment.xsd\" xmlns:cen=\"http://www.stormware.cz/schema/version_2/centre.xsd\" xmlns:acv=\"http://www.stormware.cz/schema/version_2/activity.xsd\" xmlns:ftr=\"http://www.stormware.cz/schema/version_2/filter.xsd\">"), XmlEncoding);
        }

        public IEnumerable<Dictionary<string, string>> LoadAmazonReports(IEnumerable<string> fileNames)
        {
            var dictList = new List<Dictionary<string, string>>();
            foreach (var fileName in fileNames)
            {
                var validLines = GetOrderDataLinesFromSingleFile(fileName, "\t");

                for (int lineIndex = 1; lineIndex < validLines.Count; lineIndex++)
                {
                    var invoiceDict = new Dictionary<string, string>();
                    for (int columnIndex = 0; columnIndex < validLines[0].Length; ++columnIndex)
                    {
                        string columnNameKey = validLines[0][columnIndex];
                        invoiceDict.Add(columnNameKey, validLines[lineIndex][columnIndex]);
                    }

                    dictList.Add(invoiceDict);
                }
            }

            return dictList;
        }

        private List<string[]> GetOrderDataLinesFromSingleFile(string fileName, string delimiter)
        {
            var lineItems = new List<string[]>();
            using (var textFieldParser = new TextFieldParser(fileName))
            {
                textFieldParser.TextFieldType = FieldType.Delimited;
                textFieldParser.SetDelimiters(delimiter);
                while (!textFieldParser.EndOfData)
                {
                    string[] orderLine = textFieldParser.ReadFields();
                    lineItems.Add(orderLine);
                }
            }

            var validLines = new List<string[]>();
            // remove empty lines
            for (var orderLineIndex = 0; orderLineIndex < lineItems.Count; orderLineIndex++)
            {
                var orderLine = lineItems[orderLineIndex];
                if (orderLine.Count(string.IsNullOrEmpty) > orderLine.Length / 2)
                {
                    UserNotification.Invoke(this,$"Objednavka {orderLine[0]} na radku {orderLineIndex} " +
                        $"ze souboru \'{Path.GetFileName(fileName)}\' obsahuje neplatny zaznam (prazdny). " +
                        "Zaznam bude odstranen.");
                    //MessageBox.Show(
                    //    $"Objednavka {orderLine[0]} na radku {orderLineIndex} " +
                    //    $"ze souboru \'{Path.GetFileName(fileName)}\' obsahuje neplatny zaznam (prazdny). " +
                    //    "Zaznam bude odstranen."); /// SHOULD RETURN WARNINGS!!
                    continue;
                }

                validLines.Add(orderLine);
            }

            return validLines;
        }
    }

    public interface IInvoicesXmlManager: IInteractionRequester
    {
        IEnumerable<Dictionary<string, string>> LoadAmazonReports(IEnumerable<string> fileNames);

        void SerializeXmlInvoice(string fileName, InvoiceXML.dataPack invoice);

        InvoiceXML.dataPackDataPackItem PrepareDatapackItem();

        InvoiceXML.dataPack PrepareInvoice(IEnumerable<InvoiceXML.dataPackDataPackItem> dataItems);
    }
}