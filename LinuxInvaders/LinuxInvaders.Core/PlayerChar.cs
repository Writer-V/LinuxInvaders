using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LinuxInvaders.Core.Input;
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

		// Where a shot leaves the player: top edge, horizontally centred.
		public Vector2 MuzzlePosition => new Vector2(pos.X + texture.FrameWidth / 2f, pos.Y);

		public PlayerChar(AnimatedTexture texture, Vector2 pos, int windowSizeX)
		{
			this.texture = texture;
			this.pos = pos;
			this.windowSizeX = windowSizeX;
		}

		public void Update(GameTime gameTime, PlayerInputs input)
		{
			//Get the animation to move along
			texture.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

			// Update player logic here (e.g., movement, animation)
			pos.X += 5 * input.GetMoveAxis();

			// Keep the player within the window bounds.
			pos.X = MathHelper.Clamp(pos.X, 0, windowSizeX - texture.FrameWidth);

			// Fire on press, not every frame it's held.
			if (input.IsActionPressed(InputAction.Fire))
				Fired?.Invoke(this, EventArgs.Empty);
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