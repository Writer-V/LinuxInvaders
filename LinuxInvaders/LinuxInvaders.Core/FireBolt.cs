using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LinuxInvaders.Core.Graphics;

namespace LinuxInvaders.Core
{
	public class FireBolt
	{
		// The sheet's fireballs point right, so turn them a quarter turn
		// anticlockwise to fly up the screen.
		public const float UpwardRotation = -MathHelper.PiOver2;

		private const float Speed = 2f;

		private SpriteAnimator animationPlayer;
		private Vector2 pos;

		// False once the bolt has left play; the game sweeps these out of its list.
		public bool Exists { get; private set; } = true;

		// The bolt's texture is loaded with a centred origin, so pos is the middle
		// of the sprite - unlike Enemy, where pos is the top-left corner.
		public Rectangle Bounds => new Rectangle(
			(int)(pos.X - animationPlayer.FrameWidth / 2f),
			(int)(pos.Y - animationPlayer.FrameHeight / 2f),
			animationPlayer.FrameWidth, animationPlayer.FrameHeight);

		public FireBolt(SpriteAnimator texture, Vector2 pos)
		{
			animationPlayer = texture;
			this.pos = pos;
		}

		public void Update(GameTime gameTime)
		{
			if (!Exists)
				return;

			animationPlayer.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
			pos.Y -= Speed;

			// Gone once the whole sprite has cleared the top edge.
			if (pos.Y + animationPlayer.FrameHeight / 2f < 0)
				Exists = false;
		}

		// Take this bolt out of play - it hit something.
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
