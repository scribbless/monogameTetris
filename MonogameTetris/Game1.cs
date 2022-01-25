using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonogameTetris.TetrisLib;
using static Microsoft.Xna.Framework.Color;

namespace MonogameTetris
{
    public class Game1 : Game
    {
        private SpriteFont _font;
        private readonly FrameCounter _frameCounter = new FrameCounter();
        private readonly InputLib _inputLib = new InputLib();
        private bool _paused;
        private TetrisGame _player;
        private TetrisGame _ai;
        private SpriteBatch _spriteBatch;
        private Color[] _squareData;
        private Texture2D _squareTexture;
        private string _testText;
        private int _tileSize;
        private IntVector2 _gamePosition;

        private bool _menu;
        //private AiAndThreadManager _aiAndThreadManager;

        public Game1()
        {
            var graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here
            _font = Content.Load<SpriteFont>("Font/Akira");

            //SETUP VARIABLES
            //tile size = 20 board padding = 0
            _tileSize = 20;
            _gamePosition = new IntVector2(200, 100);
            _menu = true;

            _squareData = new Color[_tileSize * _tileSize];
            _squareTexture = new Texture2D(GraphicsDevice, _tileSize, _tileSize);
            for (var i = 0; i < _squareData.Length; ++i)
                _squareData[i] = White;
            _squareTexture.SetData(_squareData);
            
            //_aiAndThreadManager = new AiAndThreadManager(3, 9, _tileSize, _gamePosition, _squareTexture, _font);

            //_boardSettings = new BoardSettings(20, 0, new Vector2(100, 100), _squareTexture);

            /*  0 = AggregateHeight
                1 = GarbageAmount
                2 = HolesNum
                3 = ColumnHolesNum
                4 = Bumpiness
                5 = HorizontalTransitions
                6 = VerticalTransitions
                7 = Pits
                8 = DeepestWell
             */
            
            //MEDIUM DIFF {0.0869992666351604, 0.95414121041733007, -4.051184477680916, -1.881675142367219, -0.39356570290101967, -1.4050722571113483, -0.3283603063450941, -0.2504628801487685, -0.9286245450045114}
            
            
            _player = new TetrisGame(true, new BoardSettings(_tileSize, 0, _gamePosition, _squareTexture, _font),
                new PlayerSettings(300, 30), new [] {-0.2869992666351604, -0.25414121041733007, -4.051184477680916, -1.881675142367219, -0.39356570290101967, -1.4050722571113483, -0.3283603063450941, -2.9504628801487685, -1.9286245450045114}, false);

            _ai = new TetrisGame(false, new BoardSettings(_tileSize, 0, new IntVector2(_gamePosition.X + (_tileSize * 22), _gamePosition.Y), _squareTexture, _font),
                new PlayerSettings(300, 30), new[] {0.0869992666351604, 0.95414121041733007, -4.051184477680916, -1.881675142367219, -0.39356570290101967, -1.4050722571113483, -0.3283603063450941, -0.2504628801487685, -0.9286245450045114}, false);
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            //show currently pressed keys
            _testText = string.Join(" ", Keyboard.GetState().GetPressedKeys());

            _inputLib.Update();

            if (!_paused && !_menu)
            {
                
                _player.Update(gameTime);
                _ai.Update(gameTime);

                _player.ReceiveGarbage(ref _ai.SendGarbage());
                _ai.ReceiveGarbage(ref _player.SendGarbage());
                
                
                //_aiAndThreadManager.Update(gameTime);
            }
            else
            {
                if (_inputLib.IsNewPress(Keys.Enter))
                {
                    _menu = false;
                }
            }

            if (_inputLib.IsNewPress(Keys.P)) _paused = !_paused;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //draw background
            GraphicsDevice.Clear(DarkSlateBlue);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            //DRAW PLAYER BOARD

            if (_menu)
            {
                _spriteBatch.DrawString(_font, $"AWESOME TETRIS\n\nleft arrow: move left\nright arrow: move right\ndown arrow: soft drop\nspace key: hard drop\nc key: hold piece\nup arrow: rotate right\nz key: rotate left\np key: pause\nyou are the left board\n\npress enter to start", new Vector2(500, 100), Black);
            }
            else
            {
                _player.Draw(gameTime, _spriteBatch);
                _ai.Draw(gameTime, _spriteBatch);
            }

            //_aiAndThreadManager.Draw(gameTime, _spriteBatch);
            

            // Finds the center of the string in coordinates inside the text rectangle
            var textMiddlePoint = _font.MeasureString(_testText) / 2;
            // Places text in center of the screen
            var position = new Vector2(Window.ClientBounds.Width / 2,
                (int) Math.Round(Window.ClientBounds.Height / 1.2f));
            _spriteBatch.DrawString(_font, _testText, position, Black, 0, textMiddlePoint, 1.0f, SpriteEffects.None,
                0.5f);

            var deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter.Update(deltaTime);
            var fps = $"FPS: {_frameCounter.AverageFramesPerSecond}";
            _spriteBatch.DrawString(_font, fps, new Vector2(1, 1), Black);
            //_spriteBatch.DrawString(_font, $"Generation: {_aiAndThreadManager.GenerationNum}", new Vector2(300, 1), Black);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}