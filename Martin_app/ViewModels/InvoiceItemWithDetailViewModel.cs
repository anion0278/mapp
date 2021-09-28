using GalaSoft.MvvmLight;
using Shmap.CommonServices;
using Shmap.DataAccess;

namespace Shmap.ViewModels
{
    public class InvoiceItemWithDetailViewModel : ViewModelBase
    {
        private readonly InvoiceItemBase _model;
        private readonly IAutocompleteData _autocompleteData;
        private string _amazonNumber;
        private string _salesChannel;
        private string _amazonProductName;
        private string _amazonSku;
        private string _warehouseProductCode;
        private uint _packQuantityMultiplier;

        public string AmazonNumber
        {
            get => _amazonNumber;
            //set => Set(ref _amazonNumber, value);
        }

        public string SalesChannel
        {
            get => _salesChannel;
            //set => Set(ref _salesChannel, value);
        }

        public string AmazonProductName
        {
            get => _amazonProductName;
            set
            {
                Set(ref _amazonProductName, value);
                if (value == ApplicationConstants.EmptyItemCode || string.IsNullOrWhiteSpace(value) || _model.Type != InvoiceItemType.Shipping) return;

                var rememberedDictionary = _autocompleteData.ShippingNameBySku;
                _autocompleteData.UpdateAutocompleteData(value, rememberedDictionary, AmazonSku);
            }
        }

        public string AmazonSku
        {
            get => _amazonSku;
            //set => Set(ref _amazonSku, value);
        }

        public string WarehouseProductCode
        {
            get => _warehouseProductCode;
            set
            {
                Set(ref _warehouseProductCode, value);
                if (value == ApplicationConstants.EmptyItemCode || string.IsNullOrWhiteSpace(value)) return;

                var rememberedDictionary = _autocompleteData.PohodaProdCodeBySku;
                _autocompleteData.UpdateAutocompleteData(value, rememberedDictionary, AmazonSku);
            }
        }

        public uint PackQuantityMultiplier
        {
            get => _packQuantityMultiplier;
            set
            {
                Set(ref _packQuantityMultiplier, value);
                var rememberedDictionary = _autocompleteData.PackQuantitySku;
                _autocompleteData.UpdateAutocompleteData(value.ToString(), rememberedDictionary, AmazonSku); // TODO make autocomplete data correctly typed
            }
        }

        public InvoiceItemWithDetailViewModel(InvoiceItemBase model, IAutocompleteData autocompleteData)
        {
            _amazonNumber = model.ParentInvoice.VariableSymbolFull;
            _salesChannel = model.ParentInvoice.SalesChannel;
            _amazonProductName = model.Name;

            _packQuantityMultiplier = 1; // Should be locked for edition for non-product items
            if (model is InvoiceProduct product)
            {
                _amazonSku = product.AmazonSku;
                _warehouseProductCode = product.WarehouseCode;
                _packQuantityMultiplier = product.PackQuantityMultiplier;
            }
            _autocompleteData = autocompleteData;

            // temp
            _model = model;
        }

        public InvoiceItemBase ExportModel()
        {
            _model.Name = _amazonProductName;
            if (_model is InvoiceProduct product)
            {
                product.PackQuantityMultiplier = _packQuantityMultiplier;
                product.WarehouseCode = _warehouseProductCode;
            }
            return _model;
        }
    }
}