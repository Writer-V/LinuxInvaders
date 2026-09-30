using LinuxInvaders.Core.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;

namespace LinuxInvaders.Core.Enemies
{
	public class Enemy
	{
		private SpriteAnimator animationPlayer;
		private EnemyDefinition definition;
		public EnemyType Type => definition.Type;
		public int Points => definition.PointValue;
		private Vector2 pos;
		private Rectangle boundingBox;
		private float hitBoxScale; //Make Bounds bigger/smaller for game balance
		public Rectangle Bounds => new(
			new Point((int)(boundingBox.X * (hitBoxScale/2)), (int)(boundingBox.Y * (hitBoxScale/2))) + pos.ToPoint(),
			new Point((int)(boundingBox.Width * hitBoxScale), (int)(boundingBox.Height * hitBoxScale)));

		//Testing the padding needed for it to make sense for movement.
		private Point movementPadding = new(10,10);
		public Point TopLeft => boundingBox.Location - movementPadding + pos.ToPoint();
		public Point BottomRight => boundingBox.Location + boundingBox.Size + movementPadding + pos.ToPoint();

		int health;

		// Exists = still has any purpose; TakenOut = last animation, no game logic; Escaped = no game logic, just move off the screen
		public bool Exists { get; private set; } = true;
		public bool TakenOut { get; private set; } = false;
		public bool Escaped { get; private set;} = false;
		public void AnimateMoveOnly() => Escaped = true; // No longer used, but still displayed
		public void Remove() => Exists = false; // Off screen or otherwise removed


		public Enemy(EnemyDefinition definition, Vector2 pos, float hitBoxScale)
		{
			this.definition = definition;
			animationPlayer = new(definition.Animations, scale: definition.Scale);
			this.pos = pos;
			boundingBox = definition.ScaledBounds();
			this.hitBoxScale = hitBoxScale;
			health = definition.InitialHealth;
		}
		
		public void Update(GameTime gameTime)
		{
			if (!Exists)
				return;

			//Get the animation to move along
			animationPlayer.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
		}
		public void MoveBy(Vector2 delta)
		{
			pos += delta;
		}
		public int TakeHit(int damage = 1)
		{
			if(TakenOut) return -1; //
			health--;
			if(health < 1)
			{
				TakenOut = true;
				animationPlayer.PlayOnce(AnimSequenceType.Death);
				animationPlayer.FinishedAnim += DeathAnimationFinished;
				return 0; //It got killed
			}
			else animationPlayer.PlayOnce(AnimSequenceType.Hit, true);
			return health; // Still alive
		}

		// No need to keep track of this enemy anymore.
		private void DeathAnimationFinished(object sender, EventArgs e)
		{
			Exists = false;
		}

		public void Draw(SpriteBatch spriteBatch)
		{
			if(!Exists)
				return;
			animationPlayer.Draw(spriteBatch, pos);
		}
	}
}