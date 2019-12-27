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
            var fleetAsArray = fleet as Ship[] ?? fleet.ToArray();
            status.Player.Fleet.AddToFleet(fleetAsArray);
            status.Player.AddToField(fleetAsArray);
            status.DefineActivity();
            if (status.Active == status.Rival)
            {
                view.SetGlobalInfo("Первый ход достался машине.");
                Turn();
            } 
            else
            {
                view.SetGlobalInfo("Ваш ход.");
            }
        }

        private void ActivateNewGame()
        {
            status = new GameStatus();
            status.GameEnd += WorkOnGameEnd;
            view.Clear();
            view.SetAndDrawFleet(status.Player.Fleet.Ships, true);
            view.SetNewGameInfo(status.Player.Fleet.ToString(), status.Rival.Fleet.ToString());            
        }

        private void WorkOnGameEnd()
        {
            view.MarkWinner(status.Player.Fleet.Health > 0);
            foreach(var ship in status.Rival.Fleet.Ships)
            {
                view.MarkSurvivors(Ship.PreBody(ship).Where(c => status.Rival.Field[c].Type != CellType.Exploded));                
            }
        }

        private void Turn()
        {
            while (status.IsGameContinues)
            {
                var active = status.Active;
                var passive = status.Passive;
                var turn = status.Active.CurrentTurn;
                var cell = status.Passive.Field[turn];

                switch (cell.Type)
                {
                    case CellType.Sea:                        
                        status.Passive.Field.SetNewType(turn, CellType.Bomb);
                        status.InvertActivity();
                        break;
                    case CellType.Ship:
                        var ship = cell.Ship;                        
                        status.Passive.Field.SetNewType(turn, CellType.Exploded);
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
                active.ReturnResultBack(passive.Field[turn]);
                ShowField(passive.Field, active == status.Rival);//
                if (!status.Active.IsArtificial) break;                
            }
        }

        private void ShowField(Field field, bool isLeftField)
        {
            var shouldShowCell = isLeftField ?
                (Func<CellType, bool>) (_ => true) : c => c != CellType.Ship;
            view.DrawCells(Field.GetWorkingCells(field).Where(c => shouldShowCell(c.Type)), isLeftField);
        }
    }
}