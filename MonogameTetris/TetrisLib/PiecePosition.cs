namespace MonogameTetris.TetrisLib
{
    public struct PiecePosition
    {
        public IntVector2 Position;
        public int Rotation;

        public PiecePosition(IntVector2 position, int rotation)
        {
            this.Position = position;
            this.Rotation = rotation;
        }
    }
}