using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Shmap.ViewModels;
using Shmap.Views;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using NLog;

namespace Mapp
{
    public class BindingException: Exception
    {
        public BindingException(string message) : base(message)
        { }

        public BindingException(string message, Exception innerException) : base(message, innerException)
        {}
    }

    public class ExplicitBindingErrorTraceListener : TraceListener
    {
        public EventHandler<Exception> BindingErrorEventHandler { get; set; }

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
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        // TODO project-wide System.OverflowException check !!!

        protected override void OnStartup(StartupEventArgs e)
        {
            //var x = new[] { 1, 2, 3, };
            //x.ToImmutableArray().
            //ImmutableArrayExtensions.
            //x[1] = 8;

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

            base.OnStartup(e);
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

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private void LogException(Exception originalException, string source)
        {
            var exception = originalException;
            string message = $"Unhandled exception with source: {source}. ";
            try
            {
#if !DEBUG
                //Crashes.TrackError(exception);
#endif
                var assemblyName = Assembly.GetExecutingAssembly().GetName();
                message += $"Unhandled exception in {assemblyName.Name} v{assemblyName.Version}";
                message += $"\n * Top-most message: {exception.Message} \n * stack: {exception.StackTrace}";
                while (exception.InnerException != null) 
                {
                    exception = exception.InnerException;
                    message += $"\n * Inner message: {exception.Message} \n * stack: {exception.StackTrace}";
                } 
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception in LogUnhandledException");
            }
            finally
            {
                _logger.Error(originalException, message);
                MessageBox.Show(message);
            }
        }
    }
}
