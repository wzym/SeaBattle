using System;
using System.Collections.Generic;
using System.Drawing;

namespace SeaBattle
{
    internal class Player
    {
        internal bool IsArtificial { get; }
        internal Field Field { get; }
        internal Fleet Fleet { get; }
        internal bool IsLost => Fleet.Health == 0;
        internal TurnGenerator TurnGenerator { get; }
        
        private Point currentTurn;        
        internal Point CurrentTurn
        {
            get => IsArtificial ? TurnGenerator.NextTurn() : currentTurn;
            set => currentTurn = value;
        }

        internal Player(bool isArtificial)
        {
            IsArtificial = isArtificial;
            if (isArtificial) TurnGenerator = new TurnGenerator();
            Fleet = new Fleet();
            Field = new Field();
            InitializeFleet();
        }

        private void InitializeFleet()
        {
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
            foreach (var cell in CommonMethods.GetWorkingCells(Field))
                if (cell.Type != CellType.Ship)
                    Field.SetNewType(cell.X, cell.Y, CellType.Sea);
        }

        private void SetShip(Ship ship)
        {
            foreach (var cell in ship.PreliminaryBody())
                Field.SetShip(cell.X, cell.Y, ship);

            foreach (var cell in ship.PreliminaryBuffer())
                Field.SetNewType(cell.X, cell.Y, CellType.Bomb);
        }

        internal void AddToField(IEnumerable<Ship> fleet)
        {
            ClearField();
            foreach(var ship in fleet)            
                SetShip(ship);
            LeaveOnlyShipsOnField();
        }

        private void ClearField()
        {
            foreach (var cell in CommonMethods.GetWorkingCells(Field))
                Field.SetNewType(cell.X, cell.Y, CellType.Sea);
        }

        public void ReturnResultBack(GameCell cell)
        {
            if (IsArtificial) TurnGenerator.ReturnResultBack(cell);
        }
    }
}