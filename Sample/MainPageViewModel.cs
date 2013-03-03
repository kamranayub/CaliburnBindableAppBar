using System;
using System.Linq;
using System.Windows;
using System.Windows.Interactivity;
using Caliburn.Micro;
using Microsoft.Phone.Controls;

namespace CaliburnBindableAppBar {
    public class MainPageViewModel : Screen {
        private readonly INavigationService _nav;
        private string _addButtonText;
        private bool _showAddButton;

        public MainPageViewModel(INavigationService nav) {
            _nav = nav;
            AddButtonText = "Add";
            ShowAddButton = true;
            _canAdd = true;
            ButtonIconUri = new Uri("/Icons/ApplicationBar.Add.png", UriKind.Relative);

            PinItemText = "Pin to start";
        }

        public string AddButtonText {
            get { return _addButtonText; }
            set {
                if (value == _addButtonText) return;
                _addButtonText = value;
                NotifyOfPropertyChange("AddButtonText");
            }
        }

        public bool ShowAddButton {
            get { return _showAddButton; }
            set {
                if (value.Equals(_showAddButton)) return;
                _showAddButton = value;
                NotifyOfPropertyChange("ShowAddButton");
            }
        }

        public Uri ButtonIconUri {
            get { return _buttonIconUri; }
            set {
                if (Equals(value, _buttonIconUri)) return;
                _buttonIconUri = value;
                NotifyOfPropertyChange("ButtonIconUri");
            }
        }

        private bool _canAdd;
        public bool CanAdd {
            get { return _canAdd; }
        }

        public string PinItemText {
            get { return _pinItemText; }
            set {
                if (value == _pinItemText) return;
                _pinItemText = value;
                NotifyOfPropertyChange("PinItemText");
            }
        }

        #region Actions

        public void ToggleVisibility() {
            ShowAddButton = !ShowAddButton;
        }

        public void ToggleAvailability() {
            _canAdd = !_canAdd;
            NotifyOfPropertyChange(() => CanAdd);
        }

        public void ToggleIcon() {
            if (ButtonIconUri.ToString().IndexOf("ApplicationBar.Add.png") > -1) {
                ButtonIconUri = new Uri("/Icons/ApplicationBar.Pin.png", UriKind.Relative);
            }
            else {
                ButtonIconUri = new Uri("/Icons/ApplicationBar.Add.png", UriKind.Relative);
            }
        }

        private int _inc = 0;
        private Uri _buttonIconUri;
        private string _pinItemText;

        public void ChangeButtonText() {
            AddButtonText = "Add " + _inc++;
        }

        public void Add() {
            MessageBox.Show("Add");
        }

        private bool _pinned;
        public void Pin() {
            if (!_pinned) {
                _pinned = true;
                PinItemText = "Unpin from start";
            }
            else {
                _pinned = false;
                PinItemText = "Pin to start";
            }
        }

        public void Pivot() {
           _nav.UriFor<PivotPageViewModel>().Navigate();
        }

        #endregion
    }
}