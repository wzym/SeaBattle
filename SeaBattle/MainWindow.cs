using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace SeaBattle
{
    internal class MainWindow : Form
    {
        internal event Action<Point> UserClick;
        private int cellSize;
        private readonly CellType[,] leftCells = new CellType[10, 10];
        private readonly CellType[,] rightCells = new CellType[10, 10];
        private readonly Pen borderPen = new Pen(Brushes.Black, 1);
        private readonly Pen markPen = new Pen(Brushes.Black, 1);
        private int padding;

        public MainWindow()
        {            
            DoubleBuffered = true;
            ClientSize = new Size(2000, 1000);
            MinimumSize = new Size(800, 400);
            ChangeSizes();
            ActivateCells();

            Paint += (sender, args) => ReDraw(args.Graphics);
            SizeChanged += (sender, args) =>
            {
                ChangeSizes();
                Invalidate();
            };
            MouseClick += (sender, args) => ProcessClick(args.X, args.Y);
        }

        public void DrawCell(Point place, CellType type, bool left)
        {
            if (left)
                leftCells[place.X, place.Y] = type;
            else
                rightCells[place.X, place.Y] = type;
            Invalidate();
        }

        internal void DrawCells(IEnumerable<GameCell> cells, bool isLeftField)
        {            
            var field = isLeftField ? leftCells : rightCells;
            foreach (var cell in cells)            
                field[cell.X - 1, cell.Y - 1] = cell.Type;
            
            Invalidate();
        }

        private void ProcessClick(int x, int y)
        {
            if (x < ClientSize.Width - padding && x > ClientSize.Width - padding - 11 * cellSize
                && y > padding && y < 11 * cellSize)
            {
                var xOfCell = (x - (ClientSize.Width - padding - 10 * cellSize)) / cellSize;
                var yOfCell = (y - padding) / cellSize;
                UserClick?.Invoke(new Point(xOfCell + 1, yOfCell + 1));
            }
        }

        private void ChangeSizes()
        {
            var byWidth = ClientSize.Width / 2.5 / 10;
            var byHeight = ClientSize.Height / 1.2 / 10;
            cellSize = (int)Math.Min(byWidth, byHeight);
            markPen.Width = cellSize / 6;
        }

        private void ActivateCells()
        {
            for (var i = 0; i < 10; i++)
                for (var j = 0; j < 10; j++)
                { 
                    leftCells[i, j] = CellType.Sea;
                    rightCells[i, j] = CellType.Sea;
                }
        }

        private void ReDraw(Graphics g)
        {
            padding = cellSize;
            var leftX = padding;
            var rightX = ClientSize.Width - padding - 10 * cellSize;
            var cY = padding;
            for (var y = 0; y < 10; y++)
            {
                for (var x = 0; x < 10; x++)
                {
                    g.FillRectangle(Brushes.White, leftX, cY, cellSize, cellSize);
                    g.DrawRectangle(borderPen, leftX, cY, cellSize, cellSize);
                    g.FillRectangle(Brushes.White, rightX, cY, cellSize, cellSize);
                    g.DrawRectangle(borderPen, rightX, cY, cellSize, cellSize);
                    DrawCellObject(leftX, cY, leftCells[x, y], g);
                    DrawCellObject(rightX, cY, rightCells[x, y], g);


                    leftX += cellSize;
                    rightX += cellSize;
                }
                leftX = padding;
                rightX = ClientSize.Width - padding - (10 * cellSize);
                cY += cellSize;
            }           
        }

        private void DrawCellObject(int x, int y, CellType cellType, Graphics g)
        {
            var cellBorder = cellSize * 0.15f;
            switch (cellType)
            {
                case CellType.Bomb:                                        
                    var d = cellSize * 0.7f;                   
                    g.DrawEllipse(markPen, x + cellBorder, y + cellBorder, d, d);                    
                    break;                    
                case CellType.Exploded:
                    g.DrawLine(markPen, x + cellBorder, y + cellBorder
                            , x + cellSize - cellBorder, y + cellSize - cellBorder);
                    g.DrawLine(markPen, x + cellBorder, y + cellSize - cellBorder
                        , x + cellSize - cellBorder, y + cellBorder);
                    break;                    
                case CellType.Ship:
                    g.FillRectangle(Brushes.Black, x, y, cellSize, cellSize);
                    g.DrawRectangle(new Pen(Brushes.White, 1), x + cellBorder, y + cellBorder
                        , cellSize - 2 * cellBorder, cellSize - 2 * cellBorder);
                    break;                    
                case CellType.Sea:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cellType), cellType, null);
            }
        }

        public void CheckWinner(bool winnerIsLeft)
        {
            var colour = winnerIsLeft ? Brushes.Aqua : Brushes.Red;
            Paint += (_, arg) =>
            arg.Graphics.FillRectangle(colour
            , ClientSize.Width - padding - (10 * cellSize), padding
            , 10 * cellSize, 10 * cellSize);
        }
    }
}