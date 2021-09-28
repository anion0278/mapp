using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Shmap.ViewModels;
using Shmap.Views;
using NLog;

namespace Mapp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetupExceptionHandling();

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        private void LogUnhandledException(Exception originalException, string source)
        {
            var exception = originalException;
            string message = $"Unhandled exception with source: {source}.";
            try
            {
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
