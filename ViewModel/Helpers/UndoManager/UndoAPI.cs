using JotWin.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media.Imaging;

namespace JotWin.ViewModel.Helpers.UndoManager
{
    public class UndoAPI
    {
        private List<canvasState> states = new();
        private int currentIndex = -1;

        public int Count => states.Count;
        public bool CanUndo => currentIndex > -1;
        public bool CanRedo => (currentIndex < states.Count - 1);

        public int highestZIndexCount
        {
            get
            {
                if (currentIndex < 0 || currentIndex >= states.Count)
                {
                    Debug.WriteLine("highestZIndexCount bad index " + currentIndex);
                    return 0;
                }
                var rectCount = states[currentIndex]._shapeList[0].Rectangles.Count;
                var ellipseCount = states[currentIndex]._shapeList[0].Ellipses.Count;
                var imageCount = states[currentIndex]._imageList.Count;
                var textCount = states[currentIndex]._textList.Count;
                return rectCount + ellipseCount + imageCount + textCount;
            }
        }

        public MainAppWindow mainWin;

        public UndoAPI(MainAppWindow mainWin)
        {
            this.mainWin = mainWin;
        }

        public void SaveState(InkCanvas inkCanvas)
        {
            //StrokeCollection clonedStrokes = new StrokeCollection(inkCanvas.Strokes);
            //StrokeCollection clonedStrokes = inkCanvas.Strokes.Clone();
            StrokeCollection clonedStrokes = new(inkCanvas.Strokes.Select(s => s.Clone()));
            ExtendedStroke clonedExtendedStroke = new(clonedStrokes);
            List<ExtendedRectangle> clonedRectangles = CanvasAnalyzer.FindRectangle(inkCanvas);
            List<ExtendedEllipse> clonedEllipses = CanvasAnalyzer.FindEllipse(inkCanvas);
            List<ExtendedTextBox> clonedTextBoxes = CanvasAnalyzer.FindTextBoxes(inkCanvas);
            List<ExtendedImage> clonedImages = CanvasAnalyzer.FindImages(inkCanvas);

            ExtendedShape clonedShape = new()
            {
                Rectangles = clonedRectangles,
                Ellipses = clonedEllipses
            };

            canvasState x = new()
            {
                //_strokeList = clonedStrokes,
                _extendedStroke = clonedExtendedStroke,
                _shapeList = new List<ExtendedShape> { clonedShape },
                _textList = clonedTextBoxes,
                _imageList = clonedImages
            };

            states.Add(x);

            if (currentIndex < states.Count - 2)
            {
                int currenCount = states.Count;
                int index = currentIndex + 1;
                for (int i = index; i < currenCount - 1; i++)
                {
                    states.RemoveAt(index);
                }
                mainWin.tabDataList[mainWin.mainTabsVM.SelectedTab].zIndexCount = highestZIndexCount + 1;
                SetRedoBtn(mainWin, "inactive");
            }

            currentIndex = states.Count - 1;
        }

        public canvasState? Undo()
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                return new canvasState(states[currentIndex]);
            }
            else if (currentIndex == 0)
            {
                currentIndex--;
                // states.Clear();
                return new canvasState();
            }

            return null;
        }

        public canvasState? Redo()
        {
            if (currentIndex < states.Count - 1)
            {
                currentIndex++;
                return new canvasState(states[currentIndex]);
            }

            MessageBox.Show("Nothing to redo.");
            return null;
        }

        public canvasState? getCurrentState()
        {
            if (currentIndex > -1)
            {
                return states[currentIndex];
            }
            return null;
        }


        public static void SetUndoBtn(MainAppWindow main_win, string status)
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

        public static void SetRedoBtn( MainAppWindow main_win, string status)
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
