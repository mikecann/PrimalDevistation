using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using PrimalDevistation.Objects;
using PrimalDevistation.Level;
using Microsoft.Xna.Framework.Input;
using PrimalDevistation.Sprites;
using PrimalDevistation.Weapons;
using PrimalDevistation.Usefuls;
using PrimalDevistation.Particles;

namespace PrimalDevistation.Player
{
    public class cPlayer : cPhysicsObject
    {
        private bool _playing;
        private const float _maxAirControl = 0.05f;
        private const float _playerJumpSpeed = 5f;
        private const float _playerMaxSpeed = 4f;
        private const float _maxWalkSpeed = 1;
        //protected cAnimatedSprite _sprite;
        private KeyboardState _lastKS;
        private GamePadState _lastGPS;
        private PlayerIndex _playerIndex;
        private Texture2D _crosshair;
        private int _currentWeaponIndex;
        private int _weaponSwitchTimer;
        private bool _weaponSwitching;
        private float _firePointRight;
        private float _firePointLeft;
        private int _hitPoints;
        private int _healthBarWidth = 50;
        private Texture2D _flatTex;
        private int _score;
        private cGrapple _currentGrapple;
        private int[,] _currentAmmoLevels;
        private Texture2D _charBase;
        private Texture2D _charTurret;        
        private Color _primaryColor;
        private bool _dJump;
        private int _maxHitPoints = 25;
        private Texture2D _bubble;
        private double _reticuleAngle;
        private double _moveToReticuleAngle;
        private float _reticuleRotateRate = 0.1f;
        private Texture2D _muzzleFlash;

        private bool _bubbles;

        public bool Bubbles
        {
            get { return _bubbles; }
            set { _bubbles = value; }
        }

        public int MaxHitPoints
        {
            get { return _maxHitPoints; }
            set { _maxHitPoints = value; }
        }

        public Color PrimaryColor
        {
            get { return _primaryColor; }
            set { _primaryColor = value; }
        }        

        public cGrapple CurrentGrapple
        {
            get { return _currentGrapple; }
            set { _currentGrapple = value; }
        }        

        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }	

        public int HitPoints
        {
            get { return _hitPoints; }
            set { _hitPoints = value; }
        }

        public PlayerIndex PlayerIndex
        {
            get { return _playerIndex; }
            set { _playerIndex = value; }
        }
	
        public bool Playing
        {
            get { return _playing; }
            set { _playing = value; }
        }
	
        public cPlayer(PlayerIndex playerIndex)
        {
            _friction = 0.2f;
            _playing = false;
            _lastKS = Keyboard.GetState();
            _playerIndex = playerIndex;
            _lastGPS = GamePad.GetState(playerIndex);
            _collisionBounds = new Rectangle(-15, -15, 30, 30);
            _boundsCollision = true;
            _weaponSwitching = false;
            _currentWeaponIndex = 0;
            _firePointRight = 0;
            _firePointLeft = 0;
            _canWalk = true;
            _currentGrapple = null;
            _dJump = false;
            _score = 0;
            _reticuleAngle = 0;


            if (playerIndex == PlayerIndex.One) { _primaryColor = new Color(78, 164, 222);  }
            if (playerIndex == PlayerIndex.Two) { _primaryColor = new Color(154, 222, 78); }
            if (playerIndex == PlayerIndex.Three) { _primaryColor = new Color(249, 157, 28); }
            if (playerIndex == PlayerIndex.Four) { _primaryColor = new Color(222, 78, 185); }


            
            Respawn();                   
        }

        public void Respawn()
        {
            _hitPoints=_maxHitPoints;
        }

