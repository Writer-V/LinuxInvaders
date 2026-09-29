using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinuxInvaders.Core.Graphics
{
    public record class AnimationClip
    {
		// The animation texture.
		public Texture2D Texture { get; init; }

		// Time to display each frame
		public float TimePerFrame => 1f / framesPerSec;

        // The frame width, height, and number of them.
        public int Frames { get; init; }
		public int FrameWidth { get; init; }
		public int FrameHeight { get; init; }
		public int Row { get; init; }
		private int framesPerSec { get; init; }
        public bool Loop { get; init; }
        public SpriteEffects ApplyEffect {get; init;} = SpriteEffects.None;

        public AnimationClip(Texture2D texture, int frames, int frameWidth, int row, int frameHeight, bool loop = true,
            int framesPerSec = 12, bool flipX = false, bool flipY = false)
        {
            this.Texture = texture; //For others to reference, if nothing else.
            Frames = frames;
            Row = row;
            Loop = loop;
            this.framesPerSec = framesPerSec;
            if(flipX) ApplyEffect |= SpriteEffects.FlipHorizontally;
            if(flipY) ApplyEffect |= SpriteEffects.FlipVertically;
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;

        }
        public Rectangle FrameRectangle(int frameNumber)
        {
            return new Rectangle(frameNumber * FrameWidth, Row * FrameHeight, FrameWidth, FrameHeight);
        }
    }
}