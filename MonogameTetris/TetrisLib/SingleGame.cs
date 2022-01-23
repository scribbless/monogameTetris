using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonogameTetris.TetrisLib
{
    public class SingleGame
    {
        private TetrisGame _game1;
        private TetrisGame _game2;

        public double[] game1weights;
        public double[] game2weights;

        public SingleGame(int _tileSize, IntVector2 _gamePosition, Texture2D _squareTexture, SpriteFont _font, double[] game1Weights, double[] game2Weights)
        {
            this.game1weights = game1Weights;
            this.game2weights = game2Weights;
            
            _game1 = new TetrisGame(false, new BoardSettings(_tileSize, 0, new IntVector2(_gamePosition.X, _gamePosition.Y), _squareTexture, _font),
                new PlayerSettings(300, 30), game1weights);
            
            _game2 = new TetrisGame(false, new BoardSettings(_tileSize, 0, new IntVector2(_gamePosition.X + (_tileSize * 22), _gamePosition.Y), _squareTexture, _font),
                new PlayerSettings(300, 30), game2weights);
        }

        public void Update(GameTime gameTime)
        {
            _game1.Update(gameTime);
            _game2.Update(gameTime);
            _game1.ReceiveGarbage(ref _game2.SendGarbage());
            _game2.ReceiveGarbage(ref _game1.SendGarbage());
            
        }

        public void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {
            _game1.Draw(gameTime, _spriteBatch);
            _game2.Draw(gameTime, _spriteBatch);
        }
    }
}