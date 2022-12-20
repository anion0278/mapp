using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Xml;
using System;
using System.Threading.Tasks;

namespace Shmap.DataAccess;

public class StockQuantityUpdater
{

    public async Task<IEnumerable<StockData>> ConvertWarehouseData(IEnumerable<StockDataXmlSourceDefinition> stockDataXmlSources)
    {
      
        var httpClient = new HttpClient();

        var stockData = new List<StockData>();
        foreach (var source in stockDataXmlSources)
        {
            var stream = await (await httpClient.GetAsync(source.Url)).Content.ReadAsStreamAsync();
            stockData.AddRange(ExtractStockData(stream, source));
        }

        return stockData;
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


public record StockData(string Sku, int Quantity);

public record ValueParsingOption(string ElementName, string SubstringPattern);

//public record StockDataXmlSourceDefinition(string Url, string ItemNodeName, string SkuNodeName, string StockQuantityNodeName);e

public record StockDataXmlSourceDefinition()
{
    public string Url { get; init; }
    public string ItemNodeName { get; init; }
    public IEnumerable<ValueParsingOption> SkuNodeParsingOptions { get; init; }
    public IEnumerable<ValueParsingOption> StockQuantityNodeParsingOptions { get; init; }
}

