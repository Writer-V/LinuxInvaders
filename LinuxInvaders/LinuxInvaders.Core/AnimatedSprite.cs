using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace LinuxInvaders.Core.AnimatedSprite
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

		// Total amount of time the animation has been running.
		private float totalElapsed;

		// Is the animation currently running?
		public bool IsRunning { get; private set; }

		// The current rotation, scale and draw depth for the animation. Possibly for future use, but not currently used in the game.
		private float rotation, scale, depth;

		// The origin point of the animated texture. Same about the future.
		private Vector2 origin;

		public AnimatedTexture(Vector2 origin = default(Vector2), float rotation = 0f, float scale = 1f, float depth = 0f)
		{
			this.origin = origin;
			this.rotation = rotation;
			this.scale = scale;
			this.depth = depth;
		}

		public void Load(Texture2D texture, int frameCount = 1, int framesPerSec = 1, int frameWidth = 0, int frameHeight = 0)
		{
			this.frameCount = frameCount;
			this.texture = texture;
			if (frameWidth == 0)
				this.FrameWidth = texture.Width / frameCount;
			else
				this.FrameWidth = frameWidth;
			if (frameHeight == 0)
				this.FrameHeight = texture.Height;
			else
				this.FrameHeight = frameHeight;

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
			Rectangle sourceRect = new Rectangle(FrameWidth * frame, 0,
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
