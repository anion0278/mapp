using CommunityToolkit.Mvvm.Input;
using Shmap.BusinessLogic.Transactions;
using Shmap.CommonServices;
using Shmap.UI.Localization;
using Shmap.UI.Settings;

namespace Shmap.UI.ViewModels;

public class AutoFillViewModel: TabViewModelBase
{
    private readonly ISettingsWrapper _settingsWrapper;
    private readonly ITransactionsReader _transactionsReader;
    private readonly IGpcGenerator _gpcGenerator;
    private readonly IFileOperationService _fileOperationService;
    private readonly IDialogService _dialogService;
    public RelayCommand ConvertTransactionsCommand { get; }

    public AutoFillViewModel(ISettingsWrapper settingsWrapper,
        ITransactionsReader transactionsReader,
        IGpcGenerator gpcGenerator,
        IFileOperationService fileOperationService,
        IDialogService dialogService) : base(LocalizationStrings.AutoFillTabTitle.GetLocalized())
    {
        _settingsWrapper = settingsWrapper;
        _transactionsReader = transactionsReader;
        _gpcGenerator = gpcGenerator;
        _fileOperationService = fileOperationService;
        _dialogService = dialogService;
    }
}