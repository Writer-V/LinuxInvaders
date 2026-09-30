using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LinuxInvaders.Core.Enemies
{
    public class EnemyFormation
    {
        private Enemy[,] enemyGrid;
        private bool directionRight = true;
        private Point windowSize;
        private Vector2 startSpeed; // Across + down
        private Vector2 highSpeed; // Across + down with leas enemies left
        private int reachedEndHeight; // Y-value when an enemy has reached the player
        public event EventHandler<EnemyHitEventArgs> EnemyHit;
        public event EventHandler EnemyReachedPlayer;
        
        //For the currently assumed 60px² enemies. Would ofc be set elsewhere in real production
        private int gridCellSize = 70;

        public EnemyFormation(EnemyDefinition[] rowSpread, int columns, Vector2 startPos, 
            float hitBoxScale, Vector2 startSpeed, Vector2 highSpeed, Point windowSize, int reachedEndHeight)
        {            
            enemyGrid = new Enemy[rowSpread.Length,columns];
            for(int row = 0; row < enemyGrid.GetLength(0); row++)
            {
                for(int col = 0; col < enemyGrid.GetLength(1); col++)
                {
                    enemyGrid[row, col] = new Enemy(rowSpread[row], startPos + new Vector2(col * gridCellSize, row * gridCellSize), hitBoxScale);
                }
            }
            this.startSpeed = startSpeed;
            this.highSpeed = highSpeed;
            this.windowSize = windowSize;
            this.reachedEndHeight = reachedEndHeight;
        }

        public void Update(GameTime gameTime)
        {
            bool reachedEdge = false;
            foreach(Enemy enemy in enemyGrid)
            {
                if(enemy.TakenOut) continue;

                if(directionRight && enemy.BottomRight.X > windowSize.X)
                {
                    reachedEdge = true;
                    break;
                }
                if(!directionRight && enemy.TopLeft.X < 0)
                {
                    reachedEdge = true;
                    break;
                }
            }
            
            Vector2 delta = Vector2.Zero;
            float lerpEasing = 1 - EnemiesAlive()/(float)enemyGrid.Length;
            if(directionRight && reachedEdge)
            {
                directionRight = false;
                delta.Y = MathHelper.Lerp(startSpeed.Y, highSpeed.Y, lerpEasing);
            }
            else if(!directionRight && reachedEdge)
            {
                directionRight = true;
                delta.Y = MathHelper.Lerp(startSpeed.Y, highSpeed.Y, lerpEasing);
            }
            else if(directionRight) delta.X = MathHelper.Lerp(startSpeed.X, highSpeed.X, lerpEasing) * (float)gameTime.ElapsedGameTime.TotalSeconds;
            else delta.X = 0 - (MathHelper.Lerp(startSpeed.X, highSpeed.X, lerpEasing) * (float)gameTime.ElapsedGameTime.TotalSeconds);

            foreach (Enemy enemy in enemyGrid)
            {
                enemy.Update(gameTime);

                if(!enemy.TakenOut)
                    enemy.MoveBy(delta);
            }
            
            foreach (Enemy enemy in enemyGrid)
            {
                if(enemy.TakenOut || enemy.Escaped) continue;
                if(enemy.BottomRight.Y > reachedEndHeight)
                {
                    enemy.AnimateMoveOnly();
                    EnemyReachedPlayer?.Invoke(this, EventArgs.Empty);
                    break;
                }
            }

            foreach (Enemy enemy in enemyGrid)
                if(enemy.TakenOut && enemy.Exists)
                    if(enemy.BottomRight.Y > windowSize.Y) enemy.Remove();
        }

        public int EnemiesAlive()
        {
            int counter = 0;
            foreach(Enemy enemy in enemyGrid)
            {
                if(!enemy.TakenOut) counter++;
            }
            return counter;
        }

        public bool TryHit(Rectangle area)
        {
            foreach(Enemy enemy in enemyGrid)
            {
                if(area.Intersects(enemy.Bounds) && !enemy.TakenOut)
                {
                    int hitResult = enemy.TakeHit();
                    if(hitResult < 0) continue;
                    if(hitResult == 0)
                        EnemyHit?.Invoke(this, new EnemyHitEventArgs(
                            enemy.Type, 
                            enemy.Bounds.Center, 
                            true,
                            enemy.Points));
                    else
                        EnemyHit?.Invoke(this, new EnemyHitEventArgs( // For sound/fx/others who need every hit
                            enemy.Type, 
                            new Point(enemy.Bounds.X + (enemy.Bounds.Width/2), enemy.Bounds.Y + (enemy.Bounds.Height / 2)), 
                            false));
                    return true;
                }
            }
            return false;
        }

        public void Draw(SpriteBatch spriteBatch, bool debugView, Texture2D whitePixel)
        {
            foreach(Enemy enemy in enemyGrid)
            {
                enemy.Draw(spriteBatch);
            }
            #if DEBUG
                if(debugView) 
                {
                    foreach(Enemy enemy in enemyGrid)
                    {
                        if(enemy.Exists) spriteBatch.Draw(whitePixel, enemy.Bounds, Color.Violet * 0.3f);
                    }
                }
            #endif
        }
    }
}