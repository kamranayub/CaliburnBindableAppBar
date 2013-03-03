using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using Caliburn.Micro.BindableAppBar;
using Microsoft.Phone.Controls;

namespace CaliburnBindableAppBar {

    public class PivotPageViewModel : SampleConductorBase {}

    public class PanoramaPageViewModel : SampleConductorBase {}

    public class SampleConductorBase : Conductor<Screen>.Collection.OneActive {

        public SampleConductorBase() {
                  
        }

        protected override void OnInitialize() {
            base.OnInitialize();
                                 
            Items.Add(new Item1ViewModel());
            Items.Add(new Item2ViewModel() { ShowAppBar = true });
            Items.Add(new Item2ViewModel());

            ActivateItem(Items[0]);

            // Initialize appbar, the views have been attached by this point
            AppBarConductor.Mixin(this);
        }        
    }

    public class Item1ViewModel : Screen {
        public Item1ViewModel() {
            base.DisplayName = "appbar1";
        }

        public void RefreshData() {
            MessageBox.Show("Refresh");
        }
    }

    public class Item2ViewModel : Screen {
        private bool _showAppBar;

        public Item2ViewModel() {
            base.DisplayName = "appbar2";
        }

        public bool ShowAppBar {
            get { return _showAppBar; }
            set {
                if (value.Equals(_showAppBar)) return;
                _showAppBar = value;
                NotifyOfPropertyChange("ShowAppBar");
            }
        }

        public void Star() {
            MessageBox.Show("Star");
        }
    }
}
