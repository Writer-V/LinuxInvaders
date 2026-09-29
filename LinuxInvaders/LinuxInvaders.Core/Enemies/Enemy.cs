using LinuxInvaders.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LinuxInvaders.Core.Enemies
{
	public class Enemy
	{
		private SpriteAnimator animationPlayer;
		private EnemyDefinition definition;
		private Vector2 pos;
		private Rectangle boundingBox;
		public Rectangle Bounds => new(
			boundingBox.Location + pos.ToPoint(),
			boundingBox.Size
		);
		int windowSizeX;
		int windowSizeY;
		public event EventHandler ReachedBottom;

		// Should it exist? If false: don't update; don't draw; don't anything
		public bool Exists { get; private set; } = true;
		// Display as it dies, but no longer treat as present for the game
		public bool TakenOut { get; private set; } = false;

		public Enemy(EnemyDefinition definition, Vector2 pos, int windowSizeX, int windowSizeY)
		{
			this.definition = definition;
			animationPlayer = new(definition.Animations, scale: definition.Scale);
			this.pos = pos;
			boundingBox = definition.ScaledBounds();
			this.windowSizeX = windowSizeX;
			this.windowSizeY = windowSizeY;
		}
		
		public void Update(GameTime gameTime)
		{
			if (!Exists)
				return;

			//Get the animation to move along
			animationPlayer.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
			// Update enemy logic here (e.g., movement, animation)
			pos.Y += 1;
			if (pos.Y > windowSizeY)
			{
				// Mark for removal and throw an event, just in case.
				Exists = false;
				ReachedBottom?.Invoke(this, EventArgs.Empty);
			}
		}

		// Mark for removal.
		public void Deactivate()
		{
			Exists = false;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			animationPlayer.Draw(spriteBatch, pos);
		}
	}
}