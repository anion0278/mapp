using System.Collections.Generic;
using System.Threading.Tasks;
using Shmap.Models.StockQuantity;

namespace Shmap.BusinessLogic.StockQuantity;

public interface IStockQuantityUpdater
{
    Task<IEnumerable<StockData>> ConvertWarehouseData();
}