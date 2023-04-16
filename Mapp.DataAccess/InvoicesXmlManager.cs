using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Shmap.CommonServices;
using Microsoft.VisualBasic.FileIO;

namespace Shmap.DataAccess
{
    public class InvoicesXmlManager : IInvoicesXmlManager
    {
        private readonly IDialogService _dialogService;
        private readonly IConfigProvider _configProvider;


        private readonly Encoding _xmlEncoding = CodePagesEncodingProvider.Instance.GetEncoding("windows-1250"); // pass encoding as param

        public InvoicesXmlManager(IDialogService dialogService, IConfigProvider configProvider)
        {
            _dialogService = dialogService;
            _configProvider = configProvider;
        }

        public InvoiceXml.dataPackDataPackItem PrepareDatapackItem()
        {
            InvoiceXml.dataPack dataPack;
            using (var streamReader = new StreamReader(Path.Combine(_configProvider.InvoiceConverterConfigsDir, "InvoiceBasic"), _xmlEncoding))
            {
                Debug.WriteLine("Following FileNotFoundException - is normal XmlSerializer behavior");
                dataPack = (InvoiceXml.dataPack)new XmlSerializer(typeof(InvoiceXml.dataPack)).Deserialize(streamReader);
            }
            return dataPack.dataPackItem[0];
        }

        private InvoiceXml.dataPackDataPackItem FillDataPackItem(Invoice invoice, int index)
        {
            var packDataPackItem = PrepareDatapackItem();

            packDataPackItem.id = $"Usr01 ({index.ToString().PadLeft(3, '0')})";

            packDataPackItem.invoice.invoiceHeader.accounting.ids = "3Fv";
            packDataPackItem.invoice.invoiceHeader.invoiceType = "issuedInvoice";

            packDataPackItem.invoice.invoiceHeader.number.numberRequested = invoice.Number;
            packDataPackItem.invoice.invoiceHeader.symVar = invoice.VariableSymbolShort;
            packDataPackItem.invoice.invoiceHeader.symPar = invoice.VariableSymbolFull;
            packDataPackItem.invoice.invoiceHeader.date = invoice.ConversionDate;
            packDataPackItem.invoice.invoiceHeader.dateTax = invoice.DateTax;
            packDataPackItem.invoice.invoiceHeader.dateAccounting = invoice.DateAccounting;
            packDataPackItem.invoice.invoiceHeader.dateDue = invoice.DateDue;
            packDataPackItem.invoice.invoiceHeader.classificationVAT.ids = invoice.Classification.GetDescriptionFromEnum();
            packDataPackItem.invoice.invoiceHeader.classificationVAT.classificationVATType = invoice.VatType;
            packDataPackItem.invoice.invoiceHeader.text = "This is your invoice:"; // TODO Remove? use feature flag
            packDataPackItem.invoice.invoiceHeader.paymentType.ids = invoice.SalesChannel;
            packDataPackItem.invoice.invoiceHeader.centre.ids = invoice.RelatedWarehouseName;
            packDataPackItem.invoice.invoiceHeader.histRate = true; // always true

            packDataPackItem.invoice.invoiceHeader.partnerIdentity.address = FillPartnerInfo(invoice.ClientInfo);
            SetCustomsDeclarationIntoMobilePhone(invoice, packDataPackItem); 

            packDataPackItem.invoice.invoiceHeader.liquidation.amountHome = invoice.TotalPrice.AmountHome.DefRound(); ;
            packDataPackItem.invoice.invoiceHeader.liquidation.amountForeign = invoice.TotalPrice.AmountForeign.DefRound();

            packDataPackItem.invoice.invoiceSummary.foreignCurrency.amount = 1; // always rate of 1 currency unit
            packDataPackItem.invoice.invoiceSummary.foreignCurrency.currency.ids = invoice.TotalPrice.ForeignCurrencyName;
            packDataPackItem.invoice.invoiceSummary.foreignCurrency.rate = invoice.TotalPrice.Rate.DefRound();
            packDataPackItem.invoice.invoiceSummary.foreignCurrency.priceSum = invoice.TotalPrice.AmountForeign.DefRound();

            packDataPackItem.invoice.invoiceSummary.homeCurrency.priceNone = invoice.TotalPrice.AmountHome.DefRound();
            packDataPackItem.invoice.invoiceSummary.homeCurrency.priceHighSum = invoice.TotalPrice.AmountHome.DefRound();

            if (invoice.CountryVat.Percentage != 0 ) // bad but not like michael jackson
            {
                packDataPackItem.invoice.invoiceSummary.homeCurrency.priceHigh = (invoice.TotalPrice.AmountHome - invoice.TotalPriceVat.AmountHome).DefRound();
                packDataPackItem.invoice.invoiceSummary.homeCurrency.priceHighVAT = invoice.TotalPriceVat.AmountHome.DefRound();
            }

            if (invoice.IsMoss) 
            {
                packDataPackItem.invoice.invoiceHeader.MOSS = new InvoiceXml.invoiceInvoiceHeaderMOSS() { ids = invoice.MossCountryCode };
                packDataPackItem.invoice.invoiceHeader.evidentiaryResourcesMOSS = new InvoiceXml.invoiceInvoiceHeaderEvidentiaryResourcesMOSS() { ids = "A" };
            }

            var invoiceInvoiceItemList = new List<InvoiceXml.invoiceInvoiceItem>();
            invoiceInvoiceItemList.AddRange(invoice.InvoiceItems.Select(CreateInvoiceItem));
            packDataPackItem.invoice.invoiceDetail = invoiceInvoiceItemList.ToArray();

            return packDataPackItem;
        }

