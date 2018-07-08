using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PrimalDevistation.Particles
{
    public interface iPhysicsParticleSystem
    {
        void SpawnParticles(List<Vector2> positions, List<Vector2> velocities, List<Color> colors);
        void AddForce(Vector2 where, float strength, float holeSize);
    }
}
