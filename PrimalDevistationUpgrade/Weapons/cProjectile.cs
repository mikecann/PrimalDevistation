using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml;

namespace PrimalDevistation.Weapons
{
    public class cProjectile : cWeapon
    {

        public cProjectile() : base() { }

        public cProjectile(XmlNode properties)
            : this()
        {
        }
        
        public override void Update(GameTime gameTime, Vector4[] forces)
        {
            base.Update(gameTime, forces);
            _rotation = (float)Math.Atan2(_velocity.Y, _velocity.X);
        }

        public override cWeapon Instantiate()
        {
            cProjectile wep = new cProjectile();
            base.SetProps(wep);
            return wep;
        }
    }
}
