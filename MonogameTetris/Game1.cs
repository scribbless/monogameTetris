using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;
using MonogameTetris.TetrisLib;
using static Microsoft.Xna.Framework.Color;

namespace MonogameTetris
{
    public class Game1 : Game
    {
        private SpriteFont _font;
        private string _testText;
        private int[,] _boardArray;
        private int[,] _staticBoardArray;
        private SpriteBatch _spriteBatch;
        private readonly Dictionary<int, Color> _colorDict;
        private Color[] _squareData;
        private Texture2D _squareTexture;
        private int _tileSize;
        private int _boardPadding;
        private readonly InputLib _inputLib = new InputLib();
        private readonly TetrisUtil _tetrisUtil = new TetrisUtil();
        private double _lastTimeUpdate;
        private double _gravityIntervalTime;
        private readonly ActivePiece _activePiece = new ActivePiece(new IntVector2(5, 15), 0, 4, 3);
        private FrameCounter _frameCounter = new FrameCounter();
        private readonly IntVector2 _boardSize = new IntVector2(10, 20);
        private readonly List<int> _pieceQueuePart = new List<int>() {2, 3, 4, 5, 6, 7, 8};
        private readonly List<int> _masterPieceQueue = new List<int>();
        private bool _touchingGroundCheck1;
        private int _rotateCount;
        private int _moveCount;
        private int _delayTime;
        private int _repeatTime;
        private double _startTime;
        private double _currentTime;
        private double _lastTime;
        private int heldPiece;
        private int hasHeld = 0;

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
                {0, DimGray},
                {1, DarkGray},
                {2, Aqua},
                {3, Blue},
                {4, OrangeRed},
                {5, Goldenrod},
                {6, LimeGreen},
                {7, Purple},
                {8, Red}
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
            _delayTime = 200;
            _repeatTime = 30;
            //drop speed
            _gravityIntervalTime = 300;


            _squareData = new Color[_tileSize * _tileSize];
            _squareTexture = new Texture2D(GraphicsDevice, _tileSize, _tileSize);
            for (int i = 0; i < _squareData.Length; ++i) 
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
                _activePiece.IncreaseRotState(_staticBoardArray);

