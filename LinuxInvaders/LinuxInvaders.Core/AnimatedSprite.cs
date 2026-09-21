using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LinuxInvaders.Core
{
	public class AnimatedTexture
	{
		// Number of frames in the animation.
		private int frameCount;

		// The animation texture.
		private Texture2D texture;

		// The number of frames to draw per second.
		private float timePerFrame;

		// The frame width, height, and current frame being drawn.
		public int Frame { get; private set; }
		public int FrameWidth { get; private set; }
		public int FrameHeight { get; private set; }

		// Which row of a multi-row sheet to animate. 0 for a single strip.
		public int Row { get; private set; }

		// Total amount of time the animation has been running.
		private float totalElapsed;

		// Is the animation currently running?
		public bool IsRunning { get; private set; }

		// For getting the sprite with complicated parameters.
		private float rotation, scale, depth;
		private Vector2 origin;

		public AnimatedTexture(Vector2 origin = default(Vector2), float rotation = 0f, float scale = 1f, float depth = 0f)
		{
			this.origin = origin;
			this.rotation = rotation;
			this.scale = scale;
			this.depth = depth;
		}

		// Set centerOrigin when the sprite rotates: it makes rotation spin the sprite
		// in place, but also makes the position passed to Draw mean the sprite's
		// centre rather than its top-left corner.
		public void Load(Texture2D texture, int frameCount = 1, int framesPerSec = 1, int frameWidth = 0, int frameHeight = 0, int row = 0, bool centerOrigin = false)
		{
			this.frameCount = frameCount;
			this.texture = texture;
			this.Row = row;
			if (frameWidth == 0)
				this.FrameWidth = texture.Width / frameCount;
			else
				this.FrameWidth = frameWidth;
			if (frameHeight == 0)
				this.FrameHeight = texture.Height;
			else
				this.FrameHeight = frameHeight;

			// Must come after FrameWidth/FrameHeight are known.
			if (centerOrigin)
				origin = new Vector2(FrameWidth / 2f, FrameHeight / 2f);

			timePerFrame = (float)1 / framesPerSec;
			Frame = 0;
			totalElapsed = 0;
			IsRunning = true;
		}

		public void Update(float elapsed)
		{
			if (!IsRunning)
				return;
			totalElapsed += elapsed;
			while (totalElapsed > timePerFrame)
			{
				Frame++;
				// Keep the Frame between 0 and the total frames, minus one.
				Frame %= frameCount;
				totalElapsed -= timePerFrame;
			}
		}

		public void Draw(SpriteBatch batch, Vector2 screenPos)
		{
			DrawFrame(batch, Frame, screenPos);
		}

		public void DrawFrame(SpriteBatch batch, int frame, Vector2 screenPos)
		{
			Rectangle sourceRect = new Rectangle(FrameWidth * frame, FrameHeight * Row,
				FrameWidth, FrameHeight);
			batch.Draw(texture, screenPos, sourceRect, Color.White,
				rotation, origin, scale, SpriteEffects.None, depth);
		}

		public void Reset()
		{
			Frame = 0;
			totalElapsed = 0f;
		}

		public void Stop()
		{
			Pause();
			Reset();
		}

		public void Play ()
		{
			IsRunning = true;
		}

		public void Pause()
		{
			IsRunning = false;
		}
	}
}
