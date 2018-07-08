using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using PrimalDevistation.Usefuls;
using Microsoft.Xna.Framework.Graphics;
using PrimalDevistation.Level;
using Microsoft.Xna.Framework.Input;

namespace PrimalDevistation.Player
{
    public class cPlayerManager : DrawableGameComponent
    {
        private cPlayer[] _players;
        private List<cPlayer> _activePlayers;
        private Vector4[] _forces;
        private int _nextForce;
        private SpriteFont _font;
        private Texture2D _flatTex;
        private GamePadState[] _lastGPSs;

        public List<cPlayer> ActivePlayers
        {
            get { return _activePlayers; }
            set { _activePlayers = value; }
        }

        public cPlayer[] Players
        {
            get { return _players; }
            set { _players = value; }
        }

        public cPlayerManager(Game game)
            : base(game)
        {
            _players = new cPlayer[4];
            _players[0] = new cPlayer(PlayerIndex.One);
            _players[1] = new cPlayer(PlayerIndex.Two);
            _players[2] = new cPlayer(PlayerIndex.Three);
            _players[3] = new cPlayer(PlayerIndex.Four);
            //LieroXNA.Instance.LieroConsole.AddCommand("spawnPlayer", "spawns a new player randomly about the world", delegate { SpawnNewPlayer(); });
            PrimalDevistation.Instance.Console.AddCommand("bubbles", "activates or deactivates bubbles", delegate 
            {
                for (int i = 0; i < _players.Length; i++)
                {
                    _players[i].Bubbles = !_players[i].Bubbles;                    
                }
            });

            _lastGPSs = new GamePadState[4];

            _activePlayers = new List<cPlayer>();
            _forces = new Vector4[4];
            for (int i = 0; i < _forces.Length; i++) { _forces[i] = Vector4.Zero; }
            _nextForce = 0;
        }

        public void SpawnNewPlayer(int player)
        {
            int spawnSize = 128;
            Vector2 levSize = PrimalDevistation.Instance.Level.Size;
            cCollisionMap cm = PrimalDevistation.Instance.Level.CollisionMap;
            int x, y;
            do
            {
                //x = (int)cMath.Rand(600 + (spawnSize / 2), levSize.X - (spawnSize / 2)-600);
                //y = (int)cMath.Rand(600 + (spawnSize / 2), levSize.Y - (spawnSize / 2)-600);
                x = (int)cMath.Rand((spawnSize / 2), levSize.X - (spawnSize / 2));
                //y = (int)cMath.Rand((spawnSize / 2), levSize.Y - (spawnSize / 2));
                y = (int)cMath.Rand((spawnSize / 2), 400);
            } while (cm.CheckCollisionIndestructable(x - 64, y - 64, cm.Explosions[128]._data, SpriteEffects.None));

            SpawnNewPlayer(player, x, y);
        }

        private void Respawn(cPlayer p)
        {
            int spawnSize = 128;
            Vector2 levSize = PrimalDevistation.Instance.Level.Size;
            cCollisionMap cm = PrimalDevistation.Instance.Level.CollisionMap;
            int x, y;
            do
            {
                //x = (int)cMath.Rand(600 + (spawnSize / 2), levSize.X - (spawnSize / 2)-600);
                //y = (int)cMath.Rand(600 + (spawnSize / 2), levSize.Y - (spawnSize / 2)-600);
                x = (int)cMath.Rand((spawnSize / 2), levSize.X - (spawnSize / 2));
                //y = (int)cMath.Rand((spawnSize / 2), levSize.Y - (spawnSize / 2));
                y = (int)cMath.Rand((spawnSize / 2), 400);
            } while (cm.CheckCollisionIndestructable(x - 64, y - 64, cm.Explosions[128]._data, SpriteEffects.None));
            PrimalDevistation.Instance.Level.AddExplosion(128, new Vector2(x, y), Vector2.Zero);
            p.Position = new Vector2(x, y);
            p.HitPoints = p.MaxHitPoints;
        }

