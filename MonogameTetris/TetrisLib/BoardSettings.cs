using Microsoft.Xna.Framework;
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
        
        public BoardSettings(int tileSize, int boardPadding, IntVector2 location, Texture2D squareTexture, SpriteFont font)
        {
            this.TileSize = tileSize;
            this.BoardPadding = boardPadding;
            this.Location = location;
            this.SquareTexture = squareTexture;
            this.Font = font;
        }
    }
}