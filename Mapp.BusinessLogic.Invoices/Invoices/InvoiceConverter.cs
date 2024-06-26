﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mapp.BusinessLogic.Currency;
using Mapp.Common;
using Mapp.DataAccess;

namespace Mapp.BusinessLogic.Invoices
{
    public interface IInvoiceConverter
    {
        IAutocompleteData AutocompleteData { get; }
        IEnumerable<Invoice> LoadAmazonReports(IEnumerable<string> reportsFileNames, InvoiceConversionContext conversionContext);

        void ProcessInvoices(IEnumerable<Invoice> invoices, string fileName);
    }

    public class InvoiceConverter : IInvoiceConverter
    {
        public IAutocompleteData AutocompleteData { get; }
        private readonly ICurrencyConverter _currencyConverter;
        private readonly IInvoicesXmlManager _invoicesXmlManager;
        private readonly IAutocompleteDataLoader _autocompleteDataLoader;
        private readonly IDialogService _dialogService;
        private readonly int MaxAddressNameLength = 60;
        private readonly int MaxCityLength = 45;
        private readonly int MaxClientNameLength = 30;
        private Dictionary<string, decimal> _rates;
        private readonly Dictionary<string, decimal> _vatPercentage;

        public InvoiceConverter(
            ICurrencyConverter currencyConverter,
            ICsvLoader csvLoader,
            IInvoicesXmlManager invoicesXmlManager,
            IAutocompleteDataLoader autocompleteDataLoader,
            IDialogService dialogService)
        {
            _currencyConverter = currencyConverter;
            _invoicesXmlManager = invoicesXmlManager;
            _autocompleteDataLoader = autocompleteDataLoader;
            _dialogService = dialogService;

            _rates = csvLoader.LoadFixedCurrencyRates(); // TODO make it possible to choose from settings
            _vatPercentage = csvLoader.LoadCountryVatRates(); // TODO make it possible to choose from settings
            
            AutocompleteData = _autocompleteDataLoader.LoadSettings();
        }

        public IEnumerable<Invoice> LoadAmazonReports(IEnumerable<string> reportsFileNames, InvoiceConversionContext conversionContext)
        {
            var dataFromAmazonReports = _invoicesXmlManager.LoadAmazonReports(reportsFileNames);
            if (dataFromAmazonReports == null) return Array.Empty<Invoice>();

            return MergeInvoicesOfSameOrders(dataFromAmazonReports, conversionContext);
        }

        private IEnumerable<Invoice> MergeInvoicesOfSameOrders(IEnumerable<Dictionary<string, string>> dataFromAmazonReports, InvoiceConversionContext conversionContext)
        {
            var mergedInvoices = new List<Invoice>();
            foreach (var report in dataFromAmazonReports)
            {
                var singleAmazonInvoice = ProcessInvoiceLine(report, (uint)(mergedInvoices.Count + 1), conversionContext);
                var existingDataPack = mergedInvoices.FirstOrDefault(i => i.VariableSymbolFull == singleAmazonInvoice.VariableSymbolFull);
                if (existingDataPack != null)
                {
                    existingDataPack.MergeInvoice(singleAmazonInvoice);
                }
                else
                {
                    mergedInvoices.Add(singleAmazonInvoice);
                }
            }

            return mergedInvoices;
        }

