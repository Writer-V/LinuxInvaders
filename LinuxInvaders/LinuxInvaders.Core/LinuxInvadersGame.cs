using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using LinuxInvaders.Core.Input;
using LinuxInvaders.Core.Graphics;
using System;
using LinuxInvaders.Core.Enemies;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace LinuxInvaders.Core
{
    public class LinuxInvadersGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private PlayerInputs input = new PlayerInputs();
        private EnemyFormation enemies;
        private List<FireBolt> fireBolts = new List<FireBolt>();
        private List<PlacedEffect> explosions = new List<PlacedEffect>();
        private PlayerChar player;
        private HitPointDisplay hitPointDisplay;
        private int score = 0;
        private bool win = true; // Simple bool for now
        private GameState state = GameState.Start;
        private GameState? pendingState = null;
        //Start, game over, and player should really be subclasses to a Screen class
        private StartScreen startScreen;
        private GameOverScreen gameOverScreen;
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
        private SpriteFont font;
        private Texture2D startButton;
        private Texture2D restartButton;
        private BackgroundDraw backgroundDraw;
        private Texture2D startBackground;
        private Texture2D playingBackground;
        private Texture2D winBackground;
        private Texture2D loseBackground;
        private Texture2D WhitePixel; //Just used for debugging
        private Song startBackgroundSong;
        private Song gameBackgroundSong;
        private Song winBackgroundSong;
        private Song loseBackgroundSong;
        private SoundEffect batDeathSound;
        private SoundEffect behDeathSound;
        private SoundEffect owlDeathSound;
        private SoundEffect boltHitSound;
        private SoundEffect boltLaunchedSound;
        private SoundEffect enemyReachedBottomSound;
        private SoundEffect playerDeathSound;
        private SoundEffect buttonPressSound;

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

            font = Content.Load<SpriteFont>("font");
            startButton = Content.Load<Texture2D>("Startknapp");
            restartButton = Content.Load<Texture2D>("restartButton");

            //Background textures. Could be loaded/unloaded instead. Another time, another game.
            startBackground = Content.Load<Texture2D>("introBackground");
            playingBackground = Content.Load<Texture2D>("gameBackground");
            winBackground = Content.Load<Texture2D>("successBackground");
            loseBackground = Content.Load<Texture2D>("failedBackground");
            backgroundDraw = new(startBackground, new(windowSizeX,windowSizeY));

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
                (AnimSequenceType.Death, 8, 12, false),
                (AnimSequenceType.Creation, 8, 12, false)
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

            startScreen = new(startButton, playerAnimations, new Point(windowSizeX,windowSizeY));
            startScreen.StartButtonClicked += StartScreen_StartButtonClicked;
            gameOverScreen = new(
                [behEnemyAnimations,batEnemyAnimations,batEnemyAnimations,owlEnemyAnimations],
                [explosionAnimations], playerAnimations, bossEnemyAnimations, restartButton,
                font, new(windowSizeX,windowSizeY));
            gameOverScreen.RestartButtonClicked += GameOverScreen_RestartButtonClicked;

            startBackgroundSong = Content.Load<Song>("startBgMusic");
            gameBackgroundSong = Content.Load<Song>("gameBgMusic");
            winBackgroundSong = Content.Load<Song>("winBgMusic");
            loseBackgroundSong = Content.Load<Song>("lostBgMusic");
            batDeathSound = Content.Load<SoundEffect>("batDies");
            behDeathSound = Content.Load<SoundEffect>("behDeath");
            owlDeathSound = Content.Load<SoundEffect>("owlDeath");
            boltHitSound = Content.Load<SoundEffect>("boldHit");
            boltLaunchedSound = Content.Load<SoundEffect>("boltShhot");
            enemyReachedBottomSound = Content.Load<SoundEffect>("enemyReachedBottom");
            playerDeathSound = Content.Load<SoundEffect>("playerDeath");
            buttonPressSound = Content.Load<SoundEffect>("buttonConfirm");

            MediaPlayer.Play(startBackgroundSong);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = 0.6f;

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
            explosions.Clear();
            switch (state)
            {
                case GameState.Start:
                    backgroundDraw.SwitchBackground(startBackground);
                    MediaPlayer.Play(startBackgroundSong);
                    break;
                case GameState.Playing:
                    backgroundDraw.SwitchBackground(playingBackground);
                    MediaPlayer.Play(gameBackgroundSong);
                    score = 0;
                    StartNewGame();
                    break;
                case GameState.GameOver:
                    if(win)
                    {
                        backgroundDraw.SwitchBackground(winBackground);
                        MediaPlayer.Play(winBackgroundSong);
                    }
                    else
                    {
                        backgroundDraw.SwitchBackground(loseBackground);
                        MediaPlayer.Play(loseBackgroundSong);
                    }
                    gameOverScreen.SetUp(win,score);
                    break;
            }
        }

        //Later versions would split this into a Playing class and details in a Level class.
        private void StartNewGame()
        {
            // Reset the game state.
            fireBolts.Clear();
            score = 0;
            win = false;

            // Only 1 player — no need to separate definitions.
            int playerStartingLives = 3; // Theoretically set by difficulty
            player = new PlayerChar(playerAnimations, playerStartingLives,
                new Vector2((windowSizeX / 2) - (playerAnimations.FrameWidth / 2), 
                windowSizeY - playerAnimations.FrameHeight), windowSizeX);
            player.OutOfLives += Player_OutOfLives;
            player.Fired += Player_Fired;

            hitPointDisplay = new(hitPointAnimations, new Point(windowSizeX - 10, 10), player.MaxLives);

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
            Vector2 startSpeed = new(60f, 20f);
            Vector2 highSpeed = new(200f, 60f);

            enemies = new(lvl1Enemies, columns, Vector2.Zero, hitBoxScale, startSpeed, highSpeed, 
                new Point(windowSizeX,windowSizeY), player.Bounds.Top);
            enemies.EnemyHit += Enemies_EnemyHit;
            enemies.EnemyReachedPlayer += Enemy_ReachedBottom;
        }

        protected override void Update(GameTime gameTime)
        {
            //All updating not in this method should be in another class... c'est la vie.
            input.UpdateState(IsActive);
            if(input.IsActionPressed(InputAction.Debug)) debugView = !debugView;
            if (input.IsActionPressed(InputAction.Quit))
                Exit();
            if (pendingState != null)
                ChangeState(pendingState.Value);
            
            switch (state)
            {
                case GameState.Start:
                    startScreen.Update(gameTime, input);
                    break;
                case GameState.Playing:
                    UpdatePlaying(gameTime);
                    break;
                case GameState.GameOver:
                    gameOverScreen.Update(gameTime, input);
                    break;
            }

            base.Update(gameTime);
        }

        private void UpdatePlaying(GameTime gameTime)
        {
            enemies.Update(gameTime);
            if(enemies.EnemiesAlive() < 1) EndGame(true);

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
            foreach (PlacedEffect explosion in explosions)
                explosion.Update(gameTime);
            
            fireBolts.RemoveAll(bolt => !bolt.Exists);
            explosions.RemoveAll(explosion => !explosion.Exists);
            hitPointDisplay.Update(gameTime, player.RemainingLives);
        }
        private void StartScreen_StartButtonClicked(object sender, EventArgs e)
        {
            buttonPressSound.Play(1, 0, 0);
            RequestStateChange(GameState.Playing);
        }
        private void GameOverScreen_RestartButtonClicked(object sender, EventArgs e)
        {
            buttonPressSound.Play(1,0,0);
            RequestStateChange(GameState.Playing);
        }
        private void Player_Fired(object sender, EventArgs e)
        {
            // Build the bolt, reuse the cashed sheet.
            SpriteAnimator boltTexture = new SpriteAnimator(fireBoltAnimations, rotation: FireBolt.UpwardRotation, centerOrigin: true);
            fireBolts.Add(new FireBolt(boltTexture, player.MuzzlePosition));
            boltLaunchedSound.Play((5 + Random.Shared.Next(2))/10f, Random.Shared.Next(2)/10f - 0.9f, 0);
        }

        private void Enemies_EnemyHit(object sender, EnemyHitEventArgs e)
        {
            boltHitSound.Play((5 + Random.Shared.Next(2))/10f, Random.Shared.Next(2)/10f - 0.9f, 0);
            if(e.Killed)
            {
                score += e.Points;
                explosions.Add(new(explosionAnimations, e.Location));
                if(e.Type == EnemyType.Bat) batDeathSound.Play((6 + Random.Shared.Next(4))/10f, Random.Shared.Next(3)/10f - 0.9f, 0);
                else if(e.Type == EnemyType.Beholder) behDeathSound.Play((5 + Random.Shared.Next(5))/10f, Random.Shared.Next(3)/10f - 0.9f, 0);
                else if(e.Type == EnemyType.Owl) owlDeathSound.Play((7 + Random.Shared.Next(3))/10f, Random.Shared.Next(2)/10f - 0.9f, 0);
            }
        }

        private void Enemy_ReachedBottom(object sender, EventArgs e)
        {
            enemyReachedBottomSound.Play(1, Random.Shared.Next(2)/10f - 0.9f, 0);
            player.Damage();
        }

        private void Player_OutOfLives(object sender, System.EventArgs e)
        {
            playerDeathSound.Play(1,0,0);
            EndGame(false);
        }

        private void EndGame(bool win)
        {
            if(pendingState != null) return;
            this.win = win;
            RequestStateChange(GameState.GameOver);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            backgroundDraw.Draw(gameTime, _spriteBatch);

            switch (state)
            {
                case GameState.Start:
                    startScreen.Draw(_spriteBatch);
                    break;
                case GameState.Playing:
                    DrawPlaying(gameTime, _spriteBatch);
                    break;
                case GameState.GameOver:
                    gameOverScreen.Draw(_spriteBatch);
                    break;
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void DrawPlaying(GameTime gameTime, SpriteBatch spriteBatch)
        {
            enemies.Draw(spriteBatch, debugView, WhitePixel);

            foreach (var bolt in fireBolts)
                bolt.Draw(spriteBatch, debugView, WhitePixel);
            foreach (PlacedEffect explosion in explosions)
                explosion.Draw(spriteBatch);
            player.Draw(spriteBatch);
            hitPointDisplay.Draw(_spriteBatch);
            
            spriteBatch.DrawString(font,score.ToString(), new Vector2(10, 5), Color.LightSteelBlue);
        }

        protected override void UnloadContent()
        {
            WhitePixel.Dispose();
            base.UnloadContent();
        }
    }
}
