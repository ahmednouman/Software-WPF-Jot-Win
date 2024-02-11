using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using JotWin.View;
using JotWin.ViewModel.Tabs;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace JotWin.ViewModel
{
    public class MainTabsVM : ViewModelBase, INotifyPropertyChanged
    {
        public SolidColorBrush unSelectedHeaderBG = new(Color.FromRgb(0xF6, 0xF6, 0xF6));
        public SolidColorBrush selectedHeaderBG = new(Color.FromRgb(0xFA, 0xFA, 0xFA));

        public SolidColorBrush unSelectedBottomBorder = new(Color.FromRgb(0xE6, 0xE6, 0xE6));
        public SolidColorBrush selectedBottomBorder = new(Colors.Transparent);

        public SolidColorBrush editingHeaderBorder = new(Color.FromRgb(0x00, 0x0, 0x00));
        public SolidColorBrush normalHeaderBorder = new(Colors.Transparent);

        private readonly MainAppWindow mainWindow;

        private int tabNameCount = 1;

        private ObservableCollection<TabVM> _tabs = new();
        private int _tabWidth;
        private int _tabMaxWidth;
        private int _selectedTab = 0;

        public ObservableCollection<TabVM> Tabs
        {
            get => _tabs;
            set
            {
                if (_tabs != value)
                {
                    _tabs = value;
                    RaisePropertyChanged(nameof(Tabs));
                }
            }
        }

        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (_selectedTab != value)
                {
                    _selectedTab = value;
                    RaisePropertyChanged(nameof(SelectedTab));
                }
            }
        }

        public int TabWidth
        {
            get => _tabWidth;
            set
            {
                if (_tabWidth != value)
                {
                    _tabWidth = value;
                    RaisePropertyChanged(nameof(TabWidth));
                }
            }
        }

        public int TabMaxWidth
        {
            get => _tabMaxWidth;
            set
            {
                if (_tabMaxWidth != value)
                {
                    _tabMaxWidth = value;
                    RaisePropertyChanged(nameof(TabMaxWidth));
                }
            }
        }

        public ICommand AddTabCommand { get; }
        public RelayCommand<TabVM> TabSelectionChangedCommand { get; }

        public MainTabsVM()
        {
            mainWindow = Application.Current.Windows.OfType<MainAppWindow>().FirstOrDefault()
                         ?? throw new System.Exception("MainTabsVM failed to find main app window");

            TabMaxWidth = 250;
            TabWidth = 250;

            AddTabCommand = new RelayCommand(AddTab);
            TabSelectionChangedCommand = new RelayCommand<TabVM>(TabSelectionChanged);

            var firstTab = new TabVM(CloseTab)
            {
                TabName = "Jot " + tabNameCount.ToString(),
                IsNotEditing = true,
                isTabSelected = false,
                HeaderColorBG = selectedHeaderBG,
                HeaderColorBorder = normalHeaderBorder,
                BottomRectColor = selectedBottomBorder
            };

            Tabs.Add(firstTab);
            TabHelpers.addNewTabData(firstTab, mainWindow);

            tabNameCount++;
        }

        public void AddTab()
        {
            var newTab = new TabVM(CloseTab)
            {
                TabName = "Jot " + (tabNameCount).ToString(),
                HeaderColorBorder = normalHeaderBorder,
                IsNotEditing = true
            };

            maintainTabCount();

            updateTabWidth();

            Tabs.Add(newTab);
            TabHelpers.addNewTabData(newTab, mainWindow);

            SelectedTab = Tabs.Count - 1;
            tabNameCount++;
        }

        private void maintainTabCount()
        {
            int tabsCount = Tabs.Count;
            if (tabsCount == 10)
            {
                Tabs.RemoveAt(0);
                mainWindow.tabDataList.RemoveAt(0);
            }
        }

        private void updateTabWidth()
        {
            int tabsCount = Tabs.Count + 1;

            double tab_grid_width = mainWindow.tabGrid.ColumnDefinitions[0].ActualWidth;
            tab_grid_width -= mainWindow.addTabBtn.Width;

            if ((tabsCount * TabMaxWidth) >= tab_grid_width)
            {
                TabWidth = (int)((tab_grid_width / tabsCount) - 1);
            }
            else
            {
                TabWidth = TabMaxWidth;
            }
        }

        public void CloseTab(TabVM tabViewModel)
        {
            if (Tabs.Count > 1)
            {
                mainWindow.tabDataList.Remove(tabViewModel.canvasData);
                Tabs.Remove(tabViewModel);
                updateTabWidth();
            }
        }

        public void updateTabSelected(TabVM selected_tab)
        {
            foreach (var tab in Tabs)
            {
                if (tab == selected_tab)
                {
                    tab.isTabSelected = true;
                    tab.HeaderColorBG = selectedHeaderBG;
                    tab.BottomRectColor = selectedBottomBorder;
                    tab.HeaderColorBorder = editingHeaderBorder;
                    tab.HeaderBorderThickness = 0;
                }
                else
                {
                    tab.isTabSelected = false;
                    tab.HeaderColorBG = unSelectedHeaderBG;
                    tab.BottomRectColor = unSelectedBottomBorder;
                    tab.HeaderColorBorder = normalHeaderBorder;
                    tab.HeaderBorderThickness = 0;
                }
            }
        }

        public void selectTabByObject(TabVM selected_tab)
        {
            int tabIndex = Tabs.IndexOf(selected_tab);
            SelectedTab = tabIndex;
        }

        public void tabEditReset()
        {
            foreach (var tab in Tabs)
            {
                tab.IsNotEditing = true;
                tab.HeaderColorBorder = normalHeaderBorder;
                tab.HeaderBorderThickness = 0;
            }
        }


        private void TabSelectionChanged(TabVM selectedTab)
        {

        }

        #region INotifyPropertyChanged

        new public event PropertyChangedEventHandler? PropertyChanged;

        override public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
