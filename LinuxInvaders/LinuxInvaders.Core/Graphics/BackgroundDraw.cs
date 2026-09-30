using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinuxInvaders.Core.Graphics
{
    public class BackgroundDraw
    {
        private Texture2D background;
        private Point windowSize;
        private Rectangle sourceRectangle;
        public BackgroundDraw(Texture2D background, Point windowSize)
        {
            this.windowSize = windowSize;
            SwitchBackground(background);
        }
        public void SwitchBackground(Texture2D newBackground)
        {
            background = newBackground;
            float scaleFactor = MathHelper.Max(windowSize.X/(float)background.Width, windowSize.Y/(float)background.Height);
            sourceRectangle = new((int)((background.Width - (windowSize.X/scaleFactor))/2f), 0, 
                (int)(windowSize.X/scaleFactor), (int)(windowSize.Y/scaleFactor));
        }
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(background, new Rectangle(Point.Zero, windowSize), sourceRectangle, Color.White);
        }
    }
}