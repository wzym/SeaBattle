using SeaBattle.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SeaBattle
{
    internal class GameModel
    {
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
                if (isTurnLocked) return;
                status.Player.CurrentTurn = p;
                status.SetPlayerActive();
                Turn();
            };
        }

        private void SetFleetChosenByPlayer(IEnumerable<Ship> fleet)
        {            
            status.Player.AddToField(fleet);
            status.DefineActivity();
            if (status.Active == status.Rival)
            {
                view.SetGlobalInfo(Resources.First_turn_is_rivals_one);
                Turn();
            } 
            else
            {
                view.SetGlobalInfo(Resources.First_turn_is_yours);
            }
        }

        private void ActivateNewGame()
        {
            status = new GameStatus();            
            view.Clear();
            view.SetAndDrawFleet(status.Player.Fleet.Ships, true);
            view.SetNewGameInfo(status.Player.Fleet.ToString(), status.Rival.Fleet.ToString());            
        }

        private void WorkOnGameEnd()
        {
            view.MarkWinner(status.Player.Fleet.Health > 0);
            status.Rival.Fleet.Ships.ForEach(ship => view.MarkSurvivors(
                ship.PreliminaryBody()
                    .Where(c => status.Rival.Field[c].Type != CellType.Exploded)));
        }

        private bool isTurnLocked;

        private async void Turn()
        {
            if (isTurnLocked) return;
            isTurnLocked = true;
            Cursor.Hide();
            while (status.IsGameContinues)
            {
                var active = status.Active;
                var passive = status.Passive;                
                if (active.IsArtificial) await Task.Delay(CommonParameters.TimeLapse).ConfigureAwait(true);
                var turn = status.Active.CurrentTurn;
                var cell = status.Passive.Field[turn];

                switch (cell.Type)
                {
                    case CellType.Sea:                        
                        HandleSea(turn);
                        break;
                    case CellType.Ship:
                        HandleShip(cell, turn, passive);
                        break;
                    case CellType.Bomb:
                        if (active.IsArtificial) throw new ArgumentException(Resources.Same_AI_Turn);
                        break;
                    case CellType.Exploded:
                        if (active.IsArtificial) throw new ArgumentException(Resources.Same_AI_Turn);
                        break;
                    case CellType.SailingShip:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(Resources.Unintended_Cell_Type);
                }                
                active.ReturnResultBack(passive.Field[turn]);                
                ShowField(passive.Field, active == status.Rival);//
                if (!status.Active.IsArtificial) break;
            }
            isTurnLocked = false;
            Cursor.Show();
        }

        private void HandleShip(GameCell cell, Point turn, Player passive)
        {
            var ship = cell.Ship;
            status.Passive.Field.SetNewType(turn, CellType.Exploded);
            ship.SetDamage();
            if (!ship.IsDead) return;
            status.RecordShipDeath(ship);
            view.SetNewGameInfo(status.Player.Fleet.ToString()
                , status.Rival.Fleet.ToString());
            var fieldName = passive == status.Rival ? Resources.Of_Rival : Resources.Yours_one;
            view.SetGlobalInfo($"{fieldName} {ship.Name} {Resources.Drowned} ({ship}).");
            if (!status.IsGameContinues) WorkOnGameEnd();
        }

        private void HandleSea(Point turn)
        {
            status.Passive.Field.SetNewType(turn, CellType.Bomb);
            status.InvertActivity();
        }

        private void ShowField(Field field, bool isLeftField)
        {
            var shouldShowCell = isLeftField ?
                (Func<CellType, bool>) (_ => true) : c => c != CellType.Ship;
            view.DrawCells(CommonMethods
                .GetWorkingCells(field)
                .Where(c => shouldShowCell(c.Type)), isLeftField);
        }
    }
}