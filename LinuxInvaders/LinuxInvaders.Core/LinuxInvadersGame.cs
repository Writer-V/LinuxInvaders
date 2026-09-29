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
                169, AnimSequenceType.Idle,
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
                [EnemyType.Beholder]    = new(EnemyType.Beholder, behEnemyAnimations, 20, 1, 60f / behEnemyAnimations.FrameWidth, new(17, 12, 31, 44)),
                [EnemyType.Bat]         = new(EnemyType.Bat, batEnemyAnimations, 10, 1, 60f / batEnemyAnimations.FrameWidth, new(12, 19, 42, 28)),
                [EnemyType.Owl]         = new(EnemyType.Owl, owlEnemyAnimations, 30, 2, 60f / owlEnemyAnimations.FrameWidth, new(11, 6, 90, 83))
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
            fireBolts.Clear();

            // Only 1 player — no need to separate definitions.
            player = new PlayerChar(playerAnimations, 
                new Vector2((windowSizeX / 2) - (playerAnimations.FrameWidth / 2), 
                windowSizeY - playerAnimations.FrameHeight), windowSizeX);
            player.OutOfLives += Player_OutOfLives;
            player.Fired += Player_Fired;

            //In a theoretical full version, a Level struct/class would be appropriate for all of this.
            EnemyDefinition[] lvl1Enemies = {
                EnemyDefined[EnemyType.Owl], 
                EnemyDefined[EnemyType.Beholder], 
                EnemyDefined[EnemyType.Beholder], 
                EnemyDefined[EnemyType.Bat],
                EnemyDefined[EnemyType.Bat]
            };
            int columns = 6;
            float hitBoxScale = 1.3f;
            Vector2 startSpeed = new(25f, 20f);
            Vector2 highSpeed = new(100f, 50f);

            enemies = new(lvl1Enemies, columns, Vector2.Zero, hitBoxScale, startSpeed, highSpeed, 
                new Point(windowSizeX,windowSizeY), player.Bounds.Top);

            enemies.EnemyReachedPlayer += Enemy_ReachedBottom;
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
            enemies.Update(gameTime);

            player.Update(gameTime, input);

            foreach (var bolt in fireBolts)
            {
                bolt.Update(gameTime);
                if(enemies.TryHit(bolt.Bounds))
                {
                    bolt.Deactivate();
                    continue;
                }
            }
            fireBolts.RemoveAll(bolt => !bolt.Exists);
            Window.Title = $"Invaders - Lives: {player.RemainingLives} - Enemies: {enemies.EnemiesAlive()}"; //Remove when there's in-window UI
        }

        private void UpdateGameOver(GameTime gameTime)
        {}

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
            enemies.Draw(_spriteBatch, debugView, WhitePixel);

            foreach (var bolt in fireBolts)
            {
                bolt.Draw(_spriteBatch, debugView, WhitePixel);
            }

            player.Draw(_spriteBatch);

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