        private Invoice ProcessInvoiceLine(IReadOnlyDictionary<string, string> valuesFromAmazon, uint index, InvoiceConversionContext context)
        {
            var invoice = new Invoice(_vatPercentage);

            string shipCountry = valuesFromAmazon["ship-country"];

            // TODO handle optional fields
            decimal promotionDiscount = 0;
            if (valuesFromAmazon.TryGetValue("item-promotion-discount", out string itemDiscount))
            {
                promotionDiscount = decimal.Parse(itemDiscount);
            }

            if (valuesFromAmazon.TryGetValue("ship-promotion-discount", out string shippingDiscount))
            {
                decimal shipPromotionDiscount = decimal.Parse(shippingDiscount);
                promotionDiscount += shipPromotionDiscount; // shipping discount se scita do total discount
            }

            string sku = valuesFromAmazon["sku"];

            DateTime today = context.ConvertToDate;

            invoice.CurrencyName = _currencyConverter.Convert(valuesFromAmazon["currency"]);
            invoice.Number = context.ExistingInvoiceNumber + index;
            invoice.VariableSymbolFull = valuesFromAmazon["order-id"];
            invoice.ShipCountryCode = shipCountry;
            invoice.ConversionDate = today;

            string clientName = FormatClientName(valuesFromAmazon["recipient-name"], invoice.VariableSymbolFull);
            string city = FormatCity(valuesFromAmazon["ship-city"], valuesFromAmazon["ship-state"], string.Empty, invoice.VariableSymbolFull);
            string fullAddress = FormatFullAddress(valuesFromAmazon["ship-address-1"], valuesFromAmazon["ship-address-2"], valuesFromAmazon["ship-address-3"], invoice.VariableSymbolFull);
            string phoneNumber = FormatPhoneNumber(valuesFromAmazon["ship-phone-number"], valuesFromAmazon["buyer-phone-number"], invoice.VariableSymbolFull);

            invoice.ClientInfo = new ClientInfo
            {
                Name = clientName,
                Address = new Address
                {
                    City = city,
                    Street = fullAddress,
                    Country = shipCountry,
                    Zip = valuesFromAmazon["ship-postal-code"]
                },
                Contact = new ContactData
                {
                    Email = context.DefaultEmail,
                    Phone = phoneNumber
                }
            };

            invoice.CustomsDeclaration = GetCustomsDeclarationBySkuOnlyForNonEu(sku, invoice.Classification);
            invoice.RelatedWarehouseName = GetSavedWarehouseBySku(sku);

            if (valuesFromAmazon.TryGetValue("sales-channel", out string salesChannelValue))
            {
                invoice.SalesChannel = salesChannelValue;
            }

            var invoiceItems = new List<InvoiceItemBase>();
            var invoiceProduct = new InvoiceProduct(invoice)
            {
                WarehouseCode = GetSavedItemCodeBySku(sku),
                AmazonSku = sku
            };


            var invoiceItemProduct = FillInvoiceItem(
                invoiceProduct,
                valuesFromAmazon["product-name"],
                decimal.Parse(valuesFromAmazon["item-price"]),
                    decimal.Parse(valuesFromAmazon["item-tax"]),
                decimal.Parse(valuesFromAmazon["quantity-purchased"]));

            invoiceProduct.PackQuantityMultiplier = 1;
            if (!string.IsNullOrEmpty(invoiceProduct.AmazonSku) &&
                AutocompleteData.PackQuantitySku.ContainsKey(invoiceProduct.AmazonSku))
            {
                invoiceProduct.PackQuantityMultiplier = uint.Parse(AutocompleteData.PackQuantitySku[invoiceProduct.AmazonSku]);
            }
            invoiceItems.Add(invoiceItemProduct);


            var invoiceItemShipping = FillInvoiceItem(
                new InvoiceItemGeneral(invoice, InvoiceItemType.Shipping),
                GetSavedShippingType(sku, invoice.ClientInfo, invoice.Classification),
                decimal.Parse(valuesFromAmazon["shipping-price"]),
                decimal.Parse(valuesFromAmazon["shipping-tax"]),
                1);
            invoiceItems.Add(invoiceItemShipping);


            if (promotionDiscount != 0)
            {
                var invoiceItemDiscount = FillInvoiceItem(
                    new InvoiceItemGeneral(invoice, InvoiceItemType.Discount),
                    "Discount",
                    promotionDiscount,
                    0,
                    1);
                invoiceItems.Add(invoiceItemDiscount);
            }

            // TODO fix
            if (valuesFromAmazon.TryGetValue("gift-wrap-price", out var giftWrapPrice) // columns are not always available in the amazon invoice
                && valuesFromAmazon.TryGetValue("gift-wrap-tax", out var giftWrapTax)
                && decimal.TryParse(giftWrapPrice, out var giftWrapPriceValue)
                && giftWrapPriceValue != 0)
            {
                decimal giftWrapTaxVal = 0;

                if (decimal.TryParse(giftWrapTax, out var valueTax))
                {
                    giftWrapTaxVal = valueTax;
                }

                string giftWrapType = "Gift wrap " + valuesFromAmazon["gift-wrap-type"];
                var invoiceItemGiftWrap = FillInvoiceItem(
                    new InvoiceItemGeneral(invoice, InvoiceItemType.GiftWrap),
                    giftWrapType,
                    giftWrapPriceValue,
                    giftWrapTaxVal,
                    1);
                invoiceItems.Add(invoiceItemGiftWrap);
            }

            foreach (var item in invoiceItems)
            {
                invoice.AddInvoiceItem(item);
            }

            return invoice;
        }


