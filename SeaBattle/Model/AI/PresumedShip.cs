using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SeaBattle
{
    internal class PresumedShip
    {
        private readonly LinkedList<Point> body;
        private readonly Field fieldModel;
        private bool? isHorizontal;

        internal PresumedShip(Point firstKnown, Field model)
        {
            fieldModel = model;
            body = new LinkedList<Point>();
            body.AddFirst(firstKnown);
        }

        internal IEnumerable<Point> GetFinishOffTurns()
        {
            if (body.Count == 1) return FourNeighbors(body.First.Value)
                .Where(c => fieldModel[c].Type != CellType.Bomb);
            if (body.Count > 1) return TwoNeighbors(body.First.Value, body.Last.Value)
                .Where(c => fieldModel[c].Type != CellType.Bomb);
            throw new Exception("Mind hasn't found neighbor cells of the ship.");
        }

        private IEnumerable<Point> TwoNeighbors(Point first, Point last)
            => isHorizontal != null && isHorizontal.Value
                ? new List<Point>
                {
                    new Point(first.X - 1, first.Y), new Point(last.X + 1, last.Y)
                }
                : new List<Point>
                {
                    new Point(first.X, first.Y - 1), new Point(last.X, last.Y + 1)
                };

        private static IEnumerable<Point> FourNeighbors(Point cell)
        {
            return new List<Point>
            {
                new Point(cell.X - 1, cell.Y),
                new Point(cell.X + 1, cell.Y),
                new Point(cell.X, cell.Y - 1),
                new Point(cell.X, cell.Y + 1)
            };
        }

        internal void ReportOnHit(Point hitCell)
        {
            if (hitCell.X < body.First.Value.X)
                body.AddFirst(hitCell);
            else if (hitCell.X > body.Last.Value.X)
                body.AddLast(hitCell);
            else if (hitCell.Y < body.First.Value.Y)
                body.AddFirst(hitCell);
            else if (hitCell.Y > body.Last.Value.Y)
                body.AddLast(hitCell);

            if (body.Count == 2)
                SetOrientation();
        }

        private void SetOrientation()
        {
            var head = body.First.Value;
            var tail = body.Last.Value;
            if (head.X == tail.X) isHorizontal = false;
            else if (head.Y == tail.Y) isHorizontal = true;
            else throw new Exception("Not straight body of presumed ship.");
        }
    }
}