        public void LoadGraphicsContent(GraphicsDevice gd, ContentManager cm)
        {
            //_sprite = LieroXNA.Instance.AnimManager.addSprite("worm_anim", @"Resources/Sprites/worm_anim.xml");
            //_sprite.Play("walk");
            _crosshair = cm.Load<Texture2D>(@"Sprites/crosshair");
            _flatTex = cm.Load<Texture2D>(@"Sprites/particle2x2");

            if (_playerIndex == PlayerIndex.One)
            {
                _charBase = cm.Load<Texture2D>(@"Sprites/brown");
                _charTurret = cm.Load<Texture2D>(@"Sprites/brown_t");
            }
            else if (_playerIndex == PlayerIndex.Two)
            {
                _charBase = cm.Load<Texture2D>(@"Sprites/green");
                _charTurret = cm.Load<Texture2D>(@"Sprites/green_t");
            }
            else if (_playerIndex == PlayerIndex.Three)
            {
                _charBase = cm.Load<Texture2D>(@"Sprites/orange");
                _charTurret = cm.Load<Texture2D>(@"Sprites/orange_t");
            }
            else if (_playerIndex == PlayerIndex.Four)
            {
                _charBase = cm.Load<Texture2D>(@"Sprites/red");
                _charTurret = cm.Load<Texture2D>(@"Sprites/red_t");
            }

            _bubble = cm.Load<Texture2D>(@"Sprites/bubble");

            _muzzleFlash = cm.Load<Texture2D>(@"Sprites/flash32");

            this.Origin = new Vector2(22, 22);

            List<cWeapon> weps = PrimalDevistation.Instance.Weapons.WeaponBlueprints;
            _currentAmmoLevels = new int[weps.Count, 6];
            for (int i = 0; i < weps.Count; i++)
            {
                _currentAmmoLevels[i, 0] = weps[i].MaxAmmo;
                _currentAmmoLevels[i, 1] = weps[i].MaxAmmo;
                _currentAmmoLevels[i, 2] = weps[i].RateOfFire+1;
                _currentAmmoLevels[i, 3] = weps[i].RateOfFire;
                _currentAmmoLevels[i, 4] = weps[i].ReloadTime+1;
                _currentAmmoLevels[i, 5] = weps[i].ReloadTime;
            }         
        }       

        public void Update(GameTime gameTime, Vector4[] forces)
        {
            if (_weaponSwitching) 
            {
                _weaponSwitchTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (_weaponSwitchTimer > 1000) { _weaponSwitching = false; }
            }

            if (_currentAmmoLevels[_currentWeaponIndex, 4] <= _currentAmmoLevels[_currentWeaponIndex, 5])
            {
                _currentAmmoLevels[_currentWeaponIndex, 4] += gameTime.ElapsedGameTime.Milliseconds;
                if (_currentAmmoLevels[_currentWeaponIndex, 4] > _currentAmmoLevels[_currentWeaponIndex, 5])
                {
                    PrimalDevistation.Instance.Audio.play("RELOAD_GUN");
                }
            }

            if (_currentAmmoLevels[_currentWeaponIndex, 2] <= _currentAmmoLevels[_currentWeaponIndex, 3])
            {
                _currentAmmoLevels[_currentWeaponIndex, 2] += gameTime.ElapsedGameTime.Milliseconds;
                if (_currentAmmoLevels[_currentWeaponIndex, 2] > _currentAmmoLevels[_currentWeaponIndex, 3])
                {
                    cWeapon wep = PrimalDevistation.Instance.Weapons.WeaponBlueprints[_currentWeaponIndex];
                    if (wep.Name == "shotgun")
                    {
                        PrimalDevistation.Instance.Audio.play("RELOAD_SHOTGUN");
                    }
                }
            }
            
            //if (_reticuleAngle != _moveToReticuleAngle) 
            //{
            //    float inc = _reticuleRotateRate;
            //    if (_reticuleAngle > _moveToReticuleAngle) { inc = -inc; }
            //    //if (_moveToReticuleAngle > Math.PI / 2 && _reticuleAngle< -Math.PI/2) { inc = -inc; }
            //    _reticuleAngle += inc;
            //    if (inc < 0 && _reticuleAngle < _moveToReticuleAngle) { _reticuleAngle = _moveToReticuleAngle; }
            //    else if (inc > 0 && _reticuleAngle > _moveToReticuleAngle) { _reticuleAngle = _moveToReticuleAngle; }
            //} 
             
           

            //_sprite.Update(gameTime);
            UpdateInput(gameTime);
            this.UpdateForces(forces);
            this.UpdatePhysics(gameTime, null);
            _velocity = _velocity * 0.99f;
            _rotation += (_velocity.X/16);
            if (_walkDir != 0) { _rotation += _walkDir/20;  }

            if (_bubbles) { PrimalDevistation.Instance.AddForce(Position, -1, (_bubble.Width / 2)+10 ); }
                
        }



