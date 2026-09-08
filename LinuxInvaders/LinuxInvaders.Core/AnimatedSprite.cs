using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using LinuxInvaders.Core;

namespace LinuxInvaders.Core.AnimatedSprite
{
	public class AnimatedSprite
	{
		public Texture2D texture { get; private set; }
		public int frameWidth { get; private set; }
		public int frameHeight { get; private set; }
		public int frameCount { get; private set; }
		public float frameTime { get; private set; }
		public int currentFrame { get; private set; }
		private float elapsedTime, rotation, scale, depth;
		private Vector2 position;
		private bool isLooping, isPlaying;

		public AnimatedSprite(Texture2D texture, Vector2 position = Vector2.Zero, int frameWidth = 0, int frameHeight = 0, int frameCount = 1 , float frameTime = 0.1f)
		{
			this.texture = texture;
			this.frameCount = frameCount;
			this.frameTime = frameTime;
			this.currentFrame = 0;
			this.elapsedTime = 0f;
			this.rotation = 0f;
			this.scale = 1f;
			this.depth = 0f;
			this.position = position;
			this.isLooping = true;
			this.isPlaying = true;

			if (frameWidth == 0)
				this.frameWidth = texture.Width / frameCount;
			else
				this.frameWidth = frameWidth;
			if (frameHeight == 0)
				this.frameHeight = texture.Height / frameCount;
			else
				this.frameHeight = frameHeight;
		}

		public void Update(GameTime gameTime)
		{
			if (!isPlaying)
				return;

			elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

			if (elapsedTime >= frameTime)
			{
				currentFrame++;
				if (currentFrame >= frameCount)
				{
					if (isLooping)
						currentFrame = 0;
					else
						currentFrame = frameCount - 1;
				}
				elapsedTime = 0f;
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			Rectangle sourceRectangle = new Rectangle(currentFrame * frameWidth, 0, frameWidth, frameHeight);
			spriteBatch.Draw(texture, position, sourceRectangle, Color.White, rotation, Vector2.Zero, scale, SpriteEffects.None, depth);
		}

		public void Play()
		{
			isPlaying = true;
		}

		public void Pause()
		{
			isPlaying = false;
		}

		public void Stop()
		{
			isPlaying = false;
			currentFrame = 0;
			elapsedTime = 0f;
		}

		public void SetPosition(Vector2 position)
		{
			this.position = position;
		}

		public void SetRotation(float rotation)
		{
			this.rotation = rotation;
		}

		public void SetScale(float scale)
		{
			this.scale = scale;
		}

		public void SetDepth(float depth)
		{
			this.depth = depth;
		}

		public void SetLooping(bool isLooping)
		{
			this.isLooping = isLooping;
		}
	}
}
