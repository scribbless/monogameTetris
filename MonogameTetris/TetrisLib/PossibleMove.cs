namespace MonogameTetris.TetrisLib
{
    public struct PossibleMove
    {
        public PiecePosition EndPosition;
        public PiecePosition DropPosition;
        public readonly string Path;
        public readonly double Cost;

        public PossibleMove(PiecePosition endPosition, PiecePosition dropPosition, string path, double cost)
        {
            this.EndPosition = endPosition;
            this.DropPosition = dropPosition;
            this.Path = path;
            this.Cost = cost;
        }
    }
}