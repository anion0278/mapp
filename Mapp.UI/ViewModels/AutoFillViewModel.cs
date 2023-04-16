using CommunityToolkit.Mvvm.Input;
using Shmap.BusinessLogic.Transactions;
using Shmap.CommonServices;

namespace Shmap.UI.ViewModels;

public class AutoFillViewModel: TabViewModelBase
{
    private readonly IConfigProvider _configProvider;
    private readonly ITransactionsReader _transactionsReader;
    private readonly IGpcGenerator _gpcGenerator;
    private readonly IFileOperationService _fileOperationService;
    private readonly IDialogService _dialogService;
    public RelayCommand ConvertTransactionsCommand { get; }

    public AutoFillViewModel(IConfigProvider configProvider,
        ITransactionsReader transactionsReader,
        IGpcGenerator gpcGenerator,
        IFileOperationService fileOperationService,
        IDialogService dialogService) : base("AutoFiller")
    {
        _configProvider = configProvider;
        _transactionsReader = transactionsReader;
        _gpcGenerator = gpcGenerator;
        _fileOperationService = fileOperationService;
        _dialogService = dialogService;
    }
}