using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using AutoUpdaterDotNET;
using Mapp.Common;
using Mapp.Models.StockQuantity;
using Mapp.Models.Transactions;


namespace Mapp.DataAccess
{
    public interface IJsonManager
    {
        IEnumerable<UpdateInfoEventArgs> DeserializeUpdates(string remoteJsonData);
        Dictionary<string, string> DeserializeJsonDictionary(string fileName);
        void SerializeDictionaryToJson(Dictionary<string, string> map, string fileName);
        IEnumerable<MarketPlaceTransactionsConfigData> LoadTransactionsConfigs();
        IReadOnlyList<StockDataXmlSourceDefinition> LoadStockQuantityUpdaterConfigs();
        void SaveStockQuantityUpdaterConfigs(IReadOnlyList<StockDataXmlSourceDefinition> definitions);
    }

    public class JsonManager : IJsonManager
    {
        private readonly ISettingsWrapper _settings;
        private readonly IFileManager _fileManager;

        public JsonManager(ISettingsWrapper settings, IFileManager fileManager)
        {
            _settings = settings;
            this._fileManager = fileManager;
        }

        public IEnumerable<UpdateInfoEventArgs> DeserializeUpdates(string remoteJsonData)
        {
            return JsonSerializer.Deserialize<IEnumerable<UpdateInfoEventArgs>>(remoteJsonData);
        }

        public Dictionary<string, string> DeserializeJsonDictionary(string fileName)
        {
            // TODO Handle file absence, show msg
            string json;
            try
            {
                json = _fileManager.ReadAllTextFromFile(fileName);
            }
            catch (Exception ex)
            {
                throw new SettingsDataAccessException($"'{fileName}' loading error!", ex);
            }

            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }

        public void SerializeDictionaryToJson(Dictionary<string, string> map, string fileName) // TODO store data only if change occured
        {
            string json = JsonSerializer.Serialize(map, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            });
            _fileManager.WriteAllTextToFile(fileName, json);
        }

        public IEnumerable<MarketPlaceTransactionsConfigData> LoadTransactionsConfigs()
        {
            // WE need it because when app is started from other dir (for example during UI tests), it would not otherwise find the configs!!
            var fileNames = Directory.GetFiles(_settings.TransactionConverterConfigsDir);
            var configDtos = new List<MarketPlaceTransactionsConfigData>();
            foreach (var fileName in fileNames.Where(fn => fn.Contains("TransactionsConfig")))
            {
                string json = File.ReadAllText(fileName);
                var configDto = JsonSerializer.Deserialize<MarketPlaceTransactionsConfigData>(json);
                configDto.Name = Path.GetFileNameWithoutExtension(fileName);
                configDtos.Add(configDto);
            }

            return configDtos;
        }

        public IReadOnlyList<StockDataXmlSourceDefinition> LoadStockQuantityUpdaterConfigs()
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            string json = _fileManager.ReadAllTextFromFile(Path.Join("StockQuantityUpdater", "config.json"));
            return JsonSerializer.Deserialize<StockDataXmlSourceDefinition[]>(json, serializeOptions);
        }

        public void SaveStockQuantityUpdaterConfigs(IReadOnlyList<StockDataXmlSourceDefinition> definitions)
        {
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(definitions, serializeOptions);
            _fileManager.WriteAllTextToFile(Path.Join("StockQuantityUpdater", "config.json"), json);
        }
    }
}