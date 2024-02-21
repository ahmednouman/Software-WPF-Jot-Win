using JotWin.View;
using JotWin.ViewModel.Helpers.UndoManager;
using SQLite;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace JotWin.ViewModel.Helpers.SaveLoad
{
    public class SaveLoad
    {

        public static void saveCanvasToFile(MainAppWindow mainWin)
        {
            using (SQLiteConnection connection = new(App.savedTabsDB))
            {
                connection.CreateTable<savedTab>();
                var tabs = connection.Table<savedTab>().ToList();

                bool isFileExists = tabs.Any(tab => tab.Name == mainWin.selectedTab.TabName);
                if(isFileExists)
                {
                    //SaveStrokesToFile(mainWin);
                    serializeCanvasToXml(mainWin);
                    saveCanvasThumb(mainWin);
                    return;
                }
                else
                {

                }

            }

            //SaveStrokesToFile(mainWin);
            serializeCanvasToXml(mainWin);
            saveCanvasThumb(mainWin);

            savedTab saved_tab = new()
            {
                Name = mainWin.selectedTab.TabName,
                Path = Path.Combine(App.savedCanvasDirectory, mainWin.selectedTab.TabName),
                ImagePath = Path.Combine(App.savedCanvasDirectory, mainWin.selectedTab.TabName + ".png")
            };

            using (SQLiteConnection connection = new(App.savedTabsDB))
            {
                connection.CreateTable<savedTab>();
                connection.Insert(saved_tab);
            }

        }

        public static void openCanvasFile(MainAppWindow mainWin, savedTab fileData)
        {
            mainWin.mainTabsVM.AddTab();
            mainWin.canvasLogoAction(false);
            UndoAPI.SetUndoButtonActive(mainWin, false);
            mainWin.selectedTab.TabName = fileData.Name;
            deserializeXml(mainWin, fileData);
            //openStrokeFile(mainWin, fileData);

            mainWin.tabDataList[mainWin.mainTabsVM.SelectedTab].tabUndoManager.SaveState(mainWin.DrawingCanvas);
        }

        public static void readSaveTabsDB(MainAppWindow mainWin)
        {
            using (SQLiteConnection connection = new(App.savedTabsDB))
            {
                connection.CreateTable<savedTab>();
                var tabs = connection.Table<savedTab>().ToList();

                mainWin.activate_load_window(tabs);
            }
        }

        public static void deleteTabsDB(MainAppWindow mainWin, savedTab fileData)
        {
            using (SQLiteConnection connection = new(App.savedTabsDB))
            {
                connection.CreateTable<savedTab>();
                connection.Delete(fileData);

            }
        }

        //public static void openStrokeFile(MainAppWindow mainWin, savedTab fileData)
        //{
        //    string filePath = fileData.Path + ".ink";
        //    //string filePath = Path.Combine(App.savedCanvasDirectory, "test.ink");

        //    using (System.IO.FileStream fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Open))
        //    {
        //        StrokeCollection loadedStrokes = new StrokeCollection(fileStream);
        //        fileStream.Close();

        //        mainWin.DrawingCanvas.Strokes.Clear();
        //        mainWin.DrawingCanvas.Strokes.Add(loadedStrokes);
        //    }
        //}

        //public static void SaveStrokesToFile(MainAppWindow mainWin)
        //{
        //    canvasState current_canvas = mainWin.selectedTab.canvasData.tabUndoManager.getCurrentState();

        //    string filePath = Path.Combine(App.savedCanvasDirectory, mainWin.selectedTab.TabName + ".ink");
        //    //string filePath = Path.Combine(App.savedCanvasDirectory, "test.ink");

        //    StrokeCollection strokes = current_canvas._extendedStroke.Strokes;

        //    using (System.IO.FileStream fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
        //    {
        //        strokes.Save(fileStream);
        //        fileStream.Close();
        //    }
        //}

        public static void serializeCanvasToXml(MainAppWindow mainWin)
        {
            CanvasState? currentCanvas = mainWin.selectedTab.canvasData.tabUndoManager.getCurrentState();

            string filePath = Path.Combine(App.savedCanvasDirectory, mainWin.selectedTab.TabName+".xml");

            var xmlSerializer = new XmlSerializer(typeof(CanvasState));
            using (var writer = new StreamWriter(filePath))
            {
                xmlSerializer.Serialize(writer, currentCanvas);
            }
        }

        public static void deserializeXml(MainAppWindow mainWin, savedTab fileData)
        {
            string filePath = fileData.Path + ".xml";

            XmlSerializer xmlSerializer = new(typeof(CanvasState));
            using StreamReader reader = new(filePath);
            var canvasContent = (CanvasState)xmlSerializer.Deserialize(reader);
            CanvasAnalyzer.reconstructCanvas(mainWin.DrawingCanvas, canvasContent);
        }

        public static void saveCanvasThumb(MainAppWindow mainWin)
        {
            string filePath = Path.Combine(App.savedCanvasDirectory, mainWin.selectedTab.TabName + ".png");

            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)mainWin.DrawingCanvas.ActualWidth,
                                                                      (int)mainWin.DrawingCanvas.ActualHeight,
                                                                      96d,
                                                                      96d,
                                                                      PixelFormats.Default);

            renderBitmap.Render(mainWin.DrawingCanvas);

            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            if (File.Exists(filePath))
            {
                using (FileStream existingStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                }

                System.Threading.Thread.Sleep(1000);
            }

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                pngEncoder.Save(fileStream);
            }
        }

    }
}
