using LinuxInvaders.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LinuxInvaders.Core
{
	public class Enemy
	{
		private SpriteAnimator animationPlayer;
		private Vector2 pos;
		int windowSizeX;
		int windowSizeY;
		public event EventHandler ReachedBottom;

		// Should it exist?
		public bool IsActive { get; private set; } = true;

		// Collision box TODO: Make into something more accurate.
		public Rectangle Bounds => new Rectangle((int)pos.X, (int)pos.Y,
			animationPlayer.FrameWidth, animationPlayer.FrameHeight);

		public Enemy(SpriteAnimator enemyTexture, Vector2 pos, int windowSizeX, int windowSizeY)
		{
			this.animationPlayer = enemyTexture;
			this.pos = pos;
			this.windowSizeX = windowSizeX;
			this.windowSizeY = windowSizeY;
		}
		
		public void Update(GameTime gameTime)
		{
			if (!IsActive)
				return;

			//Get the animation to move along
			animationPlayer.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
			// Update enemy logic here (e.g., movement, animation)
			pos.Y += 1;
			if (pos.Y > windowSizeY)
			{
				// Mark for removal and throw an event, just in case.
				IsActive = false;
				ReachedBottom?.Invoke(this, EventArgs.Empty);
			}
		}

		// Mark for removal.
		public void Deactivate()
		{
			IsActive = false;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			animationPlayer.Draw(spriteBatch, pos);
		}
	}
}