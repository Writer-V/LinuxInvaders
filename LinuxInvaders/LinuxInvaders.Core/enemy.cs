using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using LinuxInvaders.Core.AnimatedSprite;

namespace LinuxInvaders.Core
{
	public class Enemy
	{
		private AnimatedTexture enemyTexture;
		private Vector2 pos;
		int windowSizeX;
		int windowSizeY;
		public event EventHandler ReachedBottom;

		// Should it exist?
		public bool IsActive { get; private set; } = true;

		// Collision box TODO: Make into something more accurate.
		public Rectangle Bounds => new Rectangle((int)pos.X, (int)pos.Y,
			enemyTexture.FrameWidth, enemyTexture.FrameHeight);

		public Enemy(AnimatedTexture enemyTexture, Vector2 pos, int windowSizeX, int windowSizeY)
		{
			this.enemyTexture = enemyTexture;
			this.pos = pos;
			this.windowSizeX = windowSizeX;
			this.windowSizeY = windowSizeY;
		}
		
		public void Update(GameTime gameTime)
		{
			if (!IsActive)
				return;

			//Get the animation to move along
			enemyTexture.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
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
			enemyTexture.Draw(spriteBatch, pos);
		}
	}
}