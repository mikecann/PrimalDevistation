using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using PrimalDevistation.Usefuls;

namespace PrimalDevistation.Particles
{
    public class cPhysicsParticlesR2VB
    {
        private Effect _renderEffect;
        private Effect _updateEffect;

        private RenderTarget2D[] _positions;
        private RenderTarget2D[] _velocities;
        private Texture2D _particleTexture;
        private Texture2D _terrainTexture;

        public Texture2D TerrainTexture
        {
            get { return _terrainTexture; ; }
            set { _terrainTexture = value; }
        }

        private int _maxParticles;
        private int _currentTargetIn;
        private int _currentTargetOut;
        private int _textureWidth;
        private int _textureHeight;
        private int _nextEmitRow;
        private Random _random;
        private Matrix _proj;
        private Color[] _colors = new Color[] { Color.Red, Color.Orange, Color.Red, Color.Yellow };

        private VertexPositionColorTexture[] _verts;
        private VertexDeclaration _dec;

        public cPhysicsParticlesR2VB()
        {
            _maxParticles = 150000;
            _random = new Random();
            _currentTargetIn = 0;
            _currentTargetOut = 1;

            int multiple = _maxParticles / 768;
            _textureHeight = 768;
            _textureWidth = multiple;
            _maxParticles = _textureWidth * _textureHeight;
        }

        public void LoadGraphicsContent(GraphicsDevice gd, ContentManager cm)
        {
            _renderEffect = cm.Load<Effect>(@"Shaders/PointSpriteRenderVT");
            _updateEffect = cm.Load<Effect>(@"Shaders/ParticleUpdateMRT");
            _particleTexture = cm.Load<Texture2D>(@"Sprites/square2-2");

            _positions = new RenderTarget2D[2];
            _positions[0] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.Vector4);
            _positions[1] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.Vector4);

