using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    public class Field : IEnumerable<GameCell>
    {
        private readonly GameCell[,] model;//
        
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

        internal GameCell this[Point index] => model[index.X, index.Y];

        internal GameCell this[int x, int y] => model[x, y];

        internal void SetNewType(int x, int y, CellType newType)
        {
            model[x, y].SetNewType(newType);
        }

        internal void SetNewType(Point cell, CellType newType)
        {
            model[cell.X, cell.Y].SetNewType(newType);
        }

        public IEnumerator<GameCell> GetEnumerator()
            => model.Cast<GameCell>().GetEnumerator();
        
        internal void SetShip(int x, int y, Ship ship)
        {
            model[x, y].Ship = ship;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}