using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SeaBattle
{
    internal class MainWindow : Form
    {
        private const int widthFieldKoeff = 26;
        private const int heightFieldKoeff = 12;
        internal event Action<Point> UserClick;
        internal event Action MainButtonOnClick;
        private readonly Brush LeftBackgroundColour = Brushes.GreenYellow;
        private readonly Brush RightBackgroundColour = Brushes.BlueViolet;
        private int cellSize;
        private readonly CellType[,] leftCells = new CellType[10, 10];
        private readonly CellType[,] rightCells = new CellType[10, 10];
        private readonly Pen borderPen = new Pen(Brushes.Black, 1);
        private readonly Pen markPen = new Pen(Brushes.Black, 1);
        private int padding;
        private readonly Label leftInfo = new Label();
        private readonly Label rightInfo = new Label();
        private Font font = new Font("Times New Roman", 50);
        private readonly Button mainButton = new Button();

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
            AddNActivateInfoLabels();
            AddNActivateButtons();            
        }

        internal void Clear()
        {
            for(var y = 0; y < GameModel.HeightOfField; y++)
                for(var x = 0; x < GameModel.WidthOfField; x++)
                {
                    leftCells[x, y] = CellType.Sea;
                    rightCells[x, y] = CellType.Sea;
                }           
        }

        private void AddNActivateButtons()
        {
            mainButton.Click += (sender, args) => MainButtonOnClick.Invoke();
            mainButton.Text = "Start?";            
            Controls.Add(mainButton);
            SetButtonPosition();            
        }

        private void AddNActivateInfoLabels()
        {
            leftInfo.BackColor = Color.Transparent;
            rightInfo.BackColor = Color.Transparent;
            rightInfo.TextAlign = ContentAlignment.MiddleRight;
            leftInfo.Font = font;
            Controls.Add(leftInfo); 
            Controls.Add(rightInfo);
        }

        internal void SetNewGameInfo(string leftStatus, string rightStatus)
        {
            leftInfo.Text = leftStatus;
            rightInfo.Text = rightStatus;
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
            if (x < ClientSize.Width - padding && x > ClientSize.Width - padding - cellSize * (GameModel.WidthOfField + 1)
                && y > padding && y < cellSize * (GameModel.HeightOfField + 1))
            {
                var xOfCell = (x - (ClientSize.Width - padding - GameModel.WidthOfField * cellSize)) / cellSize;
                var yOfCell = (y - padding) / cellSize;
                UserClick?.Invoke(new Point(xOfCell + 1, yOfCell + 1));
            }
        }

        private void ChangeSizes()
        {
            var byWidth = ClientSize.Width / widthFieldKoeff;
            var byHeight = ClientSize.Height / heightFieldKoeff;
            cellSize = Math.Min(byWidth, byHeight);
            markPen.Width = cellSize / 6;
        }

        private void ActivateCells()
        {
            for (var x = 0; x < GameModel.WidthOfField; x++)
                for (var y = 0; y < GameModel.HeightOfField; y++)
                { 
                    leftCells[x, y] = CellType.Sea;
                    rightCells[x, y] = CellType.Sea;
                }
        }

        private void ReDraw(Graphics g)
        {            
            padding = cellSize;
            DrawBackground(g);
            var leftX = padding;
            var rightX = ClientSize.Width - padding - GameModel.WidthOfField * cellSize;
            var cY = padding;
            for (var y = 0; y < GameModel.HeightOfField; y++)
            {
                for (var x = 0; x < GameModel.WidthOfField; x++)
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
                rightX = ClientSize.Width - padding - GameModel.WidthOfField * cellSize;
                cY += cellSize;
            }
            MoveInfoLabels();
            SetButtonPosition();
        }

        private void MoveInfoLabels()
        {
            var boxHeight = cellSize * GameModel.HeightOfField / 2 - cellSize;
            var boxWidth = 5 * cellSize / 2;
            leftInfo.Height = boxHeight;
            leftInfo.Width = boxWidth;
            rightInfo.Height = boxHeight;
            rightInfo.Width = boxWidth;
            font = new Font("Times New Roman", Math.Min(ClientSize.Width / 40, ClientSize.Height / 20));
            leftInfo.Font = font;
            rightInfo.Font = font;
            leftInfo.Top = 3 * padding / 2;
            leftInfo.Left = cellSize * GameModel.WidthOfField + 3 * padding / 2;
            rightInfo.Top = cellSize * GameModel.HeightOfField + padding / 2 - rightInfo.Height;
            rightInfo.Left = ClientSize.Width - 3 * padding / 2 - (GameModel.WidthOfField * cellSize) - rightInfo.Width;
        }

        private void SetButtonPosition()
        {
            mainButton.Width = cellSize * 2;
            mainButton.Height = cellSize;
            mainButton.Font = new Font("Times New Roman", Math.Min(ClientSize.Width / 80, ClientSize.Height / 40));
            mainButton.Left = ClientSize.Width / 2 - mainButton.Width / 2;
            mainButton.Top = padding + GameModel.HeightOfField / 2 * cellSize - mainButton.Height / 2;
        }


        private void DrawBackground(Graphics g)
        {
            var leftX = padding / 2;
            var upY = padding / 2;
            var middleY = padding + cellSize * GameModel.HeightOfField / 2;
            var middleX1 = 3 * padding / 2 + GameModel.HeightOfField * cellSize;
            var middleX2 = GetRightFieldUpLeft().X - padding /2;
            var leftWidth = middleX2 - leftX;
            var heightY = cellSize * GameModel.HeightOfField + padding;
            var rightWidth = ClientSize.Width - padding / 2 - middleX2;
            var leftWidth2 = middleX2 - middleX1;

            g.FillRectangle(LeftBackgroundColour, leftX, upY, leftWidth, heightY);
            g.FillRectangle(RightBackgroundColour, middleX2, upY, rightWidth, heightY);
            g.FillRectangle(RightBackgroundColour, middleX1, middleY, leftWidth2, heightY / 2);
        }

        private Point GetRightFieldUpLeft() 
            => new Point(ClientSize.Width - padding - cellSize * GameModel.WidthOfField, padding);

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