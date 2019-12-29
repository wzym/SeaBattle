using System.Collections.Generic;
using System.Drawing;

namespace SeaBattle
{
    internal class Ship
    {
        internal readonly string Name;
        internal bool IsHorizontal { get; private set; }
        internal Point HeadPosition { get; private set; }
        internal int Size { get; }
        private int Health { get; set; }
        internal bool IsDead => Health <= 0;

        internal Ship(Point head, int size, bool isHorizontal = true, string name = "")
        {
            this.Name = name;
            HeadPosition = head;
            IsHorizontal = isHorizontal;
            Size = size;
            Health = size;
        }

        internal void SetDamage()
        {
            Health--;
        }
        
        internal void Sail(Point newHead)
        {
            HeadPosition = newHead;
        }

        internal void Reverse()
            => IsHorizontal = !IsHorizontal;

        public override string ToString()
        {
            var orientation = IsHorizontal ? "horizontal" : "vertical";
            return $"{Name}: size - {Size}, {orientation}, head in {HeadPosition}";
        }
    }
}