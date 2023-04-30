using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Mapp.Common;
using Mapp.DataAccess;
using Mapp.Models.StockQuantity;

namespace Mapp.BusinessLogic.StockQuantity;

public class StockQuantityUpdater : IStockQuantityUpdater
{
    private readonly IJsonManager _jsonManager;
    private readonly IDialogService _dialogService;
    private readonly IEnumerable<StockDataXmlSourceDefinition> _sourceDefinitions;

    public StockQuantityUpdater(IJsonManager jsonManager, IDialogService dialogService)
    {
        _jsonManager = jsonManager;
        _dialogService = dialogService;
        _sourceDefinitions = _jsonManager.LoadStockQuantityUpdaterConfigs();
    }

    public async Task<IEnumerable<StockData>> ConvertWarehouseData()
    {
        var httpClient = new HttpClient();

        var stockDataTotal = new List<StockData>();

        Dictionary<string, int> statistics = new Dictionary<string, int>();

        foreach (var source in _sourceDefinitions)
        {
            var stream = await (await httpClient.GetAsync(source.Url)).Content.ReadAsStreamAsync();
            var stockData = ExtractStockData(stream, source);
            stockDataTotal.AddRange(stockData);

            statistics.Add(source.Url, stockData.Count());
        }

        string msg = "Konvertovano:\n";
        foreach (var (url, count) in statistics)
        {
            msg += $"{count} zaznamu z: {url} \n";
        }
        _dialogService.ShowMessage(msg);

        return stockDataTotal;
    }

    private IEnumerable<StockData> ExtractStockData(Stream stream, StockDataXmlSourceDefinition source)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(stream);

        var products = GetProductNodes(xmlDoc, source);
        var stockData = new List<StockData>(products.Count);
        foreach (XmlNode productNode in products)
        {
            if (TryParseDataNodeText(productNode, source.SkuNodeParsingOptions, out string sku)
                && TryParseDataNodeText(productNode, source.StockQuantityNodeParsingOptions, out string stockQuantityStr)
                && int.TryParse(stockQuantityStr, out int stockQuantity))
            {
                stockData.Add(new StockData(sku, stockQuantity));
            }
        }

        return stockData;
    }

    private bool TryParseDataNodeText(XmlNode startNode, IEnumerable<ValueParsingOption> parsingOptions, out string output)
    {
        foreach (var parsingOption in parsingOptions)
        {
            string nodeText = GetSingleDescendantNode(startNode, parsingOption.ElementName)?.InnerText;

            if (string.IsNullOrEmpty(nodeText)) continue;

            string pattern = parsingOption.SubstringPattern ?? ".+"; // take whole string if none
            output = Regex.Match(nodeText, pattern).Value;
            return true;
        }

        output = null;
        return false;
    }

    private XmlNodeList GetProductNodes(XmlDocument xmlDocument, StockDataXmlSourceDefinition source)
    {
        return xmlDocument.GetElementsByTagName(source.ItemNodeName);
    }

    //[CanBeNull]
    private XmlNode GetSingleDescendantNode(XmlNode startNode, string targetNodeName)
    {
        string targetNodeNameWithoutNs = RemoveNamespacePrefixIfProvided(targetNodeName);
        string nsAgnosticXpath = $".//*[local-name() = '{targetNodeNameWithoutNs}']";
        return startNode.SelectNodes(nsAgnosticXpath)?.Item(0);
    }

    private string RemoveNamespacePrefixIfProvided(string targetNodeName)
    {
        return targetNodeName.Remove(0, targetNodeName.IndexOf(':') + 1);
    }
}
