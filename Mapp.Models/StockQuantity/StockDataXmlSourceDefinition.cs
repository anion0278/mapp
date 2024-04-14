using System.Collections.Generic;

namespace Mapp.Models.StockQuantity;

public record StockDataXmlSourceDefinition()
{
    public string Url { get; init; }
    public bool? IsEnabled { get; set; }
    public string ItemNodeName { get; init; }
    public IEnumerable<ValueParsingOption> SkuNodeParsingOptions { get; init; }
    public IEnumerable<ValueParsingOption> StockQuantityNodeParsingOptions { get; init; }
}