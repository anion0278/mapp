using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using Shmap.CommonServices;
using Shmap.DataAccess;

namespace Shmap.ViewModels
{
    public class InvoiceViewModel : ViewModelBase
    {
        private readonly Invoice _model;
        private readonly IAutocompleteData _autocompleteData;
        private string _amazonNumber;
        private string _salesChannel;
        private IEnumerable<string> _invoiceProductNames;
        private IEnumerable<string> _amazonSkuCodes;
        private string _customsDeclaration;
        private string _relatedWarehouseSection;
        private InvoiceVatClassification _vatType;

        public string AmazonNumber
        {
            get => _amazonNumber;
            set => Set(ref _amazonNumber, value);
        }

        public string SalesChannel
        {
            get => _salesChannel;
            set => Set(ref _salesChannel, value);
        }

        public IEnumerable<string> InvoiceProductNames // TODO AmazonSkuCodes and InvoiceProductNames should be joined and shown in table together
        {
            get => _invoiceProductNames;
            set => Set(ref _invoiceProductNames, value);
        }

        public IEnumerable<string> AmazonSkuCodes
        {
            get => _amazonSkuCodes;
            set => Set(ref _amazonSkuCodes, value);
        }


        public string CustomsDeclaration
        {
            get => _customsDeclaration;
            set
            {
                Set(ref _customsDeclaration, value);
                if (value == ApplicationConstants.EmptyItemCode || string.IsNullOrWhiteSpace(value)) return;

                var rememberedDictionary = _autocompleteData.CustomsDeclarationBySku;
                string productSku = AmazonSkuCodes.FirstOrDefault();
                _autocompleteData.UpdateAutocompleteData(value, rememberedDictionary, productSku);
            }
        }

        public InvoiceVatClassification VatType
        {
            get => _vatType;
            set => Set(ref _vatType, value);
        }

        public string RelatedWarehouseSection
        {
            get => _relatedWarehouseSection;
            set
            {
                Set(ref _relatedWarehouseSection, value);
                if (value == ApplicationConstants.EmptyItemCode || string.IsNullOrWhiteSpace(value)) return;

                var rememberedDictionary = _autocompleteData.ProdWarehouseSectionBySku;
                string productSku = AmazonSkuCodes.FirstOrDefault();
                _autocompleteData.UpdateAutocompleteData(value, rememberedDictionary, productSku);
            }
        }


        public InvoiceViewModel(Invoice model, IAutocompleteData autocompleteData)
        {
            _amazonNumber = model.VariableSymbolFull;
            _salesChannel = model.SalesChannel;
            _relatedWarehouseSection = model.RelatedWarehouseName;
            _amazonSkuCodes = model.InvoiceItems.OfType<InvoiceProduct>().Select(p => p.AmazonSku);
            _invoiceProductNames = model.InvoiceItems.OfType<InvoiceProduct>().Select(p => p.Name);
            _customsDeclaration = model.CustomsDeclaration;
            _vatType = model.Classification;
            _autocompleteData = autocompleteData;
            
            // for now saving model
            _model = model;
        }

        public Invoice ExportModel()
        {
            _model.RelatedWarehouseName = _relatedWarehouseSection;
            _model.CustomsDeclaration = _customsDeclaration;

            return _model;
        }
    }

}