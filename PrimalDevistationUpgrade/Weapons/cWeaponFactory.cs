using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using PrimalDevistation.Weapons.Commands;
using PrimalDevistation.Weapons.Events;
using PrimalDevistation.Player;
using PrimalDevistation.Usefuls;

namespace PrimalDevistation.Weapons
{
    public class cWeaponFactory : DrawableGameComponent
    {
        private List<cWeapon> _blueprints;
        private List<cWeapon> _activeWeapons;
        private cGrapple _grappleBP;
        private Vector4[] _forces;
        private int _nextForce;

        public cGrapple GrappleBP
        {
            get { return _grappleBP; }
            set { _grappleBP = value; }
        }	

        public List<cWeapon> WeaponBlueprints
        {
            get { return _blueprints; }
            set { _blueprints = value; }
        }

        public List<cWeapon> ActiveWeapons
        {
            get { return _activeWeapons; }
            set { _activeWeapons = value; }
        }
	
        public cWeaponFactory(Game game) : base(game)
        {
            _blueprints = new List<cWeapon>();
            _activeWeapons = new List<cWeapon>();
            _forces = new Vector4[4];
            for (int i = 0; i < _forces.Length; i++) { _forces[i] = Vector4.Zero; }
            _nextForce = 0;
        }

        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
                DirectoryInfo di = new DirectoryInfo(@"Resources/Weapons/");
                FileInfo[] files = di.GetFiles();
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Extension == @".xml")
                    {
                        LoadWeapon(files[i].Name);
                    }
                }
            }
        }       

        public void LoadWeapon(string name)
        {
            GraphicsDevice gd = GraphicsDevice;
            ContentManager cm = PrimalDevistation.Instance.CM;

            // Load the XML doc
            XmlDocument doc = new XmlDocument();
            doc.Load(@"Resources/Weapons/" + name);  
    
            // Create the weapon and set its name      
            XmlNode node;     
            XmlNode weaponNode = doc.SelectSingleNode("weapon");
            string wepName = weaponNode.Attributes["name"].Value.ToLower();
            string type = weaponNode.Attributes["type"].Value.ToLower();

            XmlNode properties = weaponNode.SelectSingleNode("properties");
            if (properties == null) { throw new Exception("There needs to be atleast some properties for the '" + name + "' weapon"); }

            cWeapon wep;
            if (type == "projectile") { wep = new cProjectile(properties); }
            else if (type == "grenade") { wep = new cGrenade(properties); }
            else if (type == "vortex") { wep = new cVortex(properties); }
            else if (type == "grapple") { _grappleBP = new cGrapple(properties); wep = _grappleBP; }
            else { throw new Exception("Weapon type not valid"); }

            wep.Name = wepName;
            wep.Type = type;
            if (!(wep is cGrapple)) {  _blueprints.Add(wep); }

            // Load the various properties
            node = properties.SelectSingleNode("texture");
            if (node!=null) { wep.Texture = cm.Load<Texture2D>(@"Weapons/" + node.InnerText); }
            node = properties.SelectSingleNode("trail");
            if (node != null) { wep.TrailTexture = cm.Load<Texture2D>(@"Weapons/" + node.InnerText); }
            node = properties.SelectSingleNode("friction");
            if (node != null) { wep.Friction = float.Parse(node.InnerText); }
            node = properties.SelectSingleNode("maxAmmo");
            if (node != null) { wep.MaxAmmo = int.Parse(node.InnerText); }
            node = properties.SelectSingleNode("reloadTime");
            if (node != null) { wep.ReloadTime = int.Parse(node.InnerText); }
            node = properties.SelectSingleNode("rateOfFire");
            if (node != null) { wep.RateOfFire = int.Parse(node.InnerText); }
            node = properties.SelectSingleNode("launchPower");
            if (node != null) { wep.LaunchPower = float.Parse(node.InnerText); }
            node = properties.SelectSingleNode("numToFire");
            if (node != null) { wep.NumToFire = int.Parse(node.InnerText); }
            node = properties.SelectSingleNode("fireRangeAngle");
            if (node != null) { wep.FireRangeAngle = float.Parse(node.InnerText); }


            // Load the events and thier commands (if there are any)
            XmlNode eventsNode = weaponNode.SelectSingleNode("events");
            if (eventsNode != null)
            {    
                XmlNodeList events;

                // Load the on collision events
                events = eventsNode.SelectNodes("onTerrainCollide");
                foreach (XmlNode eventNode in events) { wep.OnCollisionEvents.Add(new cOnCollisionEvent(eventNode)); }  

                // Load the on age events
                events = eventsNode.SelectNodes("onAge");
                foreach (XmlNode eventNode in events) { wep.OnAgeEvents.Add(new cOnAgeEvent(eventNode)); }                
            }

        }

        public cWeapon GetWepByName(string name)
        {
            for (int i = 0; i < _blueprints.Count; i++) { if (_blueprints[i].Name == name) { return _blueprints[i]; } }
            return null;
        }

        public void SpawnWeapon(string name, Vector2 where, Vector2 vel, cPlayer owner)
        {
            cWeapon bp = GetWepByName(name);
            SpawnWeapon(bp, where, vel, owner);
        }

        public void SpawnWeapon(cWeapon blueprint, Vector2 where, Vector2 vel, cPlayer owner)
        {
            if (blueprint.Name == "shotgun")
            {
                Vector2 normVel = new Vector2(vel.X, vel.Y);
                normVel.Normalize();
                float angle = (float)Math.Atan2(normVel.Y, normVel.X);
                float ang;    
                float len = vel.Length();

                for (int i = 0; i < blueprint.NumToFire; i++)
                {
                    ang = cMath.Rand(angle - blueprint.FireRangeAngle / 2f, angle + blueprint.FireRangeAngle / 2f);
                    normVel.Y = (float)Math.Sin(ang);
                    normVel.X = (float)Math.Cos(ang);
                    normVel *= cMath.Rand(len * 0.8f, len);
                    cWeapon instance = blueprint.Instantiate();
                    instance.Position = where;
                    instance.Velocity = normVel;
                    instance.Owner = owner;
                    _activeWeapons.Add(instance);
                }
            }
            else
            {
                cWeapon instance = blueprint.Instantiate();
                instance.Position = where;
                instance.Velocity = vel * instance.LaunchPower;
                instance.Owner = owner;
                _activeWeapons.Add(instance);
            }

            if (blueprint.Name == "machinegun") { PrimalDevistation.Instance.Audio.play("GUN_SINGLE1"); }
            if (blueprint.Name == "grenade") { PrimalDevistation.Instance.Audio.play("GRENADE_LAUNCH"); }
            if (blueprint.Name == "rocket") { PrimalDevistation.Instance.Audio.play("rocketLaunch"); }
            if (blueprint.Name == "shotgun") { PrimalDevistation.Instance.Audio.play("SHOTGUN_SINGLE"); }
        }

        public cGrapple SpawnGrapple(cPlayer attTo, Vector2 where, Vector2 vel)
        {
            cGrapple instance = _grappleBP.Instantiate(attTo);
            instance.Position = where;
            instance.Velocity = vel;
            _activeWeapons.Add(instance);
            //RampantRollers.Instance.Audio.play("GRPPLER_EXTEND_ONESHOT");
            return instance;
        }

        public override void Update(GameTime gameTime)
        {         
            for (int i = 0; i < _activeWeapons.Count; i++)
            {
                _activeWeapons[i].Update(gameTime, _forces);     
            }

            for (int i = 0; i < _activeWeapons.Count; i++)
            {
                if (_activeWeapons[i].KillFlag) { _activeWeapons.RemoveAt(i); i--; }
            }

            for (int i = 0; i < _forces.Length; i++) { _forces[i].Z = 0; }
            _nextForce = 0;
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch batch = PrimalDevistation.Instance.Batch;
            batch.Begin();          
            for (int i = 0; i < _activeWeapons.Count; i++)
            {
                _activeWeapons[i].Draw(batch);           
            }
            batch.End();
        }
        
        public void ReleaseGrapple(cGrapple grapple)
        {
            _activeWeapons.Remove(grapple);
            grapple = null;
        }

        public void AddForce(Vector2 where, float strength, float holeSize)
        {
            _forces[_nextForce].X = where.X;
            _forces[_nextForce].Y = where.Y;
            _forces[_nextForce].Z = strength;
            _forces[_nextForce].W = holeSize;
            _nextForce++;
            if (_nextForce >= _forces.Length) { _nextForce = 0; }
        }
    }
}
