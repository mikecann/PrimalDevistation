using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace PrimalDevistation.Usefuls
{
    class cMath
    {
        private static Random _rand;

        public static Random Random
        {
            get { if (_rand == null) { _rand = new Random(); } return _rand; }
            set { _rand = value; }
        }

        public static float Rand(float min, float max)
        {
            float div = 100000;
            int imin = (int)(min*div);
            int imax = (int)(max*div);

            return Random.Next(imin, imax) / div;
        }

        public static float Rand(int min, int max)
        {           
            return Random.Next(min, max);
        }

        public static void newLength( ref Vector2 v, float len )
        {
            float l = v.Length();        	
	        v.X /= (l / len);
	        v.Y /= (l / len);   
        }		

        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        public static float ComputeGaussian(float n, float blurAmount)
        {
            return (float)((1.0 / Math.Sqrt(2 * Math.PI * blurAmount)) *
                           Math.Exp(-(n * n) / (2 * blurAmount * blurAmount)));
        }
    }
}