        private InvoiceXml.invoiceInvoiceItem CreateInvoiceItem(InvoiceItemBase invoiceItem)
        {
            var data = new InvoiceXml.invoiceInvoiceItem();

            int MaxItemNameLength = 85;
            //this name is not actually used in pohoda, so it can be automatically truncated
            if (invoiceItem.Name.Length > MaxItemNameLength) // TODO do this operation during BLO->DTO convertion
            {
                invoiceItem.Name = invoiceItem.Name.Substring(0, MaxItemNameLength);
            }

            data.text = invoiceItem.Name;
            data.quantity = invoiceItem.Quantity;

            data.discountPercentage = 0;
            data.foreignCurrency = new InvoiceXml.invoiceInvoiceItemForeignCurrency();
            data.homeCurrency = new InvoiceXml.invoiceInvoiceItemHomeCurrency();
            data.PDP = false;

            data.rateVAT = "historyHigh";
            data.foreignCurrency.unitPrice = invoiceItem.UnitPrice.AmountForeign.DefRound();
            data.foreignCurrency.price = invoiceItem.TotalPrice.AmountForeign.DefRound();
            data.foreignCurrency.priceSum = invoiceItem.TotalPrice.AmountForeign.DefRound();
            data.foreignCurrency.priceVAT = invoiceItem.VatPrice.AmountForeign.DefRound();

            data.homeCurrency.unitPrice = invoiceItem.UnitPrice.AmountHome.DefRound();
            data.homeCurrency.price = invoiceItem.TotalPrice.AmountHome.DefRound();
            data.homeCurrency.priceSum = invoiceItem.TotalPrice.AmountHome.DefRound();
            data.homeCurrency.priceVAT = invoiceItem.VatPrice.AmountHome.DefRound();
            data.payVAT = invoiceItem.ParentInvoice.PayVat;

            if (invoiceItem.IsMoss) 
            {
                data.typeServiceMOSS = new InvoiceXml.typeServiceMOSS { ids = "GD" };
                data.percentVAT = invoiceItem.PercentVat;
            }

            if (invoiceItem is not InvoiceProduct product) return data;

            data.stockItem = new InvoiceXml.invoiceInvoiceItemStockItem();
            data.stockItem.stockItem = new InvoiceXml.stockItem();
            data.stockItem.store = new InvoiceXml.store();
            data.code = product.WarehouseCode;
            data.amazonSkuCode = product.AmazonSku;
            data.stockItem.stockItem.ids = product.WarehouseCode;
            data.stockItem.store.ids = "Zboží";

            return data;
        }

