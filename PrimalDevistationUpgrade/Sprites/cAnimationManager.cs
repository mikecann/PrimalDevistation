using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Xml;

namespace PrimalDevistation.Sprites
{
    public class cAnimationManager
    {
        private Dictionary<string,cAnimatedSprite> _sprites;

        public Dictionary<string,cAnimatedSprite> Sprites
        {
            get { return _sprites; }
            set { _sprites = value; }
        }

        public cAnimationManager()
        {
            _sprites = new Dictionary<string,cAnimatedSprite>();
        }

        public cAnimatedSprite addSprite(string name, string URL)
        {
            // Check to see if it already exists first
            foreach (string key in _sprites.Keys) { if (key == name) { return new cAnimatedSprite(_sprites[name]); } }

            // If not lets load the bugger
            cAnimatedSprite sprite = new cAnimatedSprite(URL);
            sprite.Name = name;
            _sprites.Add(name,sprite);
            return new cAnimatedSprite(sprite);
        }

      
    }
}
