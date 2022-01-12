using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonogameTetris.TetrisLib;
using static Microsoft.Xna.Framework.Color;

namespace MonogameTetris.TetrisLib
{
    public class TetrisGame
    {
        public bool IsHuman;
        public BoardSettings BoardSettings;
        public PlayerSettings PlayerSettings;

        private readonly Dictionary<int, Color> _colorDict;
        private readonly InputLib _inputLib = new InputLib();
        private readonly List<int> _masterPieceQueue = new List<int>();
        private readonly List<int> _pieceQueuePart = new List<int> {2, 3, 4, 5, 6, 7, 8};
        private readonly TetrisUtil _tetrisUtil = new TetrisUtil();
        
        private double _currentTime;
        private int _rotateCount;
        private int[,] _boardArray;
        private int[,] _staticBoardArray;
        private readonly ActivePiece _activePiece = new ActivePiece(new IntVector2(5, 15), 0, 4, 3);
        private readonly ActivePiece _ghostPiece = new ActivePiece(new IntVector2(5, 15), 0, 4, 3);
        private readonly IntVector2 _boardSize = new IntVector2(10, 20);
        
        private bool _touchingGroundCheck1;
        private int _hasHeld;
        private int _heldPiece;
        private bool _lastMoveIsSpin = false;
        private int _backToBack;
        private bool _wasLastWallkickUsed;
        private int _garbageSent;
        private double _gravityIntervalTime;
        private double _lastTime;
        private double _lastTimeUpdate;
        private int _moveCount;
        private readonly PieceDictionary _pieceDictionary = new PieceDictionary();
        private double _startTime;

        public TetrisGame(bool isHuman, BoardSettings boardSettings, PlayerSettings playerSettings)
        {
            this.IsHuman = isHuman;
            this.BoardSettings = boardSettings;
            this.PlayerSettings = playerSettings;

            _gravityIntervalTime = 500;
            
            _staticBoardArray = new int[_boardSize.X, _boardSize.Y];
            Array.Clear(_staticBoardArray, 0, _staticBoardArray.Length);

            _boardArray = new int[_boardSize.X, _boardSize.Y];
            Array.Clear(_boardArray, 0, _boardArray.Length);

            //initiate piece queue
            _pieceQueuePart.Shuffle();
            _masterPieceQueue.AddRange(_pieceQueuePart);
            _pieceQueuePart.Shuffle();
            _masterPieceQueue.AddRange(_pieceQueuePart);

            //0: blank space, 1: garbage, 2: I piece, 3: J piece, 4: L piece, 5: O piece, 6: Z piece, 7: T piece, 8: S piece
            _colorDict = new Dictionary<int, Color>
            {
                //blank tile
                {0, new Color(59, 53, 57)},
                //garbage
                {1, new Color(153, 153, 153)},
                //I
                {2, new Color(32, 156, 213)},
                //J
                {3, new Color(36, 70, 195)},
                //L
                {4, new Color(225, 91, 29)},
                //O
                {5, new Color(225, 158, 37)},
                //S
                {6, new Color(92, 175, 31)},
                //T
                {7, new Color(173, 46, 137)},
                //Z
                {8, new Color(213, 22, 59)},
                
                //I
                {20, new Color(32, 156, 213, 127)},
                //J
                {30, new Color(36, 70, 195, 127)},
                //L
                {40, new Color(225, 91, 29, 127)},
                //O
                {50, new Color(225, 158, 37, 127)},
                //S
                {60, new Color(92, 175, 31, 127)},
                //T
                {70, new Color(173, 46, 137, 127)},
                //Z
                {80, new Color(213, 22, 59, 127)}
            };
        }
        
