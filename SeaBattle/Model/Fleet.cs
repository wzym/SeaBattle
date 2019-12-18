using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public override string ToString()
        {
            var status = new Dictionary<int, int>();
            foreach(var ship in body)            
                if (status.ContainsKey(ship.Size))
                    status[ship.Size]++;
                else
                    status[ship.Size] = 1;
            
            var result = new StringBuilder();
            foreach(var stEntry in status.OrderByDescending(e => e.Key))                            
                result.Append($"{stEntry.Key} - {stEntry.Value}\n");
            
            return result.ToString();
        }
    }
}