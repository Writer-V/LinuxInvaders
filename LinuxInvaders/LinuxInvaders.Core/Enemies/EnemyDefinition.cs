using LinuxInvaders.Core.Graphics;
using Microsoft.Xna.Framework;

namespace LinuxInvaders.Core.Enemies
{
    public record EnemyDefinition
    {
        public AnimationSet Animations { get; init; }
        public int PointValue { get; init; }
        public int InitialHealth { get; init; } //Hits to kill
        public float Scale {get; init; }
        public Rectangle Bounds {get; init;}
        // When there's a projectile class add here, including the possibility to not have one (null? bool?)
        public EnemyDefinition(AnimationSet animationSet, int points, int initialHealth, 
            float scale = 1, Rectangle? bounds = null)
        {
            Animations = animationSet;
            PointValue = points;
            InitialHealth = initialHealth;
            Scale = scale;

            // Allow for lazy input and fall back. Probably loads of padding, 
            // but better than nothing for quicker prototyping.
            if(bounds == null) Bounds = new(0, 0, Animations.FrameWidth, Animations.FrameHeight);
            else Bounds = (Rectangle)bounds;
        }

        public Rectangle ScaledBounds()
        {
            return new Rectangle(
                (int)(Bounds.X * Scale),
                (int)(Bounds.Y * Scale),
                (int)(Bounds.Width * Scale),
                (int)(Bounds.Height * Scale)
            );
        }
    }
}