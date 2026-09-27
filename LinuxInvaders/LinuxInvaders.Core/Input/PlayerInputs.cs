using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LinuxInvaders.Core.Input
{
    public class PlayerInputs
    {
        private KeyboardState previousKeyboard = new KeyboardState();
        private KeyboardState currentKeyboard = new KeyboardState();
        private MouseState previousMouse = new MouseState();
        private MouseState currentMouse = new MouseState();
        private readonly Dictionary<InputAction, InputBinding[]> bindings = new()
        {
            [InputAction.MoveLeft]  = new[] {new InputBinding(Keys.Left), new InputBinding(Keys.A)},
            [InputAction.MoveRight] = new[] {new InputBinding(Keys.Right), new InputBinding(Keys.D)},
            [InputAction.Fire]      = new[] {new InputBinding(Keys.Space)},
            [InputAction.Confirm]   = new[] {new InputBinding(Keys.Enter)},
            [InputAction.Quit]      = new[] {new InputBinding(Keys.Escape)}
        };
        public IReadOnlyList<InputBinding> GetBindings(InputAction action) => bindings[action];
        private bool IsKeyPressed(InputBinding input)
        {
            return (input.IsDown(currentKeyboard) && !input.IsDown(previousKeyboard)) || (input.IsDown(currentMouse) && !input.IsDown(previousMouse));
        }
        private bool IsKeyReleased(InputBinding input)
        {
            return (!input.IsDown(currentKeyboard) && input.IsDown(previousKeyboard)) || (!input.IsDown(currentMouse) && input.IsDown(previousMouse));
        }
        private bool IsKeyHeld(InputBinding input)
        {
            return input.IsDown(currentKeyboard) || input.IsDown(currentMouse);
        }
        public bool IsAreaClicked(Rectangle area)
        {
            return previousMouse.LeftButton == ButtonState.Pressed && 
                currentMouse.LeftButton == ButtonState.Released
                && area.Contains(currentMouse.Position);
        }
        public bool IsActionPressed(InputAction action)
        {
            InputBinding[] actionKeys = bindings[action];
            for (int i = 0; i < actionKeys.Length; i++)
            {
                if (IsKeyPressed(actionKeys[i]))
                    return true;
            }
            return false;
        }
        public bool IsActionReleased(InputAction action)
        {
            InputBinding[] actionKeys = bindings[action];
            for (int i = 0; i < actionKeys.Length; i++)
            {
                if (IsKeyReleased(actionKeys[i]))
                    return true;
            }
            return false;
        }
        public bool IsActionHeld(InputAction action)
        {
            InputBinding[] actionKeys = bindings[action];
            for (int i = 0; i < actionKeys.Length; i++)
            {
                if (IsKeyHeld(actionKeys[i]))
                    return true;
            }
            return false;
        }
        public float GetMoveAxis() //TODO: Implement mouse version based on relative position to player object.
        {
            float xDirection = 0;
            if(IsActionHeld(InputAction.MoveRight))
                xDirection = 1;
            if (IsActionHeld(InputAction.MoveLeft))
                xDirection -= 1;
            return xDirection;
        }
        public void UpdateState(bool shouldUpdate)
        {
            previousKeyboard = currentKeyboard;
            previousMouse = currentMouse;
            if (shouldUpdate)
            {
                currentKeyboard = Keyboard.GetState();
                currentMouse = Mouse.GetState();
            }
            else
            {
                currentKeyboard = new KeyboardState();
                currentMouse = new MouseState();
            }
        }
        public bool TrySetBindings(InputAction action, params InputBinding[] newBindings)
        {
            ArgumentNullException.ThrowIfNull(newBindings);
            if(newBindings.Length < 1)  //Every action needs at least one key
                return false;
            foreach (KeyValuePair<InputAction,InputBinding[]> assignedKeys in bindings) //Keys can only be used for one thing
            {
                if (assignedKeys.Key != action)
                {
                    for(int i = 0; i < newBindings.Length; i++)
                    {
                        for(int j = 0; j < assignedKeys.Value.Length; j++)
                        {
                            if (newBindings[i] == assignedKeys.Value[j]) return false;
                        }
                    }
                }
            }
            bindings[action] = (InputBinding[])newBindings.Clone(); //You made it this far, we'll take the new bindings.
            return true;
        }
    }
}
