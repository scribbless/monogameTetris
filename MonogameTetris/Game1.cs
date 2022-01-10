using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonogameTetris.TetrisLib;
using static Microsoft.Xna.Framework.Color;

namespace MonogameTetris
{
    public class Game1 : Game
    {
        private readonly ActivePiece _activePiece = new ActivePiece(new IntVector2(5, 15), 0, 4, 3);
        private readonly ActivePiece _ghostPiece = new ActivePiece(new IntVector2(5, 15), 0, 4, 3);
        private readonly IntVector2 _boardSize = new IntVector2(10, 20);
        private readonly Dictionary<int, Color> _colorDict;
        private readonly InputLib _inputLib = new InputLib();
        private readonly List<int> _masterPieceQueue = new List<int>();
        private readonly List<int> _pieceQueuePart = new List<int> {2, 3, 4, 5, 6, 7, 8};
        private readonly TetrisUtil _tetrisUtil = new TetrisUtil();
        private int[,] _boardArray;
        private int _boardPadding;
        private double _currentTime;
        private int _delayTime;
        private SpriteFont _font;
        private FrameCounter _frameCounter = new FrameCounter();
        private double _gravityIntervalTime;
        private double _lastTime;
        private double _lastTimeUpdate;
        private int _moveCount;
        private readonly PieceDictionary _pieceDictionary = new PieceDictionary();
        private int _repeatTime;
        private int _rotateCount;
        private SpriteBatch _spriteBatch;
        private Color[] _squareData;
        private Texture2D _squareTexture;
        private double _startTime;
        private int[,] _staticBoardArray;
        private string _testText;
        private int _tileSize;
        private bool _touchingGroundCheck1;
        private int _hasHeld;
        private int _heldPiece;
        private bool _lastMoveIsSpin = false;

        public Game1()
        {
            var graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _staticBoardArray = new int[_boardSize.x, _boardSize.y];
            Array.Clear(_staticBoardArray, 0, _staticBoardArray.Length);

            _boardArray = new int[_boardSize.x, _boardSize.y];
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

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            _font = Content.Load<SpriteFont>("Font/Akira");

            //SETUP VARIABLES
            _tileSize = 20;
            _boardPadding = 0;
            _delayTime = 100;
            _repeatTime = 30;
            //drop speed
            _gravityIntervalTime = 500;


            _squareData = new Color[_tileSize * _tileSize];
            _squareTexture = new Texture2D(GraphicsDevice, _tileSize, _tileSize);
            for (var i = 0; i < _squareData.Length; ++i)
                _squareData[i] = White;
            _squareTexture.SetData(_squareData);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //show currently pressed keys
            _testText = string.Join(" ", Keyboard.GetState().GetPressedKeys());

            //update inputlib states
            _inputLib.Update();

            _currentTime = gameTime.TotalGameTime.TotalMilliseconds;

            //handle keypresses
            if (_inputLib.IsNewPress(Keys.Up) && _rotateCount <= 15)
            {
                if (_activePiece.IncreaseRotState(_staticBoardArray)) _lastMoveIsSpin = true;

                if (_activePiece.IsTouchingBlock(_staticBoardArray))
                {
                    _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                    _rotateCount++;
                }
            }
            else if (_inputLib.IsNewPress(Keys.Z))
            {
                if (_activePiece.DecreaseRotState(_staticBoardArray)) _lastMoveIsSpin = true;

                if (_activePiece.IsTouchingBlock(_staticBoardArray) && _rotateCount <= 15)
                {
                    _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                    _rotateCount++;
                }
            }
            else if (_inputLib.DelayRepeatPress(Keys.Right, _delayTime, _repeatTime, _lastTime, _currentTime,
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
            else if (_inputLib.DelayRepeatPress(Keys.Left, _delayTime, _repeatTime, _lastTime, _currentTime,
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

                _staticBoardArray = _activePiece.ReturnLockedInBoard(_staticBoardArray);
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

                foreach (var line in _tetrisUtil.CheckLines(_staticBoardArray))
                    _staticBoardArray = _tetrisUtil.ClearRow(_staticBoardArray, line);
            }
            else if (_inputLib.RepeatPress(Keys.Down, _repeatTime, _lastTime, _currentTime))
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
                        _heldPiece = _activePiece.pieceType;

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
                        var pieceTemp = _activePiece.pieceType;
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
                        _staticBoardArray = _activePiece.ReturnLockedInBoard(_staticBoardArray);
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

                        foreach (var line in _tetrisUtil.CheckLines(_staticBoardArray))
                            _staticBoardArray = _tetrisUtil.ClearRow(_staticBoardArray, line);
                    }
                }
                else
                {
                    _activePiece.MoveDown(_staticBoardArray);
                    _lastMoveIsSpin = false;
                }
            }

            _ghostPiece.CurrentLocation = _activePiece.CurrentLocation;
            _ghostPiece.rotState = _activePiece.rotState;
            _ghostPiece.pieceType = _activePiece.pieceType;
            _ghostPiece.sideLength = _activePiece.sideLength;
            _ghostPiece.HardDrop(_staticBoardArray);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //draw background
            GraphicsDevice.Clear(DarkSlateBlue);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            //set up board 1

            Array.Clear(_boardArray, 0, _boardArray.Length);
            var staticBoardArrayCopy = (int[,]) _staticBoardArray.Clone();
            _boardArray = staticBoardArrayCopy;
            _boardArray = _tetrisUtil.ReturnBoardWithGhostPiece(staticBoardArrayCopy, _ghostPiece);
            _boardArray = _tetrisUtil.ReturnBoardWithPiece(_boardArray, _activePiece);

            //draw board
            _tetrisUtil.DrawBoard(_boardSize.x, _boardSize.y, 100, 100, _tileSize, _boardPadding, _spriteBatch,
                _colorDict, _boardArray, _squareTexture);

            //draw queue + held piece
            _tetrisUtil.DrawQueue(_masterPieceQueue, new Vector2(400, 100), _tileSize, _boardPadding, _spriteBatch,
                _colorDict, _squareTexture, _pieceDictionary);
            _tetrisUtil.DrawHeld(_heldPiece, new Vector2(15, 100), _tileSize, _boardPadding, _spriteBatch, _colorDict,
                _squareTexture, _pieceDictionary);

            //FPS COUNTER
            /*
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter.Update(deltaTime);
            var fps = string.Format("FPS: {0}", _frameCounter.AverageFramesPerSecond);
            _spriteBatch.DrawString(font, fps, new Vector2(1, 1), Color.Black);
            */
            
            _spriteBatch.DrawString(_font, _lastMoveIsSpin.ToString(), new Vector2(1, 1), Color.Black);

            // Finds the center of the string in coordinates inside the text rectangle
            var textMiddlePoint = _font.MeasureString(_testText) / 2;
            // Places text in center of the screen
            var position = new Vector2(Window.ClientBounds.Width / 2,
                (int) Math.Round(Window.ClientBounds.Height / 1.2f));
            _spriteBatch.DrawString(_font, _testText, position, Black, 0, textMiddlePoint, 1.0f, SpriteEffects.None,
                0.5f);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}