﻿using System;
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
        private SpriteFont _font;
        private FrameCounter _frameCounter = new FrameCounter();
        private SpriteBatch _spriteBatch;
        private Color[] _squareData;
        private Texture2D _squareTexture;
        private string _testText;
        private int _tileSize;
        private int _gravityIntervalTime;
        private TetrisGame _player;


        public Game1()
        {
            var graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferredBackBufferWidth = 1280;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
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
            //tile size = 20 board padding = 0
            _tileSize = 20;

            _squareData = new Color[_tileSize * _tileSize];
            _squareTexture = new Texture2D(GraphicsDevice, _tileSize, _tileSize);
            for (var i = 0; i < _squareData.Length; ++i)
                _squareData[i] = White;
            _squareTexture.SetData(_squareData);

            //_boardSettings = new BoardSettings(20, 0, new Vector2(100, 100), _squareTexture);

            _player = new TetrisGame(true, new BoardSettings(20, 0, new IntVector2(100, 100), _squareTexture, _font),
                new PlayerSettings(300, 30));
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //show currently pressed keys
            _testText = string.Join(" ", Keyboard.GetState().GetPressedKeys());

            _player.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            //draw background
            GraphicsDevice.Clear(DarkSlateBlue);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);

            //DRAW PLAYER BOARD
            _player.Draw(gameTime, _spriteBatch);

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