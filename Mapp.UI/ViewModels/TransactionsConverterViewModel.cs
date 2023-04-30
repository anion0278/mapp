using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Shmap.BusinessLogic.Transactions;
using Shmap.Common;
using Shmap.UI.Localization;
using Shmap.UI.Settings;

namespace Shmap.UI.ViewModels;

public interface ITransactionsConverterViewModel
{
}

public class TransactionsConverterViewModel: TabViewModelBase, ITransactionsConverterViewModel
{
    private readonly ISettingsWrapper _settingsWrapper;
    private readonly ITransactionsReader _transactionsReader;
    private readonly IGpcGenerator _gpcGenerator;
    private readonly IFileOperationService _fileOperationService;
    private readonly IDialogService _dialogService;
    public RelayCommand ConvertTransactionsCommand { get; }

    public TransactionsConverterViewModel(ISettingsWrapper settingsWrapper,
        ITransactionsReader transactionsReader,
        IGpcGenerator gpcGenerator,
        IFileOperationService fileOperationService,
        IDialogService dialogService) : base(LocalizationStrings.TransactionsConverterTabTitle.GetLocalized())
    {
        _settingsWrapper = settingsWrapper;
        _transactionsReader = transactionsReader;
        _gpcGenerator = gpcGenerator;
        _fileOperationService = fileOperationService;
        _dialogService = dialogService;
        ConvertTransactionsCommand = new RelayCommand(ConvertTransactions, () => true);
    }

    private void ConvertTransactions()
    {
        var fileNames = _fileOperationService.GetTransactionFileNames();
        if (!fileNames.Any()) return;

        var transactions = _transactionsReader.ReadTransactionsFromMultipleFiles(fileNames);

        string saveFileName = _fileOperationService.GetSaveFileNameForConvertedTransactions();
        if (string.IsNullOrWhiteSpace(saveFileName)) return;

        _gpcGenerator.SaveTransactions(transactions, saveFileName);

        if (_settingsWrapper.OpenTargetFolderAfterConversion)
        {
            _fileOperationService.OpenFileFolder(saveFileName);
        }
    }

}