using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Mapp.Common;
using Mapp.Common.Extensions;
using Mapp.UI.Extensions;
using Mapp.UI.Localization;
using Mapp.UI.Settings;
using WPFLocalizeExtension.Engine;
using Moq;

namespace Mapp.UI.ViewModels;

public interface IMainViewModel
{
}

public class MainViewModel : ViewModelBase, IMainViewModel
{
    private ObservableCollection<TabViewModelBase> _tabs = new();
    private ObservableCollection<string> _availableLanguages = new();

    public ICollectionView Tabs { get; }
    public ICollectionView AvailableLanguages { get; set; }

    public ICommand SetLanguageCommand { get; set; }

    /// <summary>
    /// Design-time ctor
    /// </summary>
    [Obsolete("Design-time only!")]
    public MainViewModel()
    {

    }

    public MainViewModel(
        IInvoiceConverterViewModel invoiceConverterVM,
        AutoFillViewModel autofillVm,
        IWarehouseQuantityUpdaterViewModel warehouseQuantityUpdaterVM,
        ITransactionsConverterViewModel transactionsConverterVM)
    {
        SetupLanguages();

        _tabs.Add(invoiceConverterVM as TabViewModelBase);
        _tabs.Add(warehouseQuantityUpdaterVM as TabViewModelBase);
        _tabs.Add(autofillVm);
        _tabs.Add(transactionsConverterVM as TabViewModelBase);
        Tabs = CollectionViewSource.GetDefaultView(_tabs);

        SetLanguageCommand = new RelayCommand(SetLanguage);
    }

    private void SetupLanguages()
    {
        AvailableLanguages = CollectionViewSource.GetDefaultView(_availableLanguages);
        _availableLanguages.AddRange(GetAvailableCultures().Select(c => c.Name));
        // LocalizeDictionary.Instance.MergedAvailableCultures; does not find all the cultures
        AvailableLanguages.MoveCurrentTo(LocalizeDictionary.Instance.Culture.Name);
    }

    private void SetLanguage()
    {
        LocalizeDictionary.Instance.SetCultureCommand.Execute(AvailableLanguages.CurrentItem);
    }

    public static IEnumerable<CultureInfo> GetAvailableCultures()
    {
        var availableCultures = new List<CultureInfo>();
        var rm = new ResourceManager(typeof(LocalizationStrings));
        foreach (var culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
        {
            try
            {
                //do not use "==", won't work
                if (culture.Equals(CultureInfo.InvariantCulture)) continue;

                var rs = rm.GetResourceSet(culture, true, false);
                if (rs != null) availableCultures.Add(culture);
            }
            catch (CultureNotFoundException)
            {
                Debug.Print("No matching culture is found");
            }
        }
        return availableCultures.Append(CultureInfo.GetCultureInfo(Constants.NeutralDefaultLanguageName));
    }

}