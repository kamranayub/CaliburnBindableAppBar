using System.Diagnostics;
using System.Windows;
using Caliburn.Micro;
using Caliburn.Micro.BindableAppBar;

namespace CaliburnBindableAppBar {
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using Microsoft.Phone.Controls;
    using Caliburn.Micro;

    public class AppBootstrapper : Phone8Bootstrapper
    {
        PhoneContainer _container;

        protected override void Configure()
        {
            _container = new PhoneContainer(RootFrame);

			_container.RegisterPhoneServices();
            _container.PerRequest<MainPageViewModel>();
            _container.PerRequest<PivotPageViewModel>();
            _container.PerRequest<PanoramaPageViewModel>();
            _container.PerRequest<Item1ViewModel>();
            _container.PerRequest<Item2ViewModel>();
            _container.PerRequest<Item3ViewModel>();

            AddCustomConventions();

            if (Debugger.IsAttached) {
                LogManager.GetLog = type => new DebugLogger(type);
            }
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        static void AddCustomConventions()
        {

            // App Bar Conventions
            ConventionManager.AddElementConvention<BindableAppBarButton>(
                Control.IsEnabledProperty, "DataContext", "Click");
            ConventionManager.AddElementConvention<BindableAppBarMenuItem>(
                Control.IsEnabledProperty, "DataContext", "Click");
            
            // Other conventions
            ConventionManager.AddElementConvention<Pivot>(Pivot.ItemsSourceProperty, "SelectedItem", "SelectionChanged").ApplyBinding =
                (viewModelType, path, property, element, convention) => {
                    if (ConventionManager
                        .GetElementConvention(typeof(ItemsControl))
                        .ApplyBinding(viewModelType, path, property, element, convention))
                    {
                        ConventionManager
                            .ConfigureSelectedItem(element, Pivot.SelectedItemProperty, viewModelType, path);
                        ConventionManager
                            .ApplyHeaderTemplate(element, Pivot.HeaderTemplateProperty, null, viewModelType);
                        return true;
                    }

                    return false;
                };

            ConventionManager.AddElementConvention<Panorama>(Panorama.ItemsSourceProperty, "SelectedItem", "SelectionChanged").ApplyBinding =
                (viewModelType, path, property, element, convention) => {
                    if (ConventionManager
                        .GetElementConvention(typeof(ItemsControl))
                        .ApplyBinding(viewModelType, path, property, element, convention))
                    {
                        ConventionManager
                            .ConfigureSelectedItem(element, Panorama.SelectedItemProperty, viewModelType, path);
                        ConventionManager
                            .ApplyHeaderTemplate(element, Panorama.HeaderTemplateProperty, null, viewModelType);
                        return true;
                    }

                    return false;
                };
        }
    }

    public class DebugLogger : ILog {
        private readonly Type _type;
        private const string DateFormat = "hh:mm:ss.ffff";

        public DebugLogger(Type type) {
            _type = type;
        }

        public void Info(string format, params object[] args) {

            Debug.WriteLine(String.Format("{0} INFO [Thread:{2}][{1}] - ", DateTime.Now.ToString(DateFormat), _type.Name, System.Threading.Thread.CurrentThread.ManagedThreadId) + format, args);

        }

        public void Warn(string format, params object[] args) {

            Debug.WriteLine(String.Format("{0} WARN [Thread:{2}][{1}] - ", DateTime.Now.ToString(DateFormat), _type.Name, System.Threading.Thread.CurrentThread.ManagedThreadId) + format, args);

        }

        public void Error(Exception exception) {

            Debug.WriteLine(String.Format("{0} ERROR [Thread:{2}][{1}] - ", DateTime.Now.ToString(DateFormat), _type.Name, System.Threading.Thread.CurrentThread.ManagedThreadId) + exception.Message);
            Debug.WriteLine(exception.StackTrace);

        }
    }
}

namespace Caliburn.Micro {
    using System.Windows.Navigation;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;

    /// <summary>
    /// A custom bootstrapper designed to setup phone applications.
    /// </summary>
    public class Phone8Bootstrapper : Bootstrapper {
        bool phoneApplicationInitialized;
        PhoneApplicationService phoneService;

        /// <summary>
        /// The root frame used for navigation.
        /// </summary>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Provides an opportunity to hook into the application object.
        /// </summary>
        protected override void PrepareApplication() {
            base.PrepareApplication();

            phoneService = new PhoneApplicationService();
            phoneService.Activated += OnActivate;
            phoneService.Deactivated += OnDeactivate;
            phoneService.Launching += OnLaunch;
            phoneService.Closing += OnClose;

            Application.UnhandledException += OnUnhandledException;
            Application.ApplicationLifetimeObjects.Add(phoneService);

            if (phoneApplicationInitialized) {
                return;
            }

            RootFrame = CreatePhoneApplicationFrame();
            RootFrame.Navigated += OnNavigated;
            RootFrame.NavigationFailed += OnNavigationFailed;

            phoneApplicationInitialized = true;
        }

        void OnNavigated(object sender, NavigationEventArgs e) {
            if (Application.RootVisual != RootFrame) {
                Application.RootVisual = RootFrame;
            }

            RootFrame.Navigated -= OnNavigated;
        }

        // Code to execute if a navigation fails
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e) {
            if (System.Diagnostics.Debugger.IsAttached) {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        protected override void OnUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e) {
            if (System.Diagnostics.Debugger.IsAttached) {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }

        }

        /// <summary>
        /// Creates the root frame used by the application.
        /// </summary>
        /// <returns>The frame.</returns>
        protected virtual PhoneApplicationFrame CreatePhoneApplicationFrame() {
            return new PhoneApplicationFrame();
        }

        /// <summary>
        /// Occurs when a fresh instance of the application is launching.
        /// </summary>
        protected virtual void OnLaunch(object sender, LaunchingEventArgs e) {

        }

        /// <summary>
        /// Occurs when a previously tombstoned or paused application is resurrected/resumed.
        /// </summary>
        protected virtual void OnActivate(object sender, ActivatedEventArgs e) {

        }

        /// <summary>
        /// Occurs when the application is being tombstoned or paused.
        /// </summary>
        protected virtual void OnDeactivate(object sender, DeactivatedEventArgs e) {

        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        protected virtual void OnClose(object sender, ClosingEventArgs e) {

        }
    }
}