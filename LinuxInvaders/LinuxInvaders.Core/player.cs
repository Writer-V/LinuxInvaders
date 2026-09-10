using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using LinuxInvaders.Core.AnimatedSprite;

namespace LinuxInvaders.Core
{
	public class PlayerChar
	{
		private AnimatedTexture texture;
		private Vector2 pos;
		private int windowSizeX;
		public int RemainingLives { get; private set; } = 3;
		public event EventHandler OutOfLives;

		public PlayerChar(AnimatedTexture texture, Vector2 pos, int windowSizeX)
		{
			this.texture = texture;
			this.pos = pos;
			this.windowSizeX = windowSizeX;
		}

		public void Update(GameTime gameTime)
		{
			//Get the animation to move along
			texture.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

			// Update player logic here (e.g., movement, animation)
			if ((Keyboard.GetState().IsKeyDown(Keys.Left)) || (Keyboard.GetState().IsKeyDown(Keys.A)))
				pos.X -= 5;
			if ((Keyboard.GetState().IsKeyDown(Keys.Right)) || (Keyboard.GetState().IsKeyDown(Keys.D)))
				pos.X += 5;

			// Keep the player within the window bounds.
			pos.X = MathHelper.Clamp(pos.X, 0, windowSizeX - texture.FrameWidth);
		}

		public void Damage(int amount = 1)
		{
			// Already dead: don't fire OutOfLives a second time.
			if (RemainingLives <= 0)
				return;

			RemainingLives -= amount;
			if (RemainingLives <= 0)
			{
				RemainingLives = 0;
				OutOfLives?.Invoke(this, EventArgs.Empty);
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			texture.Draw(spriteBatch, pos);
		}
	}
}