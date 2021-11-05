using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Shmap.CommonServices;
using Shmap.DataAccess;
using NLog.LayoutRenderers;

namespace Shmap.ViewModels
{
    public class InvoiceItemViewModel : ViewModelWithErrorValidationBase
    {
        private readonly InvoiceItemBase _model;
        private readonly IAutocompleteData _autocompleteData;
        private string _amazonProductName;
        private string _warehouseProductCode;
        private uint? _packQuantityMultiplier;

        public RelayCommand GoToInvoicePageCommand { get; }

        public string AmazonNumber { get; }
        public string SalesChannel { get; }
        public string AmazonSku { get; }
        public bool IsFullyEditable => _model.Type == InvoiceItemType.Product;

        public string AmazonProductName
        {
            get => _amazonProductName;
            set
            {
                Set(ref _amazonProductName, value);

                if (string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(AmazonSku)) return;
                var rememberedDictionary = _autocompleteData.ShippingNameBySku;
                _autocompleteData.UpdateAutocompleteData(value, rememberedDictionary, AmazonSku);
            }
        }

        public string WarehouseProductCode
        {
            get => _warehouseProductCode;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(AmazonSku)) return;
                Set(ref _warehouseProductCode, value);

                var rememberedDictionary = _autocompleteData.PohodaProdCodeBySku;
                _autocompleteData.UpdateAutocompleteData(value, rememberedDictionary, AmazonSku);
            }
        }

        public uint? PackQuantityMultiplier
        {
            get => _packQuantityMultiplier;
            set
            {
                if (string.IsNullOrEmpty(AmazonSku)) return;
                Set(ref _packQuantityMultiplier, value);
                var rememberedDictionary = _autocompleteData.PackQuantitySku;
                _autocompleteData.UpdateAutocompleteData(value.ToString(), rememberedDictionary, AmazonSku); // TODO make autocomplete data correctly typed
            }
        }

        public InvoiceItemViewModel(InvoiceItemBase model, IAutocompleteData autocompleteData)
        {
            AmazonNumber = model.ParentInvoice.VariableSymbolFull;
            SalesChannel = model.ParentInvoice.SalesChannel;
            _amazonProductName = model.Name;

            GoToInvoicePageCommand = new RelayCommand(GoToInvoicePage);

            AddValidationRule(() => PackQuantityMultiplier,
                () => PackQuantityMultiplier > 0,
                "Pocet musi byt vetsi nez nula");

            AddValidationRule(() => WarehouseProductCode,
                ValidateProductCode,
                "Neni zadan kod produktu");

            _packQuantityMultiplier = 1;
            if (model is InvoiceProduct product)
            {
                AmazonSku = product.AmazonSku;
                _warehouseProductCode = product.WarehouseCode;
                _packQuantityMultiplier = product.PackQuantityMultiplier;
            }
            _autocompleteData = autocompleteData;

            // temp
            _model = model;
        }

        private void GoToInvoicePage() // TODO into separate provider + tests
        {
            string salesChannel = _model.ParentInvoice.SalesChannel;

            var amazonCentralExceptions = new Dictionary<IEnumerable<string>, string>()
            {
                {new []{"amazon.com", "amazon.com.mx", "amazon.ca"}, "https://sellercentral.amazon.com/orders-v3/order/"}
            };
            string defaultAmazonCentralUrl = "https://sellercentral.amazon.co.uk/orders-v3/order/";

            string url = amazonCentralExceptions.SingleOrDefault(pair => pair.Key.Contains(salesChannel)).Value
                         ?? defaultAmazonCentralUrl;

            url += AmazonNumber;
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private bool ValidateProductCode()
        {
            return _model.Type != InvoiceItemType.Product || !string.IsNullOrWhiteSpace(WarehouseProductCode);
        }

        public InvoiceItemBase ExportModel()
        {
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