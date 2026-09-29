using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LinuxInvaders.Core.Input;
using LinuxInvaders.Core.Graphics;
using System;

namespace LinuxInvaders.Core
{
	public class PlayerChar
	{
		private SpriteAnimator animationPlayer;
		private Vector2 pos;
		private Rectangle boundingBox;
		public Rectangle Bounds => new(boundingBox.Location + pos.ToPoint(), boundingBox.Size);
		private int windowSizeX;
		public int RemainingLives { get; private set; } = 3;
		public event EventHandler OutOfLives;
		public event EventHandler Fired;

		// Where a shot leaves the player: top edge, horizontally centred.
		public Vector2 MuzzlePosition => new(pos.X + animationPlayer.FrameWidth / 2f, pos.Y);

		public PlayerChar(AnimationSet animations, Vector2 pos, int windowSizeX)
		{
			animationPlayer = new(animations, initialAnimType: AnimSequenceType.Idle);
			this.pos = pos;
			this.windowSizeX = windowSizeX;
			boundingBox = new(49,22,69,125); //Should obviously be set outside in a real project
		}

		public void Update(GameTime gameTime, PlayerInputs input)
		{
			//Get the animation to move along
			animationPlayer.Update((float)gameTime.ElapsedGameTime.TotalSeconds);

			// Update player logic here (e.g., movement, animation)
			float moveDelta = input.GetMoveAxis();
			pos.X += 5 * moveDelta;
			if (moveDelta > 0) animationPlayer.Play(AnimSequenceType.MoveRight);
			else if (moveDelta < 0) animationPlayer.Play(AnimSequenceType.MoveLeft);
			else animationPlayer.Play(AnimSequenceType.Idle);

			// Keep the player within the window bounds.
			pos.X = MathHelper.Clamp(pos.X, 0 - boundingBox.X, windowSizeX - boundingBox.Width);

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
			animationPlayer.Draw(spriteBatch, pos);
		}
	}
}