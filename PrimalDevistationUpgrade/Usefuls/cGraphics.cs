using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace PrimalDevistation.Usefuls
{
    class cGraphics
    {
        public static void ClearRenderTarget(GraphicsDevice gd, RenderTarget2D target, Color color)
        {
            DepthStencilBuffer dsb = gd.DepthStencilBuffer;
            gd.DepthStencilBuffer = null;
            gd.SetRenderTarget(0, target);
            gd.Clear(ClearOptions.Target, color, 1, 0);
            gd.SetRenderTarget(0, null);
            gd.DepthStencilBuffer = dsb;
        }  

        public static void ClearRenderTargetSlow(RenderTarget2D target, Color color)
        {
            Texture2D tex = target.GetTexture();
            Color[] data = new Color[tex.Width * tex.Height];
            tex.GetData<Color>(data);
            for (int i = 0; i < data.Length; i++) { data[i] = Color.White; }
            tex.SetData<Color>(data);
        }  
       
        public static void RenderToTex(GraphicsDevice gd, Texture2D texture, RenderTarget2D renderTarget, Effect effect)
        {
            int width = renderTarget.GetTexture().Width;
            int height = renderTarget.GetTexture().Height;
            SpriteBatch batch = PrimalDevistation.Instance.Batch;
            gd.SetRenderTarget(0, renderTarget);
            batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
            batch.Draw(texture, new Rectangle(0, 0, width, height), new Color(100, 100, 100));
            batch.End();
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
            gd.SetRenderTarget(0, null);
        }

        public static void RenderToTex(GraphicsDevice gd, Texture2D texture, RenderTarget2D renderTarget)
        {
            int width = renderTarget.GetTexture().Width;
            int height = renderTarget.GetTexture().Height;
            SpriteBatch batch = PrimalDevistation.Instance.Batch;
            gd.SetRenderTarget(0, renderTarget);
            //gd.Clear(ClearOptions.Target, new Color(100, 149, 237, 0), 0, 0);
            batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
            batch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            batch.End();
            gd.SetRenderTarget(0, null);
        }
         
        /// <summary>
        /// Helper for drawing a texture into a rendertarget, using
        /// a custom shader to apply postprocessing effects.
        /// </summary>
        public static void DrawFullscreenQuad(GraphicsDevice gd, Texture2D texture, RenderTarget2D renderTarget, Effect effect)
        {
            gd.SetRenderTarget(0, renderTarget);
            //gd.Clear(Color.CornflowerBlue);

            DrawFullscreenQuad(texture,
                               renderTarget.Width, renderTarget.Height,
                               effect);
            gd.SetRenderTarget(0, null);
        }


        /// <summary>
        /// Helper for drawing a texture into the current rendertarget,
        /// using a custom shader to apply postprocessing effects.
        /// </summary>
        public static void DrawFullscreenQuad(Texture2D texture, int width, int height, Effect effect)
        {
            SpriteBatch batch = PrimalDevistation.Instance.Batch;
            batch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.SaveState);

            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
            
            batch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            batch.End();

            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }

        public static byte[,] GetCollisionData(Texture2D tex)
        {
            Color[] texData = new Color[tex.Width * tex.Height];
            tex.GetData<Color>(texData);
            byte[,] dataOut = new byte[tex.Height, tex.Width];
            Color c;

            for (int i = 0; i < tex.Height; i++)
            {
                for (int j = 0; j < tex.Width; j++)
                {
                    c=texData[(i*tex.Width)+j];
                    dataOut[i, j] = c.A;
                }
            }
            return dataOut;
        }

    }
}
