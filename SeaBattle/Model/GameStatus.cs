using System;
using System.Collections.Generic;
using System.Linq;

namespace SeaBattle
{
    internal class GameStatus
    {        
        internal Player Player { get; }
        internal Player Rival { get; }
        internal bool IsGameContinues { get; private set; } = true;
        internal Player Active { get; private set; }
        internal Player Passive { get; private set; }
        internal static List<string> NamesForTheGameShips;
        
        internal GameStatus()
        {
            NamesForTheGameShips = CommonParameters.ShipNamesByHashSet.ToList();
            Player = new Player(false);
            Rival = new Player(true);
        }
        
        internal void RecordShipDeath(Ship deadShip)
        {
            foreach (var bufCell in deadShip.PreliminaryBuffer())
                Passive.Field.SetNewType(bufCell, CellType.Bomb);
            if (Active.IsArtificial) Active.TurnGenerator.ReportAbtDeath(deadShip);
            Passive.Fleet.Remove(deadShip);

            if (Passive.IsLost)
                IsGameContinues = false;            
        }

        internal void DefineActivity()
        {
            var rnd = new Random();
            if (rnd.Next(2) == 0)
            {
                Active = Player;
                Passive = Rival;
            }
            else
            {
                Active = Rival;
                Passive = Player;
            }
        }

        internal void InvertActivity()
        {
            var buffer = Active;
            Active = Passive;
            Passive = buffer;
        }

        internal void SetPlayerActive()
        {
            Active = Player;
            Passive = Rival;
        }
    }
}