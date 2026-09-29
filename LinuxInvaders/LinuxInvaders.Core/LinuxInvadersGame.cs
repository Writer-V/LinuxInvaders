using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using LinuxInvaders.Core.Input;
using LinuxInvaders.Core.Graphics;
using System;
using LinuxInvaders.Core.Enemies;

namespace LinuxInvaders.Core
{
    public class LinuxInvadersGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private PlayerInputs input = new PlayerInputs();
        private EnemyFormation enemies;
        private List<FireBolt> fireBolts = new List<FireBolt>();
        private PlayerChar player;
        private GameState state = GameState.Start;
        private GameState? pendingState = null;
        private bool debugView = false;
        private int windowSizeX, windowSizeY;

        // Textures that need to be kept around
        private AnimationSet behEnemyAnimations; //Beholder enemy — Row 1: idle (14 frames), Row 2: attack (6 frames), Row 3: damaged (5 frames), Row 4: dead (10 frames)
        private AnimationSet batEnemyAnimations; //Bat enemy — 4 frames
        private AnimationSet owlEnemyAnimations; //Owl enemy — 5 frames
        private AnimationSet bossEnemyAnimations; //Boss enemy — 15 frames
        private AnimationSet playerAnimations; //Player — Row 1: Right (12 frames), Row 2: Stand (12 frames), Row 3: Left (11 frames), Row 4: Start screen (12 frames)
        private AnimationSet hitPointAnimations; // Heart — Row 1: full → empty, Row 2: empty → full
        private AnimationSet barrierAnimations; //Barrier — Row 1: Idle (24 frames), Row 2: Creation (8 frames), Row 3: Death (6 frames)
        private AnimationSet fireBoltAnimations; // Player projectile — 4 frames, 5 rows (orange, purple, green, red, blue)
        private AnimationSet enemyBoltAnimations; //Enemy projectile — Row 1: creation (6 frames), Row2: travel (3 frames)
        private AnimationSet explosionAnimations; //Explosion animation — 13 frames
        private Texture2D WhitePixel; //Just used for debugging

        private Dictionary<EnemyType, EnemyDefinition> EnemyDefined;

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

            //Single white pixel for highlighting while debugging
            WhitePixel = new(GraphicsDevice, 1, 1);
            WhitePixel.SetData(new Color[1] { Color.White});


            //Animation sets — Details would in theory come from JSON or similar
            behEnemyAnimations = new(
                Content.Load<Texture2D>("behEnemy"), 
                64, AnimSequenceType.Idle,
                (AnimSequenceType.Idle, 14, 12, true),
                (AnimSequenceType.Attack, 6, 12, true),
                (AnimSequenceType.Hit, 5, 12, false),
                (AnimSequenceType.Death, 10, 12, false)
            );
            batEnemyAnimations = new(
                Content.Load<Texture2D>("batEnemy"),
                64, AnimSequenceType.Idle,
                (AnimSequenceType.Idle, 4, 4, true)
            );
            owlEnemyAnimations = new(
                Content.Load<Texture2D>("owlEnemy"),
                112, AnimSequenceType.Idle,
                (AnimSequenceType.Idle, 5, 4, true)
            );
            bossEnemyAnimations = new(
                Content.Load<Texture2D>("bossEnemy"),
                136, AnimSequenceType.Idle,
                (AnimSequenceType.Idle, 15, 12, true)
            );
            playerAnimations = new(
                Content.Load<Texture2D>("wizardSheet"),
                168, AnimSequenceType.Idle,
                (AnimSequenceType.MoveRight, 12, 12, true),
                (AnimSequenceType.Idle, 12, 12, true),
                (AnimSequenceType.MoveLeft, 11, 12, true),
                (AnimSequenceType.Pose, 12, 12, true)
            );
            hitPointAnimations = new(
                Content.Load<Texture2D>("hitPoint"),
                23, AnimSequenceType.Creation,
                (AnimSequenceType.Creation, 8, 12, false),
                (AnimSequenceType.Death, 8, 12, false)
            );
            barrierAnimations = new(
                Content.Load<Texture2D>("barrierSheet"),
                167, AnimSequenceType.Idle,
                (AnimSequenceType.Idle, 24, 12, true),
                (AnimSequenceType.Creation, 8, 12, false),
                (AnimSequenceType.Death, 6, 12, false)
            );
            fireBoltAnimations = new(
                Content.Load<Texture2D>("Fireball_Sprite_Sheet"),
                32, AnimSequenceType.RegularAttack,
                (AnimSequenceType.RegularAttack, 4, 6, true),
                (AnimSequenceType.EvilAttack, 4, 6, true),
                (AnimSequenceType.ToxicAttack, 4, 6, true),
                (AnimSequenceType.PowerAttack, 4, 6, true),
                (AnimSequenceType.SpiritAttack, 4, 6, true)
            );
            enemyBoltAnimations = new(
                Content.Load<Texture2D>("enemyBolt"),
                32, AnimSequenceType.RegularAttack,
                (AnimSequenceType.Creation, 6, 12, false),
                (AnimSequenceType.RegularAttack, 3, 12, true)
            );
            explosionAnimations = new(
                Content.Load<Texture2D>("explosionSheet"),
                100, AnimSequenceType.Explosion,
                (AnimSequenceType.Explosion, 13, 12, false)
            );

