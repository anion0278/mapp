using System.Collections.Generic;
using System.IO;
using Shmap.CommonServices;

namespace Shmap.DataAccess
{
    public interface IAutocompleteData
    {
        public Dictionary<string, string> ProdWarehouseSectionBySku { get; set; } // TODO fix names

        public Dictionary<string, string> PohodaProdCodeBySku { get; set; } // TODO decide - SKU or AmazonProdCode

        public Dictionary<string, string> ShippingNameBySku { get; set; }

        public Dictionary<string, string> PackQuantitySku { get; set; }

        public Dictionary<string, string> CustomsDeclarationBySku { get; set; }
    }

    internal class AutocompleteData: IAutocompleteData
    {
        public Dictionary<string, string> ProdWarehouseSectionBySku { get; set; } // TODO fix names

        public Dictionary<string, string> PohodaProdCodeBySku { get; set; } // TODO decide - SKU or AmazonProdCode

        public Dictionary<string, string> ShippingNameBySku { get; set; }

        public Dictionary<string, string> PackQuantitySku { get; set; }

        public Dictionary<string, string> CustomsDeclarationBySku { get; set; }
    }

    public interface IAutocompleteDataLoader
    {
        IAutocompleteData LoadSettings();
        void SaveSettings(IAutocompleteData data);
    }

    public class AutocompleteDataLoader : IAutocompleteDataLoader
    {
        private readonly IJsonManager _jsonManager;
        private readonly IConfigProvider _configProvider;

        private string ShippingNameBySkuJson;
        private string ProdWarehouseSectionBySkuJson;
        private string PohodaProdCodeBySkuJson;
        private string ProductQuantityBySkuJson;
        private string CustomsDeclarationBySkuJson;

        public AutocompleteDataLoader(IJsonManager jsonManager, IConfigProvider configProvider)
        {
            _jsonManager = jsonManager;
            _configProvider = configProvider;
        }

        public IAutocompleteData LoadSettings()
        {
            PohodaProdCodeBySkuJson = Path.Combine(_configProvider.InvoiceConverterConfigsDir, "AutocompletePohodaProdCodeBySku.json");
            ProdWarehouseSectionBySkuJson =
                Path.Combine(_configProvider.InvoiceConverterConfigsDir, "AutocompleteProdWarehouseSectionBySku.json");
            ShippingNameBySkuJson =
                Path.Combine(_configProvider.InvoiceConverterConfigsDir, "AutocompleteShippingTypeBySku.json");
            ProductQuantityBySkuJson =
                Path.Combine(_configProvider.InvoiceConverterConfigsDir, "AutocompleteProdQuantityBySku.json");
            CustomsDeclarationBySkuJson =
                Path.Combine(_configProvider.InvoiceConverterConfigsDir, "AutocompleteCustomsDeclarationBySku.json");

            var autocompleteData = new AutocompleteData // TODO factory
            {
                // TODO intelligible exception if some files are missing
                PohodaProdCodeBySku = _jsonManager.DeserializeJsonDictionary(PohodaProdCodeBySkuJson),
                ProdWarehouseSectionBySku = _jsonManager.DeserializeJsonDictionary(ProdWarehouseSectionBySkuJson),
                ShippingNameBySku = _jsonManager.DeserializeJsonDictionary(ShippingNameBySkuJson),
                PackQuantitySku = _jsonManager.DeserializeJsonDictionary(ProductQuantityBySkuJson),
                CustomsDeclarationBySku = _jsonManager.DeserializeJsonDictionary(CustomsDeclarationBySkuJson)
            };
            // TODO fix names
            return autocompleteData;
        }

        public void SaveSettings(IAutocompleteData data)
        {
            _jsonManager.SerializeDictionaryToJson(data.PohodaProdCodeBySku, PohodaProdCodeBySkuJson);
            _jsonManager.SerializeDictionaryToJson(data.ProdWarehouseSectionBySku, ProdWarehouseSectionBySkuJson);
            _jsonManager.SerializeDictionaryToJson(data.ShippingNameBySku, ShippingNameBySkuJson);
            _jsonManager.SerializeDictionaryToJson(data.PackQuantitySku, ProductQuantityBySkuJson);
            _jsonManager.SerializeDictionaryToJson(data.CustomsDeclarationBySku, CustomsDeclarationBySkuJson);
        }

    }
}