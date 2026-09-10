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
        private PlayerChar player;
        private bool isGameOver = false;
        private int windowSizeX, windowSizeY;

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
            // The sprite sheet is a GPU resource: load it once and share the reference.
            const int enemyFrameCount = 14;
            const int enemyFramesPerSec = 12;
            Texture2D enemySheet = Content.Load<Texture2D>("beh_64_idle");
            int enemyFrameWidth = enemySheet.Width / enemyFrameCount;
            int enemyFrameHeight = enemySheet.Height;

            for (int i = 0; (i * (enemyFrameHeight + 10)) < windowSizeY / 2; i++)
            {
                for (int j = 0; (j * (enemyFrameWidth + 10)) < (windowSizeX - enemyFrameWidth); j++)
                {
                    // Animation state is per-enemy, so each one gets its own AnimatedTexture.
                    AnimatedTexture enemyTexture = new AnimatedTexture();
                    enemyTexture.Load(enemySheet, frameCount: enemyFrameCount, framesPerSec: enemyFramesPerSec);
                    Vector2 pos = new Vector2(j * (enemyFrameWidth + 10) + 10, i * enemyFrameHeight + 10);
                    Enemy enemy = new Enemy(enemyTexture, pos, windowSizeX, windowSizeY);
                    enemy.ReachedBottom += Enemy_ReachedBottom;
                    enemies.Add(enemy);
                }
            }
            Texture2D playerTexture = Content.Load<Texture2D>("attack_idle_dir3");
            AnimatedTexture playerAnimatedTexture = new AnimatedTexture();
            playerAnimatedTexture.Load(playerTexture, frameCount: 23, framesPerSec: 8);
            player = new PlayerChar(playerAnimatedTexture, new Vector2((windowSizeX / 2) - (playerAnimatedTexture.FrameWidth / 2), windowSizeY - playerAnimatedTexture.FrameHeight), windowSizeX);
            player.OutOfLives += Player_OutOfLives;
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

                // Removals after the loop.
                enemies.RemoveAll(enemy => !enemy.IsActive);
            }

            base.Update(gameTime);
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
            player.Draw(_spriteBatch);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
