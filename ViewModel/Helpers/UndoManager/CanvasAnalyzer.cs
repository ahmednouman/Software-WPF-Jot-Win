using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JotWin.ViewModel.Helpers.UndoManager
{
    public class CanvasAnalyzer
    {
        public static void reconstructCanvas(InkCanvas inkCanvas, canvasState canvasContent)
        {
            inkCanvas.Children.Clear();
            inkCanvas.Strokes.Clear();

            //inkCanvas.Strokes = canvasContent._strokeList;

            foreach (var stroke in canvasContent._extendedStroke.Strokes)
            {
                inkCanvas.Strokes.Add(new Stroke(stroke.StylusPoints.Clone(), stroke.DrawingAttributes.Clone()));
            }

            foreach (ExtendedImage image in canvasContent._imageList)
            {
                Image newImage = new()
                {
                    Width = image.Width,
                    Height = image.Height
                };

                if (image.SourceUri != null)
                {
                    newImage.Source = new BitmapImage(image.SourceUri);
                }
                else
                {
                    newImage.Source = image.ImageSrc;
                }

                InkCanvas.SetLeft(newImage, image.X);
                InkCanvas.SetTop(newImage, image.Y);

                Panel.SetZIndex(newImage, image.Z);

                inkCanvas.Children.Add(newImage);
            }


            foreach (ExtendedShape shape in canvasContent._shapeList)
            {
                foreach (ExtendedRectangle r in shape.Rectangles)
                {
                    Rectangle rectangle = new()
                    {
                        Width = r.Width,
                        Height = r.Height,
                        StrokeThickness = r.StrokeThickness,
                        Stroke = r.strokeColor,
                        Fill = r.FillColor
                    };

                    InkCanvas.SetLeft(rectangle, r.X);
                    InkCanvas.SetTop(rectangle, r.Y);

                    Panel.SetZIndex(rectangle, r.Z);

                    inkCanvas.Children.Add(rectangle);
                }

                foreach (ExtendedEllipse e in shape.Ellipses)
                {
                    Ellipse ellipse = new()
                    {
                        Width = e.Width,
                        Height = e.Height,
                        StrokeThickness = e.StrokeThickness,
                        Stroke = e.strokeColor,
                        Fill = e.FillColor
                    };

                    InkCanvas.SetLeft(ellipse, e.X);
                    InkCanvas.SetTop(ellipse, e.Y);

                    Panel.SetZIndex(ellipse, e.Z);

                    inkCanvas.Children.Add(ellipse);
                }
            }

            foreach (ExtendedTextBox textBox in canvasContent._textList)
            {
                TextBox newTextBox = new()
                {
                    Width = textBox.Width,
                    Height = textBox.Height,
                    BorderThickness = new Thickness(textBox.BorderThickness),
                    BorderBrush = textBox.BorderColor,
                    Background = textBox.HasBackground ? textBox.BackgroundColor : Brushes.Transparent,
                    Text = textBox.Text,
                    FontSize = textBox.FontSize,
                    FontWeight = textBox.FontWeight,
                    FontFamily = textBox.FontFamily,
                    Foreground = textBox.FontColor,
                    TextWrapping = TextWrapping.Wrap
                };

                InkCanvas.SetLeft(newTextBox, textBox.X);
                InkCanvas.SetTop(newTextBox, textBox.Y);

                Panel.SetZIndex(newTextBox, textBox.Z);

                inkCanvas.Children.Add(newTextBox);
            }
        }

        public static List<Stroke> FindStrokes(InkCanvas inkCanvas)
        {
            List<Stroke> foundStrokes = new();

            foreach (Stroke originalStroke in inkCanvas.Strokes)
            {
                foundStrokes.Add(originalStroke);
            }

            return foundStrokes;
        }

        public static List<ExtendedRectangle> FindRectangle(InkCanvas inkCanvas)
        {
            List<ExtendedRectangle> foundRectangles = new();

            foreach (UIElement child in inkCanvas.Children)
            {
                if (child is Rectangle originalRectangle)
                {
                    double x = InkCanvas.GetLeft(originalRectangle);
                    double y = InkCanvas.GetTop(originalRectangle);
                    int z = Panel.GetZIndex(originalRectangle);

                    ExtendedRectangle newRectangle = new(x,
                                                         y,
                                                         z,
                                                         originalRectangle.Width,
                                                         originalRectangle.Height,
                                                         originalRectangle.StrokeThickness,
                                                         originalRectangle.Stroke,
                                                         true,
                                                         originalRectangle.Fill);

                    foundRectangles.Add(newRectangle);
                }
            }

            return foundRectangles;
        }

        public static List<ExtendedEllipse> FindEllipse(InkCanvas inkCanvas)
        {
            List<ExtendedEllipse> foundEllipses = new();

            foreach (UIElement child in inkCanvas.Children)
            {
                if (child is Ellipse originalEllipse)
                {
                    double x = InkCanvas.GetLeft(originalEllipse);
                    double y = InkCanvas.GetTop(originalEllipse);
                    int z = Panel.GetZIndex(originalEllipse);

                    ExtendedEllipse newEllipse = new(x,
                                                     y,
                                                     z,
                                                     originalEllipse.Width,
                                                     originalEllipse.Height,
                                                     originalEllipse.StrokeThickness,
                                                     originalEllipse.Stroke,
                                                     true,
                                                     originalEllipse.Fill);

                    foundEllipses.Add(newEllipse);
                }
            }

            return foundEllipses;
        }

        public static List<ExtendedTextBox> FindTextBoxes(InkCanvas inkCanvas)
        {
            List<ExtendedTextBox> foundTextBoxes = new();

            foreach (UIElement child in inkCanvas.Children)
            {
                if (child is TextBox originalTextBox)
                {
                    double x = InkCanvas.GetLeft(originalTextBox);
                    double y = InkCanvas.GetTop(originalTextBox);
                    int z = Panel.GetZIndex(originalTextBox);

                    ExtendedTextBox newTextBox = new(x,
                                                     y,
                                                     z,
                                                     originalTextBox.Width,
                                                     originalTextBox.Height,
                                                     originalTextBox.BorderThickness.Left,
                                                     originalTextBox.BorderBrush,
                                                     originalTextBox.Background != Brushes.Transparent,
                                                     originalTextBox.Background,
                                                     originalTextBox.Text,
                                                     originalTextBox.FontSize,
                                                     originalTextBox.FontWeight,
                                                     originalTextBox.FontFamily,
                                                     originalTextBox.Foreground);

                    foundTextBoxes.Add(newTextBox);
                }
            }

            return foundTextBoxes;
        }

        public static List<ExtendedImage> FindImages(InkCanvas inkCanvas)
        {
            List<ExtendedImage> foundImages = new();

            foreach (UIElement child in inkCanvas.Children)
            {
                if (child is Image originalImage)
                {
                    double x = InkCanvas.GetLeft(originalImage);
                    double y = InkCanvas.GetTop(originalImage);
                    int z = Panel.GetZIndex(originalImage);

                    Uri sourceUri = ((BitmapImage)originalImage.Source).UriSource;

                    ExtendedImage newImage = new(
                        x,
                        y,
                        z,
                        originalImage.Width,
                        originalImage.Height,
                        sourceUri,
                        originalImage.Source
                    );

                    foundImages.Add(newImage);
                }
            }

            return foundImages;
        }
    }
}
