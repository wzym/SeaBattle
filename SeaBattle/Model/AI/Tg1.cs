using System.Collections.Generic;
using System.Drawing;

namespace SeaBattle
{
    internal class Tg1 : TurnGenerator
    {
        protected override List<Point> GetSearchingTurns()
        {
            var result = new List<Point>();
            for (var y = 1; y < Model.GetLength(1) - 1; y++)
            for (var x = 1; x < Model.GetLength(0) - 1; x++)
                if (Model[x, y].Type == CellType.Sea)
                    result.Add(new Point(x, y));
            return result;
        }

        protected override List<Point> GetFinishingOffTurns()
            => GetSearchingTurns();
    }
}