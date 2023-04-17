using CommunityToolkit.Mvvm.Input;
using Shmap.CommonServices;
using Shmap.DataAccess;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Shmap.UI.Localization;
using Shmap.UI.Settings;

namespace Shmap.UI.ViewModels;

public interface IWarehouseQuantityUpdaterViewModel
{
}

public class WarehouseQuantityUpdaterViewModel : TabViewModelBase, IWarehouseQuantityUpdaterViewModel
{
    private readonly ISettingsWrapper _settingsWrapper;
    private readonly IJsonManager _jsonManager;
    private readonly IFileOperationService _fileOperationService;
    private readonly IDialogService _dialogService;
    public ICommand ConvertWarehouseDataCommand { get; set; }

    public WarehouseQuantityUpdaterViewModel(
        ISettingsWrapper settingsWrapper,
        IJsonManager jsonManager,
        IFileOperationService fileOperationService,
        IDialogService dialogService) : base(LocalizationStrings.QuantityUpdaterTabTitle.GetLocalized())
    {
        _settingsWrapper = settingsWrapper;
        _jsonManager = jsonManager;
        _fileOperationService = fileOperationService;
        _dialogService = dialogService;

        //AsyncRelayCommandOptions.None - disable button during execution of Async Task (AllowConcurrentExecutions = false)
        ConvertWarehouseDataCommand = new AsyncRelayCommand(ConvertWarehouseData, AsyncRelayCommandOptions.None);
    }

    private async Task ConvertWarehouseData()
    {
        StockDataXmlSourceDefinition[] sources =
        {
                    new()
                    {
                        Url = "https://www.rappa.cz/export/vo.xml",
                        ItemNodeName = "SHOPITEM",
                        SkuNodeParsingOptions = new[]
                        {
                            new ValueParsingOption("EAN", null),
                        },
                        StockQuantityNodeParsingOptions = new[]
                        {
                            new ValueParsingOption("STOCK", null),
                        },
                    },
                    new()
                    {
                        Url = "https://en.bushman.eu/content/feeds/uQ5TueFNQh_expando_4.xml",
                        ItemNodeName = "item",
                        SkuNodeParsingOptions = new[]
                        {
                            new ValueParsingOption("g:gtin", null),
                            new ValueParsingOption("g:sku_with_ean", @"\d{13}"),
                        },
                        StockQuantityNodeParsingOptions = new[]
                        {
                            new ValueParsingOption("g:quantity", null),
                        },
                    }
                };

        var stockQuantityUpdater = new StockQuantityUpdater(_jsonManager, _dialogService);
        var stockData = await stockQuantityUpdater.ConvertWarehouseData();
        var columnNamesLine =
            "sku\tprice\tminimum-seller-allowed-price\tmaximum-seller-allowed-price\tquantity\thandling-time\tfulfillment-channel";
        var lines = new List<string>(stockData.Count() + 1) { columnNamesLine };
        string lineTemplate = "{0}\t\t\t\t{1}\t\t";
        foreach (var stockInfo in stockData)
        {
            lines.Add(lineTemplate.FormatWith(stockInfo.Sku, stockInfo.Quantity));
        }

        var saveFileDialog = new SaveFileDialog
        {
            Title = LocalizationStrings.ChooseOutputLocationTitle.GetLocalized(),
            FileName = "stockQuantity_" + DateTime.Today.ToString("dd-MM-yyyy") + ".txt",
            Filter = LocalizationStrings.TextFilesExtensionDescription.GetLocalized() + "|*.txt"
        };
        bool? result = saveFileDialog.ShowDialog();
        if (result != true) return;

        await File.WriteAllLinesAsync(saveFileDialog.FileName, lines);
        if (_settingsWrapper.OpenTargetFolderAfterConversion)
        {
            _fileOperationService.OpenFileFolder(saveFileDialog.FileName);
        }
    }

}
