using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    internal abstract class TurnGenerator
    {
        protected readonly GameCell[,] Model;
        protected PresumedShip PresumedShip;
        protected readonly Random Rnd = new Random();
        protected IEnumerator<Point> Enumerator;
        protected Dictionary<int, int> PresumedFleet { get; private set; }
        
        internal TurnGenerator()
        {
            Model = new GameCell[GameModel.WidthOfField + 2, GameModel.HeightOfField + 2];
            InitializeModel();
            SetPresumedFleet();
        }

        private void SetPresumedFleet()
        {
            PresumedFleet = new Dictionary<int, int>();
            foreach (var (length, amount) in GameModel.FleetParams)
                PresumedFleet[length] = amount;
        }

        protected int LongestShipLength()
            => PresumedFleet
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

        internal abstract void ReturnResultBack(GameCell result);

        internal abstract void ReportAbtDeath(Ship ship);

        internal Point Next()
        {            
            if (!Enumerator.MoveNext()) throw new Exception("У ума ходы кончились.");            
            return Enumerator.Current;
        }
    }
}