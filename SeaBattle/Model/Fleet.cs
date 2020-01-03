using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SeaBattle.Properties;

namespace SeaBattle
{
    internal class Fleet
    {
        internal Dictionary<int, int> Status { get; private set; } = new Dictionary<int, int>();
        internal List<Ship> Ships { get; private set; }  = new List<Ship>();
        internal int Health => Ships.Count;

        internal void AddShip(Ship ship)
        {
            Ships.Add(ship);
            if (Status.ContainsKey(ship.Size))
                Status[ship.Size]++;
            else
                Status[ship.Size] = 1;
        }

        internal void Remove(Ship deadShip)
        {
            if (!Ships.Contains(deadShip)) throw new ArgumentException(Resources.Removing_Not_Exist_Ship);
            Ships.Remove(deadShip);
            Status[deadShip.Size]--;
            if (Status[deadShip.Size] < 0) 
                throw new ArgumentException(Resources.Negative_Ships_Amount);
            if (Status[deadShip.Size] == 0) Status.Remove(deadShip.Size);//
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            foreach (var stEntry in Status.OrderByDescending(e => e.Key))
                result.Append($"{ GetShipSymPicture(stEntry.Key) } { stEntry.Value }\n");
            
            return result.ToString();
        }     

        private string GetShipSymPicture(int size)
        {
            var result = "";
            for (var i = 0; i < size; i++)
                result += "\u25A0";
            return result;
        }
    }
}