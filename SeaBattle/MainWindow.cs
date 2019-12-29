using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SeaBattle.Properties;

namespace SeaBattle
{
    internal sealed class MainWindow : Form
    {
        private const double WindowStartSizeCoefficient = 0.6;
        private const int WidthCoefficient = 26;
        private const int HeightCoefficient = 14;
        private const string FontStyle = "Times New Roman";
        private const int WidthFontCoefficient = 50;
        private const int HeightFontCoefficient = 28;
        private const int MarkPenCoefficient = 6;
        private readonly Color leftBackground = Color.FromArgb(0x00, 0xBC, 0xD4);
        private readonly Color rightBackground = Color.FromArgb(0xFF, 0xEB, 0x3B);
        private readonly Color darkGray = Color.FromArgb(0x42, 0x42, 0x42);
        private readonly Color lightGray = Color.FromArgb(100, 0x42, 0x42, 0x42);
        private readonly Color whiteTextBack = Color.FromArgb(0xEE, 0xEE, 0xEE);
        private readonly Color whiteFieldBack = Color.FromArgb(0xFF, 0xFE, 0xFE);

        internal event Action<Point, MouseEventArgs> TurnDone;        
        internal event Action RestartGame;
        internal event Action<IEnumerable<Ship>> FleetIsChosen;

        private Brush leftBackgroundBrush;
        private Brush rightBackgroundBrush;
        private int cellSize;
        private readonly Field leftCells = new Field();
        private readonly Field rightCells = new Field();
        private readonly Pen borderPen;
        private readonly Pen markPen;
        private int padding;
        private readonly Label leftInfo = new Label();
        private readonly Label rightInfo = new Label();
        private readonly Label globalGameInfo = new Label();        
        private readonly Button mainButton = new Button();
        private bool shipSettingRegime = true;        
        private Ship sailingShip;
        private IEnumerable<Ship> playerFleet;

        public MainWindow()
        {
            BackColor = darkGray;
            borderPen = new Pen(darkGray, 1);
            markPen = new Pen(darkGray, 1);
            //Icon = new Icon("../../ship.ico");
            DoubleBuffered = true;
            SetClientSizeCore();            
            ChangeSizes();           
            
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
            globalGameInfo.Text = Resources.Ship_setting_instruction;
        }

        internal void MarkSurvivors(IEnumerable<Point> survivedDecks)
        {
            foreach(var cell in survivedDecks)            
                rightCells.SetNewType(cell, CellType.SailingShip);
            
            Invalidate();
        }

        private void SetClientSizeCore()
        {
            var screen = Screen.PrimaryScreen.Bounds;
            var x = Convert.ToInt32(screen.Width * WindowStartSizeCoefficient);
            var y = Convert.ToInt32(screen.Height * WindowStartSizeCoefficient);
            ClientSize = new Size(x, y);
            MinimumSize = new Size(600, 350);
        }

        private void Sail(MouseEventArgs args)
        {
            if (sailingShip == null) return;
            if (IsPointInLeftField(args.X, args.Y))
            {
                var head = new Point(LeftXCellIndex(args.X), LeftYCellIndex(args.Y));
                if (!IsPlaceSuitableForSailing(head)) return;

                foreach (var cell in sailingShip.PreliminaryBody())
                    leftCells.SetNewType(cell, CellType.Sea);
                sailingShip.Sail(head);
                foreach (var cell in sailingShip.PreliminaryBody())
                    leftCells.SetNewType(cell, CellType.SailingShip);
            }
            Invalidate();
        }

        internal void Clear()
        {
            foreach(var cell in Field.GetWorkingCellsIndexes(leftCells))            
                leftCells.SetNewType(cell, CellType.Sea);
                            
            foreach(var cell in Field.GetWorkingCellsIndexes(rightCells))
                rightCells.SetNewType(cell, CellType.Sea);
            
            leftBackgroundBrush = new SolidBrush(leftBackground);
            rightBackgroundBrush = new SolidBrush(rightBackground);
        }

        private void AddNActivateButtons()
        {
            mainButton.BackColor = whiteTextBack;
            mainButton.ForeColor = darkGray;
            mainButton.Click += BeforeGameOnButtonClick();
            mainButton.Text = Resources.MainWindow_before_game;
            Controls.Add(mainButton);
            SetButtonPosition();
        }