        public void Update(GameTime gameTime)
        {
            //update inputlib states
            _inputLib.Update();

            _currentTime = gameTime.TotalGameTime.TotalMilliseconds;

            if (IsHuman)
            {
                //handle keypresses
            if (_inputLib.IsNewPress(Keys.Up) && _rotateCount <= 15)
            {
                var rotStateReturn = _activePiece.IncreaseRotState(_staticBoardArray);
                if (rotStateReturn[0]) _lastMoveIsSpin = true;
                _wasLastWallkickUsed = rotStateReturn[1];

                if (_activePiece.IsTouchingBlock(_staticBoardArray))
                {
                    _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                    _rotateCount++;
                }
            }
            else if (_inputLib.IsNewPress(Keys.Z))
            {
                var rotStateReturn = _activePiece.DecreaseRotState(_staticBoardArray);
                if (rotStateReturn[0]) _lastMoveIsSpin = true;
                _wasLastWallkickUsed = rotStateReturn[1];

                if (_activePiece.IsTouchingBlock(_staticBoardArray) && _rotateCount <= 15)
                {
                    _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                    _rotateCount++;
                }
            }
            else if (_inputLib.DelayRepeatPress(Keys.Right, PlayerSettings.DelayTime, PlayerSettings.RepeatTime, _lastTime, _currentTime,
                         _startTime))
            {
                if (_inputLib.IsNewPress(Keys.Right)) _startTime = _currentTime;

                _lastTime = _currentTime;
                if (_activePiece.MoveRight(_staticBoardArray)) _lastMoveIsSpin = false;

                if (_activePiece.IsTouchingBlock(_staticBoardArray) && _moveCount <= 20)
                {
                    _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                    _moveCount++;
                }
            }
            else if (_inputLib.DelayRepeatPress(Keys.Left, PlayerSettings.DelayTime, PlayerSettings.RepeatTime, _lastTime, _currentTime,
                         _startTime))
            {
                if (_inputLib.IsNewPress(Keys.Left)) _startTime = _currentTime;

                _lastTime = _currentTime;
                if (_activePiece.MoveLeft(_staticBoardArray)) _lastMoveIsSpin = false;

                if (_activePiece.IsTouchingBlock(_staticBoardArray) && _moveCount <= 20)
                {
                    _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                    _moveCount++;
                }
            }
            else if (_inputLib.IsNewPress(Keys.Space))
            {
                _hasHeld = 0;
                _activePiece.HardDrop(_staticBoardArray);

                var staticBoardArrayCopy = (int[,]) _staticBoardArray.Clone();
                _staticBoardArray = _activePiece.ReturnLockedInBoard(_staticBoardArray);
                
                _garbageSent = _tetrisUtil.ClearLines(ref _staticBoardArray, staticBoardArrayCopy, ref _backToBack,
                    _lastMoveIsSpin, _activePiece, _wasLastWallkickUsed);
                
                if (_masterPieceQueue[0] == 2)
                    _activePiece.ResetPiece(_masterPieceQueue[0], 4);
                else
                    _activePiece.ResetPiece(_masterPieceQueue[0], 3);
                _masterPieceQueue.RemoveAt(0);

                //refill queue
                if (_masterPieceQueue.Count <= 7)
                {
                    _pieceQueuePart.Shuffle();
                    _masterPieceQueue.AddRange(_pieceQueuePart);
                }

                _rotateCount = 0;
                _touchingGroundCheck1 = false;

                /*
                foreach (var line in _tetrisUtil.CheckLines(_staticBoardArray))
                    _staticBoardArray = _tetrisUtil.ClearRow(_staticBoardArray, line);
                    */
            }
            else if (_inputLib.RepeatPress(Keys.Down, PlayerSettings.RepeatTime, _lastTime, _currentTime))
            {
                _lastTime = _currentTime;
                if (_activePiece.MoveDown(_staticBoardArray)) _lastMoveIsSpin = false;
            }
            else if (_inputLib.IsNewPress(Keys.C))
            {
                if (_hasHeld == 0)
                {
                    _hasHeld = 1;

                    if (_heldPiece == 0)
                    {
                        _heldPiece = _activePiece.PieceType;

                        if (_masterPieceQueue[0] == 2)
                            _activePiece.ResetPiece(_masterPieceQueue[0], 4);
                        else
                            _activePiece.ResetPiece(_masterPieceQueue[0], 3);
                        _masterPieceQueue.RemoveAt(0);

                        //refill queue
                        if (_masterPieceQueue.Count <= 7)
                        {
                            _pieceQueuePart.Shuffle();
                            _masterPieceQueue.AddRange(_pieceQueuePart);
                        }
                    }
                    else
                    {
                        var pieceTemp = _activePiece.PieceType;
                        if (_heldPiece == 2)
                            _activePiece.ResetPiece(_heldPiece, 4);
                        else
                            _activePiece.ResetPiece(_heldPiece, 3);

                        _heldPiece = pieceTemp;
                    }

                    _rotateCount = 0;
                    _touchingGroundCheck1 = false;
                }
            }
            }
            
            //gravity
            if (gameTime.TotalGameTime.TotalMilliseconds - _lastTimeUpdate > _gravityIntervalTime)
            {
                //update time since last gravity
                _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;

                //if touching block
                if (_activePiece.IsTouchingBlock(_staticBoardArray))
                {
                    //Console.Write("test");
                    if (!_touchingGroundCheck1)
                    {
                        _touchingGroundCheck1 = true;
                    }
                    else
                    {
                        _hasHeld = 0;
                        var staticBoardArrayCopy = (int[,]) _staticBoardArray.Clone();
                        _staticBoardArray = _activePiece.ReturnLockedInBoard(_staticBoardArray);
                        
                        _garbageSent = _tetrisUtil.ClearLines(ref _staticBoardArray, staticBoardArrayCopy, ref _backToBack,
                            _lastMoveIsSpin, _activePiece, _wasLastWallkickUsed);
                        
                        if (_masterPieceQueue[0] == 2)
                            _activePiece.ResetPiece(_masterPieceQueue[0], 4);
                        else
                            _activePiece.ResetPiece(_masterPieceQueue[0], 3);
                        _masterPieceQueue.RemoveAt(0);

                        //refill queue
                        if (_masterPieceQueue.Count <= 7)
                        {
                            _pieceQueuePart.Shuffle();
                            _masterPieceQueue.AddRange(_pieceQueuePart);
                        }

                        _rotateCount = 0;
                        _touchingGroundCheck1 = false;

                        /*
                        foreach (var line in _tetrisUtil.CheckLines(_staticBoardArray))
                            _staticBoardArray = _tetrisUtil.ClearRow(_staticBoardArray, line);
                            */
                    }
                }
                else
                {
                    _activePiece.MoveDown(_staticBoardArray);
                    _lastMoveIsSpin = false;
                }
            }

            _ghostPiece.CurrentLocation = _activePiece.CurrentLocation;
            _ghostPiece.RotState = _activePiece.RotState;
            _ghostPiece.PieceType = _activePiece.PieceType;
            _ghostPiece.SideLength = _activePiece.SideLength;
            _ghostPiece.HardDrop(_staticBoardArray);
        }

