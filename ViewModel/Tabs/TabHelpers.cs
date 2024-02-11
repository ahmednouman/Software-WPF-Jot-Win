using JotWin.Model;
using JotWin.View;
using JotWin.ViewModel.Helpers.UndoManager;
using System.Windows;

namespace JotWin.ViewModel.Tabs
{
    public class TabHelpers
    {
        public MainAppWindow mainWindow;
        public TabHelpers(MainAppWindow _mainWindow)
        {
            mainWindow = _mainWindow;
        }

        public void loadCanvasContent(TabVM selectedTab)
        {
            mainWindow.DrawingCanvas.Strokes.Clear();
            mainWindow.DrawingCanvas.Children.Clear();

            if (selectedTab.canvasData.hasSketch && selectedTab.canvasData.tabUndoManager != null)
            {
                mainWindow.logo_1.Visibility = Visibility.Collapsed;
                //mainWindow.canvasLogoAction(false);

                canvasState? currentCanvas = selectedTab.canvasData.tabUndoManager.getCurrentState();
                
                if (currentCanvas != null)
                {
                    CanvasAnalyzer.reconstructCanvas(mainWindow.DrawingCanvas, currentCanvas);
                }

                if (selectedTab.canvasData.tabUndoManager.CanUndo)
                {
                    UndoAPI.SetUndoBtn(mainWindow, "active");
                }
                else
                {
                    UndoAPI.SetUndoBtn(mainWindow, "inactive");
                }

                if (selectedTab.canvasData.tabUndoManager.CanRedo)
                {
                    UndoAPI.SetRedoBtn(mainWindow, "active");
                }
                else
                {
                    UndoAPI.SetRedoBtn(mainWindow, "inactive");
                }
            }
            else
            {
                mainWindow.canvasLogoAction(true);
                UndoAPI.SetUndoBtn(mainWindow, "inactive");
                UndoAPI.SetRedoBtn(mainWindow, "inactive");
            }

            mainWindow.canvasTemplate = selectedTab.canvasData.canvasBackground;
        }

        public static void addNewTabData(TabVM newTab, MainAppWindow mainWin)
        {
            TabData td = new()
            {
                TabTitle = newTab.TabName,
                hasSketch = false,
                tabUndoManager = new UndoAPI(mainWin),
                canvasBackground = CanvasTemplate.Blank,
                isForMajicJot = false
            };
            mainWin.tabDataList.Add(td);
            newTab.canvasData = td;
        }
    }
}
