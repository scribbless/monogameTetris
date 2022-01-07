using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonogameTetris.TetrisLib
{
    public enum MouseButtons
    {
        LeftButton,
        MiddleButton,
        RightButton
    }

    public class InputLib
    {
        private KeyboardState _oldKeyboardState;
        private KeyboardState _newKeyboardState;
        private MouseState _oldMouseState;
        private MouseState _newMouseState;
        private bool _refreshData;

        public void Update()
        {
            if (!_refreshData)
                _refreshData = true;

            _oldKeyboardState = _newKeyboardState;
            _newKeyboardState = Keyboard.GetState();

            _oldMouseState = _newMouseState;
            _newMouseState = Mouse.GetState();
        }

        public KeyboardState OldKeyboardState => _oldKeyboardState;

        public KeyboardState NewKeyboardState => _newKeyboardState;

        public MouseState OldMouseState => _oldMouseState;

        public MouseState NewMouseState => _newMouseState;

        public Vector2 MousePosition => new Vector2(_newMouseState.X, _newMouseState.Y);

        //public float MouseScrollWheelPosition => _newMouseState.ScrollWheelValue;

        public bool IsNewPress(Keys key)
        {
            return (
                _oldKeyboardState.IsKeyUp(key) &&
                _newKeyboardState.IsKeyDown(key));
        }

        public bool IsCurPress(Keys key)
        {
            return (
                _oldKeyboardState.IsKeyDown(key) &&
                _newKeyboardState.IsKeyDown(key));
        }

        public bool IsOldPress(Keys key)
        {
            return (
                _oldKeyboardState.IsKeyDown(key) &&
                _newKeyboardState.IsKeyUp(key));
        }

        public bool DelayRepeatPress(Keys key, int delayTime, int repeatTime, double lastTime, double currentTime, double startTime)
        {
            if (_oldKeyboardState.IsKeyUp(key) && _newKeyboardState.IsKeyDown(key))
                return true;

            if (currentTime - startTime > delayTime && currentTime - lastTime > repeatTime && IsCurPress(key))
                return true;

            return false;
        }

        public bool RepeatPress(Keys key, int repeatTime, double lastTime, double currentTime)
        {
            if (currentTime - lastTime > repeatTime && IsCurPress(key))
                return true;
            
            return false;
        }

        public bool IsNewMousePress(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.LeftButton:
                    return (
                        _oldMouseState.LeftButton == ButtonState.Released &&
                        _newMouseState.LeftButton == ButtonState.Pressed);
                case MouseButtons.MiddleButton:
                    return (
                        _oldMouseState.MiddleButton == ButtonState.Released &&
                        _newMouseState.MiddleButton == ButtonState.Pressed);
                case MouseButtons.RightButton:
                    return (
                        _oldMouseState.RightButton == ButtonState.Released &&
                        _newMouseState.RightButton == ButtonState.Pressed);
                default:
                    return false;
            }
        }

        public bool IsCurMousePress(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.LeftButton:
                    return (
                        _oldMouseState.LeftButton == ButtonState.Pressed &&
                        _newMouseState.LeftButton == ButtonState.Pressed);
                case MouseButtons.MiddleButton:
                    return (
                        _oldMouseState.MiddleButton == ButtonState.Pressed &&
                        _newMouseState.MiddleButton == ButtonState.Pressed);
                case MouseButtons.RightButton:
                    return (
                        _oldMouseState.RightButton == ButtonState.Pressed &&
                        _newMouseState.RightButton == ButtonState.Pressed);
                default:
                    return false;
            }
        }

        public bool IsOldMousePress(MouseButtons button)
        {
            switch (button)
            {
                case MouseButtons.LeftButton:
                    return (
                        _oldMouseState.LeftButton == ButtonState.Pressed &&
                        _newMouseState.LeftButton == ButtonState.Released);
                case MouseButtons.MiddleButton:
                    return (
                        _oldMouseState.MiddleButton == ButtonState.Pressed &&
                        _newMouseState.MiddleButton == ButtonState.Released);
                case MouseButtons.RightButton:
                    return (
                        _oldMouseState.RightButton == ButtonState.Pressed &&
                        _newMouseState.RightButton == ButtonState.Released);
                default:
                    return false;
            }
        }
    }
}