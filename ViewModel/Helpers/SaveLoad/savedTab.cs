using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace JotWin.ViewModel.Helpers.SaveLoad
{
    public class savedTab
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } 
        public string Name { get; set; }
        public string Path { get; set; }
        public string ImagePath { get; set; }
    }
}
