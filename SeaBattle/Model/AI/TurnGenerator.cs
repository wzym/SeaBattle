using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    internal class TurnGenerator
    {
        protected readonly Field Model;
        private PresumedShip presumedShip;
        private readonly IEnumerator<Point> enumerator;
        private Dictionary<int, int> presumedFleet;
        private List<Point> processingTurns;
        protected int MaxShipLength;

        internal TurnGenerator()
        {
            Model = new Field();           
            SetPresumedFleet();
            enumerator = GetEnumerator();
            FormNotProcessedTurns();            
        }

        private void SetPresumedFleet()
        {
            presumedFleet = new Dictionary<int, int>();
            foreach (var (length, amount) in GameModel.FleetParams)
                presumedFleet[length] = amount;
            MaxShipLength = LongestShipLength();
        }

        protected int LongestShipLength()
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
                yield return StaticMethods.GetRandomElement(turns);
            }
        }

        private List<Point> GetTurns()
            => presumedShip != null ? GetFinishingOffTurns() : GetSearchingTurns(); //processingTurns;//GetSearchingTurns();

        private List<Point> GetFinishingOffTurns()
            => presumedShip.GetFinishOffTurns().ToList();
        
        internal void ReportAbtDeath(Ship deadShip)
        {
            foreach (var cellBuff in Ship.PreBuffer(deadShip))
                Model.SetNewType(cellBuff, CellType.Bomb);

            DeleteShip(deadShip);
            if (presumedFleet.Count > 0 
                && deadShip.Size > MaxShipLength) 
                FormNotProcessedTurns();
            else ClearNotProcessTurns();
        }

        private void ClearNotProcessTurns()
        {
            processingTurns = processingTurns
                .Where(t => Model[t].Type == CellType.Sea).ToList();
        }

        private void FormNotProcessedTurns()
        {
            var mask = GetMaskCell();
            var size = mask.Length;
            if (size == 1)
            {
                processingTurns = NotCheckedCells;
                return;
            }
            processingTurns = new List<Point>();
            for (var y = 1; y <= GameModel.HeightOfField; y++)
            {
                var yMaskIndex = (y - 1) % size;
                for (var x = 1; x <= GameModel.WidthOfField; x++)
                {
                    var xMaskIndex = (x - 1) % size;

                    if (mask.Contains(new Point(xMaskIndex, yMaskIndex))
                        && Model[x, y].Type == CellType.Sea)
                        processingTurns.Add(new Point(x, y));
                }
            }
        }

        private void DeleteShip(Ship ship)
        {
            presumedFleet[ship.Size]--;
            if (presumedFleet[ship.Size] > 0) return;
            presumedFleet.Remove(ship.Size);
            if (ship.Size == MaxShipLength && presumedFleet.Count > 0)
                MaxShipLength = LongestShipLength();
        }

        private bool IsVariantPossible(Ship ship)
            => Ship.PreBody(ship)
                .All(point => Model[point].Type == CellType.Sea);

        internal void ReturnResultBack(GameCell result)
        {            
            Model.SetNewType(result.X, result.Y, result.Type);
            ClearNotProcessTurns();//optimize
            if (result.Type != CellType.Exploded) return;
            presumedShip?.ReportOnHit(new Point(result.X, result.Y));
            if (result.Ship.IsDead) presumedShip = null;
            else if (presumedShip == null) 
                presumedShip = new PresumedShip(new Point(result.X, result.Y), Model);
        }

        private List<Point> GetSearchingTurns()
        {
            var size = MaxShipLength;
            if (size == 1)
                return NotCheckedCells;

            var ofHorizontal = GetShipSearchTurns(size);
            var ofVertical = GetShipSearchTurns(size, false);

            ofHorizontal = ofHorizontal.Distinct().ToList();
            ofVertical = ofVertical.Distinct().ToList();
            var intersect = ofHorizontal.Intersect(ofVertical).ToList();
            if (intersect.Count != 0) return intersect;
            ofHorizontal.AddRange(ofVertical);
            return ofHorizontal;
        }

        private List<Point> GetShipSearchTurns(int size, bool isHorizontal = true)
        {
            var result = new List<Point>();
            foreach (var cell in Field.GetWorkingCellsIndexes(Model))
            {
                var ship = new Ship(cell, size, isHorizontal);
                if (IsVariantPossible(ship))
                    result.AddRange(Ship.PreBody(ship));
            }

            return result.Distinct().ToList();
        }

        private Point[] GetMaskCell()
        {
            var size = MaxShipLength;
            var xValues = new List<int>(size);
            var yValues = new List<int>(size);

            InitializeMask(size, xValues, yValues);
            return FillMaskRandomly(size, xValues, yValues);
        }

        private static Point[] FillMaskRandomly(int size, List<int> xValues, List<int> yValues)
        {
            var result = new Point[size];
            for (var i = 0; i < size; i++)
            {
                var x = StaticMethods.GetRandomElement(xValues);
                var y = StaticMethods.GetRandomElement(yValues);
                result[i] = new Point(x, y);
                xValues.Remove(x);
                yValues.Remove(y);
            }

            return result;
        }

        private static void InitializeMask(int size, ICollection<int> xValues, ICollection<int> yValues)
        {
            for (var i = 0; i < size; i++)
            {
                xValues.Add(i);
                yValues.Add(i);
            }
        }

        private List<Point> NotCheckedCells
            => Field.GetWorkingCellsIndexes(Model)
                .Where(p => Model[p].Type == CellType.Sea)
                .ToList();
    }
}