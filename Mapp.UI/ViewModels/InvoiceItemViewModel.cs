using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Shmap.Common;
using Shmap.Common.Validation;
using Shmap.DataAccess;
using Shmap.Infrastructure;
using Shmap.UI.Localization;

namespace Shmap.UI.ViewModels
{
    public class InvoiceItemViewModel : ViewModelBase
    {
        private readonly InvoiceItemBase _model;
        private readonly IBrowserService _browserService;
        private readonly IAutocompleteData _autocompleteData;
        private string _amazonProductName;
        private string _warehouseProductCode;
        private uint? _packQuantityMultiplier;

        public RelayCommand GoToInvoicePageCommand { get; }

        public string AmazonNumber { get; }
        public string SalesChannel { get; }
        public string AmazonSku { get; }

        public InvoiceItemType ItemType => _model.Type;

        public string AmazonProductName
        {
            get => _amazonProductName;
            set
            {
                _amazonProductName = value;

                if (string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(AmazonSku)) return;

                if (_model.Type == InvoiceItemType.Shipping)
                {
                    var rememberedDictionary = _autocompleteData.ShippingNameBySku;
                    _autocompleteData.UpdateAutocompleteData(value, rememberedDictionary, AmazonSku);
                }
            }
        }

        [CustomValidation(typeof(InvoiceItemViewModel), nameof(ValidateProductCode))]
        public string WarehouseProductCode
        {
            get => _warehouseProductCode;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(AmazonSku)) return;
                _warehouseProductCode = value;

                var rememberedDictionary = _autocompleteData.PohodaProdCodeBySku;
                _autocompleteData.UpdateAutocompleteData(value, rememberedDictionary, AmazonSku);
            }
        }

        [GreaterThan<uint>(0)]
        public uint? PackQuantityMultiplier
        {
            get => _packQuantityMultiplier;
            set
            {
                if (string.IsNullOrEmpty(AmazonSku)) return;
                _packQuantityMultiplier = value;
                var rememberedDictionary = _autocompleteData.PackQuantitySku;
                _autocompleteData.UpdateAutocompleteData(value.ToString(), rememberedDictionary, AmazonSku); // TODO make autocomplete data correctly typed
            }
        }

        public InvoiceItemViewModel(InvoiceItemBase model, IAutocompleteData autocompleteData, IBrowserService browserService)
        {
            AmazonNumber = model.ParentInvoice.VariableSymbolFull;
            SalesChannel = model.ParentInvoice.SalesChannel;
            _amazonProductName = model.Name;

            GoToInvoicePageCommand = new RelayCommand(GoToInvoicePage);

            _packQuantityMultiplier = 1;
            if (model is InvoiceProduct product)
            {
                AmazonSku = product.AmazonSku;
                _warehouseProductCode = product.WarehouseCode;
                _packQuantityMultiplier = product.PackQuantityMultiplier;
            }
            _autocompleteData = autocompleteData;

            // TODO Fixme temp?
            _model = model;
            _browserService = browserService;
        }


        // TODO FIXME validation does not work immediately after loading
        public static ValidationResult ValidateProductCode(string name, ValidationContext context)
        {
            var item = (InvoiceItemViewModel)context.ObjectInstance;
            if (item.ItemType != InvoiceItemType.Product || !string.IsNullOrWhiteSpace(item.WarehouseProductCode))
            {
                return ValidationResult.Success;
            }

            return new(LocalizationStrings.ProductCodeIsNotSetValidationMsg.GetLocalized());
        }

        private void GoToInvoicePage() // TODO into separate provider + tests
        {
            string salesChannel = _model.ParentInvoice.SalesChannel;

            var amazonCentralExceptions = new Dictionary<IEnumerable<string>, string>()
            {
                {new []{"amazon.com", "amazon.com.mx", "amazon.ca"}, "https://sellercentral.amazon.com/orders-v3/order/"}
            };
            string defaultAmazonCentralUrl = "https://sellercentral.amazon.co.uk/orders-v3/order/";

            string url = amazonCentralExceptions.SingleOrDefault(pair => pair.Key.Contains(salesChannel, StringComparer.OrdinalIgnoreCase)).Value
                         ?? defaultAmazonCentralUrl;

            url += AmazonNumber;
            _browserService.OpenBrowserOnUrl(url);
        }

        public InvoiceItemBase ExportModel()
        {
            // TODO remove - this should not be needed, since this assinging should be part of the setters
            _model.Name = _amazonProductName;
            if (_model is InvoiceProduct product)
            {
                product.PackQuantityMultiplier = _packQuantityMultiplier.Value; // valid after validation
                product.WarehouseCode = _warehouseProductCode;
            }
            return _model;
        }
    }
}