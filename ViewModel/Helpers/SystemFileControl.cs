using JotWin.View;
using System;
using System.Collections.Generic;
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

            if (bounds.Width > 0 && bounds.Height > 0)
            {
                RenderTargetBitmap renderBitmap = new ((int)inkCanvas.ActualWidth,
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
        }


        public static Rect getInkBoundingBox(InkCanvas inkCanvas)
        {
            Rect boundingBox = Rect.Empty;

            double minLeft = 0;
            double minTop = 0;
            double maxBottom = 0;
            double maxRight = 0;

            int count = 0;

            foreach (var stroke in inkCanvas.Strokes)
            {
                Rect strokeBounds = stroke.GetBounds();
                boundingBox = Rect.Union(boundingBox, strokeBounds);

                if(count == 0)
                {
                    minLeft = strokeBounds.X;
                    minTop = strokeBounds.Y;
                    maxRight = strokeBounds.Right;
                    maxBottom = strokeBounds.Bottom;
                }
                else
                {
                    if (strokeBounds.X < minLeft)
                    {
                        minLeft = strokeBounds.X;
                    }
                    if (strokeBounds.Y < minTop)
                    {
                        minTop = strokeBounds.Y;
                    }
                    if(strokeBounds.Bottom > maxBottom)
                    {
                        maxBottom = strokeBounds.Bottom;
                    }
                    if(strokeBounds.Right > maxRight)
                    {
                        maxRight = strokeBounds.Right;
                    }
                }
                count++;
            }

            foreach (var visual in inkCanvas.Children)
            {
                if (visual is UIElement element)
                {
                    Rect elementBounds = VisualTreeHelper.GetDescendantBounds(element);
                    boundingBox = Rect.Union(boundingBox, elementBounds);
                }
                count++;
            }

            Rect boundingRect = new Rect(minLeft - 2, minTop, maxRight - minLeft + 5, maxBottom - minTop + 5);

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

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            Image image = new Image();
            image.Source = bitmapImage;

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

                foreach (System.Windows.Forms.Screen screen in allScreens)
                {
                    System.Drawing.Bitmap screenshot = new(screen.Bounds.Width, screen.Bounds.Height);

                    using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(screenshot))
                    {
                        graphics.CopyFromScreen(screen.Bounds.X,
                                                screen.Bounds.Y,
                                                0, 0,
                                                screen.Bounds.Size,
                                                System.Drawing.CopyPixelOperation.SourceCopy
                                                );
                    }

                    capturedScreens.Screenshots.Add(screenshot);
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