        private EventHandler DuringGameOnButtonClick() => (sender, args) =>
        {
            shipSettingRegime = true;
            RestartGame?.Invoke();
            mainButton.Click -= DuringGameOnButtonClick();
            mainButton.Click += BeforeGameOnButtonClick();
            globalGameInfo.Text = Resources.Ship_setting_instruction;
        };
        private EventHandler BeforeGameOnButtonClick() => (sender, args) =>
        {
            if (!ShipsSetCorrect())
            {
                globalGameInfo.Text = Resources.Wrong_Ship_Setting;
                return;
            }
            shipSettingRegime = false;
            FleetIsChosen?.Invoke(playerFleet);            
            mainButton.Click -= BeforeGameOnButtonClick();
            mainButton.Click += DuringGameOnButtonClick();
        };

        private bool ShipsSetCorrect()
            => playerFleet.All(ship => 
                ship
                    .PreliminaryBuffer()
                    .All(cell => leftCells[cell.X, cell.Y].Type != CellType.Ship));

        private void AddNActivateLabels()
        {
            leftInfo.ForeColor = darkGray;
            rightInfo.ForeColor = darkGray;
            leftInfo.BackColor = Color.Transparent;
            rightInfo.BackColor = Color.Transparent;
            rightInfo.TextAlign = ContentAlignment.MiddleRight;           
            Controls.Add(leftInfo);
            Controls.Add(rightInfo);

            globalGameInfo.Padding = new Padding(10, 0, 0, 1);
            globalGameInfo.ForeColor = darkGray;            
            globalGameInfo.TextAlign = ContentAlignment.MiddleRight;
            globalGameInfo.BackColor = whiteTextBack;
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
                field.SetNewType(cell.X, cell.Y, cell.Type);
            
            Invalidate();
        }

