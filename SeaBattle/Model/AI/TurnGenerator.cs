using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml;

namespace SeaBattle
{
    internal class TurnGenerator
    {
        protected readonly GameCell[,] Model;
        private PresumedShip presumedShip;
        private readonly Random rnd = new Random();
        private readonly IEnumerator<Point> enumerator;
        private Dictionary<int, int> presumedFleet;
        private List<Point> notProcessedTurns; 

        internal TurnGenerator()
        {
            Model = new GameCell[GameModel.WidthOfField + 2, GameModel.HeightOfField + 2];
            InitializeModel();
            SetPresumedFleet();
            enumerator = GetEnumerator();
            FormNotProcessedTurns();
        }

        private void SetPresumedFleet()
        {
            presumedFleet = new Dictionary<int, int>();
            foreach (var (length, amount) in GameModel.FleetParams)
                presumedFleet[length] = amount;
        }

        protected int LongestShipLength()
            => presumedFleet
                .Where(e => e.Value > 0)
                .Max(e => e.Key);

        private void InitializeModel()
        {
            for (var y = 1; y < Model.GetLength(1) - 1; y++)
                for (var x = 1; x < Model.GetLength(0) - 1; x++)
                    Model[x, y] = new GameCell(CellType.Sea, x, y);

            for (var y = 0; y < Model.GetLength(1); y++)
            {
                Model[0, y] = new GameCell(CellType.Bomb, 0, y);
                Model[Model.GetLength(0) - 1, y] = 
                    new GameCell(CellType.Bomb, Model.GetLength(0) - 1, y);
            }
            for (var x = 1; x < Model.GetLength(0) - 1; x++)
            {
                Model[x, 0] = new GameCell(CellType.Bomb, x, 0);
                Model[x, Model.GetLength(1) - 1] = 
                    new GameCell(CellType.Bomb, x, Model.GetLength(1) - 1);
            }
        }

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
                yield return GetRandomElement(turns);
            }
        }

        private List<Point> GetTurns()
            => presumedShip != null ? GetFinishingOffTurns() : notProcessedTurns;//GetSearchingTurns();

        private List<Point> GetFinishingOffTurns()
            => presumedShip.GetFinishOffTurns().ToList();
        
        internal void ReportAbtDeath(Ship deadShip)
        {
            foreach (var cellBuff in Ship.PreBuffer(deadShip))
                Model[cellBuff.X, cellBuff.Y].SetNewType(CellType.Bomb);
            DeleteShip(deadShip);
            if (presumedFleet.Count > 0 && deadShip.Size > LongestShipLength()) FormNotProcessedTurns();
            else ClearNotProcessTurns();
        }

        private void ClearNotProcessTurns()
        {
            notProcessedTurns = notProcessedTurns
                .Where(t => Model[t.X, t.Y].Type == CellType.Sea).ToList();
        }

        private void FormNotProcessedTurns()
        {
            var mask = GetMaskCell();
            var size = mask.Length;
            if (size == 1)
            {
                notProcessedTurns = NotCheckedCells;
                return;
            }
            notProcessedTurns = new List<Point>();
            for (var y = 1; y <= GameModel.HeightOfField; y++)
            {
                var restY = (y - 1) % size;
                for (var x = 1; x <= GameModel.WidthOfField; x++)
                {
                    var restX = (x - 1) % size;

                    if (mask.Contains(new Point(restX, restY))
                        && Model[x, y].Type == CellType.Sea)
                        notProcessedTurns.Add(new Point(x, y));
                }
            }
        }

        private void DeleteShip(Ship ship)
        {
            presumedFleet[ship.Size]--;
            if (presumedFleet[ship.Size] <= 0) 
                presumedFleet.Remove(ship.Size);
        }

        private bool IsVariantPossible(Ship ship)
            => Ship.PreBody(ship).All(point => Model[point.X, point.Y].Type == CellType.Sea);

        internal void ReturnResultBack(GameCell result)
        {
            Model[result.X, result.Y].SetNewType(result.Type);
            ClearNotProcessTurns();//optimize
            if (result.Type != CellType.Exploded) return;
            presumedShip?.ReportOnHit(new Point(result.X, result.Y));
            if (result.Ship.IsDead) presumedShip = null;
            else if (presumedShip == null) 
                presumedShip = new PresumedShip(new Point(result.X, result.Y), Model);
        }

        private List<Point> GetSearchingTurns()
        {
            var size = LongestShipLength();
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
            foreach (var cell in GameModel.WorkingCells(Model))
            {
                var ship = new Ship(cell, size, isHorizontal);
                if (IsVariantPossible(ship))
                    result.AddRange(Ship.PreBody(ship));
            }

            return result.Distinct().ToList();
        }

        private Point[] GetMaskCell()
        {
            var size = LongestShipLength();
            var result = new Point[size];
            var xValues = new List<int>();
            var yValues = new List<int>();

            for (var i = 0; i < size; i++)
            { 
                xValues.Add(i);
                yValues.Add(i);
            }

            for (var i = 0; i < size; i++)
            {
                var x = GetRandomElement(xValues);
                var y = GetRandomElement(yValues);
                result[i] = new Point(x, y);
                xValues.Remove(x);
                yValues.Remove(y);
            }
            return result;
        }
        
        private T GetRandomElement<T>(IReadOnlyList<T> sequence)
            => sequence[rnd.Next(sequence.Count)];
        
        
        private List<Point> NotCheckedCells
            => GameModel.WorkingCells(Model)
                .Where(p => Model[p.X, p.Y].Type == CellType.Sea)
                .ToList();
    }
}