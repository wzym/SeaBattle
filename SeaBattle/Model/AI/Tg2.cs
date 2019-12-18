using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    internal class Tg2 : TurnGenerator
    {
        internal Tg2()
        {
            Enumerator = Sequence().GetEnumerator();
        }

        private IEnumerable<Point> Sequence()
        {            
            while(true)
            {
                var turns = GetTurns();
                if (turns.Count == 0)
                    yield break;
                var index = Rnd.Next(turns.Count);                
                yield return turns[index];
            }            
        }

        private List<Point> GetTurns()
            => PresumedShip != null ? PresumedShip.GetFinishOffTurns().ToList() : GetMaxLengthShipPoints();
        

        private List<Point> GetMaxLengthShipPoints()
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

        internal override void ReturnResultBack(GameCell result)
        {
            Model[result.X, result.Y].SetNewType(result.Type);
            if (result.Type != CellType.Exploded) return;
            PresumedShip?.ReportOnHit(new Point(result.X, result.Y));
            if (result.Ship.IsDead) PresumedShip = null;
            else if (PresumedShip == null) 
                    PresumedShip = new PresumedShip(new Point(result.X, result.Y), Model);
        }

        internal override void ReportAbtDeath(Ship ship)
        {
            foreach (var cellBuff in Ship.PreBuffer(ship))
                Model[cellBuff.X, cellBuff.Y].SetNewType(CellType.Bomb);
            PresumedFleet[ship.Size]--;
            if (PresumedFleet[ship.Size] <= 0) PresumedFleet.Remove(ship.Size);
        }
    }
}