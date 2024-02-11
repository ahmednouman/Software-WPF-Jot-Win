using JotWin.ViewModel.Helpers.SaveLoad;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JotWin.View
{
    /// <summary>
    /// Interaction logic for saveTab.xaml
    /// </summary>
    public partial class saveTab : Window
    {
        public MainAppWindow mainWin = null;

        private string _title;

        public string tabTitle
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged(nameof(tabTitle)); 
               saveBtn.IsEnabled = !string.IsNullOrWhiteSpace(tabTitle);
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateSaveButtonState()
        {
            saveBtn.IsEnabled = !string.IsNullOrWhiteSpace(Title);
        }
        public saveTab(MainAppWindow main_win)
        {
            InitializeComponent();
            mainWin = main_win;
            this.Owner = main_win;
            DataContext = this;

            saveBtn.IsEnabled = false;
            tabTitle = mainWin.selectedTab.TabName;
        }

        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            mainWin.selectedTab.TabName = tabTitle;
            SaveLoad.saveCanvasToFile(mainWin);

            this.Close();
        }

        private void txtSaveInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(tabTitle))
            {
                mainWin.selectedTab.TabName = tabTitle;
                SaveLoad.saveCanvasToFile(mainWin);

                this.Close();
            }
        }

    }
}
