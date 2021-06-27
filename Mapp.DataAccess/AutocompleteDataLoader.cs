using System.Collections.Generic;
using System.IO;

namespace Shmap.DataAccess
{
    public interface IAutocompleteData
    {
        public Dictionary<string, string> ProdWarehouseSectionByAmazonProdCode { get; set; } // TODO fix names

        public Dictionary<string, string> PohodaProdCodeByAmazonProdCode { get; set; } // TODO decide - SKU or AmazonProdCode

        public Dictionary<string, string> ShippingNameByItemName { get; set; }

        public Dictionary<string, string> PackQuantityByItemName { get; set; }

        public Dictionary<string, string> CustomsDeclarationByItemName { get; set; }
    }

    internal class AutocompleteData: IAutocompleteData
    {
        public Dictionary<string, string> ProdWarehouseSectionByAmazonProdCode { get; set; } // TODO fix names

        public Dictionary<string, string> PohodaProdCodeByAmazonProdCode { get; set; } // TODO decide - SKU or AmazonProdCode

        public Dictionary<string, string> ShippingNameByItemName { get; set; }

        public Dictionary<string, string> PackQuantityByItemName { get; set; }

        public Dictionary<string, string> CustomsDeclarationByItemName { get; set; }
    }

    public class AutocompleteDataLoader
    {
        private readonly IJsonManager _jsonManager;
        private string _invoiceConverterConfigsDir;

        private string ShippingNameByItemNameJson;
        private string ProdWarehouseSectionByAmazonProdCodeJson;
        private string PohodaProdCodeByAmazonProdCodeJson;
        private string ProductQuantityByItemNameJson;
        private string CustomsDeclarationByItemNameJson;

        public AutocompleteDataLoader(IJsonManager jsonManager, string invoiceConverterConfigsDir)
        {
            _jsonManager = jsonManager;
            _invoiceConverterConfigsDir = invoiceConverterConfigsDir;
            ProductQuantityByItemNameJson = Path.Combine(_invoiceConverterConfigsDir, "AutocompleteProdQuantityByAmazonProdName.json");
            ShippingNameByItemNameJson = Path.Combine(_invoiceConverterConfigsDir, "AutocompleteShippingTypeByAmazonProdName.json");
            PohodaProdCodeByAmazonProdCodeJson = Path.Combine(_invoiceConverterConfigsDir, "AutocompletePohodaProdCodeByAmazonProdCode.json");
            ProdWarehouseSectionByAmazonProdCodeJson = Path.Combine(_invoiceConverterConfigsDir, "AutocompleteProdWarehouseSectionByAmazonProdCode.json");
            CustomsDeclarationByItemNameJson = Path.Combine(_invoiceConverterConfigsDir, "AutocompleteCustomsDeclarationByAmazonProdName.json");
        }

        public IAutocompleteData LoadSettings()
        {
            var autocompleteData = new AutocompleteData
            {
                PohodaProdCodeByAmazonProdCode = _jsonManager.DeserializeJsonDictionary(PohodaProdCodeByAmazonProdCodeJson),
                ProdWarehouseSectionByAmazonProdCode = _jsonManager.DeserializeJsonDictionary(ProdWarehouseSectionByAmazonProdCodeJson),
                ShippingNameByItemName = _jsonManager.DeserializeJsonDictionary(ShippingNameByItemNameJson),
                PackQuantityByItemName = _jsonManager.DeserializeJsonDictionary(ProductQuantityByItemNameJson),
                CustomsDeclarationByItemName = _jsonManager.DeserializeJsonDictionary(CustomsDeclarationByItemNameJson)
            };
            // TODO fix names
            return autocompleteData;
        }

        public void SaveSettings(IAutocompleteData data)
        {
            _jsonManager.SerializeDictionaryToJson(data.PohodaProdCodeByAmazonProdCode, PohodaProdCodeByAmazonProdCodeJson);
            _jsonManager.SerializeDictionaryToJson(data.ProdWarehouseSectionByAmazonProdCode, ProdWarehouseSectionByAmazonProdCodeJson);
            _jsonManager.SerializeDictionaryToJson(data.ShippingNameByItemName, ShippingNameByItemNameJson);
            _jsonManager.SerializeDictionaryToJson(data.PackQuantityByItemName, ProductQuantityByItemNameJson);
            _jsonManager.SerializeDictionaryToJson(data.CustomsDeclarationByItemName, CustomsDeclarationByItemNameJson);
        }

    }
}