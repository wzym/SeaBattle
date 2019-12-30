using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    internal class CalcGenerator : TurnGenerator
    {
        protected override List<Point> GetSearchingTurns(int size)
        {
            if (size == 1)
                return NotCheckedCells;

            var ofHorizontal = GetShipSearchTurns(size);
            var ofVertical = GetShipSearchTurns(size, false);

            ofHorizontal = ofHorizontal.Distinct().ToList();
            ofVertical = ofVertical.Distinct().ToList();
            var intersect = ofHorizontal.Intersect(ofVertical).ToList();
            if (intersect.Count != 0) return intersect;
            ofHorizontal.AddRange(ofVertical);
            return ofHorizontal;
        }
        
        private List<Point> GetShipSearchTurns(int size, bool isHorizontal = true)
        {
            var result = new List<Point>();
            foreach (var cell in CommonMethods.GetWorkingCellsIndexes(Model))
            {
                var ship = new Ship(cell, size, isHorizontal);
                if (IsVariantPossible(ship))
                    result.AddRange(ship.PreliminaryBody());
            }

            return result.Distinct().ToList();
        }
        
        private bool IsVariantPossible(Ship ship)
            => ship
                .PreliminaryBody()
                .All(point => Model[point].Type == CellType.Sea);
    }
}