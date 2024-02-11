using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using JotWin.View;
using StateManagement;

namespace JotWin.ViewModel.Helpers
{
    /***
    public class UndoManagerControl
    {
        public StrokeCollection _strokesCollected = new StrokeCollection();
        public List<Shape> _shapesCollected = new List<Shape>();
        public List<TextBox> _textBoxCollected = new List<TextBox>();
        public List<Image> _imagesCollected = new List<Image>();

        public UndoServiceAggregate serviceAggregate;

        public UndoService<StrokeCollection> undoServiceForStrokes;
        public UndoService<List<Shape>> undoServiceForShapes;
        public UndoService<List<TextBox>> undoServiceForText;
        public UndoService<List<Image>> undoServiceForImages;

        public UndoManagerControl()
        {
            undoServiceForStrokes = new UndoService<StrokeCollection>(GetStrokeState, SetStrokeState, null);
            undoServiceForShapes = new UndoService<List<Shape>>(GetShapeState, SetShapeState, null);
            undoServiceForText = new UndoService<List<TextBox>>(GetTextState, SetTextState, null);
            undoServiceForImages = new UndoService<List<Image>>(GetImageState, SetImageState, null);
            IUndoService[] subservices = { undoServiceForStrokes, undoServiceForShapes, undoServiceForText, undoServiceForImages };
            serviceAggregate = new UndoServiceAggregate(subservices);
        }

        public void GetStrokeState(out StrokeCollection state)
        {
            state = _strokesCollected;
        }

        public void SetStrokeState(StrokeCollection value)
        {
            _strokesCollected = value;
        }

        public void GetShapeState(out List<Shape> state)
        {
            state = _shapesCollected;
        }

        public void SetShapeState(List<Shape> value)
        {
            _shapesCollected = value;
        }

        public void GetTextState(out List<TextBox> state)
        {
            state = _textBoxCollected;
        }

        public void SetTextState(List<TextBox> value)
        {
            _textBoxCollected = value;
        }

        public void GetImageState(out List<Image> state)
        {
            state = _imagesCollected;
        }

        public void SetImageState(List<Image> value)
        {
            _imagesCollected = value;
        }
    }

    ***/

    public class UndoManagerControl2
    {
        public canvasItems _canvasContent = new canvasItems();

        public UndoService<canvasItems> undoService;
        public UndoManagerControl2()
        {
            undoService =  new UndoService<canvasItems>(GetCanvasState, SetCanvasState, null);
        }

        private void GetCanvasState(out canvasItems state)
        {
            state = _canvasContent;
        }

        private void SetCanvasState(canvasItems value)
        {
            _canvasContent = value;
        }
    }

    public class canvasItems
    {
        public StrokeCollection _strokeList = new StrokeCollection();
        public List<Shape> _shapeList = new List<Shape>();
        public List<TextBox> _textList = new List<TextBox>();
        public List<Image> _imageList = new List<Image>();
    }




    public class UndoHelper
    {
        public static List<Shape> storeShapes(InkCanvas inkCanvas)
        {
            List<Shape> shapes = new List<Shape>();

            foreach (UIElement child in inkCanvas.Children)
            {
                if (child is Shape shape)
                {
                    shapes.Add(shape);
                }
            }

            return shapes;
        }

        public static List<TextBox> storeText(InkCanvas inkCanvas)
        {
            List<TextBox> texts = new List<TextBox>();

            foreach (UIElement child in inkCanvas.Children)
            {
                if (child is TextBox text_item)
                {
                    texts.Add(text_item);
                }
            }

            return texts;
        }

        public static List<Image> storeImages(InkCanvas inkCanvas)
        {
            List<Image> images = new List<Image>();

            foreach (UIElement child in inkCanvas.Children)
            {
                if (child is Image image_item)
                {
                    images.Add(image_item);
                }
            }

            return images;
        }

        public static void saveAllCanvasState(UndoManagerControl2 historyControl, canvasItems canvasItemsMem, InkCanvas DrawingCanvas)
        {
            canvasItemsMem = new canvasItems();

            canvasItemsMem._strokeList = new StrokeCollection(DrawingCanvas.Strokes);
            canvasItemsMem._shapeList = storeShapes(DrawingCanvas);
            canvasItemsMem._textList = storeText(DrawingCanvas);
            canvasItemsMem._imageList = storeImages(DrawingCanvas);

            historyControl._canvasContent = canvasItemsMem;

            historyControl.undoService.RecordState();
        }