        private static void SetCustomsDeclarationIntoMobilePhone(Invoice invoice, InvoiceXml.dataPackDataPackItem packDataPackItem)
        {
            packDataPackItem.invoice.invoiceHeader.partnerIdentity.address.mobilPhone = invoice.CustomsDeclaration;
        }

        private static InvoiceXml.address FillPartnerInfo(ClientInfo clientInfo)
        {
            var data = new InvoiceXml.address
            {
                name = clientInfo.Name,
                city = clientInfo.Address.City, 
                street = clientInfo.Address.Street,
                country = new InvoiceXml.addressCountry{ ids = clientInfo.Address.Country },
                zip = clientInfo.Address.Zip,
                phone = clientInfo.Contact.Phone,
                email = clientInfo.Contact.Email
            };
            return data;
        }

        public void SerializeXmlInvoice(string fileName, IEnumerable<Invoice> invoices)
        {
            var invoice = PrepareInvoice(invoices);

            var settings = new XmlWriterSettings();
            settings.Encoding = _xmlEncoding;
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

        public InvoiceXml.dataPack PrepareInvoice(IEnumerable<Invoice> invoices)
        {
            return new()
            {
                ico = 5448034,
                id = "Usr01",
                key = "efd0db6a-9c08-4bb5-befb-36809eadcba1",
                programVersion = "12101.4 (7.1.2019)",
                application = "Transformace",
                note = "Uživatelský export",
                dataPackItem = invoices.Select(FillDataPackItem).ToArray(),
                version = new decimal(20, 0, 0, false, 1)
            };
        }

        private void FixNamespaces(string resultFilePath)
        {
            File.WriteAllText(resultFilePath,
                    File.ReadAllText(resultFilePath, _xmlEncoding)
                    .Replace("5448034", "05448034")
                    .Replace("<inv:invoiceItem xmlns:typ=\"http://www.stormware.cz/schema/version_2/type.xsd\">", "<inv:invoiceItem>")
                    .Replace("<inv:invoiceDetail>", "<inv:invoiceDetail xmlns:rsp=\"http://www.stormware.cz/schema/version_2/response.xsd\" xmlns:rdc=\"http://www.stormware.cz/schema/version_2/documentresponse.xsd\" xmlns:typ=\"http://www.stormware.cz/schema/version_2/type.xsd\" xmlns:lst=\"http://www.stormware.cz/schema/version_2/list.xsd\" xmlns:lStk=\"http://www.stormware.cz/schema/version_2/list_stock.xsd\" xmlns:lAdb=\"http://www.stormware.cz/schema/version_2/list_addBook.xsd\" xmlns:lCen=\"http://www.stormware.cz/schema/version_2/list_centre.xsd\" xmlns:lAcv=\"http://www.stormware.cz/schema/version_2/list_activity.xsd\" xmlns:acu=\"http://www.stormware.cz/schema/version_2/accountingunit.xsd\" xmlns:vch=\"http://www.stormware.cz/schema/version_2/voucher.xsd\" xmlns:int=\"http://www.stormware.cz/schema/version_2/intDoc.xsd\" xmlns:stk=\"http://www.stormware.cz/schema/version_2/stock.xsd\" xmlns:ord=\"http://www.stormware.cz/schema/version_2/order.xsd\" xmlns:ofr=\"http://www.stormware.cz/schema/version_2/offer.xsd\" xmlns:enq=\"http://www.stormware.cz/schema/version_2/enquiry.xsd\" xmlns:vyd=\"http://www.stormware.cz/schema/version_2/vydejka.xsd\" xmlns:pri=\"http://www.stormware.cz/schema/version_2/prijemka.xsd\" xmlns:bal=\"http://www.stormware.cz/schema/version_2/balance.xsd\" xmlns:pre=\"http://www.stormware.cz/schema/version_2/prevodka.xsd\" xmlns:vyr=\"http://www.stormware.cz/schema/version_2/vyroba.xsd\" xmlns:pro=\"http://www.stormware.cz/schema/version_2/prodejka.xsd\" xmlns:con=\"http://www.stormware.cz/schema/version_2/contract.xsd\" xmlns:adb=\"http://www.stormware.cz/schema/version_2/addressbook.xsd\" xmlns:prm=\"http://www.stormware.cz/schema/version_2/parameter.xsd\" xmlns:lCon=\"http://www.stormware.cz/schema/version_2/list_contract.xsd\" xmlns:ctg=\"http://www.stormware.cz/schema/version_2/category.xsd\" xmlns:ipm=\"http://www.stormware.cz/schema/version_2/intParam.xsd\" xmlns:str=\"http://www.stormware.cz/schema/version_2/storage.xsd\" xmlns:idp=\"http://www.stormware.cz/schema/version_2/individualPrice.xsd\" xmlns:sup=\"http://www.stormware.cz/schema/version_2/supplier.xsd\" xmlns:prn=\"http://www.stormware.cz/schema/version_2/print.xsd\" xmlns:sEET=\"http://www.stormware.cz/schema/version_2/sendEET.xsd\" xmlns:act=\"http://www.stormware.cz/schema/version_2/accountancy.xsd\" xmlns:bnk=\"http://www.stormware.cz/schema/version_2/bank.xsd\" xmlns:sto=\"http://www.stormware.cz/schema/version_2/store.xsd\" xmlns:grs=\"http://www.stormware.cz/schema/version_2/groupStocks.xsd\" xmlns:acp=\"http://www.stormware.cz/schema/version_2/actionPrice.xsd\" xmlns:csh=\"http://www.stormware.cz/schema/version_2/cashRegister.xsd\" xmlns:bka=\"http://www.stormware.cz/schema/version_2/bankAccount.xsd\" xmlns:ilt=\"http://www.stormware.cz/schema/version_2/inventoryLists.xsd\" xmlns:nms=\"http://www.stormware.cz/schema/version_2/numericalSeries.xsd\" xmlns:pay=\"http://www.stormware.cz/schema/version_2/payment.xsd\" xmlns:mKasa=\"http://www.stormware.cz/schema/version_2/mKasa.xsd\" xmlns:gdp=\"http://www.stormware.cz/schema/version_2/GDPR.xsd\" xmlns:est=\"http://www.stormware.cz/schema/version_2/establishment.xsd\" xmlns:cen=\"http://www.stormware.cz/schema/version_2/centre.xsd\" xmlns:acv=\"http://www.stormware.cz/schema/version_2/activity.xsd\" xmlns:ftr=\"http://www.stormware.cz/schema/version_2/filter.xsd\">"), _xmlEncoding);
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
                    _dialogService.ShowMessage($"Objednavka {orderLine[0]} na radku {orderLineIndex} " +
                                              $"ze souboru \'{Path.GetFileName(fileName)}\' obsahuje neplatny zaznam (prazdny). " +
                                              "Zaznam bude odstranen.");
                    continue;
                }

                validLines.Add(orderLine);
            }

            return validLines;
        }
    }

    public interface IInvoicesXmlManager
    {
        IEnumerable<Dictionary<string, string>> LoadAmazonReports(IEnumerable<string> fileNames);

        void SerializeXmlInvoice(string fileName, IEnumerable<Invoice> invoice);

        InvoiceXml.dataPackDataPackItem PrepareDatapackItem();

        InvoiceXml.dataPack PrepareInvoice(IEnumerable<Invoice> dataItems);
    }
}