﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonogameTetris.TetrisLib
{
    public class AiAndThreadManager
    {
        private readonly List<SingleThread> _threads;
        private readonly Dictionary<int, double[]> _weightsDict;
        private bool _genComplete;
        private List<SingleGame> _winnerWeightsDict;
        private double _lastTimeUpdate;
        private readonly int _threadsNum;
        private readonly int _gamesPerThread;
        private readonly IntVector2 _gamePosition;
        private readonly SpriteFont _font;
        private readonly Texture2D _squareTexture;
        private readonly int _tileSize;
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

        private void CreateNewGeneration()
        {
            _winnerWeightsDict = new List<SingleGame>();
            _weightsDict.Clear();
            var r = new Random();

            foreach (var threadGame in from thread in _threads from threadGame in thread.ThreadGameList select threadGame)
            {
                _winnerWeightsDict.Add(threadGame);
            }
            
            //winnerWeightsDict = _winnerWeightsDict.Sort((y, x) => x.WinnerGarbageSent.CompareTo(y.WinnerGarbageSent));
            _winnerWeightsDict = _winnerWeightsDict.OrderByDescending(game => game.WinnerGarbageSent)
                .ThenByDescending(game => game.WinnerLinesDropped).Select(game => game).ToList();
            
            var csv = new StringBuilder();
            var newLine =_winnerWeightsDict[0].WinnerGarbageSent.ToString();
            csv.AppendLine(newLine);
            File.AppendAllText("C:\\Users\\cass7\\RiderProjects\\monogameTetris\\MonogameTetris\\data.csv", csv.ToString());
            
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
                    var randomI2 = r.Next(0, 11);

                    if (randomN2 >= 0.8)
                    {
                        if (randomN >= 0.5)
                        {
                            if (_winnerWeightsDict[randomI].Winner == 1)
                            {
                                weights[j] = _winnerWeightsDict[randomI].Game1Weights[j] += ((r.NextDouble() * 2) - 1) * 0.2;
                            }
                            else
                            {
                                weights[j] = _winnerWeightsDict[randomI].Game2Weights[j] += ((r.NextDouble() * 2) - 1) * 0.2;
                            }
                        }
                        else
                        {
                            if (_winnerWeightsDict[randomI2].Winner == 1)
                            {
                                weights[j] = _winnerWeightsDict[randomI2].Game1Weights[j] += ((r.NextDouble() * 2) - 1) * 0.2;
                            }
                            else
                            {
                                weights[j] = _winnerWeightsDict[randomI2].Game2Weights[j] += ((r.NextDouble() * 2) - 1) * 0.2;
                            }
                        }
                    }
                    else
                    {
                        if (randomN >= 0.5)
                        {
                            if (_winnerWeightsDict[randomI].Winner == 1)
                            {
                                weights[j] = _winnerWeightsDict[randomI].Game1Weights[j];
                            }
                            else
                            {
                                weights[j] = _winnerWeightsDict[randomI].Game2Weights[j];
                            }
                        }
                        else
                        {
                            if (_winnerWeightsDict[randomI2].Winner == 1)
                            {
                                weights[j] = _winnerWeightsDict[randomI2].Game1Weights[j];
                            }
                            else
                            {
                                weights[j] = _winnerWeightsDict[randomI2].Game2Weights[j];
                            }
                        }
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

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // foreach (var thread in _threads)
            // {
            //     thread.Draw(gameTime, _spriteBatch);
            // }

            _threads[0].ThreadGameList[0].Draw(gameTime, spriteBatch);
        }
    }
}