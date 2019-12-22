using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    internal class LongestShipInTwoDimensionsSearcher : TurnGenerator
    {
        protected override List<Point> GetSearchingTurns()
        {
            var size = LongestShipLength();
            if (size == 1)
                return (from GameCell cell 
                        in Model 
                        where cell.Type == CellType.Sea 
                        select new Point(cell.X, cell.Y))
                    .ToList();
            
            var ofHorizontal = new List<Point>();
            var ofVertical = new List<Point>();
            foreach (var startPosition in GameModel.WorkingCells(Model))
            {
                var horizontal = new Ship(startPosition, size);
                var vertical = new Ship(startPosition, size, false);
                if (IsVariantPossible(horizontal))                    
                    ofHorizontal.AddRange(Ship.PreBody(horizontal));
                if (IsVariantPossible(vertical))
                    ofVertical.AddRange(Ship.PreBody(vertical));
            }
            ofHorizontal = ofHorizontal.Distinct().ToList();
            ofVertical = ofVertical.Distinct().ToList();
            var intersect = ofHorizontal.Intersect(ofVertical).ToList();
            if (intersect.Count != 0) return intersect;
            ofHorizontal.AddRange(ofVertical);
            return ofHorizontal;
        }
    }
}