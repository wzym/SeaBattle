using System;
using System.Collections.Generic;
using System.Drawing;

namespace SeaBattle
{
    internal class Player
    {
        internal bool IsArtificial { get; }
        internal GameCell[,] Field { get; }
        internal Fleet Fleet { get; }
        internal bool IsLost => Fleet.Health == 0;
        internal TurnGenerator TurnGenerator { get; }
        
        private Point currentTurn;        
        internal Point CurrentTurn
        {
            get => IsArtificial ? TurnGenerator.Next() : currentTurn;
            set => currentTurn = value;
        }

        internal Player(bool isArtificial)
        {
            IsArtificial = isArtificial;
            if (isArtificial) TurnGenerator = new Tg3();
            Fleet = new Fleet();
            Field = new GameCell[GameModel.HeightOfField + 2, GameModel.HeightOfField + 2];
            InitializeField();
            InitializeFleet();
        }

        private void InitializeField()
        {
            for(var i = 0; i < GameModel.WidthOfField + 2; i++)
            {
                Field[i, 0] = new GameCell(CellType.Bomb, i, 0);
                Field[i, GameModel.HeightOfField + 1] = new GameCell(CellType.Bomb, i, GameModel.HeightOfField + 1);
            }
            for(var i = 1; i < GameModel.HeightOfField + 1; i++)
            {
                Field[0, i] = new GameCell(CellType.Bomb, 0, i);
                Field[GameModel.WidthOfField + 1, i] = new GameCell(CellType.Bomb, GameModel.WidthOfField + 1, i);
            }
            for (var y = 1; y < GameModel.HeightOfField + 1; y++)
            for (var x = 1; x < GameModel.WidthOfField + 1; x++)
                Field[x, y] = new GameCell(CellType.Sea, x, y);
        }

        private void InitializeFleet()
        {
            //ClearField()
            var generator = new FleetGenerator(Field);
            foreach (var (length, shipsAmount) in GameModel.FleetParams)
                for (var i = 0; i < shipsAmount; i++)
                {
                    var newShip = generator.GetShip(length);
                    Fleet.AddShip(newShip);
                    SetShip(newShip);
                }
            LeaveOnlyShipsOnField();
        }

        private void LeaveOnlyShipsOnField()
        {
            foreach (var cell in GameModel.WorkingCells(Field))
                //if (cell.Type != CellType.Ship)
                //    cell.SetNewType(CellType.Sea);
                if (Field[cell.X, cell.Y].Type != CellType.Ship)
                    Field[cell.X, cell.Y].SetNewType(CellType.Sea);
        }

        private void SetShip(Ship ship)
        {
            foreach (var cell in Ship.PreBody(ship))
                Field[cell.X, cell.Y].Ship = ship;
            foreach (var cell in Ship.PreBuffer(ship))
                Field[cell.X, cell.Y].SetNewType(CellType.Bomb);            
        }

        internal void SetFleet(IEnumerable<Ship> fleet)
        {
            ClearField();
            foreach(var ship in fleet)            
                SetShip(ship);
            LeaveOnlyShipsOnField();
        }

        private void ClearField()
        {
            foreach (var cell in GameModel.WorkingCells(Field))
                //cell.SetNewType(CellType.Sea);
                Field[cell.X, cell.Y].SetNewType(CellType.Sea);
        }

        public void ReturnResultBack(GameCell cell)
        {
            if (IsArtificial) TurnGenerator.ReturnResultBack(cell);
        }
    }
}