using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Caliburn.Micro.BindableAppBar {

    public class BindableAppBarButton : Control, IApplicationBarIconButton {

        //public int Index { get; set; }

        #region IconUri DependencyProperty

        public Uri IconUri {
            get { return (Uri)GetValue(IconUriProperty); }
            set { SetValue(IconUriProperty, value); }
        }

        public static readonly DependencyProperty IconUriProperty =
            DependencyProperty.RegisterAttached("IconUri", typeof(Uri), typeof(BindableAppBarButton), new PropertyMetadata(OnIconUriChanged));

        private static void OnIconUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != e.OldValue) {
                var btn = ((BindableAppBarButton) d);
                var uri = e.NewValue as Uri;
                if (uri != null) {
                    btn.Button.IconUri = uri;
                }
                else if (e.NewValue != null) {
                    btn.Button.IconUri = new Uri(e.NewValue.ToString(), UriKind.RelativeOrAbsolute);
                }
                else {
                    btn.Button.IconUri = null;
                }
            }
        }

        #endregion

        #region Text DependencyProperty

        public string Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached("Text", typeof(string), typeof(BindableAppBarButton), new PropertyMetadata(OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != e.OldValue) {
                ((BindableAppBarButton)d).Button.Text = e.NewValue.ToString();
            }
        }

        #endregion

        #region Visibility DependencyProperty

        public new Visibility Visibility {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }

        public new static readonly DependencyProperty VisibilityProperty =
            DependencyProperty.RegisterAttached("Visibility", typeof(Visibility), typeof(BindableAppBarButton), new PropertyMetadata(OnVisibilityChanged));

        private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != e.OldValue) {
                var button = ((BindableAppBarButton)d);
                BindableAppBar bar = button.Parent as BindableAppBar;

                bar.Invalidate();
            }
        }

        #endregion

        public AppBarButton Button { get; set; }

        public event EventHandler Click; 

        public BindableAppBarButton() {
            Button = new AppBarButton();
            Button.Text = "Text";
            Button.IconUri = new Uri("/", UriKind.RelativeOrAbsolute);
            Button.Click += AppBarButtonClick;

            // Handle change event because Caliburn calls base Control.IsEnabled,
            // so `new` props don't get called
            IsEnabledChanged += OnIsEnabledChanged;
        }

        void AppBarButtonClick(object sender, EventArgs e) {
            if (Click != null)
                Click(this, e);
        }              

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != e.OldValue) {
                ((BindableAppBarButton)sender).Button.IsEnabled = (bool)e.NewValue;
            }
        }
    }

    public class BindableAppBarMenuItem : Control, IApplicationBarMenuItem {

        #region Text DependencyProperty

        public string Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached("Text", typeof(string), typeof(BindableAppBarMenuItem), new PropertyMetadata(OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != e.OldValue) {
                ((BindableAppBarMenuItem)d).MenuItem.Text = e.NewValue.ToString();
            }
        }

        #endregion

        #region Visibility DependencyProperty

        public new Visibility Visibility {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }

        public new static readonly DependencyProperty VisibilityProperty =
            DependencyProperty.RegisterAttached("Visibility", typeof(Visibility), typeof(BindableAppBarMenuItem), new PropertyMetadata(OnVisibilityChanged));

        private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != e.OldValue) {
                var button = ((BindableAppBarMenuItem)d);
                BindableAppBar bar = button.Parent as BindableAppBar;

                bar.Invalidate();
            }
        }

        #endregion

        public AppBarMenuItem MenuItem { get; set; }

        public event EventHandler Click;

        public BindableAppBarMenuItem() {
            MenuItem = new AppBarMenuItem();
            MenuItem.Text = "Text";
            MenuItem.Click += AppBarMenuItemClick;

            // Handle change event because Caliburn calls base Control.IsEnabled,
            // so `new` props don't get called
            IsEnabledChanged += OnIsEnabledChanged;
        }

        private void OnIsEnabledChanged(object s, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != e.OldValue) {
                ((BindableAppBarMenuItem)s).MenuItem.IsEnabled = (bool)e.NewValue;
            }
        }

        void AppBarMenuItemClick(object sender, EventArgs e) {
            if (Click != null)
                Click(this, e);
        }            

    }
    
    [ContentProperty("Buttons")]
    public class BindableAppBar : ItemsControl, IApplicationBar {
        // ApplicationBar wrapper
        internal readonly ApplicationBar ApplicationBar;

        public BindableAppBar() {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.StateChanged += ApplicationBarStateChanged;

            Loaded += BindableApplicationBarLoaded;
        }

        void ApplicationBarStateChanged(object sender, ApplicationBarStateChangedEventArgs e) {
            if (StateChanged != null)
                StateChanged(this, e);
        }

        void BindableApplicationBarLoaded(object sender, RoutedEventArgs e) {
            // Get the page
            var page = this.GetVisualAncestors().OfType<PhoneApplicationPage>().FirstOrDefault();

            // If we're not defer-loading, assign the appbar
            if (page != null && !DeferLoad) 
                page.ApplicationBar = ApplicationBar;
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            base.OnItemsChanged(e);
            Invalidate();
        }

        public void Invalidate() {
            // Clear current buttons
            ApplicationBar.Buttons.Clear();
            ApplicationBar.MenuItems.Clear();

            // TODO: Use Index prop to reorder them?
            foreach (BindableAppBarButton button in Items.Where(c => c is BindableAppBarButton && ((BindableAppBarButton)c).Visibility == Visibility.Visible)) {
                ApplicationBar.Buttons.Add(button.Button);
            }
            foreach (BindableAppBarMenuItem button in Items.Where(c => c is BindableAppBarMenuItem && ((BindableAppBarMenuItem)c).Visibility == Visibility.Visible)) {
                ApplicationBar.MenuItems.Add(button.MenuItem);
            }
        }

        #region IsVisible DependencyProperty

        public bool IsVisible {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.RegisterAttached("IsVisible", typeof(bool), typeof(BindableAppBar), new PropertyMetadata(true, OnVisibleChanged));

        private static void OnVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != e.OldValue) {
                ((BindableAppBar)d).ApplicationBar.IsVisible = (bool)e.NewValue;
            }
        }

        #endregion

        #region Mode DependencyProperty

        public static readonly DependencyProperty ModeProperty =
          DependencyProperty.RegisterAttached("Mode", typeof(ApplicationBarMode), typeof(BindableAppBar), new PropertyMetadata(ApplicationBarMode.Default, OnModeChanged));

        private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.NewValue != e.OldValue) {
                ((BindableAppBar)d).ApplicationBar.Mode = (ApplicationBarMode)e.NewValue;
            }
        }

        public ApplicationBarMode Mode {
            get { return (ApplicationBarMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        #endregion

        /// <summary>
        /// Whether or not to defer loading, e.g. during Pivot/Panorama where there could be multiple appbars declared
        /// </summary>
        public bool DeferLoad { get; set; }

        public double DefaultSize {
            get { return ApplicationBar.DefaultSize; }
        }

        public double MiniSize {
            get { return ApplicationBar.MiniSize; }
        }

        public double BarOpacity {
            get { return ApplicationBar.Opacity; }
            set { ApplicationBar.Opacity = value; }
        }

        public bool IsMenuEnabled {
            get { return ApplicationBar.IsMenuEnabled; }
            set { ApplicationBar.IsMenuEnabled = true; }
        }

        public Color BackgroundColor {
            get { return ApplicationBar.BackgroundColor; }
            set { ApplicationBar.BackgroundColor = value; }
        }

        public Color ForegroundColor {
            get { return ApplicationBar.ForegroundColor; }
            set { ApplicationBar.ForegroundColor = value; }
        }

        public IList Buttons {
            get { return Items; }

        }

        public IList MenuItems {
            get { return Items; }
        }

        public event EventHandler<ApplicationBarStateChangedEventArgs> StateChanged;
    }
}
