/////////////////////////////////////////////////////////////////////////////////////////////////
//   BEGIN MySpriteBatch.cs
//////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PrimalDevistation.Usefuls
{

    public class cSpriteBatch
    {

        protected VertexPositionTexture[] vertices;
        protected short[] indices;
        protected int vertexCount = 0;
        protected int indexCount = 0;
        protected Texture2D texture;
        protected VertexDeclaration declaration;
        protected GraphicsDevice device;

        //  these should really be properties
        public Matrix World;
        public Matrix View;
        public Matrix Projection;
        public Effect Effect;

        public cSpriteBatch(GraphicsDevice device)
        {
            this.device = device;
            this.vertices = new VertexPositionTexture[256];
            this.indices = new short[vertices.Length * 3 / 2];
        }

        public void ResetMatrices(int width, int height)
        {
            this.World = Matrix.Identity;
            this.View = new Matrix(
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, -1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, -1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f);
            this.Projection = Matrix.CreateOrthographicOffCenter(
                0, width, -height, 0, 0, 1);
        }

        public virtual void RenderToTexture(RenderTarget2D target, GraphicsDevice gd)
        {
            float fWidth = target.Width - 0.5f;
            float fHeight = target.Height - 0.5f;

             //  ensure space for my vertices and indices.
            this.EnsureSpace(6, 4);

            //  add the new indices
            indices[indexCount++] = (short)(vertexCount + 0);
            indices[indexCount++] = (short)(vertexCount + 1);
            indices[indexCount++] = (short)(vertexCount + 3);
            indices[indexCount++] = (short)(vertexCount + 1);
            indices[indexCount++] = (short)(vertexCount + 2);
            indices[indexCount++] = (short)(vertexCount + 3);

             // add the new vertices
            vertices[vertexCount++] = new VertexPositionTexture(new Vector3(-0.5f, -0.5f, 0)
                , new Vector2(0, 0));

            vertices[vertexCount++] = new VertexPositionTexture(new Vector3(fWidth, -0.5f, 0)
                , new Vector2(1, 0));

            vertices[vertexCount++] = new VertexPositionTexture(new Vector3(fWidth, fHeight, 0)
                , new Vector2(1, 1));

            vertices[vertexCount++] = new VertexPositionTexture(new Vector3(-0.5f, fHeight, 0)
                , new Vector2(0, 1));

            //  we premultiply all vertices times the world matrix.
            //  the world matrix changes alot and we don't want to have to flush
            //  every time it changes.
            Matrix world = this.World;
            for (int i = vertexCount - 4; i < vertexCount; i++)
                Vector3.Transform(ref vertices[i].Position, ref world, out vertices[i].Position);

            //_device.SetRenderTarget(0, target);
            //_device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.Black, 1.0f, 0);
            this.ResetMatrices(gd.Viewport.Width, gd.Viewport.Height);

            Render();            
        }
        
        Vector2 GetUV(float x, float y)
        {
            return new Vector2(x / (float)texture.Width, y / (float)texture.Height);
        }

        protected void EnsureSpace(int indexSpace, int vertexSpace)
        {
            if (indexCount + indexSpace >= indices.Length)
                Array.Resize(ref indices, Math.Max(indexCount + indexSpace, indices.Length * 2));
            if (vertexCount + vertexSpace >= vertices.Length)
                Array.Resize(ref vertices, Math.Max(vertexCount + vertexSpace, vertices.Length * 2));
        }

        private void Render()
        {
            if (this.vertexCount > 0)
            {
                if (this.declaration == null || this.declaration.IsDisposed)
                    this.declaration = new VertexDeclaration(device, VertexPositionTexture.VertexElements);

                device.VertexDeclaration = this.declaration;

                Effect effect = this.Effect;
               
                DepthStencilBuffer dsb = device.DepthStencilBuffer;
                device.DepthStencilBuffer = null;

                //  set the only parameter this effect takes.
                effect.Parameters["viewProjection"].SetValue(this.View * this.Projection);
                
                EffectTechnique technique = effect.CurrentTechnique;
                effect.Begin();
                EffectPassCollection passes = technique.Passes;
                for (int i = 0; i < passes.Count; i++)
                {
                    EffectPass pass = passes[i];
                    pass.Begin();

                    device.DrawUserIndexedPrimitives<VertexPositionTexture>(
                        PrimitiveType.TriangleList, this.vertices, 0, this.vertexCount,
                        this.indices, 0, this.indexCount / 3);

                    pass.End();
                }
                effect.End();

                this.vertexCount = 0;
                this.indexCount = 0;

                device.DepthStencilBuffer = dsb;
            }
        }

    }

}
//////////////////////////////////////////////////////////////////////////////////////////////////
//   END MySpriteBatch.cs
//////////////////////////////////////////////////////////////////////////////////////////////////