        /***
        public static void saveStrokesState(UndoManagerControl historyControl, InkCanvas inkCanvas)
        {
            historyControl._strokesCollected = new StrokeCollection(inkCanvas.Strokes);
            historyControl.undoServiceForStrokes.RecordState();
        }

        public static void saveShapesState(UndoManagerControl historyControl, InkCanvas inkCanvas)
        {
            List<Shape> shapeList = UndoHelper.storeShapes(inkCanvas);
            historyControl._shapesCollected = shapeList;
            historyControl.undoServiceForShapes.RecordState();
        }

        public static void saveTextState(UndoManagerControl historyControl, InkCanvas inkCanvas)
        {
            List<TextBox> textList = UndoHelper.storeText(inkCanvas);
            historyControl._textBoxCollected = textList;
            historyControl.undoServiceForText.RecordState();
        }

        public static void saveImageState(UndoManagerControl historyControl, InkCanvas inkCanvas)
        {
            List<Image> imageList = UndoHelper.storeImages(inkCanvas);
            historyControl._imagesCollected = imageList;
            historyControl.undoServiceForImages.RecordState();
        }

        public static void SaveAllState(UndoManagerControl historyControl, InkCanvas inkCanvas)
        {
            saveStrokesState(historyControl, inkCanvas);
            saveShapesState(historyControl, inkCanvas);
            saveTextState(historyControl, inkCanvas);
            saveImageState(historyControl, inkCanvas);
        }

        public  static void saveOnMoveOrResize(UndoManagerControl historyControl, InkCanvas inkCanvas)
        {
            StrokeCollection selectedStrokes = inkCanvas.GetSelectedStrokes();
            ReadOnlyCollection<UIElement> selectedElements = inkCanvas.GetSelectedElements();

            if (selectedStrokes.Count > 0)
            {
                saveStrokesState(historyControl, inkCanvas);
            }
            if (selectedElements.Count > 0)
            {
                bool foundShape = false;
                bool foundImage = false;
                bool foundText = false;
                foreach (UIElement selectedElement in selectedElements)
                {
                    if (selectedElement is Shape)
                    {
                        foundShape = true;
                        continue;

                    }
                    else if (selectedElement is Image)
                    {
                        foundImage = true;
                        continue;

                    }
                    else if (selectedElement is TextBox)
                    {
                        foundText = true;
                        continue;

                    }
                }
                if (foundShape)
                {
                    saveShapesState(historyControl, inkCanvas);
                }
                if (foundImage)
                {
                    saveImageState(historyControl, inkCanvas);
                }
                if (foundText)
                {
                    saveTextState(historyControl, inkCanvas);
                }

            }
        }

        ***/

        public static void SetUndoBtn(UndoManagerControl2 historyControl, MainAppWindow main_win, string status)
        {
            if (status == "active")
            {
                Image redoIcon = GenericHelpers.FindVisualChild<Image>(main_win.undoButton);

                if (redoIcon != null)
                {
                    redoIcon.Source = new BitmapImage(new Uri("../Resources/Assets/undo_active.png", UriKind.Relative));
                }
            }
            else
            {
                Image redoIcon = GenericHelpers.FindVisualChild<Image>(main_win.undoButton);

                if (redoIcon != null)
                {
                    redoIcon.Source = new BitmapImage(new Uri("../Resources/Assets/undo_inactive.png", UriKind.Relative));
                }
            }
        }

        public static void SetRedoBtn(UndoManagerControl2 historyControl, MainAppWindow main_win, string status)
        {
            if (status == "active")
            {
                Image redoIcon = GenericHelpers.FindVisualChild<Image>(main_win.redoButton);

                if (redoIcon != null)
                {
                    redoIcon.Source = new BitmapImage(new Uri("../Resources/Assets/redo_active.png", UriKind.Relative));
                }
            }
            else
            {
                Image redoIcon = GenericHelpers.FindVisualChild<Image>(main_win.redoButton);

                if (redoIcon != null)
                {
                    redoIcon.Source = new BitmapImage(new Uri("../Resources/Assets/redo_inactive.png", UriKind.Relative));
                }
            }
        }

        
    }

}
