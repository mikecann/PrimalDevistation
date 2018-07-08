using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using PrimalDevistation.Level;
using Microsoft.Xna.Framework.Graphics;
using PrimalDevistation.Player;

namespace PrimalDevistation.Objects
{
    public class cPhysicsObject : cMapObject
    {
        protected Vector2 _velocity;
        protected float _friction;
        protected bool _gravityAffected;
        protected Rectangle _collisionBounds;
        protected bool _boundsCollision;
        protected bool _onGround;
        protected bool _canWalk;
        protected float _walkDir;
        protected bool _stickOnCollision;
        protected bool _stuck;
        protected bool _collideWithPlayers;

        public bool CollideWithPlayers
        {
            get { return _collideWithPlayers; }
            set { _collideWithPlayers = value; }
        }
        
        public bool OnGround
        {
            get { return _onGround; }
            set { _onGround = value; }
        }    
	
        public Rectangle CollisionBounds
        {
            get { return _collisionBounds; }
            set { _collisionBounds = value; }
        }	

        public bool GravityAffected
        {
            get { return _gravityAffected; }
            set { _gravityAffected = value; }
        }
	
        public float Friction
        {
            get { return _friction; }
            set { _friction = value; }
        }	

        public Vector2 Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }	

        public cPhysicsObject()
        {
            _friction = 1f;
            _gravityAffected = true;
            _boundsCollision = false;
            _canWalk = false;
            _stickOnCollision = false;
            _collideWithPlayers = false;
        }

        protected bool Chk(int x, int y, byte[,] ppData)
        {
            //if (ppData == null) 
            //{
                if (_boundsCollision) { return PrimalDevistation.Instance.CurrentLevel.CollisionMap.CheckCollision(x, y, _collisionBounds); }
                else { return PrimalDevistation.Instance.CurrentLevel.CollisionMap.CheckCollision(x, y);}
            //}
            //else 
            //{
                //return RampantRollers.Instance.CurrentLevel.CollisionMap.CheckCollisionDestructable(x - (int)_origin.X, y - (int)_origin.Y, ppData, _spriteEffects); 
            //}
        }

        public void UpdateForces(Vector4[] forces)
        {
            if (forces == null) { return; }
            for (int i = 0; i < forces.Length; i++)
            {
                if (forces[i].Z != 0)
                {
                    Vector2 diff = new Vector2(_position.X - forces[i].X, _position.Y - forces[i].Y);
                    float len = diff.LengthSquared();
                    float absForce = Math.Abs(forces[i].Z);
                    float reachSqr = forces[i].W * forces[i].W;
                    if (len < 10) { len = 10; }

                    if (len < reachSqr)
                    {
                        diff.Normalize();

                        _velocity.X -= diff.X * forces[i].Z;
                        _velocity.Y -= diff.Y * forces[i].Z;
                        _onGround = false;
                    }
                }
            }
        }

        public void UpdatePhysics(GameTime gameTime, byte[,] pixelPerfectCollisionData)
        {
            // If this object sticks on collision and is still stuck then dont bother with the rest of this
            if (_stickOnCollision && Chk((int)_position.X, (int)_position.Y, pixelPerfectCollisionData)) { _velocity = Vector2.Zero; return; }

            float frameDelta = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            cCollisionMap cm = PrimalDevistation.Instance.CurrentLevel.CollisionMap;
         
            // Work out new velocity 
            if (_gravityAffected) { _velocity += PrimalDevistation.GRAVITY; }

            // First get the ineger (pixel locations) of the old and new positions
            float fpNewX = (_position.X + _velocity.X);
            if (_walkDir != 0) { fpNewX = _position.X + _walkDir; }
            float fpNewY = (_position.Y + _velocity.Y);
            float fpOldX = _position.X;
            float fpOldY = _position.Y;
            int oldX = (int)fpOldX;
            int oldY = (int)fpOldY;
            int newX = (int)fpNewX;
            int newY = (int)fpNewY;
            bool collision = false;

            if (_canWalk && _onGround)
            {
                if (oldX != newX)
                {      
                    for (int i = -4; i < 4; i++)
                    {
                        if (!Chk(newX, oldY - i, pixelPerfectCollisionData))
                        {                   
                            fpNewY = fpOldY - i;
                            _velocity = Vector2.Zero;
                            collision = true;                   
                            break;
                        }
                    }

                    if (!collision) { fpNewY = fpOldY; fpNewX = fpOldX; }
                }
                else
                {
                    _velocity = Vector2.Zero;   
                    fpNewY = fpOldY;
                }                
            }
            else
            {
                // If there is a collision at the new location we need to split the axis to see where 
                if (Chk(newX, newY, pixelPerfectCollisionData))
                {
                    collision = true;
                    if (_stickOnCollision)
                    {
                        fpNewX = newX;
                        fpNewY = newY;
                    }
                    else
                    {
                        // If we collide at this new X position we need to rebound
                        if (Chk(newX, oldY, pixelPerfectCollisionData))
                        {                            
                            _velocity.X = -_velocity.X * _friction;
                            fpNewX = fpOldX +_velocity.X;
                        }

                        // If we collide at this new Y position we need to rebound
                        if (Chk(oldX, newY, pixelPerfectCollisionData))
                        {                            
                            _velocity.Y = -_velocity.Y * _friction;
                            fpNewY = fpOldY +_velocity.Y;                            
                        }
                    }
                    
                }
            }
                       
            // Finally set the new position
            _position.X = fpNewX;
            _position.Y = fpNewY;
            Position = _position;

            if (Chk((int)_position.X, (int)_position.Y + 1, pixelPerfectCollisionData) && Math.Abs(_velocity.X) < 3f ){ _onGround = true; }
            else { _onGround = false; }

            if (_collideWithPlayers)
            {
                List<cPlayer> activePlayer = PrimalDevistation.Instance.PlayerManager.ActivePlayers;

                for (int i = 0; i < activePlayer.Count; i++)
                {
                    cPlayer p = activePlayer[i];
                    if (p.CollisionBounds.Contains((int)(_position.X-p.Position.X), (int)(_position.Y-p.Position.Y)))
                    {
                        collision = true;
                    }
                    
                }
            }

            if (collision) { OnCollision(); }
        }

        protected virtual void OnCollision() { }
        

    }
}
