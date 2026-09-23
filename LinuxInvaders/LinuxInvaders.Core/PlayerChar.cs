using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace LinuxInvaders.Core
{
	public class PlayerChar
	{
		private AnimatedTexture texture;
		private Vector2 pos;
		private int windowSizeX;
		public int RemainingLives { get; private set; } = 3;
		public event EventHandler OutOfLives;
		public event EventHandler Fired;

		// Last frame's keyboard
		private KeyboardState previousKeyboard;

		// Where a shot leaves the player: top edge, horizontally centred.
		public Vector2 MuzzlePosition => new Vector2(pos.X + texture.FrameWidth / 2f, pos.Y);

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

			KeyboardState keyboard = Keyboard.GetState();

			// Update player logic here (e.g., movement, animation)
			if (keyboard.IsKeyDown(Keys.Left) || keyboard.IsKeyDown(Keys.A))
				pos.X -= 5;
			if (keyboard.IsKeyDown(Keys.Right) || keyboard.IsKeyDown(Keys.D))
				pos.X += 5;

			// Keep the player within the window bounds.
			pos.X = MathHelper.Clamp(pos.X, 0, windowSizeX - texture.FrameWidth);

			// Fire on the frame Space goes down, not every frame it's held.
			if (keyboard.IsKeyDown(Keys.Space) && !previousKeyboard.IsKeyDown(Keys.Space))
				Fired?.Invoke(this, EventArgs.Empty);

			previousKeyboard = keyboard;
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