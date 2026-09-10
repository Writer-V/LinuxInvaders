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

		// False once this enemy has left play; the game sweeps these out of its list.
		public bool IsActive { get; private set; } = true;

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
				// Set this before invoking: the handler runs synchronously and
				// should already see the enemy as out of play.
				IsActive = false;
				ReachedBottom?.Invoke(this, EventArgs.Empty);
			}
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			enemyTexture.Draw(spriteBatch, pos);
		}
	}
}