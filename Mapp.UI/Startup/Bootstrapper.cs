using System.Globalization;
using System.Text;
using System.Threading;
using Autofac;
using Shmap.ApplicationUpdater;
using Shmap.BusinessLogic.AutoComplete;
using Shmap.BusinessLogic.Currency;
using Shmap.BusinessLogic.Invoices;
using Shmap.BusinessLogic.Transactions;
using Shmap.Common;
using Shmap.Common.Logging;
using Shmap.DataAccess;
using Shmap.Infrastructure;
using Shmap.Infrastructure.Input;
using Shmap.UI.Exception;
using Shmap.UI.ViewModels;
using Shmap.UI.Extensions;
using Shmap.UI.Settings;
using Shmap.UI.Views;

namespace Shmap.UI.Startup;

public class Bootstrapper
{
    public IContainer ConfigureContainer()
    {
        var builder = new ContainerBuilder();

        builder.RegisterAsInterfaceSingleton<JsonManager>();
        builder.RegisterAsInterfaceSingleton<AutoKeyboardInputHelper>();
        builder.RegisterAsInterfaceSingleton<ApplicationUpdater.ApplicationUpdater>();
        builder.RegisterAsInterfaceSingleton<InvoicesXmlManager>();
        builder.RegisterAsInterfaceSingleton<CsvLoader>();
        builder.RegisterAsInterfaceSingleton<AutocompleteDataLoader>();
        builder.RegisterAsInterfaceSingleton<CurrencyConverter>();
        builder.RegisterAsInterfaceSingleton<InvoiceConverter>();
        builder.RegisterAsInterfaceSingleton<TransactionsReader>();
        builder.RegisterAsInterfaceSingleton<GpcGenerator>();
        builder.RegisterAsInterfaceSingleton<FileOperationsService>();
        builder.RegisterAsInterfaceSingleton<DialogService>();
        builder.RegisterAsInterfaceSingleton<GlobalExceptionHandler>();
        builder.RegisterAsInterfaceSingleton<Logger>();
        builder.RegisterAsInterfaceSingleton<AutocompleteData>();
        builder.RegisterAsInterfaceSingleton<BrowserService>();
        builder.RegisterAsInterfaceSingleton<AutocompleteConfiguration>();
        builder.RegisterAsInterfaceSingleton<KeyboardHook>();
        builder.RegisterAsInterfaceSingleton<InputSimulator>();

        builder.RegisterAsInterfaceSingleton<MainViewModel>();
        builder.RegisterAsInterfaceSingleton<InvoiceConverterViewModel>();
        builder.RegisterAsInterfaceSingleton<TransactionsConverterViewModel>();
        builder.RegisterAsInterfaceSingleton<WarehouseQuantityUpdaterViewModel>();

        builder.RegisterAsInterface<ManualChangeWindowViewModel>();

        builder.RegisterInstance(new SettingsWrapper(Settings.AppSettings.Default, true)).As<ISettingsWrapper>();
        builder.RegisterType<MainWindow>();

        return builder.Build();
    }
}
