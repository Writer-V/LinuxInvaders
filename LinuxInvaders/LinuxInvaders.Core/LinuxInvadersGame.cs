using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using LinuxInvaders.Core.AnimatedSprite;

namespace LinuxInvaders.Core
{
    public class LinuxInvadersGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private List<Enemy> enemies = new List<Enemy>();
        private List<FireBolt> fireBolts = new List<FireBolt>();
        private PlayerChar player;
        private bool isGameOver = false;
        private int windowSizeX, windowSizeY;

        // Kept around because bolts are built at runtime, not in LoadContent.
        private Texture2D fireSheet;

        // Fireball to shoot: 0 orange, 1 purple, 2 green, 3 red, 4 blue
        private int attackRow = 0;

        private const int FireBoltFrameCount = 4;
        private const int FireBoltFrameSize = 32;

        public LinuxInvadersGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 500;
            _graphics.PreferredBackBufferHeight = 700;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            windowSizeX = Window.ClientBounds.Width;
            windowSizeY = Window.ClientBounds.Height;

            // Enemy sheet to memory.
            const int enemyFrameCount = 14;
            const int enemyFramesPerSec = 12;
            Texture2D enemySheet = Content.Load<Texture2D>("beh_64_idle");
            int enemyFrameWidth = enemySheet.Width / enemyFrameCount;
            int enemyFrameHeight = enemySheet.Height;
            // Then make the enemies in a grid based on space.
            // Easy to set up, stupid design.
            for (int i = 0; (i * (enemyFrameHeight + 10)) < windowSizeY / 2; i++)
            {
                for (int j = 0; (j * (enemyFrameWidth + 10)) < (windowSizeX - enemyFrameWidth); j++)
                {
                    // Animation state is per-enemy, so each one gets its own AnimatedTexture.
                    AnimatedTexture enemyTexture = new AnimatedTexture();
                    enemyTexture.Load(enemySheet, 
                     frameCount: enemyFrameCount, 
                     framesPerSec: enemyFramesPerSec);
                    Vector2 pos = new Vector2(j * (enemyFrameWidth + 10) + 10,
                     i * enemyFrameHeight + 10);
                    Enemy enemy = new Enemy(enemyTexture, pos, windowSizeX, windowSizeY);
                    enemy.ReachedBottom += Enemy_ReachedBottom;
                    enemies.Add(enemy);
                }
            }
            Texture2D playerTexture = Content.Load<Texture2D>("mageAnim");
            AnimatedTexture playerAnimatedTexture = new AnimatedTexture();
            playerAnimatedTexture.Load(playerTexture, frameCount: 12, framesPerSec: 8);
            player = new PlayerChar(playerAnimatedTexture, new Vector2((windowSizeX / 2) - (playerAnimatedTexture.FrameWidth / 2), windowSizeY - playerAnimatedTexture.FrameHeight), windowSizeX);
            player.OutOfLives += Player_OutOfLives;
            player.Fired += Player_Fired;

            fireSheet = Content.Load<Texture2D>("Fireball_Sprite_Sheet");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            if (!isGameOver)
            {
                foreach (var enemy in enemies)
                {
                    enemy.Update(gameTime);
                }
                player.Update(gameTime);

                foreach (var bolt in fireBolts)
                {
                    bolt.Update(gameTime);
                }

                ResolveBoltHits();

                // Removals after the loop.
                enemies.RemoveAll(enemy => !enemy.IsActive);
                fireBolts.RemoveAll(bolt => !bolt.IsActive);
                Window.Title = $"Invaders - Lives: {player.RemainingLives} - Enemies: {enemies.Count}";
            }

            base.Update(gameTime);
        }

        private void ResolveBoltHits() // Pulled out of Update
        {
            foreach (var bolt in fireBolts)
            {
                if (!bolt.IsActive)
                    continue;

                foreach (var enemy in enemies)
                {
                    if (!enemy.IsActive)
                        continue;

                    if (bolt.Bounds.Intersects(enemy.Bounds))
                    {
                        bolt.Deactivate();
                        enemy.Deactivate();
                        // One bolt, one enemy: stop looking once it has hit.
                        break;
                    }
                }
            }
        }

        private void Player_Fired(object sender, EventArgs e)
        {
            // Build the bolt, reuse the cashed sheet.
            AnimatedTexture boltTexture = new AnimatedTexture(rotation: FireBolt.UpwardRotation);
            boltTexture.Load(fireSheet,
                frameCount: FireBoltFrameCount,
                framesPerSec: 12,
                frameHeight: FireBoltFrameSize,
                row: attackRow,
                centerOrigin: true);

            fireBolts.Add(new FireBolt(boltTexture, player.MuzzlePosition));
        }

        private void Enemy_ReachedBottom(object sender, EventArgs e)
        {
            player.Damage();
        }

        private void Player_OutOfLives(object sender, System.EventArgs e)
        {
            isGameOver = true;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            foreach (var enemy in enemies)
            {
                enemy.Draw(_spriteBatch);
            }
            foreach (var bolt in fireBolts)
            {
                bolt.Draw(_spriteBatch);
            }
            player.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
