using System.Globalization;
using System.Text;
using System.Threading;
using Shmap.ApplicationUpdater;
using Shmap.BusinessLogic.AutocompletionHelper;
using Shmap.BusinessLogic.Currency;
using Shmap.BusinessLogic.Invoices;
using Shmap.BusinessLogic.Transactions;
using Shmap.CommonServices;
using Shmap.CommonServices.Log;
using Shmap.DataAccess;
using Shmap.Models;
using Shmap.UI.Exception;
using Shmap.UI.ViewModels;
using Serilog.Core;
using Unity;
using Logger = Shmap.CommonServices.Log.Logger;

namespace Shmap.UI.Startup
{
    public class Bootstrapper  
    {
        public UnityContainer Container { get; }
        private IApplicationUpdater _appUpdater;
        private IAutocompleteData _autocompleteData;
        private IAutoKeyboardInputHelper _keyboardInputHelper;

        public Bootstrapper()
        {
            Container = new UnityContainer();
        }

        public UnityContainer ConfigureContainer()
        {
            // TODO use naming convention auto-registering
            Container.RegisterInstance<IConfigProvider>(new ConfigProvider(AppSettings.Default, true));
            Container.RegisterTypeAsSingleton<IJsonManager, JsonManager>();
            Container.RegisterTypeAsSingleton<IAutoKeyboardInputHelper, AutoKeyboardInputHelper>();
            Container.RegisterTypeAsSingleton<IApplicationUpdater, ApplicationUpdater.ApplicationUpdater>();
            Container.RegisterTypeAsSingleton<IInvoicesXmlManager, InvoicesXmlManager>();
            Container.RegisterTypeAsSingleton<ICsvLoader, CsvLoader>();
            Container.RegisterTypeAsSingleton<IAutocompleteDataLoader, AutocompleteDataLoader>();
            Container.RegisterTypeAsSingleton<ICurrencyConverter, CurrencyConverter>();
            Container.RegisterTypeAsSingleton<IInvoiceConverter, InvoiceConverter>();
            Container.RegisterTypeAsSingleton<ITransactionsReader, TransactionsReader>();
            Container.RegisterTypeAsSingleton<IGpcGenerator, GpcGenerator>();
            Container.RegisterTypeAsSingleton<IFileOperationService, FileOperationsService>();
            Container.RegisterTypeAsSingleton<IDialogService, DialogService>();
            Container.RegisterTypeAsSingleton<IGlobalExceptionHandler, GlobalExceptionHandler>();
            Container.RegisterTypeAsSingleton<ILogger, Logger>();
            Container.RegisterTypeAsSingleton<IMainViewModel, MainViewModel>();

            Container.RegisterTypeAsSingleton<IInvoiceConverterViewModel, InvoiceConverterViewModel>();
            Container.RegisterTypeAsSingleton<ITransactionsConverterViewModel, TransactionsConverterViewModel>();
            Container.RegisterTypeAsSingleton<IWarehouseQuantityUpdaterViewModel, WarehouseQuantityUpdaterViewModel>();

            Container.RegisterType<IManualChangeWindowViewModel, ManualChangeWindowViewModel>();

            // TODO move - does not belong here
            var autocompleteDataLoader = Container.Resolve<IAutocompleteDataLoader>();
            _autocompleteData = autocompleteDataLoader.LoadSettings();
            Container.RegisterInstance(_autocompleteData);

            _keyboardInputHelper = Container.Resolve<IAutoKeyboardInputHelper>(); // SHOULD BE HERE, otherwise will not get instantiated
            _appUpdater = Container.Resolve<IApplicationUpdater>();
            _appUpdater.CheckUpdate();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); 
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // TODO avoid using

            return Container;
        }
    }
}