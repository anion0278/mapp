using System.Globalization;
using System.Text;
using System.Threading;
using Autofac;
using Mapp.ApplicationUpdater;
using Mapp.BusinessLogic.AutoComplete;
using Mapp.BusinessLogic.Currency;
using Mapp.BusinessLogic.Invoices;
using Mapp.BusinessLogic.Transactions;
using Mapp.Common;
using Mapp.Common.Logging;
using Mapp.DataAccess;
using Mapp.Infrastructure;
using Mapp.Infrastructure.Input;
using Mapp.UI.Exception;
using Mapp.UI.ViewModels;
using Mapp.UI.Extensions;
using Mapp.UI.Settings;
using Mapp.UI.Views;

namespace Mapp.UI.Startup;

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
        builder.RegisterAsInterfaceSingleton<FileManager>();
        builder.RegisterAsInterfaceSingleton<DateTimeManager>();
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
