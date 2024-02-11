using System;
using System.IO;
using System.Windows;

namespace JotWin
{
    public partial class App : Application
    {
        public static string savedTabsDB = Path.Combine(savedCanvasDirectory, "JotTabsDB.db");
        public static string savedCanvasDirectory
        {
            get
            {
                string folderPath = Path.Combine(Environment.CurrentDirectory, "savedCanvas");

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                return folderPath;
            }
        }
    }
}
