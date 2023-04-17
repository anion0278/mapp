using Shmap.CommonServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Shmap.Models;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using System.ComponentModel;
using System.Windows.Data;
using ABI.Windows.UI.ViewManagement;
using System;
using Shmap.UI.Settings;

namespace Shmap.UI.ViewModels;

public interface IMainViewModel
{
}

public class MainViewModel : ViewModelBase, IMainViewModel
{
    private readonly ISettingsWrapper _settingsWrapper;
    private ObservableCollection<TabViewModelBase> _tabs = new();

    public ICollectionView Tabs { get; }


    /// <summary>
    /// Design-time ctor
    /// </summary>
    [Obsolete("Design-time only!")]
    public MainViewModel()
    {
        
    }

    public MainViewModel(
        ISettingsWrapper settingsWrapper,
        IInvoiceConverterViewModel invoiceConverterVM, 
        IWarehouseQuantityUpdaterViewModel warehouseQuantityUpdaterVM,
        ITransactionsConverterViewModel transactionsConverterVM)
    {
        _settingsWrapper = settingsWrapper;

        _tabs.Add(invoiceConverterVM as TabViewModelBase);
        _tabs.Add(warehouseQuantityUpdaterVM as TabViewModelBase);
        _tabs.Add(transactionsConverterVM as TabViewModelBase);
        Tabs = CollectionViewSource.GetDefaultView(_tabs);
    }

}