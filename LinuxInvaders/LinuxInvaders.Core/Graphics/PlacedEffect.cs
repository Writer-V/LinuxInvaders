using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinuxInvaders.Core.Graphics
{
    public class PlacedEffect
    {
        private SpriteAnimator spriteAnimator;
        private Point pos;
        public bool Exists {get; private set;} = true;
        public PlacedEffect(AnimationSet animation, Point pos)
        {
            spriteAnimator = new(animation, centerOrigin: true);
            spriteAnimator.FinishedAnim += RemoveMe;
            this.pos = pos;
            spriteAnimator.PlayOnce(AnimSequenceType.Explosion);
        }
        public void Update(GameTime gameTime)
        {
            spriteAnimator.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteAnimator.Draw(spriteBatch, pos.ToVector2());
        }
        private void RemoveMe(object sender, EventArgs e)
        {
            Exists = false;
        }
    }
}