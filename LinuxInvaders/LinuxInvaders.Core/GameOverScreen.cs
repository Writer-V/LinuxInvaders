using System;
using System.Reflection.Metadata.Ecma335;
using LinuxInvaders.Core.Graphics;
using LinuxInvaders.Core.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinuxInvaders.Core
{
    public class GameOverScreen
    {
        private SpriteAnimator[] sprites;
        private Point[] spritesLocation;
        private SpriteAnimator[] shiftingSprites;
        private Point[] shiftingSpritesLocation;
        private SpriteAnimator mainWinSprite;
        private SpriteAnimator mainLoseSprite;
        private Point mainSpriteLocation;
        private Texture2D buttonTexture;
        private SpriteFont font;
        private string bigText;
        private Vector2 bigTextSize;
        private string scoreText;
        private Vector2 scoreTextSize;
        private bool win = false;
        private Point windowSize;
        private Rectangle buttonLocation;
        public event EventHandler RestartButtonClicked;
        public GameOverScreen(AnimationSet[] sprites, AnimationSet[] shiftingSprites, 
            AnimationSet mainWinSprite, AnimationSet mainLoseSprite, Texture2D restartButton, SpriteFont font, Point windowSize)
        {
            this.sprites = new SpriteAnimator[sprites.Length];
            spritesLocation = new Point[sprites.Length];
            for(int i = 0; i < sprites.Length; i++)
                this.sprites[i] = new(sprites[i]);

            this.shiftingSprites = new SpriteAnimator[shiftingSprites.Length];
            for(int i = 0; i < shiftingSprites.Length; i++)
            {
                this.shiftingSprites[i] = new(shiftingSprites[i]);
                this.shiftingSprites[i].FinishedAnim += ResetShiftingSprite;
            }
            shiftingSpritesLocation = new Point[shiftingSprites.Length];

            this.mainWinSprite = new SpriteAnimator(mainWinSprite, initialAnimType: AnimSequenceType.Pose);
            this.mainLoseSprite = new SpriteAnimator(mainLoseSprite, initialAnimType: AnimSequenceType.Pose);

            buttonTexture = restartButton;
            buttonLocation = new(windowSize.X/2 - buttonTexture.Width/2, 
                windowSize.Y - buttonTexture.Height - 50, buttonTexture.Width, buttonTexture.Height);
            this.font = font;
            this.windowSize = windowSize;
            
            SetUp(false, 0);
        }
        public void SetUp(bool win, int score) //Call with each new GameOver
        {
            this.win = win;
            // Random location for regular sprites
            for(int i = 0; i < spritesLocation.Length; i++)
            {
                spritesLocation[i] = new(
                    Random.Shared.Next(windowSize.X - sprites[i].FrameWidth), 
                    Random.Shared.Next(windowSize.Y - sprites[i].FrameHeight));
            }
            // Ransom locations for sprites that move
            for(int i = 0; i < shiftingSpritesLocation.Length; i++)
            {
                shiftingSpritesLocation[i] = new(
                    Random.Shared.Next(windowSize.X - shiftingSprites[i].FrameWidth), 
                    Random.Shared.Next(windowSize.Y - shiftingSprites[i].FrameHeight));
                shiftingSprites[i].PlayOnce(AnimSequenceType.Explosion);
            }
            // Semi-random location for the hero/boss
            SpriteAnimator mainSprite;
            if(win) mainSprite = mainWinSprite;
            else mainSprite = mainLoseSprite;

            mainSpriteLocation = new(Random.Shared.Next(windowSize.X - (mainSprite.FrameWidth/2)),  // Still on the ground
                windowSize.Y - 20 - mainSprite.FrameHeight - Random.Shared.Next(150));

            if(win) bigText = "YOU WON!";
            else bigText = "GAME OVER";
            bigTextSize = font.MeasureString(bigText);
            scoreText = score.ToString() + " points";
            scoreTextSize = font.MeasureString(scoreText);
        }
        // Randomize the location of the explosions and such that should move around.
        private void ResetShiftingSprite(object sender, EventArgs e)
        {
            for(int i = 0; i < shiftingSprites.Length; i++)
            {
                if(sender == shiftingSprites[i])
                {
                    shiftingSpritesLocation[i] = new(
                        Random.Shared.Next(windowSize.X - shiftingSprites[i].FrameWidth), 
                        Random.Shared.Next(windowSize.Y - shiftingSprites[i].FrameHeight));
                    shiftingSprites[i].PlayOnce(AnimSequenceType.Explosion);
                    break;
                }
            }
        }
        public void Update(GameTime gameTime, PlayerInputs input)
        {
            foreach (SpriteAnimator animator in sprites)
                animator.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            foreach (SpriteAnimator animator in shiftingSprites)
                animator.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            
            if(win) mainWinSprite.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            else mainLoseSprite.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            
            if (input.IsAreaClicked(buttonLocation) || input.IsActionPressed(InputAction.Confirm))
                RestartButtonClicked?.Invoke(this, EventArgs.Empty);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            for(int i = 0; i < sprites.Length; i++)
                sprites[i].Draw(spriteBatch, spritesLocation[i].ToVector2());
            for(int i = 0; i < shiftingSprites.Length; i++)
                shiftingSprites[i].Draw(spriteBatch, shiftingSpritesLocation[i].ToVector2());
            
            if(win) mainWinSprite.Draw(spriteBatch, mainSpriteLocation.ToVector2());
            else mainLoseSprite.Draw(spriteBatch, mainSpriteLocation.ToVector2());

            spriteBatch.Draw(buttonTexture, buttonLocation, Color.White);

            spriteBatch.DrawString(
                font, bigText, new(windowSize.X/2 - ((bigTextSize.X/2) * 1.6f), 30), 
                Color.White, 0, Vector2.Zero, 1.6f, SpriteEffects.None, 0);
            
            spriteBatch.DrawString(
                font, scoreText, new(windowSize.X/2 - ((scoreTextSize.X/2) * 1.1f),150), 
                Color.White, 0, Vector2.Zero, 1.1f, SpriteEffects.None, 0);
        }
    }
}