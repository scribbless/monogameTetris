using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonogameTetris.TetrisLib
{
    public class SingleThread
    {
        public List<SingleGame> ThreadGameList;
        private Dictionary<int, double[]> _threadGameWeights;
        public bool ThreadGenComplete;

        public SingleThread(int gamesAmount, int tileSize, IntVector2 gamePosition, Texture2D squareTexture, SpriteFont font, Dictionary<int, double[]> threadGameWeights, int indexNum)
        {
            _threadGameWeights = threadGameWeights;
            ThreadGameList = new List<SingleGame>();
            ThreadGenComplete = false;
            
            for (var i = 0; i < gamesAmount; i++)
            {
                ThreadGameList.Add(new SingleGame(tileSize, new IntVector2(gamePosition.X + ((tileSize * 47) * i), gamePosition.Y), squareTexture, font, threadGameWeights[indexNum + (i * 2)], threadGameWeights[indexNum + (i * 2) + 1], indexNum + (i * 2)));
            }
        }

        public void Update(GameTime gameTime)
        {
            var genComplete = true;
            foreach (var singleGame in ThreadGameList)
            {
                singleGame.Update(gameTime);
                if (singleGame.Winner == 0)
                {
                    genComplete = false;
                }
            }

            if (genComplete = true)
            {
                ThreadGenComplete = true;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {
            foreach (var singleGame in ThreadGameList)
            {
                singleGame.Draw(gameTime, _spriteBatch);
            }
        }
    }
}