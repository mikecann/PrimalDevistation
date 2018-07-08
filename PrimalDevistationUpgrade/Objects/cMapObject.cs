using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using PrimalDevistation.Sprites;

namespace PrimalDevistation.Objects
{
    public class cMapObject
    {
        protected Vector2 _position;
        protected float _rotation;
        protected Vector2 _origin;
        protected Vector2 _scale;
        protected SpriteEffects _spriteEffects;

        public virtual Vector2 Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        public virtual Vector2 Origin
        {
            get { return _origin; }
            set { _origin = value; }
        }

        public virtual float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public virtual Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public cMapObject()
        {
            _position = Vector2.Zero;
            _origin = Vector2.Zero;
            _scale = Vector2.Zero;
            _rotation = 0;
        }    
    }
}