        private InvoiceItemBase FillInvoiceItem(InvoiceItemBase invoiceItem, string name, decimal priceIncludingTax, decimal tax, decimal quantity)
        {
            invoiceItem.Name = name;
            invoiceItem.Quantity = quantity;
            invoiceItem.TotalPriceWithTax = new Common.Currency(
                priceIncludingTax, // There is no need to manually calculate tax (ReverseTaxValue) and subtract it since its value is available in amazon report
                                   // and recalculation logic is included into InvoiceBase Item
                invoiceItem.ParentInvoice.TotalPrice.ForeignCurrencyName,
                _rates);

            invoiceItem.Tax = new Common.Currency(
                tax,
                invoiceItem.ParentInvoice.TotalPrice.ForeignCurrencyName,
                _rates);

            invoiceItem.PercentVat = invoiceItem.ParentInvoice.CountryVat.Percentage * (decimal)100.0;

            return invoiceItem;
        }

        public void ProcessInvoices(IEnumerable<Invoice> invoices, string fileName)
        {
            _autocompleteDataLoader.SaveSettings(AutocompleteData);
            _invoicesXmlManager.SerializeXmlInvoice(fileName, invoices);
        }

        private bool IsNonEuCountryByClassification(InvoiceVatClassification classification)
        {
            return classification == InvoiceVatClassification.UVzbozi;
        }

        private string GetCustomsDeclarationBySkuOnlyForNonEu(string sku, InvoiceVatClassification classification)
        {
            if (IsNonEuCountryByClassification(classification))
            {
                return GetAutocompleteOrEmpty(
                    AutocompleteData.CustomsDeclarationBySku,
                    sku,
                    string.Empty);
            }
            return string.Empty;
        }

        private string GetSavedShippingType(string sku, ClientInfo clientInfo, InvoiceVatClassification classification)
        {
            string defaultShippingName = "Shipping";

            if (AutocompleteData.DefaultShippingByPartnerCountry.TryGetValue(clientInfo.Address.Country, out string countryDefaultShipping))
            {
                defaultShippingName = countryDefaultShipping;
            }

            if (classification != InvoiceVatClassification.UDA5 && classification != InvoiceVatClassification.RDzasEU)
            {
                return defaultShippingName;
            }

            // only for non-EU
            return GetAutocompleteOrEmpty( // TODO ПРОБЛЕМА в том что при агригации шипиногов их названия будут стираться
                AutocompleteData.ShippingNameBySku,
                sku,
                defaultShippingName);
        }

        private string GetSavedItemCodeBySku(string sku)
        {
            return GetAutocompleteOrEmpty(
                AutocompleteData.PohodaProdCodeBySku,
                sku);
        }

        private string GetSavedWarehouseBySku(string sku)
        {
            return GetAutocompleteOrEmpty(
                AutocompleteData.ProdWarehouseSectionBySku,
                sku);
        }

        private string GetAutocompleteOrEmpty(IReadOnlyDictionary<string, string> autocompleteData, string searchKey, string defaultValue = "")
        {
            return autocompleteData.TryGetValue(searchKey, out string value) ? value : defaultValue;
        }

        private string FormatCity(
            string shipCity,
            string shipState,
            string postalCode,
            string amazonOrderNumber)
        {
            string str = shipCity + ", " + shipState + " " + postalCode;
            string message = $"Nazev mesta/zeme je prilis dlouhy v objednavce C.: {amazonOrderNumber}.";
            str = _dialogService.AskToChangeLongStringIfNeeded(message, str, MaxCityLength);

            return str;
        }

        private string FormatPhoneNumber(string shippingPhone, string buyerPhone, string order)
        {
            string str = shippingPhone;
            if (string.IsNullOrEmpty(str))
                str = buyerPhone;
            return str;
        }

        private string FormatClientName(string name, string amazonOrderNumber)
        {
            string message = $"Jmeno zakazniku je prilis dlouhe v objednavce C.: {amazonOrderNumber}.";

            name = _dialogService.AskToChangeLongStringIfNeeded(message, name, MaxClientNameLength);

            return name;
        }

        private string FormatFullAddress(
            string address1,
            string address2,
            string address3,
            string amazonOrderNumber)
        {
            string str = address1;
            if (!string.IsNullOrEmpty(address2))
                str = str + ", " + address2;
            if (!string.IsNullOrEmpty(address3))
                str = str + ", " + address3;

            string message = $"Addressa je prilis dlouha v objednavce C.: {amazonOrderNumber}.";
            str = _dialogService.AskToChangeLongStringIfNeeded(message, str, MaxAddressNameLength);
            return str;
        }
    }
}
