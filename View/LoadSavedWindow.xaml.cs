using JotWin.ViewModel.Helpers.SaveLoad;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace JotWin.View
{
    public class ExtendedViewList
    {
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? ImagePath { get; set; }
        public BitmapImage? ImageSrc { get; set; }
    }

    public partial class LoadSavedWindow : Window
    {
        public MainAppWindow mainWin;
        public ObservableCollection<ExtendedViewList> savedList = new();
        public List<savedTab> rcvd_savedList = new();

        public LoadSavedWindow(List<savedTab> tab_list, MainAppWindow mainWin)
        {
            InitializeComponent();
            this.mainWin = mainWin;
            Owner = mainWin;

            rcvd_savedList = tab_list;

            // Handle the Loaded event to ensure the visual tree is ready
            Loaded += (sender, e) =>
            {
                setup_list(tab_list);
            };

            savedListView.SelectionChanged += ScreenshotsListBox_SelectionChanged;
        }

        public void setup_list(List<savedTab> tab_list)
        {
            savedList = new ObservableCollection<ExtendedViewList>();
            foreach (savedTab tab in tab_list)
            {
                ExtendedViewList extended_view = new()
                {
                    Name = tab.Name,
                    Path = tab.Path,
                    ImagePath = tab.ImagePath,
                };

                BitmapImage imgTemp = new();
                imgTemp.BeginInit();
                imgTemp.CacheOption = BitmapCacheOption.OnLoad;
                imgTemp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                imgTemp.UriSource = new Uri(tab.ImagePath);
                imgTemp.EndInit();

                extended_view.ImageSrc = imgTemp;

                savedList.Add(extended_view);
            }

            savedListView.ItemsSource = savedList;
        }

        private void ScreenshotsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (savedListView.SelectedItem == null)
            {
                return;
            }
            int selectedIndex = savedListView.SelectedIndex;

            if (selectedIndex >= 0 && selectedIndex < savedList.Count)
            {
                MainAppWindow mainAppWindow = (MainAppWindow)Owner;
                savedTab? correspondingSavedTab = rcvd_savedList.FirstOrDefault(tab => tab.Name == savedList[selectedIndex].Name);
                if (correspondingSavedTab == null)
                {
                    Debug.WriteLine("ScreenshotsListBox_SelectionChanged correspondingSavedTab == null");
                    return;
                }
                SaveLoad.openCanvasFile(mainAppWindow, correspondingSavedTab);
                Close();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button deleteButton = (Button)sender;

            if (deleteButton.DataContext is ExtendedViewList item)
            {
                savedTab? correspondingSavedTab = rcvd_savedList.FirstOrDefault(tab => tab.Name == item.Name);
                if (correspondingSavedTab == null)
                {
                    Debug.WriteLine("DeleteButton_Click correspondingSavedTab == null");
                    return;
                }
                savedList.Remove(item);
                SaveLoad.deleteTabsDB(mainWin, correspondingSavedTab);
                delete_saved(correspondingSavedTab);
                rcvd_savedList.Remove(correspondingSavedTab);
            }
        }

        public static void delete_saved(savedTab fileData)
        {
            try
            {
                string filePath = fileData.Path;

                if (File.Exists(filePath + ".png"))
                {
                    File.Delete(filePath + ".ink");
                    File.Delete(filePath + ".xml");
                    File.Delete(filePath + ".png");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
