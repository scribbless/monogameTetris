using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonogameTetris.TetrisLib
{
    public class AiAndThreadManager
    {
        private List<SingleThread> _threads;
        private Dictionary<int, double[]> _weightsDict;
        private bool _genComplete;
        private List<SingleGame> _winnerWeightsDict;
        private double _lastTimeUpdate;
        private int _threadsNum;
        private int _gamesPerThread;
        private IntVector2 _gamePosition;
        private SpriteFont _font;
        private Texture2D _squareTexture;
        private int _tileSize;
        public int GenerationNum;
            
        public AiAndThreadManager(int gamesPerThread, int threadsNum, int tileSize, IntVector2 gamePosition, Texture2D squareTexture,
            SpriteFont font)
        {
            _threads = new List<SingleThread>();
            _weightsDict = new Dictionary<int, double[]>();
            var r = new Random();
            _genComplete = false;
            _threadsNum = threadsNum;
            _gamesPerThread = gamesPerThread;
            _gamePosition = gamePosition;
            _font = font;
            _squareTexture = squareTexture;
            _tileSize = tileSize;
            GenerationNum = 1;

            for (var i = 0; i < threadsNum * (gamesPerThread * 2); i++)
            {
                var weights = new double[9];
                for (var j = 0; j < 9; j++)
                {
                    weights[j] = (r.NextDouble() * 2) - 1;
                }
                _weightsDict.Add(i, weights);
            }
            
            for (var i = 0; i < threadsNum; i++)
            {
                _threads.Add(new SingleThread(gamesPerThread, tileSize, new IntVector2(gamePosition.X, gamePosition.Y + (i * (25 * tileSize))), squareTexture, font,
                    _weightsDict, i * (gamesPerThread * 2)));
            }
        }

        public void CreateNewGeneration()
        {
            _winnerWeightsDict = new List<SingleGame>();
            _weightsDict.Clear();
            var r = new Random();
            
            foreach (var threadGame in from thread in _threads from threadGame in thread.ThreadGameList select threadGame)
            {
                _winnerWeightsDict.Add(threadGame);
            }
            
            _winnerWeightsDict.Sort((y, x) => x.WinnerGarbageSent.CompareTo(y.WinnerGarbageSent));

            var shuffleList = new List<SingleGame>();
            for (var i = 0; i < 6; i++)
            {
                shuffleList.Add(_winnerWeightsDict[i]);
            }
            shuffleList.Shuffle();

            for (var i = 0; i < _threadsNum * (_gamesPerThread * 2); i++)
            {
                var weights = new double[9];
                for (var j = 0; j < 9; j++)
                {
                    var randomN = r.NextDouble();
                    var randomN2 = r.NextDouble();
                    var randomI = r.Next(0, 11);

                    if (randomN2 >= 0.8)
                    {
                        if (randomN >= 0.5) weights[j] = _winnerWeightsDict[randomI].Game1Weights[j] += ((r.NextDouble() * 2) - 1) * 0.2;
                        else weights[j] = _winnerWeightsDict[randomI].Game2Weights[j] += ((r.NextDouble() * 2) - 1) * 0.2;
                    }
                    else
                    {
                        if (randomN >= 0.5) weights[j] = _winnerWeightsDict[randomI].Game1Weights[j];
                        else weights[j] = _winnerWeightsDict[randomI].Game2Weights[j];
                    }
                }
                _weightsDict.Add(i, weights);
            }
            
            _threads.Clear();
            
            for (var i = 0; i < _threadsNum; i++)
            {
                _threads.Add(new SingleThread(_gamesPerThread, _tileSize, new IntVector2(_gamePosition.X, _gamePosition.Y + (i * (25 * _tileSize))), _squareTexture, _font,
                    _weightsDict, i * (_gamesPerThread * 2)));
            }

            if (_winnerWeightsDict[0].Winner == 1)
            {
                Debug.WriteLine($"fittest weights generation {GenerationNum}: ");
                Array.ForEach(_winnerWeightsDict[0].Game1Weights, d => Debug.Write($"{d.ToString(CultureInfo.InvariantCulture)} "));
            }
            else
            {
                Debug.WriteLine($"fittest weights generation {GenerationNum}: ");
                Array.ForEach(_winnerWeightsDict[0].Game2Weights, d => Debug.Write($"{d.ToString(CultureInfo.InvariantCulture)} "));
            }
            
            GenerationNum++;
        }

        public void Update(GameTime gameTime)
        {
            _genComplete = true;
            Parallel.ForEach(_threads, thread =>
            {
                thread.Update(gameTime);
            });
            if (gameTime.TotalGameTime.TotalMilliseconds - _lastTimeUpdate > 200)
            {
                //update time since last gravity
                _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                
                foreach (var threadGame in from thread in _threads from threadGame in thread.ThreadGameList select threadGame)
                {
                    if (threadGame.Winner == 0) _genComplete = false;
                }
                if (_genComplete)
                {
                    CreateNewGeneration();
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {
            // foreach (var thread in _threads)
            // {
            //     thread.Draw(gameTime, _spriteBatch);
            // }

            _threads[0].ThreadGameList[0].Draw(gameTime, _spriteBatch);
        }
    }
}