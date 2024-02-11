using System.Windows.Media;
using System.Windows.Shapes;

namespace JotWin.Model
{
    public enum DrawingShape
    {
        Rectangle,
        Ellipse
    }

    public class DrawingSet
    {
        public DrawingSet()
        {
            IsPen = true;
            IsHighlighter = false;
            IsEraser = false;
            IsSelect = false;
            IsShapes = false;
            IsColorSwitch = false;
            isDrawing = false;
            isShapeFill = false;
            PenSize = 2;
            HighlighterSize = 20;
            selectedShape = DrawingShape.Rectangle;
            SelectedColor = Colors.Black;
            TextSize = 10;
            selectedFont = "Arial";
        }


        private bool isSelect;

        public bool IsSelect
        {
            get { return isSelect; }
            set { isSelect = value; }
        }

        private bool isPen;

        public bool IsPen
        {
            get { return isPen; }
            set { isPen = value; }
        }

        private bool isHighlighter;

        public bool IsHighlighter
        {
            get { return isHighlighter; }
            set { isHighlighter = value; }
        }

        private bool isEraser;

        public bool IsEraser
        {
            get { return isEraser; }
            set { isEraser = value; }
        }

        private bool isText;

        public bool IsText
        {
            get { return isText; }
            set { isText = value; }
        }

        private bool isShapes;

        public bool IsShapes
        {
            get { return isShapes; }
            set { isShapes = value; }
        }

        private bool isColorSwitch;

        public bool IsColorSwitch
        {
            get { return isColorSwitch; }
            set { isColorSwitch = value; }
        }

        public bool isDrawing;

        public bool isShapeFill;


        private int penSize;

        public int PenSize
        {
            get { return penSize; }
            set { penSize = value; }
        }

        private int highlighterSize;

        public int HighlighterSize
        {
            get { return highlighterSize; }
            set { highlighterSize = value; }
        }

        private int textSize;

        public int TextSize
        {
            get { return textSize; }
            set { textSize = value; }
        }

        public string selectedFont { get; set; }

        public DrawingShape selectedShape;
        public Shape? shape;

        private Color selectedColor;

        public Color SelectedColor
        {
            get { return selectedColor; }
            set { selectedColor = value; }
        }

    }
}
