using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using PrimalDevistation.Objects;
using PrimalDevistation.Usefuls;

namespace PrimalDevistation.Level
{

    public class cTerrainMap
    {  

        private Texture2D _texture;
        private RenderTarget2D _renderTarget;
        private Effect _terrainEffect;
        private Texture2D _indestructTex;

        public Texture2D IndestructableTexture
        {
            get { return _indestructTex; }
            set { _indestructTex = value; }
        }	

        public Texture2D Texture
        {
            get { return _renderTarget.GetTexture(); }
        }

        public Texture2D InitialTexture
        {
            get { return _texture; }
            set { _texture = value; }
        }
        
        public cTerrainMap()
        {
        }      

        public void Reset(GraphicsDevice gd)
        {    
            _renderTarget = new RenderTarget2D(gd, _texture.Width, _texture.Height, 1, SurfaceFormat.Color);
            _terrainEffect = PrimalDevistation.Instance.CM.Load<Effect>(@"Shaders/TerrainRender");
        }

        public void DrawExplosions(GraphicsDevice gd, Dictionary<int, Texture2D> textures, List<Vector3> _explosions)
        {
            //SpriteBatch batch = LieroXNA.Instance.Batch;

            //// Make sure we turn off the stencil buffer that we dont need and set the render targer
            //DepthStencilBuffer dsb = gd.DepthStencilBuffer;
            //gd.DepthStencilBuffer = null;
            //BlendFunction func = gd.RenderState.BlendFunction;
            //Blend alSource = gd.RenderState.AlphaSourceBlend;
            //Blend alDest = gd.RenderState.AlphaDestinationBlend;

            //gd.SetRenderTarget(0, _renderTargets[_currentTargetOut]);

            //// Begin rendering in reverse subtrct mode
            //batch.Begin();
            //batch.Draw(_renderTargets[_currentTargetIn].GetTexture(), new Rectangle(0, 0, _indestructTex.Width, _indestructTex.Height), Color.White);
            //batch.End();

            //batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);     
            
            //gd.RenderState.BlendFunction = BlendFunction.ReverseSubtract;
            //gd.RenderState.AlphaSourceBlend = Blend.One;
            //gd.RenderState.AlphaDestinationBlend = Blend.One;

            //// Render
            //Vector2 cam = LieroXNA.Instance.Camera.Position;
            //for (int i = 0; i < _explosions.Count; i++)
            //{
            //    int size = (int)(_explosions[i].Z);
            //    int hSize = size / 2;
            //    Texture2D tex = textures[size];
            //    batch.Draw(tex, new Rectangle((int)(_explosions[i].X - hSize), (int)(_explosions[i].Y - hSize), size, size), Color.White);
            //}

            //batch.End();


            //gd.RenderState.BlendFunction = func;
            //gd.RenderState.AlphaSourceBlend = alSource;
            //gd.RenderState.AlphaDestinationBlend = alDest;

            //batch.Begin();
            //batch.Draw(_indestructTex, new Rectangle(0, 0, _indestructTex.Width, _indestructTex.Height), Color.White);
            //batch.End();

            //gd.ResolveRenderTarget(0);
            //gd.SetRenderTarget(0, null);
            //gd.DepthStencilBuffer = dsb;


            //int tmp = _currentTargetOut;
            //_currentTargetOut = _currentTargetIn;
            //_currentTargetIn = tmp;
        }

        public void GenerateMap(GraphicsDevice gd, Texture2D explosionTex)
        {
            DepthStencilBuffer dsb = gd.DepthStencilBuffer;
            gd.DepthStencilBuffer = null;

            SpriteBatch batch = PrimalDevistation.Instance.Batch;
            gd.SetRenderTarget(0, _renderTarget);
            gd.Clear(ClearOptions.Target, new Color(100, 149, 237, 0), 0, 0);
            
            batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            _terrainEffect.Begin();
            _terrainEffect.Parameters["ExplosionTexture"].SetValue(explosionTex);
            _terrainEffect.Parameters["IndestructableTexture"].SetValue(_indestructTex);
            _terrainEffect.CurrentTechnique.Passes[0].Begin();
            batch.Draw(_texture, new Rectangle(0, 0, _texture.Width, _texture.Height), Color.White);
            batch.End();
            _terrainEffect.CurrentTechnique.Passes[0].End();
            _terrainEffect.End();

            gd.SetRenderTarget(0, null);            
            gd.DepthStencilBuffer = dsb;
        }