        public void UpdateInput(GameTime gameTime)
        {
            GamePadState gps = GamePad.GetState(_playerIndex, GamePadDeadZone.Circular);
            KeyboardState ks = Keyboard.GetState();

            _walkDir = 0; 

            //if (_playerIndex == PlayerIndex.One)
            //{
                //if (gps.ThumbSticks.Left.X == 0){ _sprite.Play("stand"); }
                //if (gps.ThumbSticks.Left.X != 0 && _lastGPS.ThumbSticks.Left.X == 0) { _sprite.Play("walk"); }
                if (gps.ThumbSticks.Left.X < 0) { _spriteEffects = SpriteEffects.FlipHorizontally; }
                if (gps.ThumbSticks.Left.X > 0) { _spriteEffects = SpriteEffects.None; }

                if (gps.ThumbSticks.Left.X != 0) { Walk(gps.ThumbSticks.Left.X); }
               
                if (gps.ThumbSticks.Left.Length()>0.3f) 
                {
                    Vector2 v = new Vector2(gps.ThumbSticks.Left.X, -gps.ThumbSticks.Left.Y);
                    _reticuleAngle = Math.Atan2(v.Y, v.X);                
                }

                if (gps.ThumbSticks.Right.Length() > 0.3f)
                {
                    Vector2 v = new Vector2(gps.ThumbSticks.Right.X, -gps.ThumbSticks.Right.Y);
                    _reticuleAngle = Math.Atan2(v.Y, v.X);
                }


                if (gps.Buttons.RightShoulder == ButtonState.Pressed && _lastGPS.Buttons.RightShoulder == ButtonState.Released) { WepSwitch(1); }
                if (gps.Buttons.LeftShoulder == ButtonState.Pressed && _lastGPS.Buttons.LeftShoulder == ButtonState.Released) { WepSwitch(-1); }

                if (_currentAmmoLevels[_currentWeaponIndex, 4] < _currentAmmoLevels[_currentWeaponIndex, 5])
                {
                    if (gps.Triggers.Right == 0 && _lastGPS.Triggers.Right != 0)
                    {
                        PrimalDevistation.Instance.Audio.play("DRYFIRE");
                    }
                }



                if (PrimalDevistation.Instance.Weapons.WeaponBlueprints[_currentWeaponIndex].Name == "machinegun")
                {
                    if (gps.Triggers.Right != 0)
                    {
                        Fire(false, 1);
                    }
                }
                else
                {
                    _firePointRight = Math.Max(gps.Triggers.Right, _firePointRight);
                    if (gps.Triggers.Right == 0 && _lastGPS.Triggers.Right != 0) { Fire(false, _firePointRight); _firePointRight = 0; }
                }



                //_firePointLeft = Math.Max(gps.Triggers.Left, _firePointLeft);
                //if (gps.Triggers.Left == 0 && _lastGPS.Triggers.Left != 0) { Fire(true, _firePointLeft); _firePointLeft = 0; }

                if (_lastGPS.Triggers.Left == 0 && gps.Triggers.Left!=0) { Fire(true, 1); }

            


                if (gps.Buttons.A == ButtonState.Pressed && _lastGPS.Buttons.A == ButtonState.Released)
                {
                    if (_dJump || _onGround)
                    {
                        if (_spriteEffects == SpriteEffects.FlipHorizontally)
                        {
                            _velocity += new Vector2(_walkDir, -_playerJumpSpeed);
                        }
                        else
                        {
                            _velocity += new Vector2(_walkDir, -_playerJumpSpeed);
                        }
                        _onGround = false;
                        _dJump = !_dJump;
                    }
                }
            #region keyboardControls
                //}
            //else
            //{
            //    if (ks.IsKeyUp(Keys.A) && ks.IsKeyUp(Keys.D)) { _sprite.Play("stand"); }
            //    if (ks.IsKeyDown(Keys.A) && _lastKS.IsKeyUp(Keys.A)) { _sprite.Play("walk"); _spriteEffects = SpriteEffects.FlipHorizontally; }
            //    if (ks.IsKeyDown(Keys.D) && _lastKS.IsKeyUp(Keys.D)) { _sprite.Play("walk"); _spriteEffects = SpriteEffects.None; }


            //    if (ks.IsKeyDown(Keys.W))
            //    {
            //        _crossHairAngle += 0.05f;
            //        _crosshairVec = new Vector2((float)Math.Sin(_crossHairAngle), (float)Math.Cos(_crossHairAngle));
            //        _crosshairVec.Normalize();
            //    }

            //    if (ks.IsKeyDown(Keys.S))
            //    {
            //        _crossHairAngle -= 0.05f;
            //        _crosshairVec = new Vector2((float)Math.Sin(_crossHairAngle), (float)Math.Cos(_crossHairAngle));
            //        _crosshairVec.Normalize();
            //    }

            //    if (ks.IsKeyDown(Keys.OemPeriod) && _lastKS.IsKeyUp(Keys.OemPeriod)) { WepSwitch(-1); }
            //    if (ks.IsKeyDown(Keys.OemQuestion) && _lastKS.IsKeyUp(Keys.OemQuestion)) { WepSwitch(1); }

            //    if (ks.IsKeyDown(Keys.A))
            //    {
            //        Walk(-_maxWalkSpeed);
            //    }

            //    if (ks.IsKeyDown(Keys.D))
            //    {
            //        Walk(_maxWalkSpeed);                   
            //    }

            //    if (ks.IsKeyDown(Keys.RightShift))
            //    {
            //        _firePointRight += 0.1f;
            //    }
            //    else if (_lastKS.IsKeyDown(Keys.RightShift))
            //    {
            //        Fire(false, _firePointRight); _firePointRight = 0;
            //    }

            //    if (ks.IsKeyDown(Keys.RightAlt))
            //    {
            //        _firePointLeft += 0.1f;
            //    }
            //    else if (_lastKS.IsKeyDown(Keys.RightAlt))
            //    {
            //        Fire(true, _firePointLeft); _firePointLeft = 0;
            //    }

            //    if (ks.IsKeyDown(Keys.RightControl) && _lastKS.IsKeyUp(Keys.RightControl))
            //    {
            //        _walkDir = 0;
            //        if (_onGround)
            //        {
            //            if (_spriteEffects == SpriteEffects.FlipHorizontally)
            //            {
            //                _velocity += new Vector2(-_playerJumpSpeed / 2f, -_playerJumpSpeed);
            //            }
            //            else
            //            {
            //                _velocity += new Vector2(_playerJumpSpeed / 2f, -_playerJumpSpeed);
            //            }
            //            _onGround = false;
            //        }
            //    }     
                //}
                #endregion

            _lastKS = ks;
            _lastGPS = gps;
        }
  
