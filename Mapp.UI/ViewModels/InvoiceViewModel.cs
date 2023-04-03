using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Shmap.CommonServices;
using Shmap.DataAccess;
using Shmap.UI.ViewModels;

namespace Shmap.ViewModels
{
    public class InvoiceViewModel : ViewModelBase
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

        [MaxLength(24)]
        public string CustomsDeclaration
        {
            get => _customsDeclaration;
            set
            {
                _customsDeclaration = value;
                if (string.IsNullOrWhiteSpace(value)) return;

                if (_model.InvoiceItems.OfType<InvoiceProduct>().Count() == 1) // v tomto pripade pamatujeme celni prohlaseni pro jeden produkt, protoze se nemeni
                {
                    var rememberedDictionary = _autocompleteData.CustomsDeclarationBySku;
                    string productSku = AmazonSkuCodes.FirstOrDefault();
                    _autocompleteData.UpdateAutocompleteData(value, rememberedDictionary, productSku);
                }

                // TODO pridat pamatovani celniho prhlaseni pro kominace produktu
            }
        }

        [Required]
        public string RelatedWarehouseSection
        {
            get => _relatedWarehouseSection;
            set
            {
                _relatedWarehouseSection = value;
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