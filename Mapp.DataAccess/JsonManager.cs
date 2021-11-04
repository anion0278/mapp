using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Text.Json;
using AutoUpdaterDotNET;
using Shmap.Models;


namespace Shmap.DataAccess
{
    public interface IJsonManager
    {
        IEnumerable<UpdateInfoEventArgs> DeserializeUpdates(string remoteJsonData);
        Dictionary<string, string> DeserializeJsonDictionary(string fileName);
        void SerializeDictionaryToJson(Dictionary<string, string> map, string fileName);
        IEnumerable<MarketPlaceTransactionsConfigDTO> LoadTransactionsConfigs();
    }

    public class JsonManager: IJsonManager
    {

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
                json = File.ReadAllText(fileName);
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
                IgnoreNullValues = true, WriteIndented = true
            });
            File.WriteAllText(fileName, json);
        }

        public IEnumerable<MarketPlaceTransactionsConfigDTO> LoadTransactionsConfigs()
        {
            var fileNames = Directory.GetFiles("Transactions Configs");
            var configDtos = new List<MarketPlaceTransactionsConfigDTO>();
            foreach (var fileName in fileNames.Where(fn => fn.Contains("TransactionsConfig")))
            {
                string json = File.ReadAllText(fileName);
                var configDto = JsonSerializer.Deserialize<MarketPlaceTransactionsConfigDTO>(json);
                configDtos.Add(configDto);
            }

            return configDtos;
        }
    }
}