        public void Walk(float dir)
        {
            if (_onGround)
            {                
                _walkDir = dir * _maxWalkSpeed;
            }
            else
            {
                _velocity.X += dir * _maxAirControl;
            }
        }

        public void WepSwitch(int dir)
        {
            _weaponSwitchTimer = 0;
            _weaponSwitching = true;
            _currentWeaponIndex += dir;
            if (_currentWeaponIndex < 0) { _currentWeaponIndex = PrimalDevistation.Instance.Weapons.WeaponBlueprints.Count - 1; }
            if (_currentWeaponIndex > PrimalDevistation.Instance.Weapons.WeaponBlueprints.Count - 1) { _currentWeaponIndex = 0; }
        }

        public void Fire(bool grapple, float velocity)
        {
            cWeapon wep;
            if (grapple) 
            {
                if (_currentGrapple == null)
                {
                    Vector2 v = new Vector2((float)Math.Cos(_reticuleAngle), (float)Math.Sin(_reticuleAngle));
                    _currentGrapple = PrimalDevistation.Instance.Weapons.SpawnGrapple(this, _position + (v * 30), v * (16 * velocity));
                }
                else
                {
                    PrimalDevistation.Instance.Weapons.ReleaseGrapple(_currentGrapple);
                    _currentGrapple = null;
                }
            }
            else 
            {
                if (_currentAmmoLevels[_currentWeaponIndex, 2] <= _currentAmmoLevels[_currentWeaponIndex, 3]) { return; }

                if (_currentAmmoLevels[_currentWeaponIndex, 4] >= _currentAmmoLevels[_currentWeaponIndex, 5])
                {
                    _currentAmmoLevels[_currentWeaponIndex, 2] = 0;
                    _currentAmmoLevels[_currentWeaponIndex, 0]--;
                    if (_currentAmmoLevels[_currentWeaponIndex, 0] <= 0) 
                    {                  
                        _currentAmmoLevels[_currentWeaponIndex, 4] = 0; 
                        _currentAmmoLevels[_currentWeaponIndex, 0] = _currentAmmoLevels[_currentWeaponIndex, 1]; 
                    }
                    wep = PrimalDevistation.Instance.Weapons.WeaponBlueprints[_currentWeaponIndex];

                    Vector2 v = new Vector2((float)Math.Cos(_reticuleAngle), (float)Math.Sin(_reticuleAngle));
                    PrimalDevistation.Instance.Weapons.SpawnWeapon(wep, _position + (v * 30), v * (8 * velocity) + Velocity, this);


                    cParticle p = new cParticle();
                    p.Texture = _muzzleFlash;
                    p.MaxAge = 100;
                    p.Position = _position + (v * 30);
                    p.StartColor = Color.White;
                    p.EndColor = Color.TransparentWhite;
                    PrimalDevistation.Instance.ParticleEffectSystem.Particles.Add(p);
                }
            }
            
        }

