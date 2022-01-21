using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonogameTetris.TetrisLib;
using static Microsoft.Xna.Framework.Color;

namespace MonogameTetris
{
    public class Game1 : Game
    {
        private TetrisGame _ai;
        private bool _causesLoss = false;
        private SpriteFont _font;
        private FrameCounter _frameCounter = new FrameCounter();
        private TetrisGame _player;
        private SpriteBatch _spriteBatch;
        private Color[] _squareData;
        private Texture2D _squareTexture;
        private string _testText;
        private int _tileSize;
        private bool _paused;
        private InputLib _inputLib = new InputLib();



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

            _squareData = new Color[_tileSize * _tileSize];
            _squareTexture = new Texture2D(GraphicsDevice, _tileSize, _tileSize);
            for (var i = 0; i < _squareData.Length; ++i)
                _squareData[i] = White;
            _squareTexture.SetData(_squareData);

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

            _player = new TetrisGame(true, new BoardSettings(20, 0, new IntVector2(100, 100), _squareTexture, _font),
                new PlayerSettings(300, 30), null);

            _ai = new TetrisGame(false, new BoardSettings(20, 0, new IntVector2(800, 100), _squareTexture, _font),
                new PlayerSettings(300, 30), new double[]{-0.2, 1, -0.4, -0.2, -0.1, -0.1, -0.1, 0.5, 0.1});
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            //show currently pressed keys
            _testText = string.Join(" ", Keyboard.GetState().GetPressedKeys());
            
            _inputLib.Update();

            if (!_paused)
            {
                _player.Update(gameTime);
                _ai.Update(gameTime);

                _player.ReceiveGarbage(ref _ai.SendGarbage());
                _ai.ReceiveGarbage(ref _player.SendGarbage());
            }

            if (_inputLib.IsNewPress(Keys.P))
            {
                _paused = !_paused;
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //draw background
            GraphicsDevice.Clear(DarkSlateBlue);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            //DRAW PLAYER BOARD
            _player.Draw(gameTime, _spriteBatch);
            _ai.Draw(gameTime, _spriteBatch);

            // Finds the center of the string in coordinates inside the text rectangle
            var textMiddlePoint = _font.MeasureString(_testText) / 2;
            // Places text in center of the screen
            var position = new Vector2(Window.ClientBounds.Width / 2,
                (int) Math.Round(Window.ClientBounds.Height / 1.2f));
            _spriteBatch.DrawString(_font, _testText, position, Black, 0, textMiddlePoint, 1.0f, SpriteEffects.None,
                0.5f);
            
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter.Update(deltaTime);
            var fps = string.Format("FPS: {0}", _frameCounter.AverageFramesPerSecond);
            _spriteBatch.DrawString(_font, fps, new Vector2(1, 1), Black);
            
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}