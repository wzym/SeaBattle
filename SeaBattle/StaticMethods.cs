using System;
using System.Collections.Generic;
using System.Drawing;

namespace SeaBattle
{
    public static class StaticMethods
    {
        private static readonly Random Rnd = new Random();
        
        internal static T GetRandomElement<T>(IReadOnlyList<T> sequence)
            => sequence[Rnd.Next(sequence.Count)];

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