                if (_activePiece.IsTouchingBlock(_staticBoardArray))
                {
                    _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                    _rotateCount++;
                }
            }
            else if (_inputLib.IsNewPress(Keys.Z))
            {
                _activePiece.DecreaseRotState(_staticBoardArray);
                
                if (_activePiece.IsTouchingBlock(_staticBoardArray) && _rotateCount <= 15)
                {
                    _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                    _rotateCount++;
                }
            }
            else if (_inputLib.DelayRepeatPress(Keys.Right, _delayTime, _repeatTime, _lastTime, _currentTime, _startTime))
            {
                if (_inputLib.IsNewPress(Keys.Right))
                {
                    _startTime = _currentTime;
                }

                _lastTime = _currentTime;
                _activePiece.MoveRight(_staticBoardArray);
                
                if (_activePiece.IsTouchingBlock(_staticBoardArray) && _moveCount <= 20)
                {
                    _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                    _moveCount++;
                }
            }
            else if (_inputLib.DelayRepeatPress(Keys.Left, _delayTime, _repeatTime, _lastTime, _currentTime, _startTime))
            {
                if (_inputLib.IsNewPress(Keys.Left))
                {
                    _startTime = _currentTime;
                }

                _lastTime = _currentTime;
                _activePiece.MoveLeft(_staticBoardArray);
                
                if (_activePiece.IsTouchingBlock(_staticBoardArray) && _moveCount <= 20)
                {
                    _lastTimeUpdate = gameTime.TotalGameTime.TotalMilliseconds;
                    _moveCount++;
                }
            } else if (_inputLib.IsNewPress(Keys.Space))
            {
                hasHeld = 0;
                _activePiece.HardDrop(_staticBoardArray);
                
                _staticBoardArray = _activePiece.ReturnLockedInBoard(_staticBoardArray);
                if (_masterPieceQueue[0] == 2)
                {
                    _activePiece.ResetPiece(_masterPieceQueue[0], 4);
                }
                else
                {
                    _activePiece.ResetPiece(_masterPieceQueue[0], 3);
                }
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
                {
                    _staticBoardArray = _tetrisUtil.ClearRow(_staticBoardArray, line);
                }
            } else if (_inputLib.RepeatPress(Keys.Down, _repeatTime, _lastTime, _currentTime))
            {
                _lastTime = _currentTime;
                _activePiece.MoveDown(_staticBoardArray);
                //touchingGrouncCheck1 = true;
            } else if (_inputLib.IsNewPress(Keys.C))
            {
                if (hasHeld == 0)
                {
                    hasHeld = 1;

                    if (heldPiece == 0)
                    {
                        heldPiece = _activePiece.pieceType;
                        _masterPieceQueue.RemoveAt(0);
                        if (_masterPieceQueue[0] == 2)
                        {
                            _activePiece.ResetPiece(_masterPieceQueue[0], 4);
                        }
                        else
                        {
                            _activePiece.ResetPiece(_masterPieceQueue[0], 3);
                        }

                        //refill queue
                        if (_masterPieceQueue.Count <= 7)
                        {
                            _pieceQueuePart.Shuffle();
                            _masterPieceQueue.AddRange(_pieceQueuePart);
                        }
                    }
                    else
                    {
                        int pieceTemp = _activePiece.pieceType;
                        if (heldPiece == 2)
                        {
                            _activePiece.ResetPiece(heldPiece, 4);
                        }
                        else
                        {
                            _activePiece.ResetPiece(heldPiece, 3);
                        }

                        heldPiece = pieceTemp;
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
                        hasHeld = 0;
                        _staticBoardArray = _activePiece.ReturnLockedInBoard(_staticBoardArray);
                        if (_masterPieceQueue[0] == 2)
                        {
                            _activePiece.ResetPiece(_masterPieceQueue[0], 4);
                        }
                        else
                        {
                            _activePiece.ResetPiece(_masterPieceQueue[0], 3);
                        }
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
                        {
                            _staticBoardArray = _tetrisUtil.ClearRow(_staticBoardArray, line);
                        }
                    }
                }
                else
                {
                    _activePiece.MoveDown(_staticBoardArray);
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //draw background
            GraphicsDevice.Clear(DarkSlateBlue);
            _spriteBatch.Begin();
            
            //set up board 1

            Array.Clear(_boardArray, 0, _boardArray.Length);
            int[,] staticBoardArrayCopy = (int[,])_staticBoardArray.Clone();
            _boardArray = staticBoardArrayCopy;
            _boardArray = _tetrisUtil.ReturnBoardWithPiece(staticBoardArrayCopy, _activePiece);

            _tetrisUtil.DrawBoard(_boardSize.x, _boardSize.y, 100, 100, _tileSize, _boardPadding, _spriteBatch, _colorDict, _boardArray, _squareTexture);
            
            _tetrisUtil.DrawQueue(_masterPieceQueue, new Vector2(400, 100), _tileSize, _boardPadding, _spriteBatch, _colorDict, _squareTexture, new PieceDictionary());
            
            //FPS COUNTER
            /*
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter.Update(deltaTime);
            var fps = string.Format("FPS: {0}", _frameCounter.AverageFramesPerSecond);
            _spriteBatch.DrawString(font, fps, new Vector2(1, 1), Color.Black);
            */
            
            _spriteBatch.DrawString(_font, heldPiece.ToString(), new Vector2(1, 1), Color.Black);

            // Finds the center of the string in coordinates inside the text rectangle
            Vector2 textMiddlePoint = _font.MeasureString(_testText) / 2;
            // Places text in center of the screen
            Vector2 position = new Vector2(Window.ClientBounds.Width / 2, (int)Math.Round(Window.ClientBounds.Height / 1.2f));
            _spriteBatch.DrawString(_font, _testText, position, Black, 0, textMiddlePoint, 1.0f, SpriteEffects.None,
                0.5f);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}