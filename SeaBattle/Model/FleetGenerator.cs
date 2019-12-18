using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    internal class FleetGenerator
    {
        private readonly GameCell[,] field;
        private static readonly Random Rnd = new Random();

        public FleetGenerator(GameCell[,] field)
        {
            this.field = field;            
        }

        internal Ship SetAndGet(int decksAmount)
        {
            var variants = FormVariants(decksAmount);
            var index = Rnd.Next(variants.Count);
            var ship = variants[index];
            SetShip(ship);
            return ship;
        }

        private void SetShip(Ship ship)
        {
            foreach (var cell in Ship.PreBody(ship))
                field[cell.X, cell.Y].Ship = ship;
            foreach (var cell in Ship.PreBuffer(ship))
                field[cell.X, cell.Y].SetNewType(CellType.Bomb);
        }

        private List<Ship> FormVariants(int decksAmount)
        {
            var result = new List<Ship>();
            for(var y = 0; y < field.GetLength(1); y++)
                for(var x = 0; x <= field.GetLength(0) - decksAmount; x++)
                {
                    var variant = new Ship(new Point(x, y), decksAmount);
                    if (IsGood(variant)) result.Add(variant);                    
                }
            for(var y = 0; y < field.GetLength(1) - decksAmount; y++)
                for(var x = 0; x < field.GetLength(0); x++)
                {
                    var variant = new Ship(new Point(x, y), decksAmount, false);
                    if (IsGood(variant)) result.Add(variant);
                }
            return result;
        }

        private bool IsGood(Ship variant)
            => Ship.PreBody(variant)
                .All(cell => field[cell.X, cell.Y].Type == CellType.Sea);
    }
}