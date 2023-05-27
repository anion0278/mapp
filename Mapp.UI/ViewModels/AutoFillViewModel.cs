using CommunityToolkit.Mvvm.Input;
using Mapp.BusinessLogic.Transactions;
using Mapp.Common;
using Mapp.UI.Localization;
using Mapp.UI.Settings;

namespace Mapp.UI.ViewModels;

public class AutoFillViewModel: TabViewModelBase
{
    private readonly ISettingsWrapper _settingsWrapper;
    private readonly ITransactionsReader _transactionsReader;
    private readonly IGpcGenerator _gpcGenerator;
    private readonly IFileOperationService _fileOperationService;
    private readonly IDialogService _dialogService;
    public RelayCommand ConvertTransactionsCommand { get; }

    public override string Title => LocalizationStrings.AutoFillTabTitle.GetLocalized();

    public AutoFillViewModel(ISettingsWrapper settingsWrapper,
        ITransactionsReader transactionsReader,
        IGpcGenerator gpcGenerator,
        IFileOperationService fileOperationService,
        IDialogService dialogService) 
    {
        _settingsWrapper = settingsWrapper;
        _transactionsReader = transactionsReader;
        _gpcGenerator = gpcGenerator;
        _fileOperationService = fileOperationService;
        _dialogService = dialogService;
    }
}