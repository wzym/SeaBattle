using System.Collections.Generic;

namespace SeaBattle
{
    internal class Fleet
    {
        private readonly List<Ship> body = new List<Ship>();
        internal int Health => body.Count;

        internal void AddShip(Ship ship)
        {
            body.Add(ship);
        }

        public void Remove(Ship deadShip)
        {
            body.Remove(deadShip);
        }
    }
}