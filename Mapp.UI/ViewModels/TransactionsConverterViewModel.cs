using System.Linq;
using CommunityToolkit.Mvvm.Input;
using Shmap.BusinessLogic.Transactions;
using Shmap.CommonServices;

namespace Shmap.UI.ViewModels;

public interface ITransactionsConverterViewModel
{
}

public class TransactionsConverterViewModel: TabViewModelBase, ITransactionsConverterViewModel
{
    private readonly IConfigProvider _configProvider;
    private readonly ITransactionsReader _transactionsReader;
    private readonly IGpcGenerator _gpcGenerator;
    private readonly IFileOperationService _fileOperationService;
    private readonly IDialogService _dialogService;
    public RelayCommand ConvertTransactionsCommand { get; }

    public TransactionsConverterViewModel(IConfigProvider configProvider,
        ITransactionsReader transactionsReader,
        IGpcGenerator gpcGenerator,
        IFileOperationService fileOperationService,
        IDialogService dialogService) : base("Transaction Converter")
    {
        _configProvider = configProvider;
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

        if (_configProvider.OpenTargetFolderAfterConversion)
        {
            _fileOperationService.OpenFileFolder(saveFileName);
        }
    }

}