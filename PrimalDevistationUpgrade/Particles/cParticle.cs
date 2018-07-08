using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrimalDevistation.Particles
{
    public class cParticle
    {
        private bool _killFlag;
        private Texture2D _texture;
        private Vector2 _position;
        private Vector2 _velocity;
        private int _age;
        private int _maxAge;
        private Rectangle _sourceRect;        
        private Color _startCol;
        private Color _endCol;
        private Vector2 _origin;

        public Color EndColor
        {
            get { return _endCol; }
            set { _endCol = value; }
        }        

        public Color StartColor
        {
            get { return _startCol; }
            set { _startCol = value; }
        }	

        public Rectangle SourceRect
        {
            get { return _sourceRect; }
            set { _sourceRect = value; }
        }
        
        public int MaxAge
        {
            get { return _maxAge; }
            set { _maxAge = value; }
        }        

        public int Age
        {
            get { return _age; }
            set { _age = value; }
        }	

        public Vector2 Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }	

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }	

        public Texture2D Texture
        {
            get { return _texture; }
            set { _texture = value; _origin = new Vector2(_texture.Width / 2, _texture.Height / 2); }
        }	

        public bool KillFlag
        {
            get { return _killFlag; }
            set { _killFlag = value; }
        }

        public cParticle()
        {
            _age = 0;
            _maxAge = 1000;          
        }

        public virtual void Update(GameTime gameTime)
        {
            _position.X += _velocity.X;
            _position.Y += _velocity.Y;
            _age += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_age > _maxAge) { _killFlag = true; }  
        }

        public virtual void Draw(SpriteBatch batch)
        {          
            float s = (float)_age / (float)_maxAge;
            Color c = new Color((byte)MathHelper.Lerp(_startCol.R, _endCol.R, s), (byte)MathHelper.Lerp(_startCol.B, _endCol.B, s), (byte)MathHelper.Lerp(_startCol.G, _endCol.G, s), (byte)MathHelper.Lerp(_startCol.A, _endCol.A, s));
            cCamera2D cam = PrimalDevistation.Instance.Camera;
            batch.Draw(_texture, cam.GetScreenPos(ref _position), null, c, 0, _origin, cam.Zoom, SpriteEffects.None, 0);               
        
        }
    }
}
