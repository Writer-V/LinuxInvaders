using Microsoft.Xna.Framework.Input;

namespace LinuxInvaders.Core.Input
{
    public readonly record struct InputBinding
    {
        public readonly InputDevice Device;
        public readonly Keys Key;
        public readonly MouseButton Button;
        public InputBinding(Keys key)
        {
            Device = InputDevice.Keyboard;
            Key = key;
        }
        public InputBinding(MouseButton button)
        {
            Device = InputDevice.Mouse;
            Button = button;
        }
        public bool IsDown(KeyboardState state)
        {
            if (Device != InputDevice.Keyboard) return false;
            return state.IsKeyDown(Key);
        }
        public bool IsDown(MouseState state)
        {
            if (Device != InputDevice.Mouse) return false;
            switch (Button)
            {
                case MouseButton.Left:
                    return state.LeftButton == ButtonState.Pressed;
                case MouseButton.Middle:
                    return state.MiddleButton == ButtonState.Pressed;
                case MouseButton.Right:
                    return state.RightButton == ButtonState.Pressed;
                case MouseButton.Button1:
                    return state.XButton1 == ButtonState.Pressed;
                case MouseButton.Button2:
                    return state.XButton2 == ButtonState.Pressed;
                default:
                    return false;
            }
        }
    }
}