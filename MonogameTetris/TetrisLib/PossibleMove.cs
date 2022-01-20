namespace MonogameTetris.TetrisLib
{
    public struct PossibleMove
    {
        public PiecePosition endPosition;
        public PiecePosition dropPosition;
        public string path;
        public double cost;

        public PossibleMove(PiecePosition endPosition, PiecePosition dropPosition, string path, double cost)
        {
            this.endPosition = endPosition;
            this.dropPosition = dropPosition;
            this.path = path;
            this.cost = cost;
        }
    }
}