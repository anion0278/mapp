using System.Globalization;
using System.Text;
using System.Threading;
using Shmap.ApplicationUpdater;
using Shmap.BusinessLogic.AutocompletionHelper;
using Shmap.BusinessLogic.Currency;
using Shmap.BusinessLogic.Invoices;
using Shmap.BusinessLogic.Transactions;
using Shmap.CommonServices;
using Shmap.DataAccess;
using Shmap.ViewModels;
using Unity;

namespace Mapp
{
    public class Bootstrapper
    {
        public UnityContainer Container { get; }
        private IApplicationUpdater _appUpdater;
        private IAutocompleteData _autocompleteData;

        public Bootstrapper()
        {
            Container = new UnityContainer();
            ConfigureContainer();
        }

        private void ConfigureContainer()
        {
            //ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(_container)); // antipattern?

            // TODO use naming convention registering
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
            Container.RegisterTypeAsSingleton<IStartWindowViewModel, StartWindowViewModel>();
            Container.RegisterTypeAsSingleton<IDialogService, DialogService>();

            var autocompleteDataLoader = Container.Resolve<IAutocompleteDataLoader>();
            _autocompleteData = autocompleteDataLoader.LoadSettings();
            Container.RegisterInstance(_autocompleteData);

            _appUpdater = Container.Resolve<IApplicationUpdater>();
            _appUpdater.CheckUpdate();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        }

        public IStartWindowViewModel ResolveStartWindowViewModel()
        {
            return Container.Resolve<IStartWindowViewModel>(); // somehow it is not possible to do it from App.xaml.cs
        }
    }
}