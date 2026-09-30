using LinuxInvaders.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinuxInvaders.Core
{
    public class HitPointDisplay
    {
        private SpriteAnimator[] heartAnimator;
        private readonly Point topRight;
        public HitPointDisplay(AnimationSet animations, Point topRight, int totalLives)
        {
            heartAnimator = new SpriteAnimator[totalLives];
            for(int i = 0; i < heartAnimator.Length; i++)
                heartAnimator[i] = new SpriteAnimator(animations, initialAnimType: AnimSequenceType.Creation);
            this.topRight = topRight;
        }

        public void Update(GameTime gameTime, int currentLives)
        {
            for(int i = 0; i < heartAnimator.Length; i++)
            {
                if(currentLives >= i + 1) heartAnimator[i].Play(AnimSequenceType.Creation);
                else heartAnimator[i].Play(AnimSequenceType.Death);
                heartAnimator[i].Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int left = topRight.X - (heartAnimator.Length * heartAnimator[0].FrameWidth);
            for(int i = 0; i < heartAnimator.Length; i++)
            {
                heartAnimator[i].Draw(spriteBatch, new Vector2(left + (heartAnimator[i].FrameWidth * i), topRight.Y));
            }
        }
    }
}