#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using PrimalDevistation.Sprites;
using PrimalDevistation.Level;
using PrimalDevistation.Objects;
using PrimalDevistation.Particles;
using PrimalDevistation.Weapons;
using PrimalDevistation.Usefuls;
using PrimalDevistation.Player;
using SpritesAndLines;
using PrimalDevistation.Audio;
#endregion

namespace PrimalDevistation
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PrimalDevistation : Microsoft.Xna.Framework.Game
    {
        public static Vector2 GRAVITY = new Vector2(0, 0.06f);
        private GraphicsDeviceManager _graphics;
        private ContentManager _content;
        private SpriteBatch _batch;
        private static PrimalDevistation _instance;
        private iPhysicsParticleSystem _physicsParticles;
        private cConsole _console;    
        
#if !XBOX  
        private cDebugMode _debugComp;
#endif

        private cLevel _level;
        private cWeaponFactory _weapons;
        private cParticleEffectSystem _particleEffectSystem;
        private cCamera2D _camera;
        private cPlayerManager _playerManager;
        private cAnimationManager _animManager;
        private LineManager _lineManager;
        private BloomComponent _bloom;
        private cAudio _audio;
        private Texture2D _expFlash32;
        private Texture2D _expFlash64;
        private Texture2D _expFlash128;
	
        public cAudio Audio
        {
            get { return _audio; }
            set { _audio = value; }
        }
        
        public LineManager LineManager
        {
            get { return _lineManager; }
            set { _lineManager = value; }
        }	

        public cAnimationManager AnimManager
        {
            get { return _animManager; }
            set { _animManager = value; }
        }

        public cPlayerManager PlayerManager
        {
            get { return _playerManager; }
            set { _playerManager = value; }
        }	
	
        public cCamera2D Camera
        {
            get { return _camera; }
            set { _camera = value; }
        }

        public cParticleEffectSystem ParticleEffectSystem
        {
            get { return _particleEffectSystem; }
            set { _particleEffectSystem = value; }
        }	

        public cWeaponFactory Weapons
        {
            get { return _weapons; }
            set { _weapons = value; }
        }
	
        public cLevel Level
        {
            get { return _level; }
            set { _level = value; }
        }	

#if !XBOX  
        public cDebugMode DebugMode
        {
            get { return _debugComp; }
            set { _debugComp = value; }
        }
#endif

        public cConsole Console
        {
            get { return _console; }
            set { _console = value; }
        }

        public iPhysicsParticleSystem PhysicsParticles
        {
            get { return _physicsParticles; }
            set { _physicsParticles = value; }
        }

        public cLevel CurrentLevel
        {
            get { return _level; }
            set { _level = value; }
        }

        public static PrimalDevistation Instance { get { return _instance; } }
        public SpriteBatch Batch { get { return _batch; } }	
        public ContentManager CM { get { return _content; } }	
        public GraphicsDevice GD { get { return _graphics.GraphicsDevice; } }
        
        public PrimalDevistation()
        {
            _instance = this;
            _graphics = new GraphicsDeviceManager(this);
            _content = new ContentManager(Services,@"Resources");
            this.IsMouseVisible = true;        
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
            //_graphics.MinimumPixelShaderProfile = ShaderProfile.PS_3_0;
            //_graphics.MinimumVertexShaderProfile = ShaderProfile.VS_3_0;

            // Create the console
            _console = new cConsole(this);      
            _console.DrawOrder = 90000;
            Components.Add(_console);

#if !XBOX  
            // Create the debug mode object
            _debugComp = new cDebugMode(this);
            _console.AddCommand("debug", "toggles whether debug mode is active or not", delegate { _debugComp.Enabled = !_debugComp.Enabled; _debugComp.Visible = !_debugComp.Visible; });
            _debugComp.Enabled = true;
            _debugComp.Visible = true;
            _debugComp.DrawOrder = int.MaxValue - 1;
            Components.Add(_debugComp);
#endif

            _level = new cLevel(this, 1, 1);
            _level.DrawOrder = 100;
            Components.Add(_level);

            _weapons = new cWeaponFactory(this);
            _weapons.DrawOrder = 600;
            Components.Add(_weapons);

            _playerManager = new cPlayerManager(this);
            _playerManager.DrawOrder = 900;
            Components.Add(_playerManager);   

            // Create the particle system      

            cPhysicsParticles360 parts1 = new cPhysicsParticles360(this);
            _physicsParticles = parts1;
            parts1.DrawOrder = 700;
            Components.Add(parts1);

            
            //cPhysicsParticlesCPU parts2 = new cPhysicsParticlesCPU(this);
            //_physicsParticles = parts2;
            //parts2.DrawOrder = 700;
            //Components.Add(parts2);


            _particleEffectSystem = new cParticleEffectSystem(this);
            _particleEffectSystem.DrawOrder = 800;
            Components.Add(_particleEffectSystem);

            _lineManager = new LineManager(this);
            _lineManager.DrawOrder = 500;
            Components.Add(_lineManager);

            _console.AddCommand("bloom", "toggles bloom", delegate { _bloom.Enabled = !_bloom.Enabled; _bloom.Visible = !_bloom.Visible; });
            _bloom = new BloomComponent(this);
            _bloom.DrawOrder = 2000;
            Components.Add(_bloom);            


            _audio = new cAudio();

           
        }
    
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related _content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {        

            GraphicsDevice gd = _graphics.GraphicsDevice;
            _batch = new SpriteBatch(gd);
            _camera = new cCamera2D(gd.Viewport);
            _camera.Position = new Vector2(500,500);
            _animManager = new cAnimationManager();
            base.Initialize();
        }
       
        /// <summary>
        /// Load your _graphics _content.  If loadAllContent is true, you should
        /// load _content from both ResourceManagementMode pools.  Otherwise, just
        /// load ResourceManagementMode.Manual _content.
        /// </summary>
        /// <param name="loadAllContent">Which type of _content to load.</param>
        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            GraphicsDevice gd = _graphics.GraphicsDevice;
            gd.Clear(Color.CornflowerBlue);
            if (loadAllContent) 
            {
                _expFlash32 = CM.Load<Texture2D>(@"Sprites/flash32");
                _expFlash64 = CM.Load<Texture2D>(@"Sprites/flash64");
                _expFlash128 = CM.Load<Texture2D>(@"Sprites/flash128");      
            }
        }


        /// <summary>
        /// Unload your _graphics _content.  If unloadAllContent is true, you should
        /// unload _content from both ResourceManagementMode pools.  Otherwise, just
        /// unload ResourceManagementMode.Manual _content.  Manual _content will get
        /// Disposed by the GraphicsDevice during a Reset.
        /// </summary>
        /// <param name="unloadAllContent">Which type of _content to unload.</param>
        protected override void UnloadGraphicsContent(bool unloadAllContent)
        {
            if (unloadAllContent)
            {
                // TODO: Unload any ResourceManagementMode.Automatic _content
                _content.Unload();
            }

            // TODO: Unload any ResourceManagementMode.Manual _content
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            GamePadState gps1 = GamePad.GetState(PlayerIndex.One);
          
            if (gps1.Buttons.Back == ButtonState.Pressed) { this.Exit(); }
            //if (gps1.Buttons.Start == ButtonState.Pressed && !_playerManager.Players[0].Playing) { _playerManager.SpawnNewPlayer(500,500); }
            
            _audio.Update(gameTime);
            _camera.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice gd = _graphics.GraphicsDevice;
            //gd.Clear(new Color(100,149,237,255));
            base.Draw(gameTime);
            _playerManager.RenderScores();

        }

        public void AddForce(Vector2 where, float strength, float reach)
        {
            PhysicsParticles.AddForce(where, strength, reach);
            PlayerManager.AddForce(where, strength*2, reach);
            Weapons.AddForce(where, strength, reach);
        }

        public void Explode(int size, Vector2 where, Vector2 direction, cPlayer owner, bool audio)
        {
            Level.AddExplosion(size,where,direction);
            AddForce(where, -(size / 64f), size * 2);
            PlayerManager.Explosion(where, size, direction, owner);

            cParticle p = new cParticle();
            if (size == 32) { p.Texture = _expFlash32; p.MaxAge = 100; }
            else if (size == 64) { p.Texture = _expFlash64; p.MaxAge = 300; }
            else if (size == 128) { p.Texture = _expFlash128; p.MaxAge = 500; }            
            
            p.Position = where;
            p.StartColor = new Color(255,255,255,255);
            p.EndColor = new Color(255,0,0,0);
            ParticleEffectSystem.Particles.Add(p);

            if (audio)
            {
                if (size == 32) { PrimalDevistation.Instance.Audio.play("EXPLOSION_LARGE1"); }
                if (size == 64) { PrimalDevistation.Instance.Audio.play("EXPLOSION_LARGE2"); }
                if (size == 128) { PrimalDevistation.Instance.Audio.play("EXPLOSION_LARGE3"); }
            }
        }
    }
}
