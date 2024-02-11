using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JotWin.View;
using Newtonsoft.Json;

namespace JotWin.ViewModel.Helpers
{
    public class ExtenalMonitorInfo
    {
        private static readonly string ExePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "thirdParty", "displayToJSON.exe");
        private static readonly string OutputFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,  "monitor_info.json");

        public static MonitorInfo monitorInfo; 

        public static int CursorX { get; private set; }
        public static int CursorY { get; private set; }
        public static int DisplayCount { get; private set; }

        public static DisplayInfo[] DisplayInfos { get; private set; }

        public ExtenalMonitorInfo()
        {
            LoadData();
        }

        public static void LoadData()
        {
            ExecuteDisplayToJSON();
            ReadJSONFile();
        }

        public static void resizeAppWindow(MainAppWindow mainWindow)
        {
            LoadData();

            System.Windows.Forms.Screen currentScreen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle);

            DisplayInfo displayInfo = DisplayInfos.FirstOrDefault(
                                    info => info.DeviceName.Contains(currentScreen.DeviceName, StringComparison.OrdinalIgnoreCase));

            mainWindow.Left = displayInfo.WorkingArea.X;
            mainWindow.Top = displayInfo.WorkingArea.Y;

            mainWindow.Width = displayInfo.WorkingArea.Width;
            mainWindow.Height = displayInfo.WorkingArea.Height;

        }

        private static void ExecuteDisplayToJSON()
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = ExePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using Process process = new() { StartInfo = startInfo };
            process.Start();
            process.WaitForExit();
        }

        private static void ReadJSONFile()
        {
            string jsonText = File.ReadAllText(OutputFilePath);
            monitorInfo = JsonConvert.DeserializeObject<MonitorInfo>(jsonText);

            CursorX = monitorInfo.CursorPosition.X;
            CursorY = monitorInfo.CursorPosition.Y;
            DisplayCount = monitorInfo.DisplayCount;

            DisplayInfos = new DisplayInfo[DisplayCount];

            for (int i = 0; i < DisplayCount; i++)
            {
                DisplayInfos[i] = new DisplayInfo
                {
                    DeviceName = monitorInfo.Displays[i].DeviceName,
                    Resolution = monitorInfo.Displays[i].Resolution,
                    WorkingArea = monitorInfo.Displays[i].WorkingArea
                };
            }
        }
    }

    public class MonitorInfo
    {
        [JsonProperty("cursorPosition")]
        public CursorPosition CursorPosition { get; set; }
        [JsonProperty("cursorRelative")]
        public CursorRelativeToDisplay cursorRelative { get; set; }
        [JsonProperty("displayCount")]
        public int DisplayCount { get; set; }
        [JsonProperty("displays")]
        public Display[] Displays { get; set; }
    }

    public class CursorRelativeToDisplay
    {
        public int Xcoordinates { get; set; }
        public int Ycoordinates { get; set; }
        [JsonProperty("cursorOn")]
        public string cursorOnDisplay { get; set; }
    }

    public class CursorPosition
    {
        [JsonProperty("x")]
        public int X { get; set; }
        [JsonProperty("y")]
        public int Y { get; set; }
    }

    public class Display
    {
        [JsonProperty("Device Name")]
        public string DeviceName { get; set; }

        [JsonProperty("Resolution")]
        public Resolution Resolution { get; set; }

        [JsonProperty("Working Area")]
        public WorkingArea WorkingArea { get; set; }
        [JsonProperty("isPrimary")]
        public bool isPrimary { get; set; }
    }


    public class DisplayInfo
    {
        public string DeviceName { get; set; }
        public Resolution Resolution { get; set; }
        public WorkingArea WorkingArea { get; set; }
        public bool isPrimary { get; set; }
    }

    public class Resolution
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class WorkingArea
    {
        public int Height { get; set; }
        public int Width { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