        //private void SetBlurWeights(float dx, float dy, int blurAmount)
        //{
        //    // Look up the sample weight and offset effect parameters.
        //    EffectParameter weightsParameter, offsetsParameter;

        //    weightsParameter = _gaussBlur.Parameters["SampleWeights"];
        //    offsetsParameter = _gaussBlur.Parameters["SampleOffsets"];

        //    // Look up how many samples our gaussian blur effect supports.
        //    int sampleCount = weightsParameter.Elements.Count;

        //    // Create temporary arrays for computing our filter settings.
        //    float[] sampleWeights = new float[sampleCount];
        //    Vector2[] sampleOffsets = new Vector2[sampleCount];

        //    // The first sample always has a zero offset.
        //    sampleWeights[0] = cMath.ComputeGaussian(0, blurAmount);
        //    sampleOffsets[0] = new Vector2(0);

        //    // Maintain a sum of all the weighting values.
        //    float totalWeights = sampleWeights[0];

        //    // Add pairs of additional sample taps, positioned
        //    // along a line in both directions from the center.
        //    for (int i = 0; i < sampleCount / 2; i++)
        //    {
        //        // Store weights for the positive and negative taps.
        //        float weight = cMath.ComputeGaussian(i + 1, blurAmount);

        //        sampleWeights[i * 2 + 1] = weight;
        //        sampleWeights[i * 2 + 2] = weight;

        //        totalWeights += weight * 2;

        //        // To get the maximum amount of blurring from a limited number of
        //        // pixel shader samples, we take advantage of the bilinear filtering
        //        // hardware inside the texture fetch unit. If we position our texture
        //        // coordinates exactly halfway between two texels, the filtering unit
        //        // will average them for us, giving two samples for the price of one.
        //        // This allows us to step in units of two texels per sample, rather
        //        // than just one at a time. The 1.5 offset kicks things off by
        //        // positioning us nicely in between two texels.
        //        float sampleOffset = i * 2 + 1.5f;

        //        Vector2 delta = new Vector2(dx, dy) * sampleOffset;

        //        // Store texture coordinate offsets for the positive and negative taps.
        //        sampleOffsets[i * 2 + 1] = delta;
        //        sampleOffsets[i * 2 + 2] = -delta;
        //    }

        //    // Normalize the list of sample weightings, so they will always sum to one.
        //    for (int i = 0; i < sampleWeights.Length; i++)
        //    {
        //        sampleWeights[i] /= totalWeights;
        //    }

        //    // Tell the effect about our new filter settings.
        //    weightsParameter.SetValue(sampleWeights);
        //    offsetsParameter.SetValue(sampleOffsets);
        //}

        //private Effect _gaussBlur;
        
        // Create the destroyed bacground
        //_destroyedTex = new RenderTarget2D(gd, _texture.Width / 2, _texture.Height / 2, 1, SurfaceFormat.Color);            
        //RenderTarget2D tmpRT = new RenderTarget2D(gd, _texture.Width / 2, _texture.Height / 2, 1, SurfaceFormat.Color);
        
        //// And blurr it abit
        //_gaussBlur = cm.Load<Effect>(@"Shaders/GaussianBlur");
        //SetBlurWeights(1.0f / (float)_texture.Width, 1.0f / (float)_texture.Height, 1);
        //DepthStencilBuffer dsb = gd.DepthStencilBuffer;
        //gd.DepthStencilBuffer = null;
        //cRender.ClearRenderTarget(gd, _destroyedTex, new Color(100, 149, 237, 0));
        //cRender.ClearRenderTarget(gd, tmpRT, new Color(100, 149, 237, 0));
        //cRender.RenderToTex(gd, _texture, _destroyedTex);
        //cRender.RenderToTex(gd, _destroyedTex.GetTexture(), tmpRT, _gaussBlur);
        //_destroyedTex = tmpRT;
        //gd.DepthStencilBuffer = dsb;


        
    }
}
