using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Shmap.Common;

namespace Shmap.DataAccess
{
    public interface IAutocompleteData
    {
        Dictionary<string, string> ProdWarehouseSectionBySku { get; set; } // TODO fix names

        Dictionary<string, string> PohodaProdCodeBySku { get; set; } // TODO decide - SKU or AmazonProdCode

        Dictionary<string, string> ShippingNameBySku { get; set; }

        Dictionary<string, string> PackQuantitySku { get; set; }

        Dictionary<string, string> CustomsDeclarationBySku { get; set; }

        Dictionary<string, string> DefaultShippingByPartnerCountry { get; set; }

        void UpdateAutocompleteData<T>(T newParamValue, IDictionary<string, T> autocompleteData, string productKey);
    }

    public class AutocompleteData : IAutocompleteData
    {
        public Dictionary<string, string> ProdWarehouseSectionBySku { get; set; } // TODO fix names

        public Dictionary<string, string> PohodaProdCodeBySku { get; set; } // TODO decide - SKU or AmazonProdCode

        public Dictionary<string, string> ShippingNameBySku { get; set; }

        public Dictionary<string, string> PackQuantitySku { get; set; }

        public Dictionary<string, string> CustomsDeclarationBySku { get; set; }

        public Dictionary<string, string> DefaultShippingByPartnerCountry { get; set; }


        public void UpdateAutocompleteData<T>(T newParamValue, IDictionary<string, T> autocompleteData, string productKey)
        {
            if (autocompleteData.ContainsKey(productKey))  // dictionary.TODO use TryGetValue
            {
                autocompleteData[productKey] = newParamValue;
            }
            else
            {
                autocompleteData.Add(productKey, newParamValue);
            }
        }
    }

    public interface IAutocompleteDataLoader
    {
        IAutocompleteData LoadSettings();
        void SaveSettings(IAutocompleteData data);
    }

    public class AutocompleteDataLoader : IAutocompleteDataLoader
    {
        private readonly IJsonManager _jsonManager;
        private readonly ISettingsWrapper _settingsWrapper;

        private string ShippingNameBySkuJson;
        private string ProdWarehouseSectionBySkuJson;
        private string PohodaProdCodeBySkuJson;
        private string ProductQuantityBySkuJson;
        private string CustomsDeclarationBySkuJson;
        private string DefaultShippingByPartnerCountryJson;

        public AutocompleteDataLoader(IJsonManager jsonManager, ISettingsWrapper settingsWrapper)
        {
            _jsonManager = jsonManager;
            _settingsWrapper = settingsWrapper;
            LoadSettings();
        }

        public IAutocompleteData LoadSettings()
        {
            // TODO PathsProvider!!
            // TODO completely rewrite
            PohodaProdCodeBySkuJson = Path.Join(_settingsWrapper.InvoiceConverterConfigsDir, "AutocompletePohodaProdCodeBySku.json");
            ProdWarehouseSectionBySkuJson =
                Path.Join(_settingsWrapper.InvoiceConverterConfigsDir, "AutocompleteProdWarehouseSectionBySku.json");
            ShippingNameBySkuJson =
                Path.Join(_settingsWrapper.InvoiceConverterConfigsDir, "AutocompleteShippingTypeBySku.json");
            ProductQuantityBySkuJson =
                Path.Join(_settingsWrapper.InvoiceConverterConfigsDir, "AutocompleteProdQuantityBySku.json");
            CustomsDeclarationBySkuJson =
                Path.Join(_settingsWrapper.InvoiceConverterConfigsDir, "AutocompleteCustomsDeclarationBySku.json");

            DefaultShippingByPartnerCountryJson =
                Path.Join(_settingsWrapper.InvoiceConverterConfigsDir, "AutocompleteDefaultShippingByPartnerCountry.json");

            var autocompleteData = new AutocompleteData // TODO factory
            {
                // TODO intelligible exception if some files are missing
                PohodaProdCodeBySku = _jsonManager.DeserializeJsonDictionary(PohodaProdCodeBySkuJson),
                ProdWarehouseSectionBySku = _jsonManager.DeserializeJsonDictionary(ProdWarehouseSectionBySkuJson),
                ShippingNameBySku = _jsonManager.DeserializeJsonDictionary(ShippingNameBySkuJson),
                PackQuantitySku = _jsonManager.DeserializeJsonDictionary(ProductQuantityBySkuJson),
                CustomsDeclarationBySku = _jsonManager.DeserializeJsonDictionary(CustomsDeclarationBySkuJson),
                DefaultShippingByPartnerCountry = _jsonManager.DeserializeJsonDictionary(DefaultShippingByPartnerCountryJson),
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
            _jsonManager.SerializeDictionaryToJson(data.DefaultShippingByPartnerCountry, DefaultShippingByPartnerCountryJson);
        }

    }
}