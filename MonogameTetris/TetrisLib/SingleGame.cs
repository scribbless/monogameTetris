using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonogameTetris.TetrisLib
{
    public class SingleGame
    {
        public TetrisGame TetrisGame1;
        public TetrisGame TetrisGame2;

        public double[] Game1Weights;
        public double[] Game2Weights;

        private int _tileSize;
        private IntVector2 _gamePosition;
        private Texture2D _squareTexture;
        private SpriteFont _font;

        private int[] score = new int [3];
        private int gameNum;
        private bool _run;

        public int IndexNum;
        public int Winner;
        public int WinnerGarbageSent;

        public SingleGame(int tileSize, IntVector2 gamePosition, Texture2D squareTexture, SpriteFont font, double[] game1Weights, double[] game2Weights, int indexNum)
        {
            Game1Weights = game1Weights;
            Game2Weights = game2Weights;
            gameNum = 0;
            Winner = 0;
            IndexNum = indexNum;
            _run = true;
            WinnerGarbageSent = 0;

            _tileSize = tileSize;
            _gamePosition = gamePosition;
            _squareTexture = squareTexture;
            _font = font;

            TetrisGame1 = new TetrisGame(false, new BoardSettings(tileSize, 0, new IntVector2(gamePosition.X, gamePosition.Y), squareTexture, font),
                new PlayerSettings(300, 2), Game1Weights, false);
            
            TetrisGame2 = new TetrisGame(false, new BoardSettings(tileSize, 0, new IntVector2(gamePosition.X + (tileSize * 22), gamePosition.Y), squareTexture, font),
                new PlayerSettings(300, 2), Game2Weights, false);
        }

        public void Update(GameTime gameTime)
        {
            if (_run)
            {
                if (TetrisGame1.HasLost || TetrisGame2.HasLost)
                {
                    if (gameNum < 3)
                    {
                        if (TetrisGame1.HasLost)
                        {
                            score[gameNum] = 1;
                            
                            TetrisGame1 = new TetrisGame(false, new BoardSettings(_tileSize, 0, new IntVector2(_gamePosition.X, _gamePosition.Y), _squareTexture, _font),
                                new PlayerSettings(300, 2), Game1Weights, false);
            
                            TetrisGame2 = new TetrisGame(false, new BoardSettings(_tileSize, 0, new IntVector2(_gamePosition.X + (_tileSize * 22), _gamePosition.Y), _squareTexture, _font),
                                new PlayerSettings(300, 2), Game2Weights, false);
                        }
                        else
                        {
                            score[gameNum] = 2;
                            
                            TetrisGame1 = new TetrisGame(false, new BoardSettings(_tileSize, 0, new IntVector2(_gamePosition.X, _gamePosition.Y), _squareTexture, _font),
                                new PlayerSettings(300, 2), Game1Weights, false);
            
                            TetrisGame2 = new TetrisGame(false, new BoardSettings(_tileSize, 0, new IntVector2(_gamePosition.X + (_tileSize * 22), _gamePosition.Y), _squareTexture, _font),
                                new PlayerSettings(300, 2), Game2Weights, false);
                        }

                        gameNum++;
                    }
                    else
                    {
                        Winner = score.Sum() > 3 ? 2 : 1;
                        WinnerGarbageSent = Winner switch
                        {
                            1 => TetrisGame1.TotalGarbageSent,
                            2 => TetrisGame2.TotalGarbageSent,
                            _ => WinnerGarbageSent
                        };
                        _run = false;
                    }
                }
                else if (TetrisGame1.TotalPiecesDropped > 1000 || TetrisGame2.TotalPiecesDropped > 1000)
                {
                    if (TetrisGame1.TotalGarbageSent > TetrisGame2.TotalGarbageSent)
                    {
                        TetrisGame2.HasLost = true;
                    }
                    else
                    {
                        TetrisGame1.HasLost = true;
                    }
                }
                else
                {
                    TetrisGame1.Update(gameTime);
                    TetrisGame2.Update(gameTime);
                    TetrisGame1.ReceiveGarbage(ref TetrisGame2.SendGarbage());
                    TetrisGame2.ReceiveGarbage(ref TetrisGame1.SendGarbage());
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            TetrisGame1.Draw(gameTime, spriteBatch);
            TetrisGame2.Draw(gameTime, spriteBatch);
        }
    }
}