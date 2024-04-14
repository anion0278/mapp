using System.Collections.Generic;
using System.Threading.Tasks;
using Mapp.Models.StockQuantity;

namespace Mapp.BusinessLogic.StockQuantity;

public interface IStockQuantityUpdater
{
    IReadOnlyList<StockDataXmlSourceDefinition> SourceDefinitions { get; }
    Task<IEnumerable<StockData>> ConvertWarehouseData(IReadOnlyList<StockDataXmlSourceDefinition> sources);
}