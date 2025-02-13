namespace Classic_Snake.classes
{
    public class GridDirection
    {
        public readonly static GridDirection Left = new GridDirection(0, -1);
        public readonly static GridDirection Right = new GridDirection(0, 1);
        public readonly static GridDirection Up = new GridDirection(-1, 0);
        public readonly static GridDirection Down = new GridDirection(1, 0);

        public int RowOffset { get; }
        public int ColOffset { get; }

        private GridDirection(int rowOffset, int colOffset)
        {
            RowOffset = rowOffset;
            ColOffset = colOffset;
        }

        public GridDirection Opposite()
        {
            return new GridDirection(-RowOffset, -ColOffset);
        }

        public override bool Equals(object obj)
        {
            return obj is GridDirection direction &&
                   RowOffset == direction.RowOffset &&
                   ColOffset == direction.ColOffset;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(RowOffset, ColOffset);
        }

        public static bool operator ==(GridDirection left, GridDirection right)
        {
            return EqualityComparer<GridDirection>.Default.Equals(left, right);
        }

        public static bool operator !=(GridDirection left, GridDirection right)
        {
            return !(left == right);
        }
    }
}
