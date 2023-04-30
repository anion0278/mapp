using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Shmap.BusinessLogic.Invoices;
using Shmap.Common;
using Shmap.DataAccess;
using Shmap.Infrastructure;
using Shmap.UI.Localization;
using Shmap.UI.Settings;
using Microsoft.Win32;
using Moq;

namespace Shmap.UI.ViewModels;
public class InvoiceConverterViewModel : TabViewModelBase, IInvoiceConverterViewModel
{
    private readonly IInvoiceConverter _invoiceConverter;
    private readonly ISettingsWrapper _settingsWrapper;
    private readonly IFileOperationService _fileOperationService;
    private readonly IAutocompleteData _autocompleteData;
    private readonly IDialogService _dialogService;
    private readonly IBrowserService _browserService;

    private uint? _existingInvoiceNumber;
    private string _defaultEmail;

    public RelayCommand SelectAmazonInvoicesCommand { get; }
    public RelayCommand ExportConvertedAmazonInvoicesCommand { get; }

    public ObservableCollection<InvoiceItemViewModel> InvoiceItems { get; } = new(); // TODO make private field?

    public ICollectionView InvoiceItemsCollectionView { get; }

    public BindingList<InvoiceViewModel> Invoices { get; } = new(); // use BindingList if it is required to update UI for inner items changes


    [Required]
    public uint? ExistingInvoiceNumber
    {
        get => _existingInvoiceNumber;
        set
        {
            if (!value.HasValue) return;

            _settingsWrapper.ExistingInvoiceNumber = value.Value * 2; // TODO join methods, since names are same
            _existingInvoiceNumber = value.Value;
        }
    }

    [EmailAddress]
    public string DefaultEmail
    {
        get => _defaultEmail;
        set
        {
            value = value.ToLower();
            _defaultEmail = value;
            _settingsWrapper.DefaultEmail = value; // TODO when value is not valid (according to the rules), it still saves data to config. Solve for all props
        }
    }

    public string TrackingCode
    {
        get => _settingsWrapper.TrackingCode;
        set => _settingsWrapper.TrackingCode = value;
    }

    public bool OpenTargetFolderAfterConversion
    {
        get => _settingsWrapper?.OpenTargetFolderAfterConversion ?? false; // TODO null conditional should not be used
        set => _settingsWrapper.OpenTargetFolderAfterConversion = value; // TODO move all config stuff to export part
    }

    //[Obsolete("Design-time only!")]
    //public InvoiceConverterViewModel() : base("Design-Time ctor")
    //{
    //    _existingInvoiceNumber = 123456789;
    //    _defaultEmail = "email@email.com";

    //    InvoiceItemsCollectionView = InitializeCollectionView();

    //    var invoice = new Invoice(new Dictionary<string, decimal>())
    //    {
    //        VariableSymbolFull = "203-5798943-2666737",
    //        ShipCountryCode = "GB",
    //        RelatedWarehouseName = "CGE",
    //        CustomsDeclaration = "1x deskova hra",
    //        SalesChannel = "amazon.com"
    //    };
    //    var items = new InvoiceItemBase[]
    //    {
    //            new InvoiceProduct(invoice)
    //            {
    //                AmazonSku = "55-KOH-FR6885",
    //                Name = "Dermacol Make-Up Cover, Waterproof Hypoallergenic for All Skin Types, Nr 218",
    //                PackQuantityMultiplier = 1,
    //                WarehouseCode = "CGE08",
    //            },
    //            new InvoiceItemGeneral(invoice, InvoiceItemType.Shipping)
    //            {
    //                Name = "Shipping",
    //            },
    //            new InvoiceItemGeneral(invoice, InvoiceItemType.Discount)
    //            {
    //                Name = "Discount"
    //            }
    //    };
    //    invoice.AddInvoiceItems(items);
    //    var dataMock = Mock.Of<IAutocompleteData>();
    //    foreach (var item in items)
    //    {
    //        InvoiceItems.Add(new InvoiceItemViewModel(item, dataMock)); // TODO create bindable collection with AddRange method
    //    }
    //    Invoices.Add(new InvoiceViewModel(invoice, dataMock));
    //}

