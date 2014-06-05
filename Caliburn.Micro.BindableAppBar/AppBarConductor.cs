using System.Threading;
using System.Windows;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace Caliburn.Micro.BindableAppBar {

    /// <summary>
    /// An AppBar conductor. 
    /// Works in sync with a given conductor and hides/shows individual view appbars, if applicable.
    /// </summary>
    public class AppBarConductor : IDeactivate {
        private readonly static ILog Log = LogManager.GetLog(typeof (AppBarConductor));

        private readonly IConductActiveItem _conductor;
        private readonly PhoneApplicationPage _page;
        private readonly Pivot _pivot;
        private readonly Panorama _panorama;
        private const int DefaultPanoramaWaitThreshold = 800;

        /// <summary>
        /// Default wait time for Panorama animation
        /// </summary>
        public int PanoramaWaitThreshold = DefaultPanoramaWaitThreshold;

        /// <summary>
        /// Attaches to a conductor, must occur OnViewReady of the conductor
        /// </summary>
        /// <param name="conductor"></param>        
        /// <param name="panoramaWaitThreshold"></param>
        public static void Mixin<T>(T conductor, int panoramaWaitThreshold = DefaultPanoramaWaitThreshold) where T : IConductActiveItem, IDeactivate, IViewAware {

            var abc = new AppBarConductor(conductor, conductor.GetView() as PhoneApplicationPage);

            // Assign threshold value
            abc.PanoramaWaitThreshold = panoramaWaitThreshold;

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

            _pivot = _page.GetVisualDescendants().OfType<Pivot>().FirstOrDefault();
            _panorama = _page.GetVisualDescendants().OfType<Panorama>().FirstOrDefault();

            if (_pivot == null && _panorama == null) {
                throw new ArgumentException("The appbar conductor must have a Pivot or Panorama to sync with.");
            }

            if (_pivot != null)
                _pivot.LoadedPivotItem += PivotOnLoadedPivotItem;

            if (_panorama != null) {
                _panorama.SelectionChanged += PanoramaOnSelectionChanged;

                // Load first appbar in Panorama after animations are complete
                var firstViewAware = conductor.ActiveItem as IViewAware;

                if (firstViewAware != null) {

                    EventHandler<ViewAttachedEventArgs> attachedHandler = null;

                    attachedHandler = (sender, args) =>
                    {
                        SyncAppBar(args.View as DependencyObject);

                        firstViewAware.ViewAttached -= attachedHandler;
                    };

                    firstViewAware.ViewAttached += attachedHandler;
                }
            }

            HandleDeferLoad();
        }

        private void HandleDeferLoad()
        {
            // Get available children that may have views
            var items = _conductor.GetChildren();

            // If we're in a Panorama, we defer all appbars to prevent
            // transition muckup
            bool panoramaActive = (_panorama != null);

            foreach (var item in items)
            {
                // Disable app bar for other non-active views
                // We have to do this, otherwise the appbars
                // will run their Loaded event and assign themselves
                // to the page, overwriting the first view's appbar
                var viewAware = item as IViewAware;

                if (viewAware != null)
                {
                    var view = viewAware.GetView() as DependencyObject;
                    bool deferLoad = ((item != _conductor.ActiveItem) || panoramaActive);
                    if (view != null)
                    {
                        SetDeferLoad(view, deferLoad);
                    }
                    else
                    {

                        EventHandler<ViewAttachedEventArgs> attachedHandler = null;

                        attachedHandler = (sender, args) =>
                        {
                            SetDeferLoad(args.View as DependencyObject, deferLoad);

                            viewAware.ViewAttached -= attachedHandler;
                        };

                        viewAware.ViewAttached += attachedHandler;
                    }
                }
            }
        }


        private void SetDeferLoad(DependencyObject view, bool deferLoad)
        {

            if (view == null) return;

            // Get all applicable appbars
            var appbars = view.GetVisualDescendants().OfType<BindableAppBar>();

            // Defer initial load of appbars
            foreach (var appbar in appbars)
            {
                appbar.DeferLoad = deferLoad;

                Log.Info("Marked appbar as {0}defered, ElementName={1}", (deferLoad ? String.Empty : "not "), appbar.Name);
            }
        }

        private void PanoramaOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs) {
            var panoramaItem = _panorama.SelectedItem as PanoramaItem;
            if (panoramaItem != null) {
                SyncAppBar(_panorama.SelectedItem as DependencyObject);
                return;
            }

            var viewAware = _panorama.SelectedItem as IViewAware;

            if (viewAware != null) {
                SyncAppBar(viewAware.GetView() as DependencyObject);
            }
        }

        private void PivotOnLoadedPivotItem(object sender, PivotItemEventArgs e) {
            SyncAppBar(e.Item);
        }

        // Flag to prevent assigning appbar too soon (causing animation to abort)
        private bool _waiting;

        private void SyncAppBar(DependencyObject view) {
            if (view == null || _page == null) return;

            // Reset waiting flag
            if (_waiting)
                _waiting = false;

            // Get the last visible BindableAppBar in the tree            
            var lastVisibleAppBar = view.GetVisualDescendants()
                .OfType<BindableAppBar>()
                .LastOrDefault(a => a.IsVisible);

            // Show the last appbar that's visible, since having two visible
            // appbars doesn't make sense in a view; but one could be visible
            // and the other invisible (hot swapping!)
            if (lastVisibleAppBar != null) {

                System.Action updateAppBar = () =>
                {
                    // Refresh button state/bg color if the appbar was "hidden"
                    lastVisibleAppBar.Invalidate();

                    // Assign the bar
                    _page.ApplicationBar = lastVisibleAppBar.ApplicationBar;
                };


                // In Panorama, changing appbar visibility causes the transition to stop
                // so we handle if there was no appbar previously shown
                if (_page.ApplicationBar == null && _panorama != null) {

                    _waiting = true;

                    // Animations take ~500ms
                    ThreadPool.QueueUserWorkItem(c =>
                    {
                        Thread.Sleep(PanoramaWaitThreshold);

                        // Are we still waiting on this?
                        if (_waiting)
                            updateAppBar.OnUIThread();
                    });
                } else {

                    // Update it
                    updateAppBar();

                }

            } else {

                // In Panorama, changing appbar visibility causes the transition to stop
                if (_panorama != null) {

                    // These properties are safe to change and do not cause the transition
                    // to abort
                    if (_page.ApplicationBar != null) {
                        _page.ApplicationBar.Buttons.Clear();
                        _page.ApplicationBar.MenuItems.Clear();
                        _page.ApplicationBar.BackgroundColor = Color.FromArgb(1, 0, 0, 0);
                    }

                } else {

                    _page.ApplicationBar = null;

                }

            }
        }

        public void Deactivate(bool close) {
            if (_panorama != null)
                _panorama.SelectionChanged -= PanoramaOnSelectionChanged;

            if (_pivot != null)
                _pivot.LoadedPivotItem -= PivotOnLoadedPivotItem;
        }

        public event EventHandler<DeactivationEventArgs> AttemptingDeactivation;
        public event EventHandler<DeactivationEventArgs> Deactivated;
    }

}
