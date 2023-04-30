namespace Shmap.Models.StockQuantity;

public record StockData(string Sku, int Quantity);

public record ValueParsingOption(string ElementName, string SubstringPattern);