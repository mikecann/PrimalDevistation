using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml;

namespace PrimalDevistation.Weapons
{
    public class cGrenade : cWeapon
    {

        public cGrenade() : base() { _stickOnCollision = true; }

        public cGrenade(XmlNode properties) : this()          
        {
            _stickOnCollision = true;
        }

        public override void Update(GameTime gameTime, Vector4[] forces)
        {
            base.Update(gameTime, forces);
            _rotation += _velocity.X / 20f;
        }

        protected override void OnCollision()
        {
            _velocity = Vector2.Zero;
            _position -= _velocity;
            PrimalDevistation.Instance.Audio.play("GRENADE_BOUNCE1");
        }

        public override cWeapon Instantiate()
        {
            cGrenade wep = new cGrenade();          
            base.SetProps(wep);
            return wep;
        }
    }
}
