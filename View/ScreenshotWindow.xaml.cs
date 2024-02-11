using JotWin.ViewModel.Helpers;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace JotWin.View
{
    public partial class ScreenshotWindow : Window
    {
        private CapturedScreens screenshots;
        public List<screenshotObject>? screenshotData;

        public ScreenshotWindow(CapturedScreens screenshots, MainAppWindow parentWindow)
        {
            InitializeComponent();

            DataContext = screenshotData;

            this.screenshots = screenshots;
            Owner = parentWindow;

            LoadScreenshots();

            ScreenshotsListBox.SelectionChanged += ScreenshotsListBox_SelectionChanged;
        }

        private void LoadScreenshots()
        {
            screenshotData = new List<screenshotObject>();
            int i = 0;
            foreach (Bitmap screenshot in screenshots.Screenshots)
            {
                BitmapImage converted_image = ConvertToBitmapImage(screenshot);
                screenshotData.Add(new screenshotObject()
                {
                    screenshotImage = converted_image,
                    screenshotIndex = i++,
                    screenshotName = $"Display {i}"
                });
               // ScreenshotsListBox.Items.Add(ConvertToBitmapImage(screenshot));
            }
            ScreenshotsListBox.ItemsSource = screenshotData;
        }

        private static BitmapImage ConvertToBitmapImage(Bitmap bitmap)
        {
            using MemoryStream memory = new();
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            memory.Position = 0;

            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }


        public void ScreenshotsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ScreenshotsListBox.SelectedItem != null)
            {
                int selectedIndex = ScreenshotsListBox.SelectedIndex;

                if (selectedIndex >= 0 && selectedIndex < screenshots.Screenshots.Count)
                {
                    Bitmap selectedScreenshot = screenshots.Screenshots[selectedIndex];
                    MainAppWindow mainAppWindow = (MainAppWindow)Owner;

                    SystemFileControl.AddScreenshotToCanvas(selectedScreenshot, mainAppWindow);
                    Close();
                }
            }
        }
    }

    public class screenshotObject
    {
        public BitmapImage? screenshotImage { get; set; }

        public int screenshotIndex { get; set; }

        public string? screenshotName { get; set; }
    }
}
