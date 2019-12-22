using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    internal abstract class TurnGenerator
    {
        protected readonly GameCell[,] Model;
        private PresumedShip presumedShip;
        private readonly Random rnd = new Random();
        private readonly IEnumerator<Point> enumerator;
        private Dictionary<int, int> presumedFleet;
        
        internal TurnGenerator()
        {
            Model = new GameCell[GameModel.WidthOfField + 2, GameModel.HeightOfField + 2];
            InitializeModel();
            SetPresumedFleet();
            enumerator = GetEnumerator();
        }

        private void SetPresumedFleet()
        {
            presumedFleet = new Dictionary<int, int>();
            foreach (var (length, amount) in GameModel.FleetParams)
                presumedFleet[length] = amount;
        }

        protected int LongestShipLength()
            => presumedFleet
                .Where(e => e.Value > 0)
                .Max(e => e.Key);

        private void InitializeModel()
        {
            for (var y = 1; y < Model.GetLength(1) - 1; y++)
                for (var x = 1; x < Model.GetLength(0) - 1; x++)
                    Model[x, y] = new GameCell(CellType.Sea, x, y);

            for (var y = 0; y < Model.GetLength(1); y++)
            {
                Model[0, y] = new GameCell(CellType.Bomb, 0, y);
                Model[Model.GetLength(0) - 1, y] = 
                    new GameCell(CellType.Bomb, Model.GetLength(0) - 1, y);
            }
            for (var x = 1; x < Model.GetLength(0) - 1; x++)
            {
                Model[x, 0] = new GameCell(CellType.Bomb, x, 0);
                Model[x, Model.GetLength(1) - 1] = 
                    new GameCell(CellType.Bomb, x, Model.GetLength(1) - 1);
            }
        }

        internal Point NextTurn()
            => enumerator.MoveNext() ? 
                enumerator.Current : 
                throw new Exception("У ума ходы кончились.");

        private IEnumerator<Point> GetEnumerator()
        {
            while(true)
            {
                var turns = GetTurns();
                if (turns.Count == 0)
                    yield break;
                var index = rnd.Next(turns.Count);                
                yield return turns[index];
            }   
        }
        
        private List<Point> GetTurns()
            => presumedShip != null ? GetFinishingOffTurns() : GetSearchingTurns();

        protected abstract List<Point> GetSearchingTurns();

        private List<Point> GetFinishingOffTurns()
            => presumedShip.GetFinishOffTurns().ToList();
        
        internal void ReportAbtDeath(Ship ship)
        {
            foreach (var cellBuff in Ship.PreBuffer(ship))
                Model[cellBuff.X, cellBuff.Y].SetNewType(CellType.Bomb);
            DeleteShip(ship);
        }

        private void DeleteShip(Ship ship)
        {
            presumedFleet[ship.Size]--;
            if (presumedFleet[ship.Size] <= 0) 
                presumedFleet.Remove(ship.Size);
        }
        
        protected bool IsVariantPossible(Ship ship)
            => Ship.PreBody(ship).All(point => Model[point.X, point.Y].Type == CellType.Sea);

        internal void ReturnResultBack(GameCell result)
        {
            Model[result.X, result.Y].SetNewType(result.Type);
            if (result.Type != CellType.Exploded) return;
            presumedShip?.ReportOnHit(new Point(result.X, result.Y));
            if (result.Ship.IsDead) presumedShip = null;
            else if (presumedShip == null) 
                presumedShip = new PresumedShip(new Point(result.X, result.Y), Model);
        }
    }
}