        public void SpawnNewPlayer(int playerIndex, int x, int y)
        {
            // Finding the next free player slot            
            cPlayer p = _players[playerIndex];
            p.Playing = true;

            PrimalDevistation.Instance.Level.AddExplosion(128, new Vector2(x, y), Vector2.Zero);
            p.Position = new Vector2(x, y);
            _activePlayers.Add(p);
        }

        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            _flatTex = PrimalDevistation.Instance.CM.Load<Texture2D>(@"Sprites/particle2x2");
            for (int i = 0; i < _players.Length; i++)
            {
                cPlayer p = _players[i];
                p.LoadGraphicsContent(PrimalDevistation.Instance.GD, PrimalDevistation.Instance.CM);
                _font = PrimalDevistation.Instance.CM.Load<SpriteFont>("Other/ConsoleFont");
            }
        }

        public override void Update(GameTime gameTime)
        {
            GamePadState[] gps = new GamePadState[4];
            gps[0] = GamePad.GetState(PlayerIndex.One);
            gps[1] = GamePad.GetState(PlayerIndex.Two);
            gps[2] = GamePad.GetState(PlayerIndex.Three);
            gps[3] = GamePad.GetState(PlayerIndex.Four);

            //if (gps[0].Buttons.Y == ButtonState.Pressed && _lastGPSs[0].Buttons.Y == ButtonState.Released)
            //{
            //    if (_players[0].PlayerIndex == PlayerIndex.One) { _players[0].PlayerIndex = PlayerIndex.Two; _players[1].PlayerIndex = PlayerIndex.One; }
            //    else if (_players[0].PlayerIndex == PlayerIndex.Two) { _players[0].PlayerIndex = PlayerIndex.Three; _players[2].PlayerIndex = PlayerIndex.One; }
            //    else if (_players[0].PlayerIndex == PlayerIndex.Three) { _players[0].PlayerIndex = PlayerIndex.Four; _players[3].PlayerIndex = PlayerIndex.One; }
            //    else if (_players[0].PlayerIndex == PlayerIndex.Four) { _players[0].PlayerIndex = PlayerIndex.One; _players[0].PlayerIndex = PlayerIndex.One; }
            //}

            for (int i = 0; i < gps.Length; i++)
            {
                if (gps[i].Buttons.Start == ButtonState.Pressed && _lastGPSs[i].Buttons.Start == ButtonState.Released)
                {
                    if (_players[i].Playing)
                    {
                        _players[i].Playing = false;

                        if (_players[i].CurrentGrapple != null)
                        {
                            PrimalDevistation.Instance.Weapons.ReleaseGrapple(_players[i].CurrentGrapple);
                            _players[i].CurrentGrapple = null;
                        }

                        _players[i].HitPoints = _players[i].MaxHitPoints;
                        _players[i].Velocity = new Vector2();

                        _activePlayers.Remove(_players[i]);

                    }
                    else
                    {
                        //if (i == 0) { SpawnNewPlayer(0); SpawnNewPlayer(1); SpawnNewPlayer(2); SpawnNewPlayer(3); }
                        if (i == 0) { SpawnNewPlayer(0, 1300, 1500); }
                        if (i == 1) { SpawnNewPlayer(1, 200, 1400); }
                        if (i == 2) { SpawnNewPlayer(2, 800, 1500); }
                        if (i == 3) { SpawnNewPlayer(3, 1000, 1500); }
                    }
                }
            }

            for (int i = 0; i < _activePlayers.Count; i++)
            {
                _activePlayers[i].Update(gameTime, _forces);
            }

            for (int i = 0; i < _forces.Length; i++) { _forces[i].Z = 0; }
            _nextForce = 0;
            
            _lastGPSs = gps;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch batch = PrimalDevistation.Instance.Batch;
            batch.Begin();

            for (int i = 0; i < _activePlayers.Count; i++) 
            {
                _activePlayers[i].Draw(batch);
            }
            batch.End();
        }

