using Microsoft.Xna.Framework.Graphics;

namespace MonogameTetris.TetrisLib
{
    public struct BoardSettings
    {
        public int TileSize;
        public int BoardPadding;
        public IntVector2 Location;
        public Texture2D SquareTexture;
        public SpriteFont Font;

        public BoardSettings(int tileSize, int boardPadding, IntVector2 location, Texture2D squareTexture,
            SpriteFont font)
        {
            TileSize = tileSize;
            BoardPadding = boardPadding;
            Location = location;
            SquareTexture = squareTexture;
            Font = font;
        }
    }
}