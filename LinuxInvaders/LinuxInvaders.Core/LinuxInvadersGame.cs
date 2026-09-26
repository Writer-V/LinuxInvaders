using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace LinuxInvaders.Core
{
    public class LinuxInvadersGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private List<Enemy> enemies = new List<Enemy>();
        private List<FireBolt> fireBolts = new List<FireBolt>();
        private PlayerChar player;
        private GameState state = GameState.Start;
        private int windowSizeX, windowSizeY;

        // Textures that need to be kept around
        private Texture2D fireSheet; // Player projectile — 4 frames, 5 rows (orange, purple, green, red, blue)
        private Texture2D enemyBolt; //Enemy projectile — Row 1: creation (6 frames), Row2: travel (3 frames)
        private Texture2D behEnemySheet; //Beholder enemy — Row 1: idle (14 frames), Row 2: attack (6 frames), Row 3: damaged (5 frames), Row 4: dead (10 frames)
        private Texture2D batEnemySheet; //Bat enemy — 4 frames
        private Texture2D owlEnemySheet; //Owl enemy — 5 frames
        private Texture2D bossEnemySheet; //Boss enemy — 15 frames
        private Texture2D barrierSheet; //Barrier
        private Texture2D playerSheet; //Player — Row 1: Right (12 frames), Row 2: Stand (12 frames), Row 3: Left (11 frames), Row 4: Start screen (12 frames)
        private Texture2D explosionSheet; //Explosion animation — 13 frames
        private Texture2D hitPointSheet; // Heart — Row 1: full → empty, Row 2: empty → full
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

            // Load all the textures — Some of thise should be part of the state change instead.
            // TODO: Change to behEnemy asset and use behEnemySheet variable.
            batEnemySheet = Content.Load<Texture2D>("batEnemy");
            owlEnemySheet = Content.Load<Texture2D>("owlEnemy");
            bossEnemySheet = Content.Load<Texture2D>("bossEnemy");
            // TODO: Change to wizardSheet asset and use playerSheet variable.
            fireSheet = Content.Load<Texture2D>("Fireball_Sprite_Sheet");
            enemyBolt = Content.Load<Texture2D>("enemyBolt");
        }

        private void ChangeState(GameState newState)
        {
            state = newState;
            switch (state)
            {
                case GameState.Start:
                    break;
                case GameState.Playing:
                    StartNewGame();
                    break;
                case GameState.GameOver:
                    break;
            }
        }

        private void StartNewGame()
        {
            // Reset the game state.
            enemies.Clear();
            fireBolts.Clear();

            // Create the enemies... this should mostly hand over to a controller class.
            Texture2D enemySheet = Content.Load<Texture2D>("beh_64_idle");
            Texture2D playerTexture = Content.Load<Texture2D>("mageAnim"); 
            const int enemyFrameCount = 14;
            const int enemyFramesPerSec = 12;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    AnimatedTexture enemyTexture = new AnimatedTexture();
                    enemyTexture.Load(enemySheet,
                     frameCount: enemyFrameCount,
                     framesPerSec: enemyFramesPerSec);
                    Vector2 pos = new Vector2(j * ((enemySheet.Width / enemyFrameCount) + 10), i * (enemySheet.Height + 10));
                    Enemy enemy = new Enemy(enemyTexture, pos, windowSizeX, windowSizeY);
                    enemy.ReachedBottom += Enemy_ReachedBottom;
                    enemies.Add(enemy);
                }
            }

            AnimatedTexture playerAnimatedTexture = new AnimatedTexture();
            playerAnimatedTexture.Load(playerTexture, frameCount: 12, framesPerSec: 8);
            player = new PlayerChar(playerAnimatedTexture, new Vector2((windowSizeX / 2) - (playerAnimatedTexture.FrameWidth / 2), windowSizeY - playerAnimatedTexture.FrameHeight), windowSizeX);
            player.OutOfLives += Player_OutOfLives;
            player.Fired += Player_Fired;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            
            switch (state)
            {
                case GameState.Start:
                    UpdateStart(gameTime);
                    break;
                case GameState.Playing:
                    UpdatePlaying(gameTime);
                    break;
                case GameState.GameOver:
                    UpdateGameOver(gameTime);
                    break;
            }

            base.Update(gameTime);
        }

        private void UpdateStart(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                ChangeState(GameState.Playing);
            }
        }

        private void UpdatePlaying(GameTime gameTime)
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

        private void UpdateGameOver(GameTime gameTime)
        {}

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
            ChangeState(GameState.GameOver);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            switch (state)
            {
                case GameState.Start:
                    DrawStart(gameTime);
                    break;
                case GameState.Playing:
                    DrawPlaying(gameTime);
                    break;
                case GameState.GameOver:
                    DrawGameOver(gameTime);
                    break;
            }

            base.Draw(gameTime);
        }

        private void DrawStart(GameTime gameTime)
        {}

        private void DrawPlaying(GameTime gameTime)
        {
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
        }

        private void DrawGameOver(GameTime gameTime)
        {}
    }
}
