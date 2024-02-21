using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Ink;
using JotWin.ViewModel.Helpers.UndoManager;
using System.Xml.Serialization;

namespace JotWin
{
    public class UIStroke : Shape
    {
        private readonly Geometry definingGeometry;
        protected override Geometry DefiningGeometry => definingGeometry;

        public double X => InkCanvas.GetLeft(this);
        public double Y => InkCanvas.GetTop(this);
        public int Z => Panel.GetZIndex(this);

        public UIStroke(Stroke stroke)
        {
            Fill = new SolidColorBrush(stroke.DrawingAttributes.Color);
            definingGeometry = stroke.GetGeometry().Clone();
            InkCanvas.SetLeft(this, definingGeometry.Bounds.Left);
            InkCanvas.SetTop(this, definingGeometry.Bounds.Top);
            Width = definingGeometry.Bounds.Width;
            Height = definingGeometry.Bounds.Height;
            definingGeometry.Transform = new TranslateTransform(-definingGeometry.Bounds.Left,
                                                                -definingGeometry.Bounds.Top);
        }

        public UIStroke(double x,
                        double y,
                        int z,
                        double width,
                        double height,
                        Brush color,
                        Geometry geo)
        {
            InkCanvas.SetLeft(this, x);
            InkCanvas.SetTop(this, y);
            Panel.SetZIndex(this, z);
            Width = width;
            Height = height;
            Fill = color;
            definingGeometry = geo.Clone();
            definingGeometry.Transform = new TranslateTransform(-definingGeometry.Bounds.Left,
                                                                -definingGeometry.Bounds.Top);
        }

        public ExtendedUIStroke ToExtended()
        {
            return new ExtendedUIStroke(X,
                                        Y,
                                        Z,
                                        Width,
                                        Height,
                                        Fill,
                                        definingGeometry);
        }
    }

    public class ExtendedUIStroke
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int Z { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string GeoString { get; set; }

        [XmlIgnore]
        public Brush? ColorBrush { get; set; }

        [XmlElement("ColorBrush")]
        public SerializableColor? StrokeColorSerializable
        {
            get { return BrushSerializationHelper.BrushToSerializableColor(ColorBrush); }
            set { ColorBrush = BrushSerializationHelper.SerializableColorToBrush(value); }
        }

        public ExtendedUIStroke() { }

        public ExtendedUIStroke(double x,
                                double y,
                                int z,
                                double width,
                                double height,
                                Brush color,
                                Geometry geo)
        {
            X = x;
            Y = y;
            Z = z;
            Width = width;
            Height = height;
            ColorBrush = color;
            GeoString = geo.ToString();
        }

        public UIStroke ToUIStroke()
        {
            return new UIStroke(X,
                                Y,
                                Z,
                                Width,
                                Height,
                                ColorBrush,
                                Geometry.Parse(GeoString));
        }
    }
}
