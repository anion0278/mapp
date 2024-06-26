﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Autofac;
using Mapp.ApplicationUpdater;
using Mapp.BusinessLogic.AutoComplete;
using Mapp.DataAccess;
using Mapp.UI.Exception;
using Mapp.UI.Localization;
using Mapp.UI.Startup;
using Mapp.UI.Views;
using NLog;
using WPFLocalizeExtension.Engine;

namespace Mapp.UI
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


    public partial class App : System.Windows.Application
    {
        private IGlobalExceptionHandler _exceptionHandler = null!;

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IApplicationUpdater _appUpdater;
        private IAutoKeyboardInputHelper _keyboardInputHelper;

        // TODO project-wide System.OverflowException check !!!

        protected void Application_Startup(object sender, StartupEventArgs e)
        {
            //ResxLocalizationProvider.SetDefaultAssembly("Mapp.UI"); // TODO use https://stackoverflow.com/questions/32612045/is-there-any-way-to-assign-default-values-globally-for-a-localization-extension
            //ResxLocalizationProvider.DefaultDictionaryProperty = "LocalizationStrings";

            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
            LocalizeDictionary.Instance.SetCultureCommand.Execute(Constants.NeutralDefaultLanguageName);

            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.ConfigureContainer();
            _exceptionHandler = container.Resolve<IGlobalExceptionHandler>();

            _exceptionHandler = container.Resolve<IGlobalExceptionHandler>();
            _keyboardInputHelper = container.Resolve<IAutoKeyboardInputHelper>(); // SHOULD BE HERE, otherwise will not get instantiated
            _appUpdater = container.Resolve<IApplicationUpdater>();
            _appUpdater.CheckUpdate();


            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // TODO avoid using
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            var mainWindow = container.Resolve<MainWindow>();
            mainWindow.Show();

#if !DEBUG
            //https://github.com/dotnet/winforms/blob/72c140e531729b58737bb7b84212ff96767a151d/src/System.Windows.Forms/src/System/Windows/Forms/Application.cs#L952
            //AppCenter.Start("9549dd3a-1371-4a23-b973-f5e80154119d", typeof(Analytics), typeof(Crashes)); // TODO should solve secrt storing somehow :(
#endif
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
