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

            if (selectedTab.canvasData.isForMajicJot)
            {
                mainWindow.doneCopyBtnControl("done");
            }
            else
            {
                mainWindow.doneCopyBtnControl("copy");
            }

            if (selectedTab.canvasData.hasSketch && selectedTab.canvasData.tabUndoManager != null)
            {
                mainWindow.logo_1.Visibility = Visibility.Collapsed;


                mainWindow.canvasCopy.IsEnabled = true;
                //mainWindow.canvasLogoAction(false);

                CanvasState? currentCanvas = selectedTab.canvasData.tabUndoManager.getCurrentState();
                
                if (currentCanvas != null)
                {
                    CanvasAnalyzer.reconstructCanvas(mainWindow.DrawingCanvas, currentCanvas);
                }

                UndoAPI.SetUndoButtonActive(mainWindow, selectedTab.canvasData.tabUndoManager.CanUndo);
                UndoAPI.SetRedoButtonActive(mainWindow, selectedTab.canvasData.tabUndoManager.CanRedo);
            }
            else
            {
                mainWindow.canvasLogoAction(true);
                mainWindow.canvasCopy.IsEnabled = false;
                UndoAPI.SetUndoButtonActive(mainWindow, false);
                UndoAPI.SetRedoButtonActive(mainWindow, false);
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