            // Enemy details would also be from an external file
            // Defining enemies Default size for now: 60 width. Scaling accordingly.
            EnemyDefined = new() {
                [EnemyType.Beholder]    = new(behEnemyAnimations, 20, 1, 60f / behEnemyAnimations.FrameWidth, new(17, 12, 31, 44)),
                [EnemyType.Bat]         = new(batEnemyAnimations, 10, 1, 60f / batEnemyAnimations.FrameWidth, new(12, 19, 42, 28)),
                [EnemyType.Owl]         = new(owlEnemyAnimations, 30, 3, 60f / owlEnemyAnimations.FrameWidth, new(11, 6, 90, 83))
            };
        }

        private void RequestStateChange(GameState newState)
        {
            if (pendingState != null)
                return; // Use the first requested one, don't override.
            pendingState = newState;
        }

        private void ChangeState(GameState newState)
        {
            state = newState;
            pendingState = null;
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
            enemies = null;
            fireBolts.Clear();

            EnemyDefinition[] lvl1Enemies = {EnemyDefined[EnemyType.Owl], EnemyDefined[EnemyType.Beholder], EnemyDefined[EnemyType.Bat]};
            enemies = new(lvl1Enemies, Vector2.Zero, new Vector2(windowSizeX,windowSizeY));

            // Only 1 player — no need to separate definitions
            player = new PlayerChar(playerAnimations, 
                new Vector2((windowSizeX / 2) - (playerAnimations.FrameWidth / 2), 
                windowSizeY - playerAnimations.FrameHeight), windowSizeX);
            player.OutOfLives += Player_OutOfLives;
            player.Fired += Player_Fired;
        }

        protected override void Update(GameTime gameTime)
        {
            input.UpdateState(IsActive);
            if(input.IsActionPressed(InputAction.Debug)) debugView = !debugView;
            if (input.IsActionPressed(InputAction.Quit))
                Exit();
            if (pendingState != null)
                ChangeState(pendingState.Value);
            
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
            if (input.IsActionPressed(InputAction.Confirm))
            {
                RequestStateChange(GameState.Playing);
            }
        }

        private void UpdatePlaying(GameTime gameTime)
        {
            foreach (var enemy in enemies.enemyGrid)
            {
                enemy.Update(gameTime);
            }

            player.Update(gameTime, input);

            foreach (var bolt in fireBolts)
            {
                bolt.Update(gameTime);
            }

            ResolveBoltHits();

            // Removals after the loop.
            //enemies.RemoveAll(enemy => !enemy.Exists); TODO: ← Needs new logic
            fireBolts.RemoveAll(bolt => !bolt.Exists);
            Window.Title = $"Invaders - Lives: {player.RemainingLives} - Enemies: {enemies.EnemiesAlive}";
        }

        private void UpdateGameOver(GameTime gameTime)
        {}

        private void ResolveBoltHits() // Pulled out of Update
        {
            foreach (var bolt in fireBolts)
            {
                if (!bolt.Exists)
                    continue;

                foreach (var enemy in enemies.enemyGrid)
                {
                    if (!enemy.Exists)
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
            SpriteAnimator boltTexture = new SpriteAnimator(fireBoltAnimations, rotation: FireBolt.UpwardRotation, centerOrigin: true);
            fireBolts.Add(new FireBolt(boltTexture, player.MuzzlePosition));
        }

        private void Enemy_ReachedBottom(object sender, EventArgs e)
        {
            player.Damage();
        }

        private void Player_OutOfLives(object sender, System.EventArgs e)
        {
            RequestStateChange(GameState.GameOver);
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
            foreach (var enemy in enemies.enemyGrid)
            {
                enemy.Draw(_spriteBatch);
                if(debugView) _spriteBatch.Draw(WhitePixel, enemy.Bounds, Color.Violet * 0.3f);
            }
            foreach (var bolt in fireBolts)
            {
                bolt.Draw(_spriteBatch);
            }

            player.Draw(_spriteBatch);

            #if DEBUG
            foreach (var enemy in enemies.enemyGrid)
            {
                if(debugView) _spriteBatch.Draw(WhitePixel, enemy.Bounds, Color.Violet * 0.3f);
            }
            foreach (var bolt in fireBolts)
            {
                bolt.Draw(_spriteBatch);
            }
            if(debugView) _spriteBatch.Draw(WhitePixel, player.Bounds, Color.Green * 0.3f);
            #endif

            _spriteBatch.End();
        }

        private void DrawGameOver(GameTime gameTime)
        {}

        protected override void UnloadContent()
        {
            WhitePixel.Dispose();
            base.UnloadContent();
        }
    }
}
