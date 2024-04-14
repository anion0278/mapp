using CommunityToolkit.Mvvm.Input;
using Mapp.DataAccess;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Mapp.BusinessLogic.StockQuantity;
using Mapp.Common;
using Mapp.Models.StockQuantity;
using Mapp.UI.Localization;
using Mapp.UI.Settings;

namespace Mapp.UI.ViewModels;

public interface IWarehouseQuantityUpdaterViewModel
{
}

public class WarehouseQuantityUpdaterViewModel : TabViewModelBase, IWarehouseQuantityUpdaterViewModel
{
    private readonly ISettingsWrapper _settingsWrapper;
    private readonly IJsonManager _jsonManager;
    private readonly IStockQuantityUpdater stockQuantityUpdater;
    private readonly IFileOperationService _fileOperationService;
    private readonly IStockQuantityUpdater _stockQuantityUpdater;
    private readonly IDialogService _dialogService;
    public ICommand ConvertWarehouseDataCommand { get; set; }

    public IReadOnlyList<StockDataXmlSourceDefinition> SourceDefinitions { get; }

    public override string Title => LocalizationStrings.QuantityUpdaterTabTitle.GetLocalized();

    public WarehouseQuantityUpdaterViewModel(
        ISettingsWrapper settingsWrapper,
        IJsonManager jsonManager,
        IStockQuantityUpdater stockQuantityUpdater,
        IFileOperationService fileOperationService,
        IDialogService dialogService)
    {
        _settingsWrapper = settingsWrapper;
        _jsonManager = jsonManager;
        _stockQuantityUpdater = stockQuantityUpdater;
        _fileOperationService = fileOperationService;
        _dialogService = dialogService;

        //AsyncRelayCommandOptions.None - disable button during execution of Async Task (AllowConcurrentExecutions = false)
        ConvertWarehouseDataCommand = new AsyncRelayCommand(ConvertWarehouseData, AsyncRelayCommandOptions.None);

        SourceDefinitions = _stockQuantityUpdater.SourceDefinitions;
        foreach (var sourceDefinition in SourceDefinitions)
        {
            if (sourceDefinition.IsEnabled == null) sourceDefinition.IsEnabled = true;
        }
    }

    private async Task ConvertWarehouseData()
    {
        _jsonManager.SaveStockQuantityUpdaterConfigs(SourceDefinitions);

        var stockData = await _stockQuantityUpdater.ConvertWarehouseData(SourceDefinitions.Where(s => s.IsEnabled == true).ToList());
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
