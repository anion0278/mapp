using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Shmap.BusinessLogic.AutocompletionHelper;
using Shmap.BusinessLogic.Currency;
using Shmap.BusinessLogic.Invoices;
using Shmap.DataAccess;
using Mapp;
using Shmap.ApplicationUpdater;
using Shmap.BusinessLogic.Transactions;
using Shmap.CommonServices;
using Microsoft.Win32;
using Unity;


namespace Mapp
{
    public partial class StartWindow : Window
    {
        private readonly IAutoKeyboardInputHelper _autoKeyboardInputHelper;
        private readonly IApplicationUpdater _appUpdater;
        private readonly IAutocompleteData _autocompleteData;
        private readonly IInvoiceConverter _invoiceConverter;

        private IUnityContainer _container;

        public StartWindow()
        {
            _container = new UnityContainer();
            //ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(_container)); // antipattern?

            _container.RegisterInstance<IConfigProvider>(new ConfigProvider(AppSettings.Default, true));
            _container.RegisterType<IJsonManager, JsonManager>();
            _container.RegisterType<IAutoKeyboardInputHelper, AutoKeyboardInputHelper>();
            _container.RegisterType<IApplicationUpdater, ApplicationUpdater.ApplicationUpdater>();
            _container.RegisterType<IInvoicesXmlManager, InvoicesXmlManager>();
            _container.RegisterType<ICsvLoader, CsvLoader>();
            _container.RegisterType<IAutocompleteDataLoader, AutocompleteDataLoader>();
            _container.RegisterType<ICurrencyConverter, CurrencyConverter>();
            _container.RegisterType<IInvoiceConverter, InvoiceConverter>();
            _container.RegisterType<ITransactionsReader, TransactionsReader>();
            _container.RegisterType<IGpcGenerator, GpcGenerator>();
            _container.RegisterType<IFileOperationService, FileOperationsService>();
            _container.RegisterType<IStartWindowViewModel, StartWindowViewModel>();
            _container.RegisterType<IDialogService, DialogService>();

            var autocompleteDataLoader = _container.Resolve<IAutocompleteDataLoader>();
            _autocompleteData = autocompleteDataLoader.LoadSettings();
            _container.RegisterInstance(_autocompleteData);

            _invoiceConverter = _container.Resolve<IInvoiceConverter>();
            _autoKeyboardInputHelper = _container.Resolve<IAutoKeyboardInputHelper>();
            _appUpdater = _container.Resolve<IApplicationUpdater>();
            _appUpdater.CheckUpdate();

            DataContext = _container.Resolve<IStartWindowViewModel>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            InitializeComponent();
        }

      
        private void TopDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            ProcessCustomChangedDataForProduct(e, 5, _autocompleteData.PackQuantitySku, (element) => // TODO its VERY BAD to rely on column NUMBER
            {
                var dataContextItem = (InvoiceItemWithDetails)e.Row.DataContext;
                string quantity = (e.EditingElement as TextBox).Text;
                if (!int.TryParse(quantity, out _)) throw new ArgumentException("Hodnota musi byt cele cislo! Prozatim aplikace pada...");
                return dataContextItem.Item.amazonSkuCode; // TODO check if the number is integer !!
            });
            ProcessCustomChangedDataForProduct(e, 4, _autocompleteData.PohodaProdCodeBySku, (element) =>
            {
                var dataContextItem = (InvoiceItemWithDetails)e.Row.DataContext;
                return dataContextItem.Item.amazonSkuCode;
            });
            ProcessCustomChangedDataForProduct(e, 2, _autocompleteData.ShippingNameBySku, (element) =>
            {
                var dataContextItem = (InvoiceItemWithDetails)e.Row.DataContext;
                var symVar = dataContextItem.Header.symVar;

                // invoiceHeader is common for items in single Invoice, so it can be used for search
                var shippedItem = _invoiceConverter.InvoiceItemsAll.FirstOrDefault(itemWithDetail =>
                    itemWithDetail.Header.symVar == symVar && itemWithDetail != dataContextItem);
                return shippedItem?.Item.amazonSkuCode ?? string.Empty; // DRY principle for Item.amazonSkuCode
            });
        }

        private void BottomDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            // for both CustomDeclarations and Warehouse code we use the first Item's SKU to remember settings
            ProcessCustomChangedDataForProduct(e, 5, _autocompleteData.CustomsDeclarationBySku, (_) =>
            {
                var dataContextItem = (InvoiceXml.dataPackDataPackItem)e.Row.DataContext;
                return dataContextItem.invoice.invoiceDetail.FirstOrDefault()?.amazonSkuCode ?? string.Empty;
            });
            ProcessCustomChangedDataForProduct(e, 4, _autocompleteData.ProdWarehouseSectionBySku, (_) =>
            {
                var dataContextItem = (InvoiceXml.dataPackDataPackItem)e.Row.DataContext;
                return dataContextItem.invoice.invoiceDetail.FirstOrDefault()?.amazonSkuCode ?? string.Empty;
            });
        }

        private void ProcessCustomChangedDataForProduct(
            DataGridCellEditEndingEventArgs e,
            int columnIndex,
            IDictionary<string, string> rememberedDictionary,
            Func<FrameworkElement, string> productNameGetter)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (!(e.Column is DataGridBoundColumn)) return;

                if (e.Column.DisplayIndex == columnIndex)
                {
                    string productSku = productNameGetter(e.EditingElement);
                    string customValue = (e.EditingElement as TextBox).Text;
                    if (productSku != null && customValue != ApplicationConstants.EmptyItemCode && !string.IsNullOrEmpty(customValue)) // TODO - fix. VERY BAD
                    {
                        if (rememberedDictionary.ContainsKey(productSku))
                        {
                            if (!rememberedDictionary[productSku].Equals(customValue))
                                rememberedDictionary[productSku] = customValue;
                        }
                        else
                        {
                            rememberedDictionary.Add(productSku, customValue);
                        }
                    }
                }
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _autoKeyboardInputHelper.Dispose();
        }
    }
}
