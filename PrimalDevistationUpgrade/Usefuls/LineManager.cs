using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpritesAndLines;
using System.Diagnostics;

namespace PrimalDevistation.Usefuls
{
    /// <summary>
    /// Class to handle drawing a list of lines.
    /// </summary>
    public class LineManager : DrawableGameComponent
    {
        private Effect effect;
        private EffectParameter wvpMatrixParameter;
        private EffectParameter timeParameter;
        private EffectParameter lengthParameter;
        private EffectParameter rotationParameter;
        private EffectParameter radiusParameter;
        private EffectParameter lineColorParameter;
        private VertexBuffer vb;
        private IndexBuffer ib;
        private VertexDeclaration vdecl;
        private int numVertices;
        private int numIndices;
        private int numPrimitives;
        private int bytesPerVertex;
        public int numLinesDrawn = 0;

        public List<cLineDraw> _lineDrawCalls = new List<cLineDraw>();

        public class cLineDraw
        {
            public List<Line> lineList;
            public float globalRadius;
            public Color globalColor;
            public float time;
            public string techniqueName;
        }

        public LineManager(Game game) : base(game) { }
        
        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
                effect = PrimalDevistation.Instance.CM.Load<Effect>(@"Shaders/Line");
                wvpMatrixParameter = effect.Parameters["worldViewProj"];
                timeParameter = effect.Parameters["time"];
                lengthParameter = effect.Parameters["length"];
                rotationParameter = effect.Parameters["rotation"];
                radiusParameter = effect.Parameters["radius"];
                lineColorParameter = effect.Parameters["lineColor"];
                CreateLineMesh();
            }
        }


        /// <summary>
        /// Create a mesh for a line.
        /// </summary>
        /// <remarks>
        /// The lineMesh has 3 sections:
        /// 1.  Two quads, from 0 to 1 (left to right)
        /// 2.  A half-disc, off the left side of the quad
        /// 3.  A half-disc, off the right side of the quad
        ///
        /// The X and Y coordinates of the "normal" encode the rho and theta of each vertex
        /// The "texture" encodes how much to scale and translate the vertex horizontally by length and radius
        /// </remarks>
        private void CreateLineMesh()
        {
            const int primsPerCap = 12; // A higher primsPerCap produces rounder endcaps at the cost of more vertices
            const int verticesPerCap = primsPerCap * 2 + 2;
            const int primsPerCore = 4;
            const int verticesPerCore = 8;

            numVertices = verticesPerCore + verticesPerCap + verticesPerCap;
            numPrimitives = primsPerCore + primsPerCap + primsPerCap;
            numIndices = 3 * numPrimitives;
            short[] indices = new short[numIndices];
            bytesPerVertex = VertexPositionNormalTexture.SizeInBytes;
            VertexPositionNormalTexture[] tri = new VertexPositionNormalTexture[numVertices];

            // core vertices
            const float pi2 = MathHelper.PiOver2;
            const float threePi2 = 3 * pi2;
            tri[0] = new VertexPositionNormalTexture(new Vector3(0.0f, -1.0f, 0), new Vector3(1, threePi2, 0), new Vector2(0, 0));
            tri[1] = new VertexPositionNormalTexture(new Vector3(0.0f, -1.0f, 0), new Vector3(1, threePi2, 0), new Vector2(0, 1));
            tri[2] = new VertexPositionNormalTexture(new Vector3(0.0f, 0.0f, 0), new Vector3(0, threePi2, 0), new Vector2(0, 1));
            tri[3] = new VertexPositionNormalTexture(new Vector3(0.0f, 0.0f, 0), new Vector3(0, threePi2, 0), new Vector2(0, 0));
            tri[4] = new VertexPositionNormalTexture(new Vector3(0.0f, 0.0f, 0), new Vector3(0, pi2, 0), new Vector2(0, 1));
            tri[5] = new VertexPositionNormalTexture(new Vector3(0.0f, 0.0f, 0), new Vector3(0, pi2, 0), new Vector2(0, 0));
            tri[6] = new VertexPositionNormalTexture(new Vector3(0.0f, 1.0f, 0), new Vector3(1, pi2, 0), new Vector2(0, 1));
            tri[7] = new VertexPositionNormalTexture(new Vector3(0.0f, 1.0f, 0), new Vector3(1, pi2, 0), new Vector2(0, 0));

            // core indices
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 3;
            indices[5] = 0;

            indices[6] = 4;
            indices[7] = 6;
            indices[8] = 5;
            indices[9] = 6;
            indices[10] = 7;
            indices[11] = 5;

            // left halfdisc
            int iVertex = 8;
            int iIndex = 12;
            for (int i = 0; i < primsPerCap + 1; i++)
            {
                float deltaTheta = MathHelper.Pi / primsPerCap;
                float theta0 = MathHelper.PiOver2 + i * deltaTheta;
                float theta1 = theta0 + deltaTheta / 2;
                // even-numbered indices are at the center of the halfdisc
                tri[iVertex + 0] = new VertexPositionNormalTexture(new Vector3(0, 0, 0), new Vector3(0, theta1, 0), new Vector2(0, 0));

                // odd-numbered indices are at the perimeter of the halfdisc
                float x = (float)Math.Cos(theta0);
                float y = (float)Math.Sin(theta0);
                tri[iVertex + 1] = new VertexPositionNormalTexture(new Vector3(x, y, 0), new Vector3(1, theta0, 0), new Vector2(1, 0));

                if (i < primsPerCap)
                {
                    // indices follow this pattern: (0, 1, 3), (2, 3, 5), (4, 5, 7), ...
                    indices[iIndex + 0] = (short)(iVertex + 0);
                    indices[iIndex + 1] = (short)(iVertex + 1);
                    indices[iIndex + 2] = (short)(iVertex + 3);
                    iIndex += 3;
                }
                iVertex += 2;
            }

            // right halfdisc
            for (int i = 0; i < primsPerCap + 1; i++)
            {
                float deltaTheta = MathHelper.Pi / primsPerCap;
                float theta0 = 3 * MathHelper.PiOver2 + i * deltaTheta;
                float theta1 = theta0 + deltaTheta / 2;
                float theta2 = theta0 + deltaTheta;
                // even-numbered indices are at the center of the halfdisc
                tri[iVertex + 0] = new VertexPositionNormalTexture(new Vector3(0, 0, 0), new Vector3(0, theta1, 0), new Vector2(0, 1));

                // odd-numbered indices are at the perimeter of the halfdisc
                float x = (float)Math.Cos(theta0);
                float y = (float)Math.Sin(theta0);
                tri[iVertex + 1] = new VertexPositionNormalTexture(new Vector3(x, y, 0), new Vector3(1, theta0, 0), new Vector2(1, 1));

                if (i < primsPerCap)
                {
                    // indices follow this pattern: (0, 1, 3), (2, 3, 5), (4, 5, 7), ...
                    indices[iIndex + 0] = (short)(iVertex + 0);
                    indices[iIndex + 1] = (short)(iVertex + 1);
                    indices[iIndex + 2] = (short)(iVertex + 3);
                    iIndex += 3;
                }
                iVertex += 2;
            }

            vb = new VertexBuffer(GraphicsDevice, numVertices * bytesPerVertex, BufferUsage.None);
            vb.SetData<VertexPositionNormalTexture>(tri);
            vdecl = new VertexDeclaration(GraphicsDevice, VertexPositionNormalTexture.VertexElements);

            ib = new IndexBuffer(GraphicsDevice, numIndices * 2, BufferUsage.None, IndexElementSize.SixteenBits);
            ib.SetData<short>(indices);
        }


        /// <summary>
        /// Given a bunch of lines (lineList), find all lines that are within referenceRadius 
        /// of referencePos and add them to nearbyLineList.
        /// </summary>
        public void FindNearbyLines(List<Line> lineList, List<Line> nearbyLineList, float globalLineRadius, Vector2 referencePos, float referenceRadius)
        {
            nearbyLineList.Clear();

            foreach (Line line in lineList)
            {
                float totalDistance;
                if (globalLineRadius == 0)
                    totalDistance = referenceRadius + line.radius;
                else
                    totalDistance = referenceRadius + globalLineRadius;
                if (line.DistanceSquaredPointToVirtualLine(referencePos) < (totalDistance * totalDistance))
                    nearbyLineList.Add(line);
            }
        }


        /// <summary>
        /// Given a bunch of lines (lineList), check their distances to the disc described by
        /// currentPos/discRadius, and find the minimum distance.  This value will be negative
        /// if the disc intersects any of the lines.
        /// </summary>
        public float MinDistanceSquaredDeviation(List<Line> lineList, Vector2 currentPos, float discRadius)
        {
            float minDeviation = float.MaxValue;
            foreach (Line line in lineList)
            {
                float minDist2 = (line.radius + discRadius) * (line.radius + discRadius);
                float curDist2 = line.DistanceSquaredPointToVirtualLine(currentPos);
                float deviation = curDist2 - minDist2; // should be positive if no intersection
                if (deviation < minDeviation)
                    minDeviation = deviation;
            }
            return minDeviation;
        }


        /// <summary>
        /// Given a bunch of lines (lineList) and a disc that wants to move from currentPos
        /// to proposedPos, handle intersections and wall sliding and set finalPos to the 
        /// position that the disc should move to.
        /// </summary>
        public void CollideAndSlide(List<Line> lineList, Vector2 currentPos, Vector2 proposedPos, float discRadius, out Vector2 finalPos)
        {
            Vector2 oldPos = currentPos;
            Vector2 oldTarget = proposedPos;
            bool pastFirstSlide = false;

            // Keep looping until there's no further desire to move somewhere else.
            while (Vector2.DistanceSquared(oldPos, oldTarget) > 0.001f * 0.001f)
            {
                // oldPos should be safe (no intersection with anything in lineList)
                Debug.Assert(MinDistanceSquaredDeviation(lineList, oldPos, discRadius) >= 0);

                // Find minimum "t" at which we collide with a line
                float minT = 1.0f; // Parametric "t" of the closest collision
                Line minTLine = null; // The line which causes closest collision
                foreach (Line line in lineList)
                {
                    float tMinThisLine;
                    float minDist = line.radius + discRadius;
                    float minDist2 = minDist * minDist;
                    float curDist2 = line.DistanceSquaredPointToVirtualLine(oldPos);
                    Debug.Assert(curDist2 - minDist2 >= 0);

                    line.FindFirstIntersection(oldPos, oldTarget, minDist, out tMinThisLine);
                    if (tMinThisLine >= 0 && tMinThisLine <= 1)
                    {
                        // We can move tMinThisLine toward the line, before intersecting it -- but
                        // we might intersect other lines first, so keep looking for
                        // smaller tMinThisLine values on other lines

                        // But first, refine tMinThisLine (if needed) until it satisfies the distance test
                        Vector2 newPos = new Vector2(MathHelper.Lerp(oldPos.X, oldTarget.X, tMinThisLine),
                                                     MathHelper.Lerp(oldPos.Y, oldTarget.Y, tMinThisLine));
                        if (line.DistanceSquaredPointToVirtualLine(newPos) - minDist2 < 0)
                        {
                            float ta = 0;
                            float tb = tMinThisLine;
                            for (int iterRefine = 0; iterRefine < 10; iterRefine++)
                            {
                                float tc = (ta + tb) / 2;
                                Vector2 newPosC = new Vector2(MathHelper.Lerp(oldPos.X, oldTarget.X, tc),
                                                              MathHelper.Lerp(oldPos.Y, oldTarget.Y, tc));
                                float newDistC = line.DistanceSquaredPointToVirtualLine(newPosC);
                                if (newDistC - minDist2 < 0)
                                    tb = tc;
                                else
                                    ta = tc;
                            }
                            tMinThisLine = ta;
                        }

                        // Remember this "t" and the line that caused it, if it's the closest so far
                        if (tMinThisLine < minT)
                        {
                            minT = tMinThisLine;
                            minTLine = line;
                        }
                    }
                    else
                    {
                        // This line has no issue with the disc moving to oldTarget...or does it?  
                        // Due to floating point variances, we have to double-check and pick a new "t"
                        // if oldTarget is actually too close to this line
                        float newDist = line.DistanceSquaredPointToVirtualLine(oldTarget);
                        if (newDist - minDist2 < 0)
                        {
                            // Find a "t" that is as large as possible while avoiding collision
                            float ta = 0;
                            float tb = 1;
                            for (int i = 0; i < 10; i++)
                            {
                                float tc = (ta + tb) / 2;
                                Vector2 ptC = new Vector2(MathHelper.Lerp(oldPos.X, oldTarget.X, tc),
                                                          MathHelper.Lerp(oldPos.Y, oldTarget.Y, tc));
                                float distC = line.DistanceSquaredPointToVirtualLine(ptC);
                                if (distC - minDist2 < 0)
                                    tb = tc;
                                else
                                    ta = tc;
                            }

                            if (ta < minT)
                            {
                                minT = ta;
                                minTLine = line;
                            }
                        }
                    }
                }

                // At this point, we've looped through all lines and found the minimum "t" value and its line

                if (minTLine == null)
                {
                    // No intersections were found, so move straight to the target
                    Debug.Assert(MinDistanceSquaredDeviation(lineList, oldTarget, discRadius) >= 0);
                    oldPos = oldTarget; // no further motion required
                }
                else
                {
                    // Collide and slide against minTLine
                    Vector2 newPos = new Vector2(MathHelper.Lerp(oldPos.X, oldTarget.X, minT),
                                                 MathHelper.Lerp(oldPos.Y, oldTarget.Y, minT));
                    Vector2 newTarget;

                    float minDist2 = (minTLine.radius + discRadius) * (minTLine.radius + discRadius);

                    // Refine minT / newPos til it passes the distance test
                    float minDistDeviation = MinDistanceSquaredDeviation(lineList, newPos, discRadius);
                    if (minDistDeviation < 0)
                    {
                        float ta = 0;
                        float tb = minT;
                        for (int i = 0; i < 10; i++)
                        {
                            float tc = (ta + tb) / 2;
                            Vector2 ptC = new Vector2(MathHelper.Lerp(oldPos.X, oldTarget.X, tc),
                                                      MathHelper.Lerp(oldPos.Y, oldTarget.Y, tc));
                            if (MinDistanceSquaredDeviation(lineList, ptC, discRadius) < 0)
                                tb = tc;
                            else
                                ta = tc;
                        }
                        minT = ta;
                        newPos = new Vector2(MathHelper.Lerp(oldPos.X, oldTarget.X, minT),
                                             MathHelper.Lerp(oldPos.Y, oldTarget.Y, minT));
                    }
                    Debug.Assert(MinDistanceSquaredDeviation(lineList, newPos, discRadius) >= 0);

                    // This is a bit of a hack to avoid "jiggling" when the disc is pressed
                    // against two walls that are at an obtuse angle.  Perhaps the real fix
                    // is to project the new slide vector against the original motion vector?
                    // In practice, we only ever need to slide once -- this fixes the issue.
                    bool doSlide = true;
                    if (pastFirstSlide)
                    {
                        newTarget = newPos;
                        doSlide = false;
                    }
                    else
                    {
                        pastFirstSlide = true;
                    }

                    if (doSlide)
                    {
                        Vector2 closestP;
                        float d2 = minTLine.DistanceSquaredPointToVirtualLine(newPos, out closestP);
                        Line connectionLine = new Line(newPos, closestP);
                        Vector2 lineNormal = (newPos - closestP);
                        lineNormal.Normalize();

                        // create a normal to the above line
                        // (which would thus be a tangent to minTLine)
                        float theta = connectionLine.theta;
                        theta += MathHelper.PiOver2;
                        Vector2 newPoint = new Vector2(newPos.X + (float)Math.Cos(theta), newPos.Y + (float)Math.Sin(theta));

                        // Project the post-intersection line onto the above line, to provide "wall sliding" effect
                        // v1 dot v2 = |v2| * (projection of v1 onto v2), and |v2| is 1
                        Vector2 v1 = oldTarget - newPos;
                        Vector2 v2 = newPoint - newPos;
                        float dotprod = Vector2.Dot(v1, v2);

                        newTarget = newPos + dotprod * v2;
                        // newTarget should not be too close to minTLine
                        float newTargetDist = minTLine.DistanceSquaredPointToVirtualLine(newTarget);
                        if (newTargetDist - minDist2 < 0)
                        {
                            float shiftAmtA = 0; // not enough
                            float shiftAmtB = -(newTargetDist - minDist2) + 0.0001f; // too much
                            for (int i = 0; i < 10; i++)
                            {
                                float shiftAmtC = (shiftAmtA + shiftAmtB) / 2.0f;
                                Vector2 newTargetTest = newTarget + (shiftAmtC * lineNormal);
                                float newTargetTestDist = minTLine.DistanceSquaredPointToVirtualLine(newTargetTest);
                                if (newTargetTestDist - minDist2 >= 0)
                                    shiftAmtB = shiftAmtC;
                                else
                                    shiftAmtA = shiftAmtC;
                            }
                            newTarget += shiftAmtB * lineNormal;
                        }
                    }
                    else
                    {
                        newTarget = newPos; // No slide
                    }

                    // Get ready to loop around and see if we can move from newPos to newTarget
                    // without colliding with anything
                    oldPos = newPos;
                    oldTarget = newTarget;

                    Debug.Assert(minTLine.DistanceSquaredPointToVirtualLine(newPos) - minDist2 >= 0);
                }
            }

            // oldTarget == oldPos (or is very close), so no further moving/sliding is needed.
            finalPos = oldPos;
            Debug.Assert(MinDistanceSquaredDeviation(lineList, finalPos, discRadius) >= 0);
        }



        /// <summary>
        /// Draw a list of Lines.
        /// </summary>
        /// <remarks>
        /// Set globalRadius = 0 to use the radius stored in each Line.
        /// Set globalColor to Color.TransparentBlack to use the color stored in each Line.
        /// </remarks>
        public void Draw(List<Line> lineList, float globalRadius, Color globalColor, Matrix viewMatrix, Matrix projMatrix,
            float time, string techniqueName)
        {
            Vector4 lineColor;
            bool uniqueColors = false;
            if (techniqueName == null)
                effect.CurrentTechnique = effect.Techniques[0];
            else
                effect.CurrentTechnique = effect.Techniques[techniqueName];
            effect.Begin();
            EffectPass pass = effect.CurrentTechnique.Passes[0];
            GraphicsDevice.VertexDeclaration = vdecl;
            GraphicsDevice.Vertices[0].SetSource(vb, 0, bytesPerVertex);
            GraphicsDevice.Indices = ib;

            pass.Begin();

            timeParameter.SetValue(time);
            if (globalColor == Color.TransparentBlack)
            {
                uniqueColors = true;
            }
            else
            {
                lineColor = globalColor.ToVector4();
                lineColorParameter.SetValue(lineColor);
            }
            if (globalRadius != 0)
                radiusParameter.SetValue(globalRadius);

            foreach (Line line in lineList)
            {
                Matrix worldViewProjMatrix = line.WorldMatrix() * viewMatrix * projMatrix;
                wvpMatrixParameter.SetValue(worldViewProjMatrix);
                lengthParameter.SetValue(line.rho);
                rotationParameter.SetValue(line.theta);
                if (globalRadius == 0)
                    radiusParameter.SetValue(line.radius);
                if (uniqueColors)
                {
                    lineColor = line.color.ToVector4();
                    lineColorParameter.SetValue(lineColor);
                }
                effect.CommitChanges();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices, 0, numPrimitives);
                numLinesDrawn++;
            }
            pass.End();

            effect.End();
        }

        public override void Draw(GameTime gameTime)
        {
            for (int i = 0; i < _lineDrawCalls.Count; i++)
            {
                cLineDraw ldc = _lineDrawCalls[i];
                Draw(ldc.lineList, ldc.globalRadius, ldc.globalColor, Matrix.Identity, PrimalDevistation.Instance.Camera.Projection, 1, ldc.techniqueName);
            }
            _lineDrawCalls.Clear();
        }

        public void AddLineDrawCall(List<Line> lineList, float globalRadius, Color globalColor, string techniqueName)
        {
            cLineDraw ldc = new cLineDraw();
            ldc.lineList = lineList;
            ldc.globalRadius = globalRadius;
            ldc.globalColor = globalColor;
            ldc.techniqueName = techniqueName;
            _lineDrawCalls.Add(ldc);
        }


    }
}
