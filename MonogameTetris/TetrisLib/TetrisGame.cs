using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using static Microsoft.Xna.Framework.Color;

namespace MonogameTetris.TetrisLib
{
    public class TetrisGame
    {
        private readonly ActivePiece _activePiece = new ActivePiece(new IntVector2(5, 15), 0, 4, 3);
        private readonly BoardSettings _boardSettings;
        private readonly IntVector2 _boardSize = new IntVector2(10, 20);

        private readonly Dictionary<int, Color> _colorDict;
        private readonly ActivePiece _ghostPiece = new ActivePiece(new IntVector2(5, 15), 0, 4, 3);
        private readonly double _gravityIntervalTime;
        private readonly InputLib _inputLib = new InputLib();
        private readonly List<int> _masterPieceQueue = new List<int>();
        private readonly PieceDictionary _pieceDictionary = new PieceDictionary();
        private readonly List<int> _pieceQueuePart = new List<int> {2, 3, 4, 5, 6, 7, 8};
        private readonly PlayerSettings _playerSettings;
        private readonly TetrisUtil _tetrisUtil = new TetrisUtil();
        private int _backToBack;
        private int[,] _boardArray;
        private bool _causesLoss;

        private double _currentTime;
        private int _garbageQueue;
        private int _garbageSent;
        private int _hasHeld;
        private int _heldPiece;
        private bool _lastMoveIsSpin;
        private double _lastTime;
        private double _lastTimeUpdate;
        private int _moveCount;
        private int _rotateCount;
        private double _startTime;
        private int[,] _staticBoardArray;

        private bool _touchingGroundCheck1;
        private bool _wasLastWallkickUsed;
        public bool IsHuman;
        private int _comboCount;
        

        public TetrisGame(bool isHuman, BoardSettings boardSettings, PlayerSettings playerSettings)
        {
            IsHuman = isHuman;
            _boardSettings = boardSettings;
            _playerSettings = playerSettings;

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

                //I ghost
                {20, new Color(32, 156, 213, 127)},
                //J ghost
                {30, new Color(36, 70, 195, 127)},
                //L ghost
                {40, new Color(225, 91, 29, 127)},
                //O ghost
                {50, new Color(225, 158, 37, 127)},
                //S ghost
                {60, new Color(92, 175, 31, 127)},
                //T ghost
                {70, new Color(173, 46, 137, 127)},
                //Z ghost
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
                else if (_inputLib.DelayRepeatPress(Keys.Right, _playerSettings.DelayTime, _playerSettings.RepeatTime,
                             _lastTime, _currentTime,
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
                else if (_inputLib.DelayRepeatPress(Keys.Left, _playerSettings.DelayTime, _playerSettings.RepeatTime,
                             _lastTime, _currentTime,
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

                    _garbageSent = TetrisUtil.ClearLines(ref _staticBoardArray, staticBoardArrayCopy, ref _backToBack,
                        _lastMoveIsSpin, _activePiece, _wasLastWallkickUsed, ref _comboCount);

                    _activePiece.ResetPiece(_masterPieceQueue[0], _masterPieceQueue[0] == 2 ? 4 : 3);
                    _masterPieceQueue.RemoveAt(0);

                    //refill queue
                    if (_masterPieceQueue.Count <= 7)
                    {
                        _pieceQueuePart.Shuffle();
                        _masterPieceQueue.AddRange(_pieceQueuePart);
                    }
                    
                    //handle garbage
                    if (_garbageQueue != 0 && _garbageSent > 0)
                    {
                        _garbageQueue -= _garbageSent;

                        if (_garbageQueue < 0)
                        {
                            _garbageSent = _garbageQueue * -1;
                            _garbageQueue = 0;
                        }
                    }

                    if (_garbageQueue != 0 && _garbageSent == 0)
                    {
                        _causesLoss = TetrisUtil.AddGarbage(ref _staticBoardArray, _garbageQueue);
                        if (_causesLoss)
                        {
                            //TODO: implement loss event
                        }
                        _garbageQueue = 0;
                    }

                    _rotateCount = 0;
                    _touchingGroundCheck1 = false;

                    /*
                    foreach (var line in _tetrisUtil.CheckLines(_staticBoardArray))
                        _staticBoardArray = _tetrisUtil.ClearRow(_staticBoardArray, line);
                        */
                }
                else if (_inputLib.RepeatPress(Keys.Down, _playerSettings.RepeatTime, _lastTime, _currentTime))
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

                            _activePiece.ResetPiece(_masterPieceQueue[0], _masterPieceQueue[0] == 2 ? 4 : 3);
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
                            _activePiece.ResetPiece(_heldPiece, _heldPiece == 2 ? 4 : 3);

                            _heldPiece = pieceTemp;
                        }

                        _rotateCount = 0;
                        _touchingGroundCheck1 = false;
                    }
                }
                else if (_inputLib.IsNewPress(Keys.G))
                {
                    Console.Write(AiFunctions.FindPossiblePositions(_activePiece.PieceType, _heldPiece, _staticBoardArray).Count.ToString());
                    Console.Write("\n");
                    foreach (var item in AiFunctions.FindPossiblePositions(_activePiece.PieceType, _heldPiece, _staticBoardArray))
                    {
                        Console.Write(item.Position.X.ToString());
                        Console.Write(" ");
                        Console.Write(item.Position.Y.ToString());
                        Console.Write(", ");
                        Console.Write(item.Rotation.ToString());
                        Console.Write("\n");
                    }
                }
            }

            //gravity
            if (gameTime.TotalGameTime.TotalMilliseconds - _lastTimeUpdate > _gravityIntervalTime)
            {
                //update time since last gravity
                _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                
                //Console.Write(_activePiece.CurrentLocation.X.ToString());

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

                        _garbageSent = TetrisUtil.ClearLines(ref _staticBoardArray, staticBoardArrayCopy,
                            ref _backToBack,
                            _lastMoveIsSpin, _activePiece, _wasLastWallkickUsed, ref _comboCount);

                        _activePiece.ResetPiece(_masterPieceQueue[0], _masterPieceQueue[0] == 2 ? 4 : 3);
                        _masterPieceQueue.RemoveAt(0);

                        //refill queue
                        if (_masterPieceQueue.Count <= 7)
                        {
                            _pieceQueuePart.Shuffle();
                            _masterPieceQueue.AddRange(_pieceQueuePart);
                        }

                        //handle garbage
                        if (_garbageQueue != 0 && _garbageSent > 0)
                        {
                            _garbageQueue -= _garbageSent;

                            if (_garbageQueue < 0)
                            {
                                _garbageSent = _garbageQueue * -1;
                                _garbageQueue = 0;
                            }
                        }

                        if (_garbageQueue != 0 && _garbageSent == 0)
                        {
                            _causesLoss = TetrisUtil.AddGarbage(ref _staticBoardArray, _garbageQueue);
                            if (_causesLoss)
                            {
                                //TODO: implement loss event
                            }
                            _garbageQueue = 0;
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

        public ref int SendGarbage()
        {
            return ref _garbageSent;
        }

        public void ReceiveGarbage(ref int garbageData)
        {
            if (garbageData == 0) return;
            _garbageQueue += garbageData;
            garbageData = 0;
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Array.Clear(_boardArray, 0, _boardArray.Length);
            var staticBoardArrayCopy = (int[,]) _staticBoardArray.Clone();
            _boardArray = staticBoardArrayCopy;
            _boardArray = _tetrisUtil.ReturnBoardWithGhostPiece(staticBoardArrayCopy, _ghostPiece);
            _boardArray = _tetrisUtil.ReturnBoardWithPiece(_boardArray, _activePiece);

            //draw board
            TetrisUtil.DrawBoard(_boardSize.X, _boardSize.Y, _boardSettings.Location.X, _boardSettings.Location.Y,
                _boardSettings.TileSize, _boardSettings.BoardPadding, spriteBatch,
                _colorDict, _boardArray, _boardSettings.SquareTexture);

            //draw queue + held piece
            TetrisUtil.DrawQueue(_masterPieceQueue,
                new Vector2(_boardSettings.Location.X + _boardSettings.TileSize * 12, _boardSettings.Location.Y), _boardSettings.TileSize,
                _boardSettings.BoardPadding, spriteBatch,
                _colorDict, _boardSettings.SquareTexture, _pieceDictionary);
            TetrisUtil.DrawHeld(_heldPiece, new Vector2(_boardSettings.Location.X - _boardSettings.TileSize * 5, _boardSettings.Location.Y),
                _boardSettings.TileSize, _boardSettings.BoardPadding, spriteBatch, _colorDict,
                _boardSettings.SquareTexture, _pieceDictionary);

            //if (IsHuman) spriteBatch.DrawString(_boardSettings.Font, _garbageSent.ToString(), new Vector2(1, 1), Black);
        }
    }
}