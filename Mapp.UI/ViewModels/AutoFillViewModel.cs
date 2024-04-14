using CommunityToolkit.Mvvm.Input;
using Mapp.BusinessLogic.Transactions;
using Mapp.Common;
using Mapp.UI.Localization;
using Mapp.UI.Settings;

namespace Mapp.UI.ViewModels;

public class AutoFillViewModel: TabViewModelBase
{
    private readonly ISettingsWrapper _settingsWrapper;
    public RelayCommand ConvertTransactionsCommand { get; }

    public string TrackingCode
    {
        get => _settingsWrapper.TrackingCode;
        set
        {
            _settingsWrapper.TrackingCode = value;
        }
    }

    public override string Title => LocalizationStrings.AutoFillTabTitle.GetLocalized();

    public AutoFillViewModel(ISettingsWrapper settingsWrapper) 
    {
        _settingsWrapper = settingsWrapper;
    }
}