        public void RenderScores()
        {
            SpriteBatch batch = PrimalDevistation.Instance.Batch;
            batch.Begin();

            if (_activePlayers.Count > 0)
            {
                Vector2 vec = _font.MeasureString("0");
                batch.Draw(_flatTex, new Rectangle((GraphicsDevice.Viewport.Width / 2) - 120, 80, 240, (int)(vec.Y)), new Color(0, 0, 0, 180));
            }

            for (int i = 0; i < _activePlayers.Count; i++)
            {
                _activePlayers[i].RenderUI(batch);
                if (i == 0) { batch.DrawString(_font, "" + _activePlayers[i].Score, new Vector2(GraphicsDevice.Viewport.Width / 2 - 75, 80), _activePlayers[i].PrimaryColor); }
                if (i == 1) { batch.DrawString(_font, "" + _activePlayers[i].Score, new Vector2(GraphicsDevice.Viewport.Width / 2 - 25, 80), _activePlayers[i].PrimaryColor); }
                if (i == 2) { batch.DrawString(_font, "" + _activePlayers[i].Score, new Vector2(GraphicsDevice.Viewport.Width / 2 + 25, 80), _activePlayers[i].PrimaryColor); }
                if (i == 3) { batch.DrawString(_font, "" + _activePlayers[i].Score, new Vector2(GraphicsDevice.Viewport.Width / 2 + 75, 80), _activePlayers[i].PrimaryColor); }
            }

            batch.End();
        }

        public void AddForce(Vector2 where, float strength, float holeSize)
        {
            _forces[_nextForce].X = where.X;
            _forces[_nextForce].Y = where.Y;
            _forces[_nextForce].Z = strength;
            _forces[_nextForce].W = holeSize;
            _nextForce++;
            if (_nextForce >= _forces.Length) { _nextForce = 0; }
        }

        public void Explosion(Vector2 where, int size, Vector2 direction, cPlayer owner)
        {
            for (int i = 0; i < _activePlayers.Count; i++)
            {
                cPlayer p = _activePlayers[i];
                Vector2 dist = where - p.Position;
                int lenSqr = (int)dist.LengthSquared();
                if (lenSqr < size * size)
                {
                    float len = dist.Length();
                    float mag = size / len;
                    if (mag > 30) { mag = 30; }

                    int sze = size;
                    if (sze == 32) { sze = 64; }
                    int subHPS = (int)(mag * (sze * 0.018f));

                    int numParticles = subHPS*10;

                    dist.Normalize();
                    dist *= -mag / 1;

                    List<Vector2> positions = new List<Vector2>(numParticles);
                    List<Vector2> velocities = new List<Vector2>(numParticles);
                    List<Color> colors = new List<Color>(numParticles);
                    //Color c = p.PrimaryColor;
                    Color c = Color.Red;

                    for (int j = 0; j < numParticles; j++)
                    {
                        positions.Add(new Vector2(p.Position.X + cMath.Rand(-10f, 10f), p.Position.Y + cMath.Rand(-10f, 10f)));
                        velocities.Add(new Vector2(cMath.Rand(1f, 3f) * dist.X, cMath.Rand(1f, 3f) * dist.Y));
                        colors.Add(c);
                    }
                    PrimalDevistation.Instance.PhysicsParticles.SpawnParticles(positions, velocities, colors);


                   
                    p.HitPoints = p.HitPoints - subHPS;
                    if (p.HitPoints < 0)
                    {
                        if (owner == null) { }
                        else if (owner == p) { p.Score = p.Score-1; }
                        else { owner.Score = owner.Score + 1; }

                        if (p.CurrentGrapple != null)
                        {
                            PrimalDevistation.Instance.Weapons.ReleaseGrapple(p.CurrentGrapple);
                            p.CurrentGrapple = null;
                        }

                        numParticles = 2000;
                        positions = new List<Vector2>(numParticles);
                        velocities = new List<Vector2>(numParticles);
                        colors = new List<Color>(numParticles);
                        //c = p.PrimaryColor;
                        c = Color.Red;
                        float angle, x, y;
                        

                        for (int j = 0; j < numParticles; j++)
                        {
                            positions.Add(new Vector2(p.Position.X + cMath.Rand(-2f, 2f), p.Position.Y + cMath.Rand(-2f, 2f)));

                            angle = cMath.Rand(0f, (float)(Math.PI*2));
                            x = (float)Math.Sin(angle) * cMath.Rand(6f, 12f);
                            y = (float)Math.Cos(angle) * cMath.Rand(6f, 12f);
                            velocities.Add( new Vector2(x,y));
                            colors.Add(c);
                        }
                        PrimalDevistation.Instance.PhysicsParticles.SpawnParticles(positions, velocities, colors);


                        Respawn(p);
                        PrimalDevistation.Instance.Audio.play("DEATH1");
                    }

                    
                }
            }
        }
        
    }
}
