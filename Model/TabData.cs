using JotWin.View;
using JotWin.ViewModel.Helpers.UndoManager;

namespace JotWin.Model
{
    public class TabData
    {
        public int Id { get; set; }
        public string? TabTitle { get; set; }

        public bool hasSketch { get; set; }

        public UndoAPI? tabUndoManager { get; set; }

        public int zIndexCount = 0; 
        public string? FileLocation { get; set; }
        public CanvasTemplate canvasBackground { get; set; }    
        public bool isForMajicJot { get; set; }
    }
}
