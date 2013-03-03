using System.Windows;
using System;
using System.Linq;
using Microsoft.Phone.Controls;

namespace Caliburn.Micro.BindableAppBar {

    /// <summary>
    /// An AppBar conductor. 
    /// Works in sync with a given conductor and hides/shows individual view appbars, if applicable.
    /// </summary>
    public class AppBarConductor : IDeactivate {
        private readonly IConductActiveItem _conductor;
        private readonly PhoneApplicationPage _page;

        /// <summary>
        /// Attaches to a conductor, must occur OnViewReady of the conductor
        /// </summary>
        /// <param name="conductor"></param>
        /// <param name="page"></param>
        public static void AttachTo<T>(T conductor, PhoneApplicationPage page) where T : IConductActiveItem, IDeactivate {

            var abc = new AppBarConductor(conductor, page);

            EventHandler<DeactivationEventArgs> deactivated = null;
            deactivated = (sender, args) =>
            {
                abc.Deactivate(true);
                abc = null;
                conductor.Deactivated -= deactivated;
            };
            conductor.Deactivated += deactivated;
        }

        protected AppBarConductor(IConductActiveItem conductor, PhoneApplicationPage page) {
            _conductor = conductor;
            _page = page;

            // Get available children that may have views
            var items = conductor.GetChildren();

            foreach (var item in items) {

                // Ignore first view, since we want that appbar
                if (item != conductor.ActiveItem) {

                    // Disable app bar for other non-active views
                    // We have to do this, otherwise the appbars
                    // will run their Loaded event and assign themselves
                    // to the page, overwriting the first view's appbar
                    var viewAware = item as IViewAware;

                    if (viewAware != null) {
                        EventHandler<ViewAttachedEventArgs> attachedHandler = null;

                        attachedHandler = (sender, args) =>
                        {
                            var view = args.View as DependencyObject;
                            if (view == null) return;

                            // Get all applicable appbars
                            var appbars = view.GetVisualDescendants().OfType<BindableAppBar>();

                            // Defer initial load of appbars
                            foreach (var appbar in appbars) {                                
                                appbar.DeferLoad = true;
                            }

                            viewAware.ViewAttached -= attachedHandler;
                        };

                        viewAware.ViewAttached += attachedHandler;
                    }
                }
            }

            _conductor.ActivationProcessed += ConductorOnActivationProcessed;
        }

        private void ConductorOnActivationProcessed(object sender, ActivationProcessedEventArgs e) {
            var viewAware = e.Item as IViewAware;

            if (viewAware != null) {
                var view = viewAware.GetView() as DependencyObject;

                if (view != null) {
                    SyncAppBar(view);
                }
            }
        }


        private void SyncAppBar(DependencyObject view) {
            if (view == null || _page == null) return;

            var lastVisibleAppBar = view.GetVisualDescendants()
                .OfType<BindableAppBar>()
                .LastOrDefault(a => a.IsVisible);

            // Show the last appbar that's visible, since having two visible
            // appbars doesn't make sense in a view; but one could be visible
            // and the other invisible (hot swapping!)
            if (lastVisibleAppBar != null) {
                _page.ApplicationBar = lastVisibleAppBar.ApplicationBar;
            }
            else {
                _page.ApplicationBar = null;
            }
        }

        public void Deactivate(bool close) {
            if (_conductor != null) {
                _conductor.ActivationProcessed -= ConductorOnActivationProcessed;
            }
        }

        public event EventHandler<DeactivationEventArgs> AttemptingDeactivation;
        public event EventHandler<DeactivationEventArgs> Deactivated;
    }

}
