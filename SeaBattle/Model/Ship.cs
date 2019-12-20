﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SeaBattle
{
    internal class Ship
    {
        internal bool IsHorizontal { get; private set; }
        internal Point HeadPosition { get; private set; }
        internal int Size { get; }
        private int Health { get; set; }
        internal bool IsDead => Health <= 0;

        internal Ship(Point head, int size, bool isHorizontal = true)
        {
            HeadPosition = head;
            IsHorizontal = isHorizontal;
            Size = size;
            Health = size;
        }

        internal void SetDamage()
        {
            Health--;
        }

        internal static IEnumerable<Point> PreBody(Ship ship)
        {
            var curSegment = ship.HeadPosition;
            for(var i = 0; i <= ship.Size; i++)
            {
                yield return curSegment;
                curSegment = IteratePoint(ship.HeadPosition, ship.IsHorizontal, i);
            }
        }

        internal static IEnumerable<Point> PreBuffer(Ship ship)
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

        internal void Seil(Point newHead)
        {
            HeadPosition = newHead;
        }

        internal void Reverse()
            => IsHorizontal = !IsHorizontal;

        internal void Seil(object findSuitablePlace)
        {
            throw new NotImplementedException();
        }
    }
}