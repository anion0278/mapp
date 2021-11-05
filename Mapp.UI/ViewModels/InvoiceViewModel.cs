using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight;
using Shmap.CommonServices;
using Shmap.DataAccess;

namespace Shmap.ViewModels
{
    public class InvoiceViewModel : ViewModelWithErrorValidationBase
    {
        private readonly Invoice _model;
        private readonly IAutocompleteData _autocompleteData;
        private string _customsDeclaration;
        private string _relatedWarehouseSection;

        public string AmazonNumber { get; }
        public string SalesChannel { get; }
        public InvoiceVatClassification VatType { get; }
        public IEnumerable<string> InvoiceProductNames { get; } 
        public IEnumerable<string> AmazonSkuCodes { get; }

        public string CustomsDeclaration
        {
            get => _customsDeclaration;
            set
            {
                Set(ref _customsDeclaration, value);
                if (string.IsNullOrWhiteSpace(value)) return;

                var rememberedDictionary = _autocompleteData.CustomsDeclarationBySku;
                string productSku = AmazonSkuCodes.FirstOrDefault();
                _autocompleteData.UpdateAutocompleteData(value, rememberedDictionary, productSku);
            }
        }

        public string RelatedWarehouseSection
        {
            get => _relatedWarehouseSection;
            set
            {
                Set(ref _relatedWarehouseSection, value);
                if (string.IsNullOrWhiteSpace(value)) return;

                var rememberedDictionary = _autocompleteData.ProdWarehouseSectionBySku;
                string productSku = AmazonSkuCodes.FirstOrDefault();
                _autocompleteData.UpdateAutocompleteData(value, rememberedDictionary, productSku);
            }
        }

        public InvoiceViewModel(Invoice model, IAutocompleteData autocompleteData)
        {
            AmazonNumber = model.VariableSymbolFull;
            SalesChannel = model.SalesChannel;
            AmazonSkuCodes = model.InvoiceItems.OfType<InvoiceProduct>().Select(p => p.AmazonSku);
            InvoiceProductNames = model.InvoiceItems.OfType<InvoiceProduct>().Select(p => p.Name);
            VatType= model.Classification;

            _relatedWarehouseSection = model.RelatedWarehouseName;
            _customsDeclaration = model.CustomsDeclaration;
            _autocompleteData = autocompleteData;
            
            AddValidationRule(() => RelatedWarehouseSection, 
                ()=>!string.IsNullOrWhiteSpace(RelatedWarehouseSection), 
                "Neni zadan kod skladu");

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