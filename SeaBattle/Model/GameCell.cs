namespace SeaBattle
{
    internal class GameCell
    {
        private Ship ship;
        internal int X { get; }
        internal int Y { get; }
        internal CellType Type { get; private set; }
        internal Ship Ship
        {
            get => Type == CellType.Ship || Type == CellType.Exploded 
                ? ship : null;
            set
            {
                ship = value;
                Type = CellType.Ship;
            }
        }

        internal GameCell(CellType type, int x, int y)
        {
            X = x;
            Y = y;
            Type = type;
        }

        internal void SetNewType(CellType newType)
        {
            Type = newType;
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}