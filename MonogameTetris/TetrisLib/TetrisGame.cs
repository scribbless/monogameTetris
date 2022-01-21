using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private bool _backToBack;
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
        private double _lastMoveUpdate;
        private double _AiMoveIntervalTime;
        private double _AiMoveIntervalTimeBase;
        private Random _random = new Random();
        private PossibleMove _move = new PossibleMove();
        private bool _movingIntoPlace;
        private char[] _path;
        private int _pathIndex;
        private double _oldYpos;
        private bool _IsWaiting;

        public readonly double[] HeuristicWeights;
        

        public TetrisGame(bool isHuman, BoardSettings boardSettings, PlayerSettings playerSettings, double[] heuristicWeights)
        {
            IsHuman = isHuman;
            _boardSettings = boardSettings;
            _playerSettings = playerSettings;
            HeuristicWeights = heuristicWeights;

            _gravityIntervalTime = 500;
            _AiMoveIntervalTimeBase = 50;

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

            if (!isHuman)
            {
                _move = AiFunctions.FindBestMoveAndPath(_activePiece.PieceType, _staticBoardArray, _backToBack, _lastMoveIsSpin,
                    _wasLastWallkickUsed, _comboCount, HeuristicWeights);
            }
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

                } else if (_inputLib.IsNewPress(Keys.J))
                {
                    Debug.Write(_activePiece.CurrentLocation.X.ToString());
                    Debug.Write(" ");
                    Debug.Write(_activePiece.CurrentLocation.Y.ToString());
                    Debug.Write(", ");
                    Debug.Write(_activePiece.RotState.ToString());
                    Debug.Write("\n");
                }
                else if (_inputLib.IsNewPress(Keys.H))
                {
                    var watch = Stopwatch.StartNew();
                    // the code that you want to measure comes here
                    var staticBoardArrayCopy = (int[,]) _staticBoardArray.Clone();
                    var positions = AiFunctions.FindBestMoveAndPath(_activePiece.PieceType, staticBoardArrayCopy, _backToBack, _lastMoveIsSpin, _wasLastWallkickUsed, _comboCount, new double[]{1,1,1,1,1,1,1,1,1});
                    watch.Stop();
                    
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Debug.WriteLine($"time taken: {elapsedMs}MS");
                }
                else if (_inputLib.IsNewPress(Keys.K))
                {
                    var watch = Stopwatch.StartNew();
                    // the code that you want to measure comes here
                    var staticBoardArrayCopy = (int[,]) _staticBoardArray.Clone();
                    var cost = AiFunctions.CalculateCost(staticBoardArrayCopy, _activePiece.ReturnLockedInBoard(_staticBoardArray), new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 },
                        _backToBack, _lastMoveIsSpin, _activePiece, _wasLastWallkickUsed, _comboCount);
                    watch.Stop();
                    
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Debug.WriteLine($"time taken: {elapsedMs}MS");
                }
            }
            //AI MOVEMENT FUNCTIONS
            else
            {
                if (gameTime.TotalGameTime.TotalMilliseconds - _lastMoveUpdate > _AiMoveIntervalTime)
                {
                    //update time since last gravity
                    _lastMoveUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                    
                    // make movement more realistic
                    _AiMoveIntervalTime = _AiMoveIntervalTimeBase + (((_random.NextDouble() * 2) - 1) * 30);

                    if (_movingIntoPlace)
                    {
                        if (_pathIndex > _path.Length)
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
                            
                            _move = AiFunctions.FindBestMoveAndPath(_activePiece.PieceType, _staticBoardArray, _backToBack,
                                _lastMoveIsSpin, false, _comboCount, HeuristicWeights);

                            _movingIntoPlace = false;
                        }
                        else
                        {
                            var character = _path[_pathIndex];
                            switch (character - '0')
                            {
                                //move right
                                case 0:
                                    if (_activePiece.MoveRight(_staticBoardArray)) _lastMoveIsSpin = false;

                                    if (_activePiece.IsTouchingBlock(_staticBoardArray) && _moveCount <= 20)
                                    {
                                        _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                                        _moveCount++;
                                    }
                                    break;
                                //move left
                                case 1:
                                    if (_activePiece.MoveLeft(_staticBoardArray)) _lastMoveIsSpin = false;

                                    if (_activePiece.IsTouchingBlock(_staticBoardArray) && _moveCount <= 20)
                                    {
                                        _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                                        _moveCount++;
                                    }
                                    break;
                                //move down
                                case 2:
                                    if (!_IsWaiting)
                                    {
                                        _IsWaiting = true;
                                    }
                                    if (_oldYpos < _activePiece.CurrentLocation.Y)
                                    {
                                        _IsWaiting = false;
                                    }
                                    
                                    //if (_activePiece.MoveDown(_staticBoardArray)) _lastMoveIsSpin = false;
                                    break;
                                //rotate R
                                case 3:
                                    var rotStateReturn1 = _activePiece.IncreaseRotState(_staticBoardArray);
                                    if (rotStateReturn1[0]) _lastMoveIsSpin = true;
                                    _wasLastWallkickUsed = rotStateReturn1[1];

                                    if (_activePiece.IsTouchingBlock(_staticBoardArray))
                                    {
                                        _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                                        _rotateCount++;
                                    }
                                    break;
                                //rotate L
                                case 4:
                                    var rotStateReturn2 = _activePiece.DecreaseRotState(_staticBoardArray);
                                    if (rotStateReturn2[0]) _lastMoveIsSpin = true;
                                    _wasLastWallkickUsed = rotStateReturn2[1];

                                    if (_activePiece.IsTouchingBlock(_staticBoardArray) && _rotateCount <= 15)
                                    {
                                        _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                                        _rotateCount++;
                                    }
                                    break;
                            }

                            //if (!_IsWaiting) _pathIndex++;
                            _pathIndex++;
                        }
                    }
                    //ROTATE LEFT
                    else if (_move.dropPosition.Rotation < _activePiece.RotState)
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
                    //ROTATE RIGHT
                    else if (_move.dropPosition.Rotation > _activePiece.RotState)
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
                    //MOVE RIGHT
                    else if (_move.dropPosition.Position.X > _activePiece.CurrentLocation.X)
                    {
                        if (_activePiece.MoveRight(_staticBoardArray)) _lastMoveIsSpin = false;

                        if (_activePiece.IsTouchingBlock(_staticBoardArray) && _moveCount <= 20)
                        {
                            _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                            _moveCount++;
                        }
                    }
                    //MOVE LEFT
                    else if (_move.dropPosition.Position.X < _activePiece.CurrentLocation.X)
                    {
                        if (_activePiece.MoveLeft(_staticBoardArray)) _lastMoveIsSpin = false;

                        if (_activePiece.IsTouchingBlock(_staticBoardArray) && _moveCount <= 20)
                        {
                            _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                            _moveCount++;
                        }
                    }
                    //HARD DROP
                    else if (_move.path == "888")
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
                        
                        _move = AiFunctions.FindBestMoveAndPath(_activePiece.PieceType, _staticBoardArray, _backToBack,
                            _lastMoveIsSpin, false, _comboCount, HeuristicWeights);
                    }
                    else if (_move.dropPosition.Position.Y < _activePiece.CurrentLocation.Y)
                    {
                        /*while (_move.dropPosition.Position.Y < _activePiece.CurrentLocation.Y)
                        {
                            if (!(gameTime.TotalGameTime.TotalMilliseconds - _lastTime > _playerSettings.RepeatTime))
                                continue;
                            _lastTime = gameTime.TotalGameTime.TotalMilliseconds;
                            if (_activePiece.MoveDown(_staticBoardArray)) _lastMoveIsSpin = false;
                        }*/

                        if (_move.dropPosition.Position.Y < _activePiece.CurrentLocation.Y)
                        {
                            if (_activePiece.MoveDown(_staticBoardArray)) _lastMoveIsSpin = false;
                            _AiMoveIntervalTime = 20;
                        }
                        else
                        {
                            var charArray = _move.path.ToCharArray();
                            Array.Reverse(charArray);
                            _movingIntoPlace = true;
                            _path = charArray;
                            _pathIndex = 0;
                        }
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

                        if (!IsHuman)
                        {
                            _move = AiFunctions.FindBestMoveAndPath(_activePiece.PieceType, _staticBoardArray, _backToBack,
                                _lastMoveIsSpin, false, _comboCount, HeuristicWeights);
                        }

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

            //if (IsHuman) spriteBatch.DrawString(_boardSettings.Font, _activePiece.CanSeeRoof(_staticBoardArray).ToString(), new Vector2(500, 1), Black);
        }
    }
}