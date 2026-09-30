using System;
using LinuxInvaders.Core.Graphics;
using LinuxInvaders.Core.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinuxInvaders.Core
{
    public class StartScreen
    {
        private Texture2D buttonTexture;
        private Rectangle buttonPosition;
        private SpriteAnimator characterSprite;
        private Point characterPosition;
        public event EventHandler StartButtonClicked;
        public StartScreen(Texture2D buttonTexture, AnimationSet character, Point windowSize)
        {
            this.buttonTexture = buttonTexture;
            buttonPosition = new(windowSize.X/2 - buttonTexture.Width/2, 
                windowSize.Y - buttonTexture.Height - 100, buttonTexture.Width, buttonTexture.Height);
            characterSprite = new(character, initialAnimType: AnimSequenceType.Pose);
            characterPosition = new(windowSize.X/2 - character.FrameWidth/2, buttonPosition.Y - character.FrameHeight - 50);
        }

        public void Update(GameTime gameTime, PlayerInputs input)
        {
            characterSprite.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            if (input.IsAreaClicked(buttonPosition) || input.IsActionPressed(InputAction.Confirm))
                StartButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            characterSprite.Draw(spriteBatch, characterPosition.ToVector2());
            spriteBatch.Draw(buttonTexture, buttonPosition, Color.White);
        }
    }
}