    public InvoiceConverterViewModel(
        ISettingsWrapper settingsWrapper,
        IInvoiceConverter invoiceConverter,
        IFileOperationService fileOperationService,
        IAutocompleteData autocompleteData,
        IDialogService dialogService,
        IBrowserService browserService) : base(LocalizationStrings.InvoiceConverterTabTitle.GetLocalized())
    {
        _invoiceConverter = invoiceConverter;
        _settingsWrapper = settingsWrapper;
        _fileOperationService = fileOperationService;
        _autocompleteData = autocompleteData;
        _dialogService = dialogService;
        _browserService = browserService;

        SelectAmazonInvoicesCommand = new RelayCommand(SelectAmazonInvoices, () => !HasErrors);
        ExportConvertedAmazonInvoicesCommand = new RelayCommand(ExportConvertedAmazonInvoices, ExportConvertedAmazonInvoicesCanExecute);

        InvoiceItemsCollectionView = InitializeCollectionView();

        _existingInvoiceNumber = _settingsWrapper.ExistingInvoiceNumber;
        _defaultEmail = _settingsWrapper.DefaultEmail;

        ValidateAllProperties(); // TODO should not be needed
    }

    private ICollectionView InitializeCollectionView()
    {
        var collectionView = GetNewCollectionViewInstance(InvoiceItems);
        collectionView.GroupDescriptions.Add(new PropertyGroupDescription(nameof(InvoiceItemViewModel.AmazonNumber)));
        return collectionView;
    }

    private void SelectAmazonInvoices()
    {
        InvoiceItems.Clear();
        Invoices.Clear();

        var fileNames = _fileOperationService.OpenAmazonInvoices();
        if (!fileNames.Any()) return;

        var conversionContext = new InvoiceConversionContext() // TODO injected factory
        {
            ConvertToDate = DateTime.Today,
            DefaultEmail = DefaultEmail, // TODO decide whether to take it from config or from VM
            ExistingInvoiceNumber = ExistingInvoiceNumber.Value,
        };
        var invoices = _invoiceConverter.LoadAmazonReports(fileNames, conversionContext).ToList();

        foreach (var invoiceItem in invoices.SelectMany(di => di.InvoiceItems))
        {
            InvoiceItems.Add(new InvoiceItemViewModel(invoiceItem, _autocompleteData, _browserService));
        }

        foreach (var invoice in invoices)
        {
            Invoices.Add(new InvoiceViewModel(invoice, _autocompleteData));
        }
    }

    private bool ExportConvertedAmazonInvoicesCanExecute()
    {
        return !HasErrors && InvoiceItems.Any() && Invoices.Any()
               && InvoiceItems.All(i => !i.HasErrors)
               && Invoices.All(i => !i.HasErrors);
    }

    private void ExportConvertedAmazonInvoices()
    {
        string fileName = _fileOperationService.SaveConvertedAmazonInvoices();
        if (string.IsNullOrWhiteSpace(fileName)) return;

        var invoices = Invoices.Select(i => i.ExportModel()).ToList();

        // TODO how to avoid need for calling ToList (due to laziness of linq)?
        var invoiceItems = InvoiceItems.Select(i => i.ExportModel()).ToList();

        if (!invoices.Any())
        {
            _dialogService.ShowMessage(LocalizationStrings.InvoiceNotConvertedMsg.GetLocalized()); // TODO solve using OperationResult
            return;
        }

        _invoiceConverter.ProcessInvoices(invoices, fileName);
        ExistingInvoiceNumber += (uint)invoices.Count;

        if (_settingsWrapper.OpenTargetFolderAfterConversion)
        {
            _fileOperationService.OpenFileFolder(fileName);
        }
    }
}

public interface IInvoiceConverterViewModel
{
    
}
