using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml;
using Microsoft.Xna.Framework.Audio;

namespace PrimalDevistation.Weapons
{
    public class cVortex : cWeapon
    {
        private float _strength;
        private float _airResistance;
        private float _reach;
        private Cue _sound;

        public float Reach
        {
            get { return _reach; }
            set { _reach = value; }
        }	

        public float AirResistance
        {
            get { return _airResistance; }
            set { _airResistance = value; }
        }
	
        public float Strength
        {
            get { return _strength; }
            set { _strength = value; }
        }

        public cVortex() : base()            
        {
            _strength = 1;
            GravityAffected = false;
            _airResistance = 1;
        }

        public cVortex(XmlNode properties) : this()            
        {            
            XmlNode node = properties.SelectSingleNode("strength");
            if (node != null) { _strength = float.Parse(node.InnerText); }
            node = properties.SelectSingleNode("airResistance");
            if (node != null) { _airResistance = float.Parse(node.InnerText); }
            node = properties.SelectSingleNode("reach");
            if (node != null) { _reach = float.Parse(node.InnerText); }
        }

        public override void Update(GameTime gameTime, Vector4[] forces)
        {
            if (_sound == null) { _sound = PrimalDevistation.Instance.Audio.play("vortexLoop"); }
           
            base.Update(gameTime, null);
            _velocity *= _airResistance;
            _rotation += 0.4f;
            if (this.KillFlag) { _strength = -_strength; _sound.Stop(AudioStopOptions.Immediate); }
            PrimalDevistation.Instance.AddForce(Position-Origin,_strength,_reach);
        }

        public override cWeapon Instantiate()
        {
            cVortex wep = new cVortex();
            base.SetProps(wep);
            wep.Strength = _strength;
            wep.AirResistance = _airResistance;
            wep.Reach = _reach;
            return wep;
        }
    }
}
