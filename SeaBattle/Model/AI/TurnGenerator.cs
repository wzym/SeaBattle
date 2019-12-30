using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    internal abstract class TurnGenerator
    {
        protected readonly Field Model;
        private PresumedShip presumedShip;
        private readonly IEnumerator<Point> enumerator;
        private Dictionary<int, int> presumedFleet;
        
        protected int MaxShipLength;

        internal TurnGenerator()
        {
            Model = new Field();           
            SetPresumedFleet();
            enumerator = GetEnumerator();
        }

        private void SetPresumedFleet()
        {
            presumedFleet = new Dictionary<int, int>();
            foreach (var lengthAmount in CommonParameters.NewFleetParams)
                presumedFleet[lengthAmount.Key] = lengthAmount.Value;
            MaxShipLength = LongestShipLength();
        }

        private int LongestShipLength()
            => presumedFleet
                .Where(e => e.Value > 0)
                .Max(e => e.Key);

        internal Point NextTurn()
            => enumerator.MoveNext() ? 
                enumerator.Current : 
                throw new Exception("У ума ходы кончились.");

        private IEnumerator<Point> GetEnumerator()
        {
            while(true)
            {
                var turns = GetTurns();
                if (turns.Count == 0) yield break;
                yield return CommonMethods.GetRandomElement(turns);
            }
        }

        private List<Point> GetTurns()
            => presumedShip != null ? GetFinishingOffTurns() : GetSearchingTurns(MaxShipLength);

        private List<Point> GetFinishingOffTurns()
            => presumedShip.GetFinishOffTurns().ToList();
        
        internal void ReportAbtDeath(Ship deadShip)
        {
            foreach (var cellBuff in deadShip.PreliminaryBuffer())
                Model.SetNewType(cellBuff, CellType.Bomb);
            DeleteShip(deadShip);
        }

        private void DeleteShip(Ship ship)
        {
            presumedFleet[ship.Size]--;
            if (presumedFleet[ship.Size] > 0) return;
            presumedFleet.Remove(ship.Size);
            if (ship.Size == MaxShipLength && presumedFleet.Count > 0)
                MaxShipLength = LongestShipLength();
        }

       internal void ReturnResultBack(GameCell result)
        {            
            Model.SetNewType(result.X, result.Y, result.Type);
            if (result.Type != CellType.Exploded) return;
            presumedShip?.ReportOnHit(new Point(result.X, result.Y));
            if (result.Ship.IsDead) presumedShip = null;
            else if (presumedShip == null) 
                presumedShip = new PresumedShip(new Point(result.X, result.Y), Model);
        }

        protected abstract List<Point> GetSearchingTurns(int size);

        protected List<Point> NotCheckedCells
            => CommonMethods.GetWorkingCellsIndexes(Model)
                .Where(p => Model[p].Type == CellType.Sea)
                .ToList();
    }
}