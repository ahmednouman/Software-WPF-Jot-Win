using System;
using System.IO;
using System.Windows;
using System.Windows.Shell;

namespace JotWin.ViewModel.Helpers.MajicJot
{
    public class JumpListHelpers
    {
        public static void InitializeJumpList()
        {
            JumpList jumpList = new JumpList();

            JumpTask jumpMajic = new JumpTask
            {
                Title = "Magic Jot",
                CustomCategory = "Launch Menu",
                IconResourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "exportAssets", "magic_drop.ico"), 
                ApplicationPath = null,
                Arguments = "/openMagic",
                Description = "Open MagicJot"
            };

            jumpList.JumpItems.Add(jumpMajic);

            JumpTask jumpBlank = new JumpTask
            {
                Title = "Blank Jot",
                CustomCategory = "Launch Menu",
                IconResourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "exportAssets", "Blank.ico"),
                ApplicationPath = null,
                Arguments = "/openBlankJot",
                Description = "Open BlankJot"
            };

            jumpList.JumpItems.Add(jumpBlank);

            JumpTask jumpScreen = new JumpTask
            {
                Title = "Screen Jot",
                CustomCategory = "Launch Menu",
                IconResourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "exportAssets", "screenshot.ico"),
                ApplicationPath = null,
                Arguments = "/openScreenshot",
                Description = "Open ScreenJot"
            };

            jumpList.JumpItems.Add(jumpScreen);


            JumpList.SetJumpList(Application.Current, jumpList);
        }
    }


}
