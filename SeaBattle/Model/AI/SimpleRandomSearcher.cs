using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    internal class SimpleRandomSearcher : TurnGenerator
    {
        protected override List<Point> GetSearchingTurns()
            => (from p in GameModel.WorkingCells(Model)
                    where Model[p.X, p.Y].Type == CellType.Sea
                    select new Point(p.X, p.Y))
                .ToList();
    }
}