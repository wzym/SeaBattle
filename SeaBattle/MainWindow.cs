using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SeaBattle
{
    internal class MainWindow : Form
    {
        private const int widthFieldKoeff = 26;
        private const int heightFieldKoeff = 14;

        internal event Action<Point, MouseEventArgs> TurnDone;        
        internal event Action RestartGame;
        internal event Action<IEnumerable<Ship>> FleetIsChosen;

        private readonly Brush LeftBackgroundColour = Brushes.GreenYellow;
        private readonly Brush RightBackgroundColour = Brushes.BlueViolet;
        private int cellSize;
        private readonly GameCell[,] leftCells = new GameCell[GameModel.WidthOfField + 2, GameModel.HeightOfField + 2];
        private readonly GameCell[,] rightCells = new GameCell[GameModel.WidthOfField + 2, GameModel.HeightOfField + 2];
        private readonly Pen borderPen = new Pen(Brushes.Black, 1);
        private readonly Pen markPen = new Pen(Brushes.Black, 1);
        private int padding;
        private readonly Label leftInfo = new Label();
        private readonly Label rightInfo = new Label();
        private readonly Label globalGameInfo = new Label();        
        private readonly Button mainButton = new Button();
        private bool shipSettingRegime = true;        
        private Ship seilingShip = null;
        private IEnumerable<Ship> playerFleet;

        public MainWindow()
        {
            //Icon = new Icon("../../ship.ico");
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
            MouseClick += (sender, args) => ProcessClick(args.X, args.Y, args);
            MouseMove += (sender, args) => Sail(args);
            AddNActivateLabels();
            AddNActivateButtons();
            globalGameInfo.Text = "Крайние клавиши мыши - выбрать, установить и повернуть корабль.";
        }

        private void Sail(MouseEventArgs args)
        {
            if (seilingShip == null) return;
            if (IsPointInLeftField(args.X, args.Y))
            {
                var head = new Point(LeftXCellIndex(args.X), LeftYCellIndex(args.Y));
                if (!IsPlaceSuitableForSeiling(head)) return;

                foreach (var cell in Ship.PreBody(seilingShip))
                    leftCells[cell.X, cell.Y].SetNewType(CellType.Sea);
                seilingShip.Seil(head);
                foreach (var cell in Ship.PreBody(seilingShip))
                    leftCells[cell.X, cell.Y].SetNewType(CellType.SeilingShip);
            }
            Invalidate();
        }

        internal void Clear()
        {
            for(var y = 1; y < GameModel.HeightOfField + 1; y++)
                for(var x = 1; x < GameModel.WidthOfField + 1; x++)
                {
                    leftCells[x, y].SetNewType(CellType.Sea);
                    rightCells[x, y].SetNewType(CellType.Sea);
                }
            Paint -= winnerChecking; 
        }

        private void AddNActivateButtons()
        {
            mainButton.BackColor = Color.DarkSlateGray;
            mainButton.ForeColor = Color.White;
            mainButton.Click += BeforeGameOnButtonClick();
            mainButton.Text = "Start?";
            Controls.Add(mainButton);
            SetButtonPosition();
        }

        private EventHandler DuringGameOnButtonClick() => (sender, args) =>
        {
            shipSettingRegime = true;
            RestartGame.Invoke();
            mainButton.Click -= DuringGameOnButtonClick();
            mainButton.Click += BeforeGameOnButtonClick();
            globalGameInfo.Text = "Крайние клавиши мыши - выбрать, установить и повернуть корабль.";
        };
        private EventHandler BeforeGameOnButtonClick() => (sender, args) =>
        {
            if (!ShipsSetCorrect())
            {
                globalGameInfo.Text = "Неверная расстановка кораблей.";
                return;
            }
            shipSettingRegime = false;
            FleetIsChosen?.Invoke(playerFleet);            
            mainButton.Click -= BeforeGameOnButtonClick();
            mainButton.Click += DuringGameOnButtonClick();
        };

        private bool ShipsSetCorrect()
        {
            foreach (var ship in playerFleet)
                foreach (var cell in Ship.PreBuffer(ship))
                    if (leftCells[cell.X, cell.Y].Type == CellType.Ship)
                        return false;
            return true;
        }

        private void AddNActivateLabels()
        {            
            leftInfo.BackColor = Color.Transparent;
            rightInfo.BackColor = Color.Transparent;
            rightInfo.TextAlign = ContentAlignment.MiddleRight;
            leftInfo.Font = new Font("Times New Roman", 50);
            Controls.Add(leftInfo); 
            Controls.Add(rightInfo);

            globalGameInfo.Padding = new Padding(10, 0, 0, 1);
            globalGameInfo.ForeColor = Color.White;
            globalGameInfo.Font = new Font("Times New Roman", 30);
            globalGameInfo.TextAlign = ContentAlignment.MiddleLeft;
            globalGameInfo.BackColor = Color.DarkSlateGray;
            Controls.Add(globalGameInfo);            
        }

        internal void SetNewGameInfo(string leftStatus, string rightStatus)
        {
            leftInfo.Text = leftStatus;
            rightInfo.Text = rightStatus;
        }

        internal void SetGlobalInfo(string message)
            => globalGameInfo.Text = message;

        internal void DrawCells(IEnumerable<GameCell> cells, bool isLeftField)
        {            
            var field = isLeftField ? leftCells : rightCells;
            foreach (var cell in cells)            
                field[cell.X, cell.Y].SetNewType(cell.Type);
            
            Invalidate();
        }

        internal void DrawFleet(IEnumerable<Ship> fleet, bool inLeftField)
        {
            playerFleet = fleet;
            var field = inLeftField ? leftCells : rightCells;
            foreach(var ship in fleet)
            {
                foreach (var cell in Ship.PreBody(ship))
                {
                    field[cell.X, cell.Y].SetNewType(CellType.Ship);
                    field[cell.X, cell.Y].Ship = ship;
                }
                    
            }
            Invalidate();
        }

        private void ProcessClick(int x, int y, MouseEventArgs args)
        {
            switch(args.Button)
            {
                case MouseButtons.Left:
                    if (shipSettingRegime && IsPointInLeftField(x, y))
                    {
                        if (seilingShip != null)
                        {
                            seilingShip = null;
                            DrawFleet(playerFleet, true);
                        } else
                        {
                            var xOfCell = LeftXCellIndex(x);
                            var yOfCell = LeftYCellIndex(y);
                            if (leftCells[xOfCell, yOfCell].Type != CellType.Ship)
                                return;
                            seilingShip = leftCells[xOfCell, yOfCell].Ship;
                            foreach (var cell in Ship.PreBody(seilingShip))
                                leftCells[cell.X, cell.Y].SetNewType(CellType.SeilingShip);
                        }
                    }
                    else if (!shipSettingRegime && IsPointInRightField(x, y))
                    {
                        var xOfCell = (x - (ClientSize.Width - padding - GameModel.WidthOfField * cellSize)) / cellSize;
                        var yOfCell = (y - padding) / cellSize;
                        TurnDone?.Invoke(new Point(xOfCell + 1, yOfCell + 1), args);
                    }
                    else if (shipSettingRegime && IsPointInRightField(x, y))
                    {
                        var pos = new Random().Next(30);
                        if (pos == 1) globalGameInfo.Text = "Нехуй тыкать - расставляй по-человечески и играй.";
                    }
                    else if (!shipSettingRegime && IsPointInLeftField(x, y))
                    {
                        var pos = new Random().Next(20);
                        if (pos == 1) globalGameInfo.Text = "Вот ведь хуяшечки: столько бестолкового движения в одном месте.";
                    }
                    break;
                case MouseButtons.Right:
                    if (shipSettingRegime && seilingShip != null)
                    {
                        foreach(var cell in Ship.PreBody(seilingShip))
                            leftCells[cell.X, cell.Y].SetNewType(CellType.Sea);
                        seilingShip.Reverse();
                        seilingShip.Seil(FindSuitablePlace(new Point(LeftXCellIndex(x), LeftYCellIndex(y))));
                        foreach (var cell in Ship.PreBody(seilingShip))
                            leftCells[cell.X, cell.Y].SetNewType(CellType.SeilingShip);
                        Invalidate();
                    }
                    break;
            }
        }

        private Point FindSuitablePlace(Point source)
        {
            var visited = new HashSet<Point>();
            var presumed = new Queue<GameCell>();
            var currPlace = source;

            do
            {
                foreach (var cell in GetNeighbors(currPlace).Where(c => c.Type == CellType.Sea && !visited.Contains(new Point(c.X, c.Y))))
                    presumed.Enqueue(cell);
                if (presumed.Count < 1) throw new Exception("Not found suitable place.");
                var curr = presumed.Dequeue();
                currPlace = new Point(curr.X, curr.Y);
                if (IsPlaceSuitableForSeiling(currPlace)) return currPlace;
                visited.Add(currPlace);
            } while (true);
        }

        private IEnumerable<GameCell> GetNeighbors(Point currPlace)        
            => new List<GameCell>()
            {
                leftCells[currPlace.X - 1, currPlace.Y],
                leftCells[currPlace.X, currPlace.Y - 1],
                leftCells[currPlace.X + 1, currPlace.Y],
                leftCells[currPlace.X, currPlace.Y + 1],
                leftCells[currPlace.X - 1, currPlace.Y - 1],
                leftCells[currPlace.X + 1, currPlace.Y - 1],
                leftCells[currPlace.X - 1, currPlace.Y + 1],
                leftCells[currPlace.X + 1, currPlace.Y + 1]
            };        

        private bool IsPlaceSuitableForSeiling(Point place)
        {
            foreach (var cell in Ship.PreBody(new Ship(place, seilingShip.Size, seilingShip.IsHorizontal)))
                if (cell.X > GameModel.WidthOfField || cell.Y > GameModel.HeightOfField
                    || cell.X < 1 || cell.Y < 1
                    || leftCells[cell.X, cell.Y].Type == CellType.Ship)
                    return false;
            return true;
        }

        private int LeftXCellIndex(int x) => (x - padding) / cellSize + 1;
        private int LeftYCellIndex(int y) => (y - padding) / cellSize + 1;

        private bool IsPointInLeftField(int x, int y)
            => x > padding && x < GameModel.WidthOfField * cellSize + padding
                && y > padding && y < cellSize * GameModel.HeightOfField + padding;

        private bool IsPointInRightField(int x, int y)
            => x < ClientSize.Width - padding && x > ClientSize.Width - padding - cellSize * (GameModel.WidthOfField + 1)
                && y > padding && y < cellSize * (GameModel.HeightOfField + 1);

        private void ChangeSizes()
        {
            var byWidth = ClientSize.Width / widthFieldKoeff;
            var byHeight = ClientSize.Height / heightFieldKoeff;
            cellSize = Math.Min(byWidth, byHeight);
            markPen.Width = cellSize / 6;
        }

        private void ActivateCells()
        {
            for(var x = 0; x < GameModel.WidthOfField + 2; x++)
            {
                leftCells[x, 0] = new GameCell(CellType.Bomb, x, 0);
                leftCells[x, leftCells.GetLength(0) - 1] = new GameCell(CellType.Bomb, x, leftCells.GetLength(0) - 1);
                rightCells[x, 0] = new GameCell(CellType.Bomb, x, 0);
                rightCells[x, rightCells.GetLength(0) - 1] = new GameCell(CellType.Bomb, x, rightCells.GetLength(0) - 1);
            }
            for (var y = 0; y < GameModel.WidthOfField + 2; y++)
            {
                leftCells[0, y] = new GameCell(CellType.Bomb, y, 0);
                leftCells[leftCells.GetLength(1) - 1, y] = new GameCell(CellType.Bomb, leftCells.GetLength(1) - 1, y);
                rightCells[0, y] = new GameCell(CellType.Bomb, y, 0);
                rightCells[rightCells.GetLength(1) - 1, y] = new GameCell(CellType.Bomb, rightCells.GetLength(1) - 1, y);
            }
            for (var x = 1; x < GameModel.WidthOfField + 1; x++)
                for (var y = 1; y < GameModel.HeightOfField + 1; y++)
                { 
                    leftCells[x, y] = new GameCell(CellType.Sea, x, y);
                    rightCells[x, y] = new GameCell(CellType.Sea, x, y);
                }
        }

        private void ReDraw(Graphics g)
        {            
            padding = cellSize;
            DrawBackground(g);
            var leftX = padding;
            var rightX = ClientSize.Width - padding - GameModel.WidthOfField * cellSize;
            var cY = padding;
            for (var y = 1; y < GameModel.HeightOfField + 1; y++)
            {
                for (var x = 1; x < GameModel.WidthOfField + 1; x++)
                {
                    g.FillRectangle(Brushes.White, leftX, cY, cellSize, cellSize);
                    g.DrawRectangle(borderPen, leftX, cY, cellSize, cellSize);
                    g.FillRectangle(Brushes.White, rightX, cY, cellSize, cellSize);
                    g.DrawRectangle(borderPen, rightX, cY, cellSize, cellSize);
                    DrawCellObject(leftX, cY, leftCells[x, y].Type, g);
                    DrawCellObject(rightX, cY, rightCells[x, y].Type, g);

                    leftX += cellSize;
                    rightX += cellSize;
                }
                leftX = padding;
                rightX = ClientSize.Width - padding - GameModel.WidthOfField * cellSize;
                cY += cellSize;
            }
            MoveLabels();
            SetButtonPosition();
        }

        private void MoveLabels()
        {
            var boxHeight = cellSize * GameModel.HeightOfField / 2 - cellSize;
            var boxWidth = 5 * cellSize / 2;
            leftInfo.Height = boxHeight;
            leftInfo.Width = boxWidth;
            rightInfo.Height = boxHeight;
            rightInfo.Width = boxWidth;
            var font = new Font("Times New Roman", Math.Min(ClientSize.Width / 40, ClientSize.Height / 20));
            leftInfo.Font = font;
            rightInfo.Font = font;
            leftInfo.Top = 3 * padding / 2;
            leftInfo.Left = cellSize * GameModel.WidthOfField + 3 * padding / 2;
            rightInfo.Top = cellSize * GameModel.HeightOfField + padding / 2 - rightInfo.Height;
            rightInfo.Left = ClientSize.Width - 3 * padding / 2 - (GameModel.WidthOfField * cellSize) - rightInfo.Width;

            globalGameInfo.Height = cellSize;
            globalGameInfo.Width = ClientSize.Width - cellSize;
            globalGameInfo.Left = cellSize / 2;
            globalGameInfo.Top = GameModel.HeightOfField * cellSize + padding * 2;
            var globalFont = new Font("Times New Roman", font.Size / 6 * 5);
            globalGameInfo.Font = globalFont;
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
                case CellType.SeilingShip:
                    g.FillRectangle(Brushes.LightGray, x, y, cellSize, cellSize);
                    g.DrawRectangle(new Pen(Brushes.White, 1), x + cellBorder, y + cellBorder
                        , cellSize - 2 * cellBorder, cellSize - 2 * cellBorder);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cellType), cellType, null);
            }
        }

        public void CheckWinner(bool winnerIsLeft)
        {
            var colour = winnerIsLeft ? Brushes.Aqua : Brushes.Red;
            winnerChecking = (_, arg) =>
            arg.Graphics.FillRectangle(colour
            , ClientSize.Width - padding - (10 * cellSize), padding
            , 10 * cellSize, 10 * cellSize);
            Paint += winnerChecking;
        }

        private PaintEventHandler winnerChecking;        
    }
}