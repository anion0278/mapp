using System.Collections.Generic;
using System.Threading.Tasks;
using Mapp.Models.StockQuantity;

namespace Mapp.BusinessLogic.StockQuantity;

public interface IStockQuantityUpdater
{
    Task<IEnumerable<StockData>> ConvertWarehouseData();
}