using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml;
using PrimalDevistation.Player;
using Microsoft.Xna.Framework.Graphics;
using SpritesAndLines;
using PrimalDevistation.Usefuls;
using Microsoft.Xna.Framework.Audio;

namespace PrimalDevistation.Weapons
{
    public class cGrapple : cProjectile
    {
        private bool _grappleStuck;
        private cPlayer _attTo;
        private Texture2D _ropeTex;
        private Line _rope;
        private const float _ropeIdealLength = 100;
        private const float _ropeStrength = 0.4f;
        private List<Line> _lines;
        private Cue _grappleShootCue;

        public cGrapple(cPlayer attTo) : base()            
        {
            _grappleStuck = false;
            _attTo = attTo;
            _lines = new List<Line>();
            _stickOnCollision = true;            
        }

        public cGrapple(XmlNode properties)
            : this((cPlayer)null)            
        {            
            XmlNode node = properties.SelectSingleNode("ropeTexture");
            if (node != null) { _ropeTex = PrimalDevistation.Instance.CM.Load<Texture2D>(@"Sprites/"+node.InnerText); }
            //node = properties.SelectSingleNode("airResistance");
            //if (node != null) { _airResistance = float.Parse(node.InnerText); }
            //node = properties.SelectSingleNode("reach");
            //if (node != null) { _reach = float.Parse(node.InnerText); }            
        }
        
        public override void Update(GameTime gameTime, Vector4[] forces)
        {            
            base.Update(gameTime, forces);

            if (_grappleStuck)
            {
                Vector2 v = _attTo.Position - _position;                
                _rotation = (float)Math.Atan2(-v.Y, -v.X);

                float lsq = v.LengthSquared();

                if (lsq > _ropeIdealLength * _ropeIdealLength)
                {

                    cMath.newLength(ref v, _ropeIdealLength);

                    v += _position;

                    Vector2 v2 = v - _attTo.Position;

                    float lenSqr = v2.LengthSquared();
                    if (lenSqr > _ropeStrength * _ropeStrength)
                    {
                        v2.Normalize();
                        v2.X *= _ropeStrength;
                        v2.Y *= _ropeStrength;

                    }

                    _attTo.Velocity = _attTo.Velocity + v2;
                }

                
                _attTo.OnGround = false;

                //if (!LieroXNA.Instance.CurrentLevel.CollisionMap.CheckCollision((int)(_position.X), (int)(_position.Y)))
                //{
                //    _grappleStuck = false;
                //}
            }

            cCamera2D cam = PrimalDevistation.Instance.Camera;
            Vector2 vec = _attTo.Position; vec.Y -= 8;
            _rope = new Line(cam.GetScreenPos(ref vec), cam.GetScreenPos(ref _position));    
        }

        protected override void OnCollision()
        {            
            _grappleStuck = true;

            // Okay we just stuck in the ground so we now need to work back till we find a point thats on the border of stuck and not stuck
            Vector2 velNormal = new Vector2(_velocity.X, _velocity.Y);
            velNormal.Normalize();           

            int intMag = (int)_velocity.Length();

            int lastX = (int)_position.X;
            int lastY = (int)_position.Y;

            for (int i = -1; i < intMag-1; i++)
            {
                int nowX = (int)(_position.X - (velNormal.X * (i + 1)));
                int nowY = (int)(_position.Y - (velNormal.Y * (i + 1)));

                if (!Chk(nowX, nowY, null))
                {
                    _position.X = lastX;
                    _position.Y = lastY;

                    List<Vector2> positions = new List<Vector2>(8);
                    List<Vector2> velocities = new List<Vector2>(8);
                    List<Color> colors = new List<Color>(8);
                    Color c = PrimalDevistation.Instance.Level.CollisionMap.GetTerrainColorAt((int)_position.X, (int)_position.Y);
                    intMag = Math.Max(1, intMag / 2);
                    for (int j = 0; j < 8; j++)
                    {
                        positions.Add(new Vector2(_position.X, _position.Y));
                        velocities.Add(new Vector2(cMath.Rand(0, intMag) * -velNormal.X, cMath.Rand(0, intMag) * -velNormal.Y));
                        colors.Add(c);
                    }
                    PrimalDevistation.Instance.PhysicsParticles.SpawnParticles(positions, velocities, colors);
                    break;
                }

                lastX = nowX;
                lastY = nowY;
            }

            _velocity = Vector2.Zero;
            PrimalDevistation.Instance.Audio.play("GRPPLER_HIT");
            PrimalDevistation.Instance.Audio.play("DEBRIS_LIGHT");
            if (_grappleShootCue != null) { _grappleShootCue.Stop(AudioStopOptions.Immediate); }
        }

        public cGrapple Instantiate(cPlayer attTo)
        {
            cGrapple wep = new cGrapple(attTo);
            base.SetProps(wep);
            wep._ropeTex = _ropeTex;
            wep._grappleShootCue = PrimalDevistation.Instance.Audio.play("GRPPLER_EXTEND_ONESHOT");
            return wep;
        }

        public override void Draw(SpriteBatch batch)
        {

            cCamera2D cam = PrimalDevistation.Instance.Camera;
            
            if (_rope != null)
            {
                _lines.Clear();
                _lines.Add(_rope);
                PrimalDevistation.Instance.LineManager.AddLineDrawCall(_lines, cam.Zoom*2, Color.BurlyWood, "Glow");
            }

            //LieroXNA.Instance.LineManager.Draw(lines, 4, Color.BurlyWood, Matrix.Identity, cam.Projection, 1, "Glow");

            base.Draw(batch);             
        }
    }
}