        public int CheckGarbage()
        {
            return _garbageSent;
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Array.Clear(_boardArray, 0, _boardArray.Length);
            var staticBoardArrayCopy = (int[,]) _staticBoardArray.Clone();
            _boardArray = staticBoardArrayCopy;
            _boardArray = _tetrisUtil.ReturnBoardWithGhostPiece(staticBoardArrayCopy, _ghostPiece);
            _boardArray = _tetrisUtil.ReturnBoardWithPiece(_boardArray, _activePiece);

            //draw board
            _tetrisUtil.DrawBoard(_boardSize.X, _boardSize.Y, BoardSettings.Location.X, BoardSettings.Location.Y, BoardSettings.TileSize, BoardSettings.BoardPadding, spriteBatch,
                _colorDict, _boardArray, BoardSettings.SquareTexture);

            //draw queue + held piece
            _tetrisUtil.DrawQueue(_masterPieceQueue, new Vector2(BoardSettings.Location.X + 200, BoardSettings.Location.Y), BoardSettings.TileSize, BoardSettings.BoardPadding, spriteBatch,
                _colorDict, BoardSettings.SquareTexture, _pieceDictionary);
            _tetrisUtil.DrawHeld(_heldPiece, new Vector2(BoardSettings.Location.X - 75, BoardSettings.Location.Y), BoardSettings.TileSize, BoardSettings.BoardPadding, spriteBatch, _colorDict,
                BoardSettings.SquareTexture, _pieceDictionary);

            //FPS COUNTER
            /*
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter.Update(deltaTime);
            var fps = string.Format("FPS: {0}", _frameCounter.AverageFramesPerSecond);
            _spriteBatch.DrawString(font, fps, new Vector2(1, 1), Color.Black);
            */
            
            spriteBatch.DrawString(BoardSettings.Font, _garbageSent.ToString(), new Vector2(1, 1), Color.Black);
        }
    }
}