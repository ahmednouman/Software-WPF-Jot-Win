using JotWin.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace JotWin.ViewModel.Helpers
{
    public class SystemFileControl
    {
        public static void SaveCanvasToFile(string fileName, InkCanvas inkCanvas)
        {
            RenderTargetBitmap renderBitmap = new((int)inkCanvas.ActualWidth,
                                                  (int)inkCanvas.ActualHeight,
                                                  96d,
                                                  96d,
                                                  PixelFormats.Default);

            renderBitmap.Render(inkCanvas);

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using FileStream fileStream = new(fileName, FileMode.Create);
            encoder.Save(fileStream);
        }

        private static void SaveCanvasAsPng(BitmapSource bitmap, string fileName)
        {
            string appDirectory = Environment.CurrentDirectory;

            string filePath = Path.Combine(appDirectory, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(stream);
        }

        private static void CopyImageToClipboard(string fileName)
        {
            try
            {
                string appDirectory = Environment.CurrentDirectory;
                string filePath = Path.Combine(appDirectory, fileName);

                if (File.Exists(filePath))
                {
                    var fileDropList = new System.Collections.Specialized.StringCollection
                    {
                        filePath
                    };

                    Clipboard.Clear();
                    Clipboard.SetFileDropList(fileDropList);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public static void CopyCanvasToClipboard(InkCanvas inkCanvas)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(inkCanvas);

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                Debug.WriteLine($"CopyCanvasToClipboard invalid bounds. w: {bounds.Width}, h: {bounds.Height}");
                return;
            }

            RenderTargetBitmap renderBitmap = new((int)inkCanvas.ActualWidth,
                                                  (int)inkCanvas.ActualHeight,
                                                  96d,
                                                  96d,
                                                  PixelFormats.Default);

            renderBitmap.Render(inkCanvas);

            Rect boundingBox = getInkBoundingBox(inkCanvas);

            boundingBox.X = Math.Max(0, Math.Min(boundingBox.X, inkCanvas.ActualWidth));
            boundingBox.Y = Math.Max(0, Math.Min(boundingBox.Y, inkCanvas.ActualHeight));
            boundingBox.Width = Math.Min(inkCanvas.ActualWidth - boundingBox.X, boundingBox.Width);
            boundingBox.Height = Math.Min(inkCanvas.ActualHeight - boundingBox.Y, boundingBox.Height);

            CroppedBitmap croppedBitmap = new(renderBitmap, new Int32Rect((int)boundingBox.X,
                                                                          (int)boundingBox.Top,
                                                                          (int)boundingBox.Width,
                                                                          (int)boundingBox.Height));

            SaveCanvasAsPng(croppedBitmap, "CroppedCanvas.png");
            CopyImageToClipboard("CroppedCanvas.png");
        }


        public static Rect getInkBoundingBox(InkCanvas inkCanvas)
        {
            Rect boundingBox = Rect.Empty;

            foreach (FrameworkElement element in inkCanvas.Children)
            {
                double left = InkCanvas.GetLeft(element);
                double top = InkCanvas.GetTop(element);

                if (double.IsNaN(left)
                    || double.IsNaN(top)
                    || double.IsNaN(element.Width)
                    || double.IsNaN(element.Height))
                {
                    continue;
                }

                Rect elementBounds = new(left, top, element.Width, element.Height);

                boundingBox = Rect.Union(boundingBox, elementBounds);
            }

            Rect boundingRect = new(boundingBox.Left - 2,
                                    boundingBox.Top,
                                    boundingBox.Width + 5,
                                    boundingBox.Height + 5);

            return boundingRect;
        }



        public void PasteFromClipboard()
        {

        }


        public static void AddScreenshotToCanvas(System.Drawing.Bitmap bitmap, MainAppWindow main_window)
        {
            using MemoryStream memory = new();

            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
            memory.Position = 0;

            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            Image image = new()
            {
                Source = bitmapImage
            };

            double canvasWidth = main_window.DrawingCanvas.ActualWidth;
            double canvasHeight = main_window.DrawingCanvas.ActualHeight;

            double scaleFactor = 0.8;

            image.Width = canvasWidth * scaleFactor;
            image.Height = canvasHeight * scaleFactor;

            double leftPosition = (canvasWidth - image.Width) / 2;
            double topPosition = (canvasHeight - image.Height) / 2;

            InkCanvas.SetLeft(image, leftPosition);
            InkCanvas.SetTop(image, topPosition);

            Panel.SetZIndex(image, main_window.tabDataList[main_window.mainTabsVM.SelectedTab].zIndexCount);

            main_window.canvasLogoAction(false);

            main_window.DrawingCanvas.Children.Add(image);

            //main_window.historyControl.SaveState(main_window.DrawingCanvas);
            main_window.tabDataList[main_window.mainTabsVM.SelectedTab].tabUndoManager.SaveState(main_window.DrawingCanvas);

            main_window.tabDataList[main_window.mainTabsVM.SelectedTab].zIndexCount++;
        }


        public static CapturedScreens? CaptureScreenArea()
        {
            CapturedScreens capturedScreens = new();

            try
            {
                System.Windows.Forms.Screen[] allScreens = System.Windows.Forms.Screen.AllScreens;

                ExtenalMonitorInfo.LoadData();

                foreach (System.Windows.Forms.Screen screen in allScreens)
                {

                    for(int i = 0; i < ExtenalMonitorInfo.monitorInfo.DisplayCount; i++)
                    {
                        if(ExtenalMonitorInfo.monitorInfo.Displays[i].DeviceName == screen.DeviceName)
                        {
                            System.Drawing.Bitmap screenshot = new(ExtenalMonitorInfo.monitorInfo.Displays[i].PhysicalResolution.Width, ExtenalMonitorInfo.monitorInfo.Displays[i].PhysicalResolution.Height);

                            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(screenshot))
                            {
                                graphics.CopyFromScreen(ExtenalMonitorInfo.monitorInfo.Displays[i].Resolution.X,
                                                        ExtenalMonitorInfo.monitorInfo.Displays[i].Resolution.Y,
                                                        0, 0,
                                                        screen.Bounds.Size,
                                                        System.Drawing.CopyPixelOperation.SourceCopy);
                            }

                            capturedScreens.Screenshots.Add(screenshot);
                        }
                    }
                    
                }

                capturedScreens.StartIndex += allScreens.Length;

                return capturedScreens;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error capturing screenshot: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null; 
            }
        }
    }

    public class CapturedScreens
    {
        public List<System.Drawing.Bitmap> Screenshots { get; set; }
        public int StartIndex { get; set; }
        public string DisplayText => $"Display {StartIndex}";

        public CapturedScreens()
        {
            Screenshots = new List<System.Drawing.Bitmap>();
            StartIndex = 1;
        }
    }
}
