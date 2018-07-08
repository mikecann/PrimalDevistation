using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using PrimalDevistation.Objects;
using PrimalDevistation.Particles;
using PrimalDevistation.Usefuls;
using PrimalDevistation.Weapons.Events;
using SpritesAndLines;
using PrimalDevistation.Player;

namespace PrimalDevistation.Weapons
{   
    public abstract class cWeapon : cPhysicsObject
    {  
        private string _name;
        private Texture2D _texture; 
        private bool _killFlag;
        private Texture2D _trailTexture;
        private string _type;        
        private int _age;
        private List<cOnAgeEvent> _onAgeEvents;
        private List<cOnCollisionEvent> _onCollisionEvents;
        private cPlayer _owner;
        private int _rateOfFire;
        private int _reloadTime;
        private int _maxAmmo;
        private float _launchPower;
        private int _numToFire;
        private float _fireRangeAngle;

        public float FireRangeAngle
        {
            get { return _fireRangeAngle; }
            set { _fireRangeAngle = value; }
        }

        public int NumToFire
        {
            get { return _numToFire; }
            set { _numToFire = value; }
        }	

        public float LaunchPower
        {
            get { return _launchPower; }
            set { _launchPower = value; }
        }	

	    public int RateOfFire
	    {
		    get { return _rateOfFire;}
		    set { _rateOfFire = value;}
	    }             

	    public int ReloadTime
	    {
		    get { return _reloadTime;}
		    set { _reloadTime = value;}
	    }

	    public int MaxAmmo
	    {
		    get { return _maxAmmo;}
		    set { _maxAmmo = value;}
	    }	

        public cPlayer Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }	

        public List<cOnCollisionEvent> OnCollisionEvents
        {
            get { return _onCollisionEvents; }
            set { _onCollisionEvents = value; }
        }
        
        public List<cOnAgeEvent> OnAgeEvents
        {
            get { return _onAgeEvents; }
            set { _onAgeEvents = value; }
        }	

        public int Age
        {
            get { return _age; }
            set { _age = value; }
        }	      

        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }
	
        public Texture2D TrailTexture
        {
            get { return _trailTexture; }
            set { _trailTexture = value; }
        }	

        public bool KillFlag
        {
            get { return _killFlag; }
            set { _killFlag = value; }
        }	   

        public Texture2D Texture
        {
            get { return _texture; }
            set 
            {
                _texture = value;
                _origin = new Vector2(_texture.Width / 2, _texture.Height / 2);
            }
        }	

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public cWeapon()
        {
            _onCollisionEvents = new List<cOnCollisionEvent>();
            _onAgeEvents = new List<cOnAgeEvent>();
            _killFlag = false;
            this.CollideWithPlayers = true;
            _age = 0;
            _launchPower = 1;
        }

        public virtual cWeapon SetProps(cWeapon w)
        { 
            w.Name = _name;
            w.Texture = _texture;
            w.TrailTexture = _trailTexture;
            w.Type = _type;
            w.OnCollisionEvents = _onCollisionEvents;
            w.Friction = _friction;
            w.OnAgeEvents = _onAgeEvents;
            w.RateOfFire = _rateOfFire;
            w.MaxAmmo = _maxAmmo;
            w.ReloadTime = _reloadTime;
            w.LaunchPower = _launchPower;
            w.NumToFire = _numToFire;
            w.FireRangeAngle = _fireRangeAngle;
            return w;
        }

        public abstract cWeapon Instantiate();

        public virtual void Update(GameTime gameTime, Vector4[] forces)
        {
            _age += gameTime.ElapsedGameTime.Milliseconds;
            for (int i = 0; i < _onAgeEvents.Count; i++) { _onAgeEvents[i].Update(this, gameTime); }
            this.UpdateForces(forces);
            this.UpdatePhysics(gameTime,null);   
            if (_trailTexture != null) { updateTrail(gameTime); }
        }

        public void updateTrail(GameTime gameTime)
        {
            cParticle p = new cParticle();
            p.Texture = _trailTexture;
            Vector2 velTmp = new Vector2(_velocity.X, _velocity.Y);
            velTmp.Normalize();
            velTmp *= -_texture.Width;
            p.Position = new Vector2(_position.X + velTmp.X, _position.Y + velTmp.Y);
            p.Velocity = new Vector2(cMath.Rand(-0.3f, 0.3f), cMath.Rand(-0.3f, 0.3f));
            p.StartColor = Color.White;
            p.EndColor = Color.TransparentWhite;
            PrimalDevistation.Instance.ParticleEffectSystem.Particles.Add(p);
        }

        protected override void OnCollision()
        {
            for (int i = 0; i < _onCollisionEvents.Count; i++) { _onCollisionEvents[i].Collision(this); }
        }
        
        public virtual void Draw(SpriteBatch batch)
        {
            if (_texture != null)
            {
                cCamera2D cam = PrimalDevistation.Instance.Camera;              
                batch.Draw(_texture, cam.GetScreenPos(ref _position), null, Color.White, _rotation, _origin, cam.Zoom, SpriteEffects.None, 0);    
            }            
        }       
    }
}
