using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;
using PrimalDevistation.Level;
using Microsoft.Xna.Framework.Graphics;
using PrimalDevistation.Usefuls;

namespace PrimalDevistation.Weapons.Commands
{
    class cSpawnCommand : cCommand
    {
        private string _type;
        private string _where;
        private int _size;
        private string _velocity;
        private int _quantity;

        public int Quantity
        {
            get { return _quantity; }
            set { _quantity = value; }
        }	

        public string Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }        

        public int ExplosionSize
        {
            get { return _size; }
            set { _size = value; }
        }	

        public string Where
        {
            get { return _where; }
            set { _where = value; }
        }	

        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public cSpawnCommand(XmlNode properties)
        {
            if (properties.Attributes["type"]==null) { throw new Exception("The spawn command needs to know what to spawn!"); }
            _type = properties.Attributes["type"].Value;
            
            _size = 32;
            if (properties.Attributes["size"] != null) { _size = int.Parse(properties.Attributes["size"].Value); }

            _where = "this";
            if (properties.Attributes["where"] != null) { _where = properties.Attributes["where"].Value; }

            _velocity = "this";
            if (properties.Attributes["velocity"] != null) { _where = properties.Attributes["velocity"].Value; }
        
            _quantity = 1;
            if (properties.Attributes["quantity"] != null) { _quantity = int.Parse(properties.Attributes["quantity"].Value); }
        }

        public override void FireCommand(cWeapon owner)
        {
            cLevel lev = PrimalDevistation.Instance.Level;
            Vector2 where = Vector2.Zero;
            Vector2 vel = Vector2.Zero;
            if (_where == "this") { where.X = owner.Position.X; where.Y = owner.Position.Y; }
            if (_velocity == "this") { vel.X = owner.Velocity.X; vel.Y = owner.Velocity.Y; }
            if (_type == "explosion") { PrimalDevistation.Instance.Explode(_size, where, vel / 2f, owner.Owner, true); }
            else if (_type == "shrapnel") 
            {
                List<Vector2> positions = new List<Vector2>(_quantity);
                List<Vector2> velocities = new List<Vector2>(_quantity);
                List<Color> colors = new List<Color>(_quantity);
                Color c = new Color(200, 200, 200, 255);
                for (int i = 0; i < _quantity; i++)
                {
                    positions.Add(new Vector2(where.X+cMath.Rand(-4f, 4f), where.Y+cMath.Rand(-4f, 4f)));
                    velocities.Add(new Vector2(cMath.Rand(-1f, 1f) + vel.X / 10, cMath.Rand(-1f, 1f) + vel.Y/10));
                    colors.Add(c);
                }
                PrimalDevistation.Instance.PhysicsParticles.SpawnParticles(positions, velocities, colors);
            }

            
        }
    }
}
