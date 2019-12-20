using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SeaBattle
{
    internal class Fleet
    {
        internal List<Ship> Ships { get; private set; }  = new List<Ship>();
        internal int Health => Ships.Count;

        internal void AddShip(Ship ship)
        {
            Ships.Add(ship);
        }

        internal void SetShips(IEnumerable<Ship> fleet)
        {
            Ships = new List<Ship>();
            foreach (var ship in fleet)
                AddShip(ship);
        }

        internal void Remove(Ship deadShip)
        {
            Ships.Remove(deadShip);
        }

        public override string ToString()
        {
            var status = new Dictionary<int, int>();
            foreach(var ship in Ships)            
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