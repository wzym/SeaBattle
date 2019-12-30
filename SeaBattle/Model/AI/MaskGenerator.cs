using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    internal class MaskGenerator : TurnGenerator
    {
        private int currentLength;
        private List<Point> processingTurns;

        public MaskGenerator()
        {
            currentLength = MaxShipLength;
            FormNotProcessedTurns();
        }

        protected override List<Point> GetSearchingTurns(int size)
        {
            if (size > currentLength) throw new Exception("Length of longest ship became longer.");
            if (size == currentLength)
            {
                ClearNotProcessTurns();
                return processingTurns;
            }
            currentLength = size;
            if (size == 1)
                processingTurns = NotCheckedCells;
            else
                FormNotProcessedTurns();
            return processingTurns;
        }
        
        private void FormNotProcessedTurns()
        {
            var mask = GetCellMask(currentLength);
            var size = mask.Length;
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
        
        private void ClearNotProcessTurns()
        {
            processingTurns = processingTurns
                .Where(t => Model[t].Type == CellType.Sea).ToList();
        }
        
        private static Point[] GetCellMask(int cellSize)
        {
            var xValues = new List<int>(cellSize);
            var yValues = new List<int>(cellSize);
            for (var i = 0; i < cellSize; i++)
            {
                xValues.Add(i);
                yValues.Add(i);
            }
            
            var mask = new Point[cellSize];
            for (var i = 0; i < mask.Length; i++)
                mask[i] = new Point(xValues.PullRandomly(), yValues.PullRandomly());
            
            return mask;
        }
    }
}