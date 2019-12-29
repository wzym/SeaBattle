using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    public static class CommonMethods
    {
        private static readonly Random Rnd = new Random();

        internal static T GetRandomElement<T>(IReadOnlyList<T> sequence)
            => sequence[Rnd.Next(sequence.Count)];
        
        internal static IEnumerable<GameCell> GetWorkingCells(Field field)
            => GetWorkingCellsIndexes(field).Select(pIndex => field[pIndex]);
        
        internal static IEnumerable<Point> GetWorkingCellsIndexes(Field field)
        {
            for (var y = 1; y < field.Height - 1; y++)
            for (var x = 1; x < field.Width - 1; x++)
                yield return new Point(x, y);
        }

        internal static T PullElement<T>(this List<T> sequence)
        {
            var result = sequence[Rnd.Next(sequence.Count)];
            sequence.Remove(result);
            return result;
        }

        internal static IEnumerable<Point> PreliminaryBody(this Ship ship)
        {
            var curSegment = ship.HeadPosition;
            for(var i = 0; i <= ship.Size; i++)
            {
                yield return curSegment;
                curSegment = IteratePoint(ship.HeadPosition, ship.IsHorizontal, i);
            }
        }

        internal static IEnumerable<Point> PreliminaryBuffer(this Ship ship)
        {
            var i = -1;
            var curSegment = IteratePoint(ship.HeadPosition, ship.IsHorizontal, i);
            yield return curSegment;
            while (i < ship.Size + 1)
            {
                yield return IteratePoint(curSegment, !ship.IsHorizontal, -1);
                yield return IteratePoint(curSegment, !ship.IsHorizontal, 1);
                curSegment = IteratePoint(curSegment, ship.IsHorizontal, 1);
                i++;
            }
            yield return IteratePoint(curSegment, ship.IsHorizontal, -1);
        }
        
        private static Point IteratePoint(Point curr, bool isHorizontal, int k)
        {
            var newX = isHorizontal ? curr.X + k : curr.X;
            var newY = isHorizontal ? curr.Y : curr.Y + k;
            return new Point(newX, newY);
        }
    }
}