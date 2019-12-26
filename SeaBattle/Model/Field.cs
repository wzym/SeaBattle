using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    public class Field : IEnumerable<GameCell>
    {
        public readonly GameCell[,] model;//
        
        internal int Height => model.GetLength(1);
        internal int Width => model.GetLength(0);
        
        public Field()
        {
            model = new GameCell[GameModel.WidthOfField + 2, GameModel.HeightOfField + 2];
            InitializeModel();
        }

        private void InitializeModel()
        {
            FillWorkingCells();
            FillBorders();
        }

        private void FillBorders()
        {
            for (var y = 0; y < model.GetLength(1); y++)
            {
                model[0, y] = new GameCell(CellType.Bomb, 0, y);
                model[model.GetLength(0) - 1, y] =
                    new GameCell(CellType.Bomb, model.GetLength(0) - 1, y);
            }

            for (var x = 1; x < model.GetLength(0) - 1; x++)
            {
                model[x, 0] = new GameCell(CellType.Bomb, x, 0);
                model[x, model.GetLength(1) - 1] =
                    new GameCell(CellType.Bomb, x, model.GetLength(1) - 1);
            }
        }

        private void FillWorkingCells()
        {
            for (var y = 1; y < model.GetLength(1) - 1; y++)
            for (var x = 1; x < model.GetLength(0) - 1; x++)
                model[x, y] = new GameCell(CellType.Sea, x, y);
        }

        internal GameCell this[Point index]
        {
            get => model[index.X, index.Y];
            set => model[index.X, index.Y] = value;
        }

        internal GameCell this[int x, int y]
        {
            get => model[x, y];
            set => model[x, y] = value;
        }

        internal void SetNewType(int x, int y, CellType newType)
        {
            model[x, y].SetNewType(newType);
        }

        internal void SetNewType(Point cell, CellType newType)
        {
            SetNewType(cell.X, cell.Y, newType);
        }

        internal static IEnumerable<Point> GetWorkingCellsIndexes(Field field)
        {
            for (var y = 1; y < field.Height - 1; y++)
            for (var x = 1; x < field.Width - 1; x++)
                yield return new Point(x, y);
        }

        internal static IEnumerable<GameCell> GetWorkingCells(Field field)
            => GetWorkingCellsIndexes(field).Select(pIndex => field[pIndex]);

        public IEnumerator<GameCell> GetEnumerator()
        {
            foreach (var cell in model)
                yield return cell;
        }

        internal void SetShip(int x, int y, Ship ship)
        {
            model[x, y].Ship = ship;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}