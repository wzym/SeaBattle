using System.Collections.Generic;
using System.Drawing;

namespace SeaBattle
{
    internal class Tg1 : TurnGenerator
    {
        internal Tg1()
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
        {
            var result = new List<Point>();
            for (var y = 1; y < Model.GetLength(1) - 1; y++)
            for (var x = 1; x < Model.GetLength(0) - 1; x++)
                if (Model[x, y].Type == CellType.Sea)
                    result.Add(new Point(x, y));
            return result;
        }

        internal override void ReturnResultBack(GameCell result)
        {
            Model[result.X, result.Y].SetNewType(result.Type);
        }

        internal override void ReportAbtDeath(Ship ship)
        {
            foreach (var cellBuff in Ship.PreBuffer(ship))
                Model[cellBuff.X, cellBuff.Y].SetNewType(CellType.Bomb);
        }
    }
}