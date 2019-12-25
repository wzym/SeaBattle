using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    internal class SimpleLongestShipSearcher : TurnGenerator
    {
        protected List<Point> GetSearchingTurns()
        {
            var wrongPoint = new Point(-1, -1);
            var cells = new Point[Model.GetLength(0), Model.GetLength(1)];
            foreach (var cell in Model)
                cells[cell.X, cell.Y] = cell.Type == CellType.Sea ? new Point(cell.X, cell.Y) : wrongPoint;

            foreach (var  cell in Model)
                if (cell.Type == CellType.Exploded)
                    RemoveNotLargestCells(cells, cell.X, cell.Y);

            return cells.Cast<Point>().Where(p => p != wrongPoint).ToList();
        }
        
        private void RemoveNotLargestCells(Point[,] cells, int x, int y)
        {
            var wrongPoint = new Point(-1, -1);
            var buffDistance = LongestShipLength() - 1;

            for (var i = 0; i < buffDistance; i++)
            {
                if (x + i < cells.GetLength(0)) cells[x + i, y] = wrongPoint;
                if (x - i >= 0) cells[x - i, y] = wrongPoint;
                if (y + i < cells.GetLength(1)) cells[x, y + i] = wrongPoint;
                if (y - i >= 0) cells[x, y - i] = wrongPoint;
            }
        }
    }
}