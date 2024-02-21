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
        private List<CanvasState> states = new();
        private int currentIndex = -1;

        public int Count => states.Count;
        public bool CanUndo => currentIndex > -1;
        public bool CanRedo => currentIndex < states.Count - 1;

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
                var strokeCount = states[currentIndex]._strokeList.Count;
                return rectCount + ellipseCount + imageCount + textCount + strokeCount;
            }
        }

        public MainAppWindow mainWin;

        public UndoAPI(MainAppWindow mainWin)
        {
            this.mainWin = mainWin;
        }

        public void SaveState(InkCanvas inkCanvas)
        {
            List<ExtendedRectangle> clonedRectangles = CanvasAnalyzer.FindRectangle(inkCanvas);
            List<ExtendedEllipse> clonedEllipses = CanvasAnalyzer.FindEllipse(inkCanvas);
            List<ExtendedTextBox> clonedTextBoxes = CanvasAnalyzer.FindTextBoxes(inkCanvas);
            List<ExtendedImage> clonedImages = CanvasAnalyzer.FindImages(inkCanvas);
            List<ExtendedUIStroke> clonedStrokes = CanvasAnalyzer.FindUIStrokes(inkCanvas);

            ExtendedShape clonedShape = new()
            {
                Rectangles = clonedRectangles,
                Ellipses = clonedEllipses
            };

            CanvasState x = new() 
            {
                _shapeList = new List<ExtendedShape> { clonedShape },
                _textList = clonedTextBoxes,
                _imageList = clonedImages,
                _strokeList = clonedStrokes
            };

            states.Add(x);

            if (currentIndex < states.Count - 2)
            {
                int currentCount = states.Count;
                int index = currentIndex + 1;
                for (int i = index; i < currentCount - 1; i++)
                {
                    states.RemoveAt(index);
                }
                mainWin.tabDataList[mainWin.mainTabsVM.SelectedTab].zIndexCount = highestZIndexCount + 1;
                SetRedoButtonActive(mainWin, false);
            }

            currentIndex = states.Count - 1;
        }

        public CanvasState? Undo()
        {
            if (currentIndex > 0)
            {
                currentIndex--;
                return new CanvasState(states[currentIndex]);
            }
            else if (currentIndex == 0)
            {
                currentIndex--;
                return new CanvasState();
            }

            return null;
        }

        public CanvasState? Redo()
        {
            if (currentIndex < states.Count - 1)
            {
                currentIndex++;
                return new CanvasState(states[currentIndex]);
            }

            MessageBox.Show("Nothing to redo.");
            return null;
        }

        public CanvasState? getCurrentState()
        {
            if (currentIndex > -1)
            {
                return states[currentIndex];
            }
            return null;
        }


        public static void SetUndoButtonActive(MainAppWindow main_win, bool active)
        {
            Image image = GenericHelpers.FindVisualChild<Image>(main_win.undoButton);

            if (image == null)
            {
                return;
            }

            string imagePath = "../Resources/Assets/undo_" + (active ? "active" : "inactive") + ".png";

            image.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
        }

        public static void SetRedoButtonActive(MainAppWindow main_win, bool active)
        {
            Image image = GenericHelpers.FindVisualChild<Image>(main_win.redoButton);
            
            if (image == null)
            {
                return;
            }

            string imagePath = "../Resources/Assets/redo_" + (active ? "active" : "inactive") + ".png";

            image.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
        }
    }
}
