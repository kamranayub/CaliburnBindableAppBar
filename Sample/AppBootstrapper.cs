using System.Diagnostics;
using Caliburn.Micro.BindableAppBar;

namespace CaliburnBindableAppBar {
	using System;
	using System.Collections.Generic;
	using System.Windows.Controls;
	using Microsoft.Phone.Controls;
	using Caliburn.Micro;

	public class AppBootstrapper : PhoneBootstrapperBase
	{
		PhoneContainer _container;

		public AppBootstrapper()
		{
			Initialize();
		}

		protected override void Configure()
		{
			_container = new PhoneContainer();
			if (!Execute.InDesignMode)
				_container.RegisterPhoneServices(RootFrame);

            _container.PerRequest<MainPageViewModel>();
            _container.PerRequest<PivotPageViewModel>();
            _container.PerRequest<PanoramaPageViewModel>();
            _container.PerRequest<Item1ViewModel>();
            _container.PerRequest<Item2ViewModel>();
            _container.PerRequest<Item3ViewModel>();
		    _container.PerRequest<Item4ViewModel>();

			AddCustomConventions();

            if (Debugger.IsAttached)
            {
                LogManager.GetLog = type => new DebugLogger(type);
            }
		}

		protected override object GetInstance(Type service, string key)
		{
			var instance = _container.GetInstance(service, key);
			if (instance != null)
				return instance;

			throw new InvalidOperationException("Could not locate any instances.");
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

    public class DebugLogger : ILog
    {
        private readonly Type _type;
        private const string DateFormat = "hh:mm:ss.ffff";

        public DebugLogger(Type type)
        {
            _type = type;
        }

        public void Info(string format, params object[] args)
        {

            Debug.WriteLine(String.Format("{0} INFO [Thread:{2}][{1}] - ", DateTime.Now.ToString(DateFormat), _type.Name, System.Threading.Thread.CurrentThread.ManagedThreadId) + format, args);

        }

        public void Warn(string format, params object[] args)
        {

            Debug.WriteLine(String.Format("{0} WARN [Thread:{2}][{1}] - ", DateTime.Now.ToString(DateFormat), _type.Name, System.Threading.Thread.CurrentThread.ManagedThreadId) + format, args);

        }

        public void Error(Exception exception)
        {

            Debug.WriteLine(String.Format("{0} ERROR [Thread:{2}][{1}] - ", DateTime.Now.ToString(DateFormat), _type.Name, System.Threading.Thread.CurrentThread.ManagedThreadId) + exception.Message);
            Debug.WriteLine(exception.StackTrace);

        }
    }
}