using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;

namespace JotWin.ViewModel.Helpers.UndoManager
{
    [Serializable]
    public class CanvasState
    {
        //public StrokeCollection _strokeList = new StrokeCollection();
        //[XmlIgnore]
        //public ExtendedStroke _extendedStroke = new();

        public List<ExtendedShape> _shapeList = new();
        public List<ExtendedTextBox> _textList = new();
        public List<ExtendedImage> _imageList = new(); 
        public List<ExtendedUIStroke> _strokeList = new();

        public CanvasState() { }

        public CanvasState(CanvasState original)
        {
            //_strokeList = new StrokeCollection(original._strokeList);
            //_extendedStroke = new ExtendedStroke(original._extendedStroke.Strokes);
            _shapeList = new List<ExtendedShape>(original._shapeList);
            _textList = new List<ExtendedTextBox>(original._textList);  
            _imageList = new List<ExtendedImage>(original._imageList);
            _strokeList = new List<ExtendedUIStroke>(original._strokeList);
        }
    }

    public class SerializableColor
    {
        public byte A { get; set; }
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
    }

    public static class BrushSerializationHelper
    {
        public static SerializableColor? BrushToSerializableColor(Brush? brush)
        {
            if (brush is SolidColorBrush solidColorBrush)
            {
                return new SerializableColor
                {
                    A = solidColorBrush.Color.A,
                    R = solidColorBrush.Color.R,
                    G = solidColorBrush.Color.G,
                    B = solidColorBrush.Color.B
                };
            }

            return null;
        }

        public static Brush? SerializableColorToBrush(SerializableColor? color)
        {
            if (color == null)
            {
                return null;
            }

            return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }
    }


    //public class ExtendedStroke
    //{
    //    public StrokeCollection Strokes { get; }

    //    public ExtendedStroke()
    //    {
    //        Strokes = new StrokeCollection();
    //    }

    //    public ExtendedStroke(StrokeCollection strokes)
    //    {
    //        Strokes = new StrokeCollection(strokes.Select(s => new Stroke(s.StylusPoints.Clone(), s.DrawingAttributes.Clone())));
    //    }
    //}

    public class ExtendedRectangle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int Z { get; set; }  
        public double Width { get; set; }
        public double Height { get; set; }
        public double StrokeThickness { get; set; }

        [XmlIgnore]
        public Brush? strokeColor { get; set; }

        [XmlElement("strokeColor")]
        public SerializableColor? strokeColorSerializable
        {
            get { return BrushSerializationHelper.BrushToSerializableColor(strokeColor); }
            set { strokeColor = BrushSerializationHelper.SerializableColorToBrush(value); }
        }

        public bool Filled { get; set; }

        [XmlIgnore]
        public Brush? FillColor { get; set; }

        [XmlElement("FillColor")]
        public SerializableColor? fillColorSerializable
        {
            get { return BrushSerializationHelper.BrushToSerializableColor(FillColor); }
            set { FillColor = BrushSerializationHelper.SerializableColorToBrush(value); }
        }

        public ExtendedRectangle()
        {
        }

        public ExtendedRectangle(double x, double y, int z, double width, double height, double stroke_thickness, Brush stroke, bool filled,  Brush fill_color)
        {
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Height = height;
            StrokeThickness = stroke_thickness;
            strokeColor = stroke;
            Filled = filled;
            FillColor = fill_color;
        }
    }

    public class ExtendedEllipse
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int Z { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double StrokeThickness { get; set; }

        [XmlIgnore]
        public Brush? strokeColor { get; set; }

        [XmlElement("strokeColor")]
        public SerializableColor? StrokeColorSerializable
        {
            get { return BrushSerializationHelper.BrushToSerializableColor(strokeColor); }
            set { strokeColor = BrushSerializationHelper.SerializableColorToBrush(value); }
        }

        public bool Filled { get; set; }

        [XmlIgnore]
        public Brush? FillColor { get; set; }

        [XmlElement("FillColor")]
        public SerializableColor? FillColorSerializable
        {
            get { return BrushSerializationHelper.BrushToSerializableColor(FillColor); }
            set { FillColor = BrushSerializationHelper.SerializableColorToBrush(value); }
        }


        public ExtendedEllipse()
        {
        }

        public ExtendedEllipse(double x, double y,int z, double width, double height, double stroke_thickness, Brush stroke, bool filled, Brush fill_color)
        {
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Height = height;
            StrokeThickness = stroke_thickness;
            strokeColor = stroke;
            Filled = filled;
            FillColor = fill_color;
        }
    }

    public class ExtendedShape
    {
        public List<ExtendedRectangle> Rectangles { get; set; }
        public List<ExtendedEllipse> Ellipses { get; set; }

        public ExtendedShape()
        {
            Rectangles = new List<ExtendedRectangle>();
            Ellipses = new List<ExtendedEllipse>();
        }

        public ExtendedShape(List<ExtendedRectangle> rectangles, List<ExtendedEllipse> ellipses)
        {
            Rectangles = rectangles ?? new List<ExtendedRectangle>();
            Ellipses = ellipses ?? new List<ExtendedEllipse>();
        }
    }

    public class ExtendedTextBox
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int Z { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double BorderThickness { get; set; }

        [XmlElement("BorderColor")]
        public SerializableColor BorderColorSerializable
        {
            get { return BrushSerializationHelper.BrushToSerializableColor(BorderColor); }
            set { BorderColor = BrushSerializationHelper.SerializableColorToBrush(value); }
        }


        [XmlIgnore]
        public Brush? BorderColor { get; set; }

        public bool HasBackground { get; set; }

        [XmlIgnore]
        public Brush? BackgroundColor { get; set; }

        [XmlElement("BackgroundColor")]
        public SerializableColor BackgroundColorSerializable
        {
            get { return BrushSerializationHelper.BrushToSerializableColor(BackgroundColor); }
            set { BackgroundColor = BrushSerializationHelper.SerializableColorToBrush(value); }
        }

        public string? Text { get; set; }
        public double FontSize { get; set; }
        public FontWeight FontWeight { get; set; }

        [XmlIgnore]
        public FontFamily? FontFamily { get; set; }

        [XmlElement("FontFamilyName")]
        public string? FontFamilyName
        {
            get { return FontFamily?.Source; }
            set { FontFamily = new FontFamily(value); }
        }

        [XmlIgnore]
        public Brush? FontColor { get; set; }

        [XmlElement("FontColor")]
        public SerializableColor FontColorSerializable
        {
            get { return BrushSerializationHelper.BrushToSerializableColor(FontColor); }
            set { FontColor = BrushSerializationHelper.SerializableColorToBrush(value); }
        }

        public ExtendedTextBox()
        {
        }

        public ExtendedTextBox(double x, double y, int z, double width, double height, double borderThickness, Brush borderColor,
                            bool hasBackground, Brush backgroundColor, string text, double fontSize, FontWeight fontWeight, FontFamily fontFamily, Brush fontColor)
        {
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Height = height;
            BorderThickness = borderThickness;
            BorderColor = borderColor;
            HasBackground = hasBackground;
            BackgroundColor = backgroundColor;
            Text = text;
            FontSize = fontSize;
            FontWeight = fontWeight;
            FontFamily = fontFamily;
            FontColor = fontColor;  
        }
    }


    public class ExtendedImage
    {
        public double X { get; set; }
        public double Y { get; set; }

        public int Z { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        [XmlIgnore]
        public Uri? SourceUri { get; set; }

        [XmlElement("SourceUri")]
        public string? SourceUriString
        {
            get { return SourceUri?.OriginalString; }
            set { SourceUri = string.IsNullOrEmpty(value) ? null : new Uri(value); }
        }

        [XmlIgnore]
        public ImageSource? ImageSrc { get; set; }

        public ExtendedImage()
        {
        }

        public ExtendedImage(double x, double y, int z, double width, double height, Uri sourceUri, ImageSource imageSrc)
        {
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Height = height;
            SourceUri = sourceUri;
            ImageSrc = imageSrc;
        }
    }
}
