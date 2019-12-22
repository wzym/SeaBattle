﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    internal class Tg3 : TurnGenerator
    {
        internal Tg3()
        {
            Enumerator = Sequence().GetEnumerator();
        }
        
        private IEnumerable<Point> Sequence()
        {            
            while(true)
            {
                var turns = GetTurns();
                if (turns.Count == 0)
                    yield break;
                var index = Rnd.Next(turns.Count);                
                yield return turns[index];
            }            
        }
        
        private List<Point> GetTurns()
            => PresumedShip != null ? PresumedShip.GetFinishOffTurns().ToList() : GetMaxLengthShipPoints();

        private List<Point> GetMaxLengthShipPoints()
        {
            var size = LongestShipLength();
            if (size == 1)
            {
                var result = new List<Point>();
                foreach(var cell in Model)                
                    if (cell.Type == CellType.Sea)
                        result.Add(new Point(cell.X, cell.Y));                
                return result;
            }
            var ofHorizontal = new List<Point>();
            var ofVertical = new List<Point>();
            foreach (var startPosition in GameModel.WorkingCells(Model))
            {
                var horizontal = new Ship(startPosition, size);
                var vertical = new Ship(startPosition, size, false);
                if (IsVariantPossible(horizontal))                    
                    ofHorizontal.AddRange(Ship.PreBody(horizontal));
                if (IsVariantPossible(vertical))
                    ofVertical.AddRange(Ship.PreBody(vertical));
            }
            ofHorizontal = ofHorizontal.Distinct().ToList();
            ofVertical = ofVertical.Distinct().ToList();
            var intersect = ofHorizontal.Intersect(ofVertical).ToList();
            if (intersect.Count != 0) return intersect;
            ofHorizontal.AddRange(ofVertical);
            return ofHorizontal;
        }

        private bool IsVariantPossible(Ship ship)
            => Ship.PreBody(ship).All(point => Model[point.X, point.Y].Type == CellType.Sea);

        internal override void ReturnResultBack(GameCell result)
        {
            Model[result.X, result.Y].SetNewType(result.Type);
            if (result.Type != CellType.Exploded) return;
            PresumedShip?.ReportOnHit(new Point(result.X, result.Y));
            if (result.Ship.IsDead) PresumedShip = null;
            else if (PresumedShip == null) 
                PresumedShip = new PresumedShip(new Point(result.X, result.Y), Model);
        }

        internal override void ReportAbtDeath(Ship ship)
        {
            foreach (var cellBuff in Ship.PreBuffer(ship))
                Model[cellBuff.X, cellBuff.Y].SetNewType(CellType.Bomb);
            DeleteShip(ship);
        }

        private void DeleteShip(Ship ship)
        {
            PresumedFleet[ship.Size]--;
            if (PresumedFleet[ship.Size] <= 0) 
                PresumedFleet.Remove(ship.Size);
        }
    }
}