using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrimalDevistation.Particles
{
    public class cParticleEffectSystem : DrawableGameComponent
    {
        private List<cParticle> _particles;

        public List<cParticle> Particles
        {
            get { return _particles; }
            set { _particles = value; }
        }	
        
        public cParticleEffectSystem(Game game) : base(game)
        {
            _particles = new List<cParticle>();
        }

        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
            }
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < _particles.Count; i++)
            {
                _particles[i].Update(gameTime);
                if (_particles[i].KillFlag) { _particles.RemoveAt(i); }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch batch = PrimalDevistation.Instance.Batch;
            batch.Begin(SpriteBlendMode.AlphaBlend);
            for (int i = 0; i < _particles.Count; i++) { _particles[i].Draw(batch); }
            batch.End();
        }

    }
}
