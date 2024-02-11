using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JotWin.Model;
using System;
using System.Windows.Media; 

namespace JotWin.ViewModel.Tabs
{
    public class TabVM : ViewModelBase
    {
        public TabData canvasData;

        private string _tabName;
        public string TabName
        {
            get => _tabName;
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && IsNameValid(value))
                {
                    Set(ref _tabName, value);
                }
            }
        }

        private bool _isNotEditing;
        public bool IsNotEditing
        {
            get => _isNotEditing;
            set
            {
                if (_isNotEditing != value)
                {
                    _isNotEditing = value;
                    RaisePropertyChanged(nameof(IsNotEditing));
                }
            }
        }

        private SolidColorBrush _headerColorBG;
        public SolidColorBrush HeaderColorBG
        {
            get => _headerColorBG;
            set
            {
                if (_headerColorBG != value)
                {
                    _headerColorBG = value;
                    RaisePropertyChanged(nameof(HeaderColorBG));
                }
            }
        }

        private SolidColorBrush _headerColorBorder;
        public SolidColorBrush HeaderColorBorder
        {
            get => _headerColorBorder;
            set
            {
                if (_headerColorBorder != value)
                {
                    _headerColorBorder = value;
                    RaisePropertyChanged(nameof(HeaderColorBorder));
                }
            }
        }

        private int _headerBorderThickness = 0;
        public int HeaderBorderThickness
        {
            get => _headerBorderThickness;
            set
            {
                if (_headerBorderThickness != value)
                {
                    _headerBorderThickness = value;
                    RaisePropertyChanged(nameof(HeaderBorderThickness));
                }
            }
        }


        public bool isTabSelected = false;

        private SolidColorBrush _bottomRectColor; 
        public SolidColorBrush BottomRectColor
        {
            get => _bottomRectColor;
            set
            {
                if (_bottomRectColor != value)
                {
                    _bottomRectColor = value;
                    RaisePropertyChanged(nameof(BottomRectColor));
                }
            }
        }

        public RelayCommand EditTabNameCommand { get; }
        public RelayCommand CloseTabCommand { get; }

        private Action<TabVM> _closeAction;

        public TabVM(Action<TabVM> closeAction)
        {
            _closeAction = closeAction;
            EditTabNameCommand = new RelayCommand(ExecuteEditTabNameCommand);
            CloseTabCommand = new RelayCommand(CloseTab);

            HeaderColorBG = new SolidColorBrush(Color.FromRgb(0xF6, 0xF6, 0xF6));
            BottomRectColor = new SolidColorBrush(Color.FromRgb(0xE6, 0xE6, 0xE6));
        }

        private bool IsNameValid(string name)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z0-9_ ]+$");
        }

        private void ExecuteEditTabNameCommand()
        {
           // IsEditing = !IsEditing;
        }

        public void CloseTab()
        {
            _closeAction?.Invoke(this);
        }

        public void HandleLostFocus()
        {
         //   IsEditing = false;
        }

        public void SetBottomRectColor(Color color)
        {
            BottomRectColor = new SolidColorBrush(color);
        }

        public override string ToString()
        {
            return "";
        }
    }
}