            _velocities = new RenderTarget2D[2];
            _velocities[0] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.Vector4);
            _velocities[1] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.Vector4);

            cGraphics.ClearRenderTarget(gd, _velocities[0], Color.Black);
            cGraphics.ClearRenderTarget(gd, _velocities[1], Color.Black);
            cGraphics.ClearRenderTarget(gd, _positions[0], Color.Black);
            cGraphics.ClearRenderTarget(gd, _positions[1], Color.Black);

            Vector4[] data = new Vector4[_maxParticles];
            _positions[0].GetTexture().GetData<Vector4>(data);
            for (int i = 0; i < data.Length; i++) { data[i].X = -100; }
            _positions[0].GetTexture().SetData<Vector4>(data);
            _positions[1].GetTexture().SetData<Vector4>(data);




            _verts = new VertexPositionColorTexture[_maxParticles];
            _dec = new VertexDeclaration(gd, VertexPositionColorTexture.VertexElements);

            float ratioW = 1f / (_textureWidth - 1);
            float ratioH = 1f / (_textureHeight - 1);

            int icounter = 0;
            for (int i = 0; i < _textureWidth; i++)
            {
                for (int j = 0; j < _textureHeight; j++)
                {
                    Vector2 v = new Vector2(i * ratioW, j * ratioH);
                    _verts[icounter++] = new VertexPositionColorTexture(Vector3.Zero, _colors[_random.Next(0, _colors.Length)], v);
                }
            }

            // Create a matrix to convert from pixel coordinates to clip space
            // So, before transformation, the screen boundaries go from 0 to (say) 640 horizontally,
            // and 0 to (say) 480 vertically.  After transformation, they go from -1 to 1 horizontally,
            // and -1 to 1 vertically.  Note in pixel coordinates, increasing Y values are further down 
            // the screen, while in clip space, increasing Y values are further up the screen -- so there
            // is a Y scaling factor of -1.
            float viewportWidth = (float)gd.Viewport.Width;
            float viewportHeight = (float)gd.Viewport.Height;
            float scaleX = 1.0f / (viewportWidth / 2);
            float scaleY = 1.0f / (viewportHeight / 2);
            _proj = Matrix.CreateScale(scaleX, scaleY, 1) *
                         Matrix.CreateScale(1, -1, 1) *
                         Matrix.CreateTranslation(-1, 1, 0);

        }

        public void Update(GameTime gameTime)
        {
        }

        public void UpdatePartcles(GraphicsDevice gd)
        {
            //Vector4[] data = new Vector4[_maxParticles];
            //_positions[_currentTargetIn].GetTexture().GetData<Vector4>(data);

            gd.SetRenderTarget(0, _velocities[_currentTargetOut]);
            gd.SetRenderTarget(1, _positions[_currentTargetOut]);

            _updateEffect.Parameters["VelocityTexture"].SetValue(_velocities[_currentTargetIn].GetTexture());
            _updateEffect.Parameters["TerrainTexture"].SetValue(_terrainTexture);
            _updateEffect.Parameters["RatioWH"].SetValue(new Vector2(1f / (1024f), 1f / (768f)));
            _updateEffect.Parameters["Gravity"].SetValue(PrimalDevistation.GRAVITY);
            _updateEffect.Parameters["Friction"].SetValue(0.5f);

            cGraphics.DrawFullscreenQuad(_positions[_currentTargetIn].GetTexture(), _textureWidth, _textureHeight, _updateEffect);

            gd.ResolveRenderTarget(0);
            gd.ResolveRenderTarget(1);
            gd.SetRenderTarget(0, null);
            gd.SetRenderTarget(1, null);
        }

        public void Render(GraphicsDevice gd)
        {
            DepthStencilBuffer buff = gd.DepthStencilBuffer;
            gd.DepthStencilBuffer = null;
            UpdatePartcles(gd);
            gd.DepthStencilBuffer = buff;


            _renderEffect.Parameters["View"].SetValue(Matrix.Identity);
            _renderEffect.Parameters["Projection"].SetValue(_proj);
            _renderEffect.Parameters["World"].SetValue(Matrix.Identity);
            _renderEffect.Parameters["PositionsTexture"].SetValue(_positions[_currentTargetIn].GetTexture());
            _renderEffect.Parameters["ParticleSize"].SetValue(2);

            gd.VertexDeclaration = _dec;

            _renderEffect.Begin();
            gd.RenderState.AlphaBlendEnable = true;
            //gd.Clear(Color.Black);

            foreach (EffectPass pass in _renderEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                gd.Textures[0] = _particleTexture;
                gd.DrawUserPrimitives(PrimitiveType.PointList, _verts, 0, _maxParticles);
                pass.End();

            }
            _renderEffect.End();

            int tmp = _currentTargetOut;
            _currentTargetOut = _currentTargetIn;
            _currentTargetIn = tmp;
        }

        public void SpawnParticles(List<Vector4> positions, List<Vector4> velocities)
        {
            Texture2D posTex = _positions[_currentTargetIn].GetTexture();
            Texture2D velTex = _velocities[_currentTargetIn].GetTexture();

            int num = positions.Count;
            int multiple = (num / posTex.Width) + 1;

            // Making sure we have enough particles to accomidate this explosion 
            // TODO: This could be made better to accomidate larger explosions!
            if (_nextEmitRow + multiple >= posTex.Height) { _nextEmitRow = 0; }
            if (_nextEmitRow + multiple >= posTex.Height) { return; }

            Vector4[] tmpPos, tmpVel;
            tmpPos = new Vector4[multiple * posTex.Width];
            tmpVel = new Vector4[multiple * posTex.Width];
            positions.CopyTo(tmpPos);
            velocities.CopyTo(tmpVel);

            posTex.SetData<Vector4>(0, new Rectangle(0, _nextEmitRow, posTex.Width, multiple), tmpPos, 0, tmpPos.Length, SetDataOptions.NoOverwrite);
            velTex.SetData<Vector4>(0, new Rectangle(0, _nextEmitRow, posTex.Width, multiple), tmpVel, 0, tmpPos.Length, SetDataOptions.NoOverwrite);

            _nextEmitRow += multiple;
        }
    }
}