        internal void SetAndDrawFleet(IEnumerable<Ship> fleet, bool inLeftField)
        {
            playerFleet = fleet;
            var field = inLeftField ? leftCells : rightCells;
            foreach(var ship in playerFleet)
            {
                foreach (var cell in ship.PreliminaryBody())
                {
                    field.SetNewType(cell, CellType.Ship);
                    field.SetShip(cell.X, cell.Y, ship);
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
                        if (sailingShip != null)
                        {
                            sailingShip = null;
                            SetAndDrawFleet(playerFleet, true);
                        } else
                        {
                            var xOfCell = LeftXCellIndex(x);
                            var yOfCell = LeftYCellIndex(y);
                            if (leftCells[xOfCell, yOfCell].Type != CellType.Ship)
                                return;
                            sailingShip = leftCells[xOfCell, yOfCell].Ship;
                            foreach (var cell in sailingShip.PreliminaryBody())
                                leftCells.SetNewType(cell, CellType.SailingShip);
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
                        if (pos == 1) globalGameInfo.Text = Resources.Excess_clicks_info;
                    }
                    else if (!shipSettingRegime && IsPointInLeftField(x, y))
                    {
                        var pos = new Random().Next(20);
                        if (pos == 1) globalGameInfo.Text = Resources.Excess_left_field_clicks_info;
                    }
                    break;
                case MouseButtons.Right:
                    if (shipSettingRegime && sailingShip != null)
                    {
                        foreach(var cell in sailingShip.PreliminaryBody())
                            leftCells.SetNewType(cell, CellType.Sea);
                        sailingShip.Reverse();
                        sailingShip.Sail(FindSuitablePlace(new Point(LeftXCellIndex(x), LeftYCellIndex(y))));
                        foreach (var cell in sailingShip.PreliminaryBody())
                            leftCells.SetNewType(cell, CellType.SailingShip);
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
                foreach (var cell in GetNeighbors(currPlace)
                    .Where(c => c.Type == CellType.Sea && !visited.Contains(new Point(c.X, c.Y))))
                    presumed.Enqueue(cell);
                if (presumed.Count < 1) throw new Exception("Not found suitable place.");
                var curr = presumed.Dequeue();
                currPlace = new Point(curr.X, curr.Y);
                if (IsPlaceSuitableForSailing(currPlace)) return currPlace;
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

        private bool IsPlaceSuitableForSailing(Point place)
            => new Ship(place, sailingShip.Size, sailingShip.IsHorizontal).PreliminaryBody()
                .All(cell => cell.X <= GameModel.WidthOfField 
                             && cell.Y <= GameModel.HeightOfField 
                             && cell.X >= 1 
                             && cell.Y >= 1 
                             && leftCells[cell.X, cell.Y].Type != CellType.Ship);

        private int LeftXCellIndex(int x) => (x - padding) / cellSize + 1;
        private int LeftYCellIndex(int y) => (y - padding) / cellSize + 1;

        private bool IsPointInLeftField(int x, int y)
            => x > padding 
               && x < GameModel.WidthOfField * cellSize + padding 
               && y > padding 
               && y < cellSize * GameModel.HeightOfField + padding;

        private bool IsPointInRightField(int x, int y)
            => x < ClientSize.Width - padding && x > ClientSize.Width - padding - cellSize * (GameModel.WidthOfField + 1)
                && y > padding && y < cellSize * (GameModel.HeightOfField + 1);

        private void ChangeSizes()
        {
            var byWidth = ClientSize.Width / WidthCoefficient;
            var byHeight = ClientSize.Height / HeightCoefficient;
            cellSize = Math.Min(byWidth, byHeight);            
            // ReSharper disable once PossibleLossOfFraction
            markPen.Width = cellSize / MarkPenCoefficient;
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
                    g.FillRectangle(new SolidBrush(whiteFieldBack), leftX, cY, cellSize, cellSize);
                    g.DrawRectangle(borderPen, leftX, cY, cellSize, cellSize);
                    g.FillRectangle(new SolidBrush(whiteFieldBack), rightX, cY, cellSize, cellSize);
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
            var oneAndHalfPadding = 3 / 2 * padding;
            var boxHeight = (int)(cellSize * GameModel.HeightOfField / 2.0 - cellSize * 1.3);
            var boxWidth = (int)(2.5 * cellSize);
            leftInfo.Height = boxHeight;
            leftInfo.Width = boxWidth;
            rightInfo.Height = boxHeight;
            rightInfo.Width = boxWidth;
            var font = new Font(FontStyle
                , Math.Min(ClientSize.Width / WidthFontCoefficient, ClientSize.Height / HeightFontCoefficient));
            leftInfo.Font = font;
            rightInfo.Font = font;
            leftInfo.Top = (int)(cellSize * 1.5);
            leftInfo.Left = cellSize * GameModel.WidthOfField + oneAndHalfPadding;            
            rightInfo.Top = (int)(cellSize * 7.5);
            rightInfo.Left = ClientSize.Width - oneAndHalfPadding - (GameModel.WidthOfField * cellSize) - rightInfo.Width;

            globalGameInfo.Height = cellSize;
            globalGameInfo.Width = ClientSize.Width - cellSize;
            globalGameInfo.Left = cellSize / 2;
            globalGameInfo.Top = GameModel.HeightOfField * cellSize + padding * 2;            
            globalGameInfo.Font = new Font(FontStyle, font.Size / 6 * 5);
        }

        private void SetButtonPosition()
        {
            mainButton.Width = (int)(cellSize * 2 * 1.5);
            mainButton.Height = (int)(cellSize * 1.5);
            var fontSize = Math.Min(ClientSize.Width / 80, ClientSize.Height / 40);
            mainButton.Font = new Font(FontStyle, fontSize);
            mainButton.Left = ClientSize.Width / 2 - mainButton.Width / 2;
            mainButton.Top = padding + GameModel.HeightOfField / 2 * cellSize - mainButton.Height / 2;
        }

        private void DrawBackground(Graphics g)
        {
            var leftX = padding / 2;
            var upY = padding / 2;
            var middleY = padding + cellSize * GameModel.HeightOfField / 2;
            var middleX1 = 3 * padding / 2 + GameModel.HeightOfField * cellSize;
            var middleX2 = GetRightFieldUpLeftPoint().X - padding /2;
            var leftWidth = middleX2 - leftX;
            var heightY = cellSize * GameModel.HeightOfField + padding;
            var rightWidth = ClientSize.Width - padding / 2 - middleX2;
            var leftWidth2 = middleX2 - middleX1;

            g.FillRectangle(leftBackgroundBrush, leftX, upY, leftWidth, heightY);
            g.FillRectangle(rightBackgroundBrush, middleX2, upY, rightWidth, heightY);
            g.FillRectangle(rightBackgroundBrush, middleX1, middleY, leftWidth2, heightY / 2);
        }

        private Point GetRightFieldUpLeftPoint() 
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
                    g.FillRectangle(new SolidBrush(darkGray), x, y, cellSize, cellSize);
                    g.DrawRectangle(new Pen(whiteFieldBack, 1), x + cellBorder, y + cellBorder
                        , cellSize - 2 * cellBorder, cellSize - 2 * cellBorder);
                    break;                    
                case CellType.Sea:
                    break;
                case CellType.SailingShip:
                    g.FillRectangle(new SolidBrush(lightGray), x, y, cellSize, cellSize);
                    g.DrawRectangle(new Pen(whiteFieldBack, 1), x + cellBorder, y + cellBorder
                        , cellSize - 2 * cellBorder, cellSize - 2 * cellBorder);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cellType), cellType, null);
            }
        }

        public void MarkWinner(bool winnerIsLeft)
        {
            var backColour = winnerIsLeft ? leftBackground : rightBackground;
            leftBackgroundBrush = new SolidBrush(backColour);
            rightBackgroundBrush = new SolidBrush(backColour);           
        }        
    }
}