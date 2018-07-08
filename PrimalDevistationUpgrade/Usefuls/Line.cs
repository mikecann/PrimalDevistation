// Line.cs
// Part of "Sprites and Lines" 1.0 - May 28, 2007
// Copyright 2007 Michael Anderson


#region Using Statements
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion


namespace SpritesAndLines
{
    /// <summary>
    /// Represents a single line segment.  Drawing is handled by the LineManager class.
    /// </summary>
    public class Line
    {
        public Vector2 p1; // Begin point of the line
        public Vector2 p2; // End point of the line
        public float radius = 0.1f; // The line's total thickness is twice its radius
        public Vector2 rhoTheta; // Length and angle of the line
        public float rho { get { return rhoTheta.X; } }
        public float theta { get { return rhoTheta.Y; } }
        public Color color = Color.White;
        

        public Line(Vector2 p1, Vector2 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
            Recalc();
        }


        public Line(Vector2 p1, Vector2 p2, float radius)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.radius = radius;
            Recalc();
        }


        public Line(Vector2 p1, Vector2 p2, float radius, Color color)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.radius = radius;
            this.color = color;
            Recalc();
        }


        public void Move(Vector2 p1, Vector2 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
            Recalc();
        }


        public void Recalc()
        {
            Vector2 delta = p2 - p1;
            float rho = delta.Length();
            float theta = (float)Math.Atan2(delta.Y, delta.X);
            rhoTheta = new Vector2(rho, theta);
        }


        /// <summary>
        /// Distance squared from an arbitrary point p to the "virtual" 
        /// version of this line, meaning assuming a thickness of 0.
        /// </summary>
        /// <remarks>
        /// See http://geometryalgorithms.com/Archive/algorithm_0102/algorithm_0102.htm, near the bottom.
        /// </remarks>
        public float DistanceSquaredPointToVirtualLine(Vector2 p, out Vector2 closestP)
        {
            Vector2 v = p2 - p1; // Vector from line's p1 to p2
            Vector2 w = p - p1; // Vector from line's p1 to p

            // See if p is closer to p1 than to the segment
            float c1 = Vector2.Dot(w, v);
            if (c1 <= 0)
            {
                closestP = p1;
                return Vector2.DistanceSquared(p, p1);
            }

            // See if p is closer to p2 than to the segment
            float c2 = Vector2.Dot(v, v);
            if (c2 <= c1)
            {
                closestP = p2;
                return Vector2.DistanceSquared(p, p2);
            }

            // p is closest to point pB, between p1 and p2
            float b = c1 / c2;
            Vector2 pB = p1 + b * v;
            closestP = pB;
            return Vector2.DistanceSquared(p, pB);
        }


        /// <summary>
        /// A variant of DistanceSquaredPointToVirtualLine that
        /// just returns the distance squared, not the closest point.
        /// </summary>
        public float DistanceSquaredPointToVirtualLine(Vector2 p)
        {
            Vector2 closestP;
            return DistanceSquaredPointToVirtualLine(p, out closestP);
        }


        /// <summary>
        // Given a point moving from point p1 to point p2, past point p3, find the two values of t
        // at which the point is distance dist from p3.  If the point never gets that close, 
        // return MaxFloat for the two t values.
        /// </summary>
        private static void FindRadialT(Vector2 p1, Vector2 p2, Vector2 p3, float dist, out float t1, out float t2)
        {
            // first, adjust things so p3 is at the origin
            p1 -= p3;
            p2 -= p3;

            float a = p1.X;
            float b = p2.X - p1.X;
            float c = p1.Y;
            float d = p2.Y - p1.Y;
            // x = a + bt
            // y = c + dt
            // x * x + y * y = dist * dist // pythagorean theorem
            // Substitute and solve for t using the quadratic formula
            // (a + bt) * (a + bt) + (c + dt) * (c + dt) = dist * dist
            // (a * a + 2 * a * b * t + bt * bt) + (c * c + 2 * c * d * t + dt * dt) = dist * dist
            // (a * a + c * c) + ((2 * a * b + 2 * c * d) * t) + ((b * b + d * d) * t * t) = dist * dist
            // (quadA * t * t) + (quadB * t) + quadC = 0
            float quadA = b * b + d * d;
            float quadB = 2 * a * b + 2 * c * d;
            float quadC = a * a + c * c - dist * dist;
            float discriminant = quadB * quadB - 4 * quadA * quadC; // "b^2-4ac"
            if (discriminant < 0)
            {
                // no real roots exist
                t1 = float.MaxValue;
                t2 = float.MaxValue;
            }
            else
            {
                float root = (float)Math.Sqrt(discriminant);
                t1 = (-quadB + root) / (2 * quadA);
                t2 = (-quadB - root) / (2 * quadA);

                // Ensure that t1 <= t2
                if (t2 < t1)
                {
                    float swap = t1;
                    t1 = t2;
                    t2 = swap;
                }

                // Note: some of the following special cases may no longer
                // be necessary...they only rarely crop up and removing them
                // requires extensive testing.
                if (t1 <= 0 && t2 >= 0)
                {
                    // We are already very close.  Are we moving away or not?
                    // Depends whether t1 or t2 is closer to 0

                    float d1 = Math.Abs(t1);
                    float d2 = Math.Abs(t2);
                    if (t2 < 0.1)
                    {
                        // just grazing it and we're leaving soon anyway
                        t1 = float.MaxValue;
                        t2 = float.MaxValue;
                    }
                    else if (d1 < d2)
                    {
                        // t1 is close to zero, t2 is a big positive number

                        // Refuse to move closer to the line
                        t1 = 0;
                    }
                    else
                    {
                        // t2 is close to zero, t1 is a big negative number

                        // If we leave t2 at near-zero, it will be difficult
                        // or impossible to move away from this line.  So
                        // pretend it's the same as t1
                        t2 = t1;
                    }
                    return;
                }

                if (Math.Abs(t1 - t2) < 0.01)
                {
                    // The path barely grazes the point (perhaps floating point error),
                    // so pretend no collision
                    t1 = float.MaxValue;
                    t2 = float.MaxValue;
                }
            }
        }


        /// <summary>
        // Rotate a Vector2 by theta radians
        /// </summary>
        private static Vector2 Rotate(Vector2 vec, float theta)
        {
            Vector4 unrotatedVec4 = new Vector4(vec.X, vec.Y, 0, 1);
            Matrix matRot = Matrix.CreateRotationZ(theta);
            Vector4 rotatedVec4 = Vector4.Transform(unrotatedVec4, matRot);
            return new Vector2(rotatedVec4.X, rotatedVec4.Y);
        }


        /// <summary>
        // Given a point moving from point p1 to point p2, past this line, find the values of t
        // at which the point is distance dist from the line.  If the point never gets that close, 
        // return MaxFloat for the two t values.
        /// </summary>
        private void FindLinearT(Vector2 p1, Vector2 p2, float dist, out float t1, out float t2)
        {
            // Transform p1 and p2 into a space where the line's p1 is at (0,0) and its p2 is at (1,0)
            // In this space, the y coordinate is the distance to the line.  The x coordinate is the
            // line's valid range (0 to 1 is on the line).
            p1 -= this.p1;
            p2 -= this.p1;
            p1 = Rotate(p1, -this.theta);
            p2 = Rotate(p2, -this.theta);
            p1.X *= 1.0f / this.rho;
            p2.X *= 1.0f / this.rho;

            // y = a + bt, where a = p1.y and b is p2.y-p1.y
            // find t where y = +- dist
            // dist = a + bt1
            // t1 = (dist - a) / b
            // -dist = a + bt2
            // t2 = (-dist - a) / b
            float a = p1.Y;
            float b = p2.Y - p1.Y;
            t1 = (dist - a) / b;
            t2 = (-dist - a) / b;

            // If we are currently in contact with the wall, pretend
            // there is no contact if we are moving away from it
            if (Math.Abs(t1) < 0.0001)
            {
                if (b > -0.00001)
                    t1 = float.MaxValue;
                else if (t1 < 0)
                    t1 = 0;
            }

            if (Math.Abs(t2) < 0.0001)
            {
                if (b < 0)
                    t2 = float.MaxValue;
                else if (t2 < 0)
                    t2 = 0;
            }

            // Ensure that t1 <= t2
            if (t2 < t1)
            {
                float temp = t2;
                t2 = t1;
                t1 = temp;
            }

            // Now compute x at t1 and t2 to make sure they are in range
            // x = c + dt
            float c = p1.X;
            float d = p2.X - p1.X;
            float x1 = c + d * t1;
            if (x1 < 0 || x1 > 1)
                t1 = float.MaxValue;
            float x2 = c + d * t2;
            if (x2 < 0 || x2 > 1)
                t2 = float.MaxValue;
        }


        /// <summary>
        // Given a point moving from point p1 to point p2, past this line, find the value of t
        // at which the point is distance dist from the line.  If the point never gets that close, 
        // set tMin to MaxFloat.
        /// </summary>
        public void FindFirstIntersection(Vector2 p1, Vector2 p2, float dist, out float tMin)
        {
            float t1;
            float t2;
            tMin = float.MaxValue;

            FindRadialT(p1, p2, this.p1, dist, out t1, out t2);
            if (t1 < tMin && t1 >= 0.0f && t1 < 1.0f)
                tMin = t1;

            FindRadialT(p1, p2, this.p2, dist, out t1, out t2);
            if (t1 < tMin && t1 >= 0.0f && t1 < 1.0f)
                tMin = t1;

            FindLinearT(p1, p2, dist, out t1, out t2);
            if (t1 < tMin && t1 >= 0.0f && t1 < 1.0f)
                tMin = t1;
        }


        public Matrix WorldMatrix()
        {
            Matrix rotate = Matrix.CreateRotationZ(theta);
            Matrix translate = Matrix.CreateTranslation(p1.X, p1.Y, 0);
            return rotate * translate;
        }
    };


    
}
