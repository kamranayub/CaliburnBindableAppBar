Caliburn Micro Bindable App Bar Extension
-----------------------------------------
More Information: http://github.com/kamranayub/CaliburnBindableAppBar

Once installed, you can wireup the default `Click` convention in your bootstrapper:

	static void AddCustomConventions()
    {

        // App Bar Conventions
        ConventionManager.AddElementConvention<BindableAppBarButton>(
            Control.IsEnabledProperty, "DataContext", "Click");
        ConventionManager.AddElementConvention<BindableAppBarMenuItem>(
            Control.IsEnabledProperty, "DataContext", "Click");

		// ... the rest of your conventions
	}