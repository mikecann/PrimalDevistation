#region File Description
//-----------------------------------------------------------------------------
// QuadDrawer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace PrimalDevistation.Usefuls
{
    /// <summary>
    /// Helper for drawing 3D quadrilaterals. This is used to draw the cat
    /// and dog billboard sprites, and also the checkered ground polygon.
    /// </summary>
    public class QuadDrawer
    {
        #region Fields

        GraphicsDevice graphicsDevice;
        VertexDeclaration vertexDeclaration;
        BasicEffect basicEffect;
        VertexPositionTexture[] vertices;

        #endregion


        /// <summary>
        /// Constructs a new quadrilateral drawing worker.
        /// </summary>
        public QuadDrawer(GraphicsDevice device)
        {
            graphicsDevice = device;

            vertexDeclaration = new VertexDeclaration(device,
                                        VertexPositionTexture.VertexElements);
            
            basicEffect = new BasicEffect(device, null);

            // Preallocate an array of four vertices.
            vertices = new VertexPositionTexture[4];

            vertices[0].Position = new Vector3(1, 1, 0);
            vertices[1].Position = new Vector3(-1, 1, 0);
            vertices[2].Position = new Vector3(-1, -1, 0);
            vertices[3].Position = new Vector3(1, -1, 0);
        }


        /// <summary>
        /// Draws a quadrilateral as part of the 3D world.
        /// </summary>
        public void DrawQuad(Texture2D texture, float textureRepeats,
                             Matrix world, Matrix view, Matrix projection)
        {
            // Set our effect to use the specified texture and camera matrices.
            basicEffect.Texture = texture;
            basicEffect.TextureEnabled = true;

            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;

            // Update our vertex array to use the specified number of texture repeats.
            vertices[0].TextureCoordinate = new Vector2(0, 0);
            vertices[1].TextureCoordinate = new Vector2(textureRepeats, 0);
            vertices[2].TextureCoordinate = new Vector2(textureRepeats, textureRepeats);
            vertices[3].TextureCoordinate = new Vector2(0, textureRepeats);

            // Draw the quad.
            basicEffect.Begin();
            basicEffect.CurrentTechnique.Passes[0].Begin();

            graphicsDevice.VertexDeclaration = vertexDeclaration;

            graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleFan, vertices, 0, 2);

            basicEffect.CurrentTechnique.Passes[0].End();
            basicEffect.End();
        }
    }
}