        public void Draw(SpriteBatch batch)
        {
            //Texture2D frame = _sprite.CurrentFrame;
            Texture2D frame = _charBase;
            if (frame != null)
            {
                // Draw the player
                cCamera2D cam = PrimalDevistation.Instance.Camera;
                Vector2 v = cam.GetScreenPos(ref _position);
                //batch.Draw(frame, v, null, Color.White, _rotation, _origin, cam.Zoom, _spriteEffects, 0);
                batch.Draw(frame, v, null, Color.White, _rotation, _origin, cam.Zoom, SpriteEffects.None, 0);
                Vector2 orig = new Vector2((_charTurret.Width / 2f)-1, (_charTurret.Height/2f)+8);

                batch.Draw(_charTurret, v, null, Color.White, (float)(_reticuleAngle+Math.PI/2), orig, cam.Zoom, SpriteEffects.None, 0);

                if (_bubbles)
                {
                    orig = new Vector2((_bubble.Width / 2f), (_bubble.Height / 2f));
                    batch.Draw(_bubble, v, null, new Color(255, 255, 255, 100), 0, orig, cam.Zoom, SpriteEffects.None, 0);
                }
                
            }
        }

        public void RenderUI(SpriteBatch batch)
        {
            Texture2D frame = _charBase;
            cCamera2D cam = PrimalDevistation.Instance.Camera;
            Vector2 v = cam.GetScreenPos(ref _position);

            // Draw health bar
            float ratio = (float)_healthBarWidth / _maxHitPoints;
            int len = (int)(ratio * _hitPoints);                          
            batch.Draw(_flatTex, new Rectangle((int)v.X - (len / 2), (int)v.Y - frame.Height - 10, len, 6), _primaryColor);

            // Draw Ammo Bar
            if (_currentAmmoLevels[_currentWeaponIndex, 4] <= _currentAmmoLevels[_currentWeaponIndex, 5])
            {
                ratio = (float)_healthBarWidth / _currentAmmoLevels[_currentWeaponIndex, 5];
                len = (int)(ratio * _currentAmmoLevels[_currentWeaponIndex, 4]);
            }
            else
            {
                ratio = (float)_healthBarWidth / _currentAmmoLevels[_currentWeaponIndex, 1];
                len = (int)(ratio * _currentAmmoLevels[_currentWeaponIndex, 0]);
            }

            batch.Draw(_flatTex, new Rectangle((int)v.X - (_healthBarWidth / 2), (int)v.Y - frame.Height - 18, _healthBarWidth, 6), new Color(255, 255, 0, 128));
            batch.Draw(_flatTex, new Rectangle((int)v.X - (len / 2), (int)v.Y - frame.Height - 18, len, 6), new Color(255, 255, 0, 255));

            // Draw crosshair
            v = new Vector2(_position.X + ((float)Math.Cos(_reticuleAngle) * 60) - (_origin.X / 2f), _position.Y + ((float)Math.Sin(_reticuleAngle) * 60) - (_origin.Y / 2f));
            v = cam.GetScreenPos(ref v);
            batch.Draw(_crosshair, v, null, Color.White, 0, new Vector2(-_crosshair.Width / 2, -_crosshair.Height / 2), cam.Zoom, SpriteEffects.None, 0);


            // Draw the current weapon
            if (_weaponSwitching)
            {
                Texture2D wep = PrimalDevistation.Instance.Weapons.WeaponBlueprints[_currentWeaponIndex].Texture;
                v = new Vector2(_position.X - (wep.Width / 2f), _position.Y - frame.Height - 20 - wep.Height);
                batch.Draw(wep, cam.GetScreenPos(ref v), null, new Color(255, 255, 255, 255), 0, Vector2.Zero, 1.5f, SpriteEffects.None, 0);
            }
        }
    }
}
