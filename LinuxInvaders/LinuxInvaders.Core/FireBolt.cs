using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using LinuxInvaders.Core.AnimatedSprite;

namespace LinuxInvaders.Core
{
	public class FireBolt
	{
		// The sheet's fireballs point right, so turn them a quarter turn
		// anticlockwise to fly up the screen.
		public const float UpwardRotation = -MathHelper.PiOver2;

		private const float Speed = 2f;

		private AnimatedTexture texture;
		private Vector2 pos;

		// False once the bolt has left play; the game sweeps these out of its list.
		public bool IsActive { get; private set; } = true;

		// The bolt's texture is loaded with a centred origin, so pos is the middle
		// of the sprite - unlike Enemy, where pos is the top-left corner.
		public Rectangle Bounds => new Rectangle(
			(int)(pos.X - texture.FrameWidth / 2f),
			(int)(pos.Y - texture.FrameHeight / 2f),
			texture.FrameWidth, texture.FrameHeight);

		public FireBolt(AnimatedTexture texture, Vector2 pos)
		{
			this.texture = texture;
			this.pos = pos;
		}

		public void Update(GameTime gameTime)
		{
			if (!IsActive)
				return;

			texture.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
			pos.Y -= Speed;

			// Gone once the whole sprite has cleared the top edge.
			if (pos.Y + texture.FrameHeight / 2f < 0)
				IsActive = false;
		}

		// Take this bolt out of play - it hit something.
		public void Deactivate()
		{
			IsActive = false;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			texture.Draw(spriteBatch, pos);
		}
	}
}
