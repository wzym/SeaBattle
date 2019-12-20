using System;
using System.Collections.Generic;
using System.Linq;

namespace SeaBattle
{
    internal class GameModel
    {
        internal static readonly Tuple<int, int>[] FleetParams =
            {Tuple.Create(4, 1), Tuple.Create(3, 2), Tuple.Create(2, 3), Tuple.Create(1, 4)};

        internal const int WidthOfField = 10;
        internal const int HeightOfField = 10;

        private readonly MainWindow view;
        private GameStatus status;

        internal GameModel(MainWindow window)
        {            
            view = window;            
            view.RestartGame += ActivateNewGame;
            view.FleetIsChosen += SetFleetChosenByPlayer;
            ActivateNewGame();
            view.TurnDone += (p, _) => 
            {
                status.Player.CurrentTurn = p;
                status.SetPlayerActive();
                Turn();
            };
        }

        private void SetFleetChosenByPlayer(IEnumerable<Ship> fleet)
        {
            status.Player.Fleet.SetShips(fleet);
            status.Player.SetFleet(fleet);
            status.DefineActivity();
            if (status.Active == status.Rival)
            {
                view.SetGlobalInfo("Первый ход достался машине.");
                Turn();
            } else
            {
                view.SetGlobalInfo("Ваш ход.");
            }
        }

        private void ActivateNewGame()
        {
            status = new GameStatus();
            status.GameEnd += WorkOnGameEnd;
            view.Clear();
            view.DrawFleet(status.Player.Fleet.Ships, true);
            view.SetNewGameInfo(status.Player.Fleet.ToString(), status.Rival.Fleet.ToString());            
        }

        private void WorkOnGameEnd()
        {
            view.CheckWinner(status.Player.Fleet.Health > 0);
        }

        private void Turn()
        {
            while (status.IsGameContinues)
            {
                var active = status.Active;
                var passive = status.Passive;
                var turn = status.Active.CurrentTurn;
                var cell = status.Passive.Field[turn.X, turn.Y];

                switch (cell.Type)
                {
                    case CellType.Sea:
                        cell.SetNewType(CellType.Bomb);
                        status.InvertActivity();
                        break;
                    case CellType.Ship:
                        var ship = cell.Ship;
                        cell.SetNewType(CellType.Exploded);
                        ship.SetDamage();
                        if (ship.IsDead)
                        {
                            status.RecordShipDeath(ship);
                            view.SetNewGameInfo(status.Player.Fleet.ToString(), status.Rival.Fleet.ToString());
                        }
                        break;
                    case CellType.Bomb:
                        if (active.IsArtificial) throw new ArgumentException("Ум повторяется.");
                        break;
                    case CellType.Exploded:
                        if (active.IsArtificial) throw new ArgumentException("Ум повторяется.");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                active.ReturnResultBack(cell);                
                ShowField(passive.Field, active == status.Rival);
                if (!status.Active.IsArtificial) break;                
            }
        }

        private void ShowField(GameCell[,] field, bool isLeftField)
        {
            var shouldShowCell = isLeftField ?
                (Func<CellType, bool>) (_ => true) : c => c != CellType.Ship;
            view.DrawCells(WorkingCells(field).Where(c => shouldShowCell(c.Type)), isLeftField);           
        }

        internal static IEnumerable<GameCell> WorkingCells(GameCell[,] field)
        {
            for (var y = 1; y < field.GetLength(1) - 1; y++)
            for (var x = 1; x < field.GetLength(0) - 1; x++)
                yield return field[x, y];
        }
    }
}