using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using Shmap.UI.Exception;
using Shmap.UI.Startup;
using Shmap.UI.Views;
using NLog;
using Unity;

namespace Shmap.UI
{
    public class BindingException : System.Exception
    {
        public BindingException(string message) : base(message)
        { }

        public BindingException(string message, System.Exception innerException) : base(message, innerException)
        { }
    }

    public class ExplicitBindingErrorTraceListener : TraceListener
    {
        public EventHandler<System.Exception> BindingErrorEventHandler { get; set; }

        public override void Write(string message)
        { }

        public override void WriteLine(string message)
        {
            //BindingErrorEventHandler?.Invoke(this, new BindingException(message));
            Flush();
        }
    }


    public partial class App : Application
    {
        private IGlobalExceptionHandler _exceptionHandler = null!;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // TODO project-wide System.OverflowException check !!!

        protected void Application_Startup(object sender, StartupEventArgs e)
        {
            //using var loggerFactory = LoggerFactory.Create(builder =>
            //{
            //    builder
            //        .AddFilter("Microsoft", LogLevel.Warning)
            //        .AddFilter("System", LogLevel.Warning)
            //        .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
            //        .AddNLog(NLog.LogFactory());
            //});

            //Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger<App>();
            //logger.LogInformation("Example log message");

            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.ConfigureContainer();
            _exceptionHandler = container.Resolve<IGlobalExceptionHandler>();
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            var mainWindow = container.Resolve<MainWindow>();
            mainWindow.Show();
#if !DEBUG
            //https://github.com/dotnet/winforms/blob/72c140e531729b58737bb7b84212ff96767a151d/src/System.Windows.Forms/src/System/Windows/Forms/Application.cs#L952
            //AppCenter.Start("9549dd3a-1371-4a23-b973-f5e80154119d", typeof(Analytics), typeof(Crashes)); // TODO should solve secrt storing somehow :(
#endif
            //SetupExceptionHandling();

            //PresentationTraceSources.Refresh();
            //var wpfBindingErrorHandler = new ExplicitBindingErrorTraceListener();
            //wpfBindingErrorHandler.BindingErrorEventHandler += (o, e) => LogException(e, "WPF Binding");
            //PresentationTraceSources.DataBindingSource.Listeners.Add(wpfBindingErrorHandler);
            //PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
            //PresentationTraceSources.FreezableSource.Switch.Level = SourceLevels.Off; // needed in order to hide exceptions from Freezables

            //var mainWindow = new MainWindow();
            //Current.MainWindow = mainWindow;
            //mainWindow.Show();
        }

        private void Application_OnExit(object sender, ExitEventArgs e)
        {
            DispatcherUnhandledException -= OnDispatcherUnhandledException;
        }

        private async void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            if (IsAutomationNonCriticalException(e))
            {
                Debug.Print("Myodam DataGrid non-fatal exception swallowed.");
                return;
            }

            await _exceptionHandler.HandleExceptionAsync(e.Exception);
        }

        private static bool IsAutomationNonCriticalException(DispatcherUnhandledExceptionEventArgs e)
        {
            // see https://stackoverflow.com/questions/16245732/nullreferenceexception-from-presentationframework-dll/16256740#16256740
            var sourceSite = (e.Exception.TargetSite?.DeclaringType?.FullName ?? string.Empty);
            return e.Exception.Source == "PresentationFramework" && sourceSite == "System.Windows.Automation.Peers.DataGridItemAutomationPeer";
        }

    }
}
