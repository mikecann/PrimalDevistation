using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using PrimalDevistation.Usefuls;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace PrimalDevistation.Particles
{
    public class cPhysicsParticlesVT : DrawableGameComponent, iPhysicsParticleSystem
    {
        private Effect _renderEffect;
        private Effect _updateEffect;
        private RenderTarget2D[] _positions;   
        private RenderTarget2D[] _velocities;
        private Texture2D _particleTexture;
        private int _maxParticles;
        private int _currentTargetIn;
        private int _currentTargetOut;
        private int _textureWidth;
        private int _textureHeight;
        private int _nextEmitRow;
        private Random _random;
        private Color[] _cols = new Color[] { Color.Red, Color.Orange, Color.Red, Color.Yellow };    
        private List<Vector4> _toAddPositions;
        private List<Vector4> _toAddVelocities;
        private sVTPointVertex[] _verts;
        private VertexDeclaration _dec;
        private VertexBuffer _vertexBuffer;
        private int _particleSize;
        private cSpriteBatch _batch;
        private int _nextForce;
        private Vector4[] _forces;   
        private bool _useMRT;
        private bool _useHV4;
        private ResolveTexture2D _bbResolveTarget;

        public bool UseMRT
        {
            get { return _useMRT; }
            set 
            { 
                _useMRT = value;
                if (_useMRT) { _updateEffect = PrimalDevistation.Instance.CM.Load<Effect>(@"Shaders/ParticleUpdateMRT"); }
                else { _updateEffect = PrimalDevistation.Instance.CM.Load<Effect>(@"Shaders/ParticleUpdate"); } 
            }
        }

        public bool UseHV4
        {
            get { return _useHV4; }
            set
            {
                _useHV4 = value;
                LoadGraphicsContent(true);
            }
        }
             
        public cPhysicsParticlesVT(Game game) : base(game)
        { 
            _maxParticles = 500000;
            _random = new Random();
            _currentTargetIn = 0;
            _currentTargetOut = 1;
            _particleSize = 2;
            _nextEmitRow = 0;
            int multiple = _maxParticles / 768;
            _textureHeight = 768;
            _textureWidth = multiple;
            _maxParticles = _textureWidth * _textureHeight;
            _toAddPositions = new List<Vector4>();
            _toAddVelocities = new List<Vector4>();
            _forces = new Vector4[4];
            for (int i = 0; i < _forces.Length; i++) { _forces[i]=Vector4.Zero; }
            _nextForce = 0;
            _useMRT = false;
            _useHV4 = true;
#if XBOX
            _useMRT = false;
            _useHV4 = true;
#endif
            //LieroXNA.Instance.LieroConsole.AddCommand("psize", "sets the particle size", SetPSize);
            PrimalDevistation.Instance.Console.AddCommand("MRT", "toggles the use of multiple render targets", delegate { UseMRT = !UseMRT; });
            PrimalDevistation.Instance.Console.AddCommand("HV4", "toggles the use of multiple render targets", delegate { UseHV4 = !UseHV4; });
        }

        public void AddForce(Vector2 where, float strength, float reach)
        {
            _forces[_nextForce].X = where.X;
            _forces[_nextForce].Y = where.Y;
            _forces[_nextForce].Z = strength;
            _forces[_nextForce].W = reach;
            _nextForce++;       
            if (_nextForce >= _forces.Length) { _nextForce = 0; }
        }

        //private void SetPSize(string[] args)
        //{
        //    if (args.Length <= 0) { LieroXNA.Instance.LieroConsole.AddLine("USAGE: 'psize [size]'"); return; }
        //    int res;
        //    if (!int.TryParse(args[0], out res)) { LieroXNA.Instance.LieroConsole.AddLine("Size is not a valid integer"); return; }
        //    _particleSize = res;
        //}

        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
                ContentManager cm = PrimalDevistation.Instance.CM;
                GraphicsDevice gd = GraphicsDevice;

                _renderEffect = cm.Load<Effect>(@"Shaders/PointSpriteRenderVT");
                if (_useMRT) { _updateEffect = cm.Load<Effect>(@"Shaders/ParticleUpdateMRT"); }
                else { _updateEffect = cm.Load<Effect>(@"Shaders/ParticleUpdate"); } 
                _particleTexture = cm.Load<Texture2D>(@"Sprites/particle2x2");

                PresentationParameters pp = GraphicsDevice.PresentationParameters;
                _bbResolveTarget = new ResolveTexture2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, 1, pp.BackBufferFormat);

                // Create the sprite batch for render to textures
                _batch = new cSpriteBatch(gd);

                // Setting up some shader vars that will never change now
                _renderEffect.Parameters["View"].SetValue(Matrix.Identity);
                _renderEffect.Parameters["World"].SetValue(Matrix.Identity);

                _velocities = new RenderTarget2D[2];
                _positions = new RenderTarget2D[2];

                if (_useHV4)
                {
                    _positions[0] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.HalfVector4);
                    _positions[1] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.HalfVector4);
                    _velocities[0] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.HalfVector4);
                    _velocities[1] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.HalfVector4);
                }
                else
                {
                    _positions[0] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.Vector4);
                    _positions[1] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.Vector4);
                    _velocities[0] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.Vector4);
                    _velocities[1] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.Vector4);
                }

                gd.SetRenderTarget(0, _positions[0]);
                gd.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

                gd.SetRenderTarget(0, _positions[1]);
                gd.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

                gd.SetRenderTarget(0, _velocities[0]);
                gd.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);

                gd.SetRenderTarget(0, _velocities[1]);
                gd.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);
                gd.SetRenderTarget(0, null);
                       
                if (_useHV4)
                {
                    HalfVector4[] data = new HalfVector4[_maxParticles];
                    _positions[0].GetTexture().GetData<HalfVector4>(data);
                    Vector2 s = PrimalDevistation.Instance.Level.Size;
#if XBOX                    
                    for (int i = 0; i < data.Length; i++) { data[i] = new HalfVector4(0, 0, cMath.Rand(0, s.Y / 2), cMath.Rand(0, s.X / 2)); }
#else
                    for (int i = 0; i < data.Length; i++) { data[i] = new HalfVector4(cMath.Rand(0, s.X / 2), cMath.Rand(0, s.Y / 2), 0, 0); }
#endif

                    _positions[0].GetTexture().SetData<HalfVector4>(data);
                    _positions[1].GetTexture().SetData<HalfVector4>(data);
                }
                else
                {
                    Vector4[] data = new Vector4[_maxParticles];
                    _positions[0].GetTexture().GetData<Vector4>(data);
                    for (int i = 0; i < data.Length; i++) { data[i] = new Vector4(0, 0, 0, 0); }
                    _positions[0].GetTexture().SetData<Vector4>(data);
                    _positions[1].GetTexture().SetData<Vector4>(data);
                }

                _verts = new sVTPointVertex[_maxParticles];
                _dec = new VertexDeclaration(gd, sVTPointVertex.Elements);

                float ratioW = 1f / (_textureWidth - 1);
                float ratioH = 1f / (_textureHeight - 1);

                int icounter = 0;
                for (int i = 0; i < _textureWidth; i++)
                {
                    for (int j = 0; j < _textureHeight; j++)
                    {
                        _verts[icounter] = new sVTPointVertex();
                        _verts[icounter].texture = new Vector2(i * ratioW, j * ratioH);
                        _verts[icounter].pointSize = cMath.Rand(1, 3);
                        icounter++;
                    }
                }

                // Create a dynamic vertex buffer.
                ResourceUsage usage = ResourceUsage.Dynamic | ResourceUsage.WriteOnly | ResourceUsage.Points;
              
                _vertexBuffer = new VertexBuffer(gd, sVTPointVertex.Size*_maxParticles, usage, ResourceManagementMode.Manual);
                _vertexBuffer.SetData(_verts);
            }

        }

        public override void Update(GameTime gameTime) 
        {            
        }
                
        public void UpdatePartcles(GraphicsDevice gd, GameTime gameTime)
        {

            //Texture2D tex = LieroXNA.Instance.Level.TerrainMap.Texture;
            //Color[] data = new Color[tex.Width * tex.Width];
            //tex.GetData<Color>(data);

            Texture2D terrainTex = PrimalDevistation.Instance.Level.TerrainMap.Texture;

            if (_useMRT)
            {

                // Set the two render targets 
                gd.SetRenderTarget(0, _velocities[_currentTargetOut]);
                gd.SetRenderTarget(1, _positions[_currentTargetOut]);

                // Set some shader params
                _updateEffect.Parameters["VelocityTexture"].SetValue(_velocities[_currentTargetIn].GetTexture());
                _updateEffect.Parameters["PositionTexture"].SetValue(_positions[_currentTargetIn].GetTexture());
                _updateEffect.Parameters["TerrainTexture"].SetValue(terrainTex);
                _updateEffect.Parameters["RatioWH"].SetValue(new Vector2(1f / (terrainTex.Width), 1f / (terrainTex.Height)));
                _updateEffect.Parameters["Gravity"].SetValue(PrimalDevistation.GRAVITY);
                _updateEffect.Parameters["Friction"].SetValue(0.5f);
                _updateEffect.Parameters["FrameDelta"].SetValue((float)gameTime.ElapsedGameTime.TotalSeconds);
                _updateEffect.Parameters["viewProjection"].SetValue(Matrix.Identity * PrimalDevistation.Instance.Camera.Projection);

                //MouseState ms = Mouse.GetState();
                //Vector2 cam = LieroXNA.Instance.Camera.Position;
                //_forces[0] = new Vector4(ms.X - cam.X, ms.Y - cam.Y, -0.1f, 10);

                _updateEffect.Parameters["Forces"].SetValue(_forces);

                _batch.Effect = _updateEffect;
                _batch.RenderToTexture(_positions[_currentTargetOut], gd);

                // Resolve and reset the render targets
                gd.ResolveRenderTarget(1);
                gd.ResolveRenderTarget(0);
                gd.SetRenderTarget(1, null);
                gd.SetRenderTarget(0, null);

            }
            else
            {
                // Set the update params
                _updateEffect.Parameters["VelocityTexture"].SetValue(_velocities[_currentTargetIn].GetTexture());
                _updateEffect.Parameters["PositionTexture"].SetValue(_positions[_currentTargetIn].GetTexture());
                _updateEffect.Parameters["TerrainTexture"].SetValue(terrainTex);
                _updateEffect.Parameters["RatioWH"].SetValue(new Vector2(1f / (terrainTex.Width), 1f / (terrainTex.Height)));
                _updateEffect.Parameters["Gravity"].SetValue(PrimalDevistation.GRAVITY);
                _updateEffect.Parameters["Friction"].SetValue(0.5f);
                _updateEffect.Parameters["FrameDelta"].SetValue((float)gameTime.ElapsedGameTime.TotalSeconds);
                _updateEffect.Parameters["viewProjection"].SetValue(Matrix.Identity * PrimalDevistation.Instance.Camera.Projection);
                _updateEffect.Parameters["Forces"].SetValue(_forces);

                gd.SetRenderTarget(0, _velocities[_currentTargetOut]);
                _updateEffect.CurrentTechnique = _updateEffect.Techniques["UpdateVel"];
                _batch.Effect = _updateEffect;
                _batch.RenderToTexture(_velocities[_currentTargetOut], gd);
                gd.ResolveRenderTarget(0);
                gd.SetRenderTarget(0, null);

                // First render the positions
                gd.SetRenderTarget(0, _positions[_currentTargetOut]);
                _updateEffect.CurrentTechnique = _updateEffect.Techniques["UpdatePos"];
                _batch.Effect = _updateEffect;
                _batch.RenderToTexture(_positions[_currentTargetOut], gd);
                gd.ResolveRenderTarget(0);   
                gd.SetRenderTarget(0, null);
            }

            _updateEffect.Parameters["TerrainTexture"].SetValue((Texture2D)null);

        }

        public override void  Draw(GameTime gameTime)
        {
            GraphicsDevice gd = GraphicsDevice;
            gd.ResolveBackBuffer(_bbResolveTarget);

            // Add the new spawns now 
            PushSpawnsToTexture();

            //HalfVector4[] dataPosOUT = new HalfVector4[_maxParticles];
            //_positions[_currentTargetIn].GetTexture().GetData<HalfVector4>(dataPosOUT);

            //HalfVector4[] dataVelOUT = new HalfVector4[_maxParticles];
            //_velocities[_currentTargetIn].GetTexture().GetData<HalfVector4>(dataVelOUT);

                       
            // First update all the particles   
            UpdatePartcles(gd, gameTime);

            PrimalDevistation.Instance.Batch.Begin();
            PrimalDevistation.Instance.Batch.Draw(_bbResolveTarget, new Rectangle(0, 0, gd.Viewport.Width, gd.Viewport.Height), Color.White);
            PrimalDevistation.Instance.Batch.End();

            //dataPosOUT = new HalfVector4[_maxParticles];
            //_positions[_currentTargetOut].GetTexture().GetData<HalfVector4>(dataPosOUT);

            //dataVelOUT = new HalfVector4[_maxParticles];
            //_velocities[_currentTargetOut].GetTexture().GetData<HalfVector4>(dataVelOUT);
                       
            // Set some shader vals
            _renderEffect.Parameters["Projection"].SetValue(PrimalDevistation.Instance.Camera.Projection);
            _renderEffect.Parameters["PositionsTexture"].SetValue(_positions[_currentTargetOut].GetTexture());
            _renderEffect.Parameters["VelocitiesTexture"].SetValue(_velocities[_currentTargetOut].GetTexture());
            //_renderEffect.Parameters["ParticleSize"].SetValue(_particleSize);
            _renderEffect.Parameters["CameraPos"].SetValue(PrimalDevistation.Instance.Camera.Position);
            _renderEffect.Parameters["ZoomCentre"].SetValue(PrimalDevistation.Instance.Camera.ZoomPoint);
            _renderEffect.Parameters["Zoom"].SetValue(PrimalDevistation.Instance.Camera.Zoom);

            // Pass in the verts used to render the particles as point sprites
            gd.VertexDeclaration = _dec;

            // Do the render
            _renderEffect.Begin();
            gd.RenderState.AlphaBlendEnable = true;
            foreach (EffectPass pass in _renderEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                gd.Textures[0] = _particleTexture;
                gd.Vertices[0].SetSource(_vertexBuffer, 0, sVTPointVertex.Size);
                gd.DrawPrimitives(PrimitiveType.PointList, 0, _maxParticles);
                //gd.DrawUserPrimitives(PrimitiveType.PointList, _verts, 0, _maxParticles);
                pass.End();

            }
            _renderEffect.End();

            // Swap the in and out textuers around now they have finished
            int tmp = _currentTargetOut;
            _currentTargetOut = _currentTargetIn;
            _currentTargetIn = tmp;
            for (int i = 0; i < _forces.Length; i++) { _forces[i].Z = 0; }
            _nextForce = 0;        
        }
        
        private void PushSpawnsToTexture()
        {
            // If there are no particles to add then we dont need to push any¬
            if (_toAddPositions.Count == 0) { return; }

            // Grab the textures we will be writing to
            Texture2D posTex = _positions[_currentTargetIn].GetTexture();
            Texture2D velTex = _velocities[_currentTargetIn].GetTexture();

            int num = _toAddPositions.Count;
            int multiple = (num / posTex.Width) + 1;

            // Making sure we have enough particles to accomidate this explosion 
            // TODO: This could be made better to accomidate larger explosions!
            if (_nextEmitRow + multiple >= posTex.Height) { _nextEmitRow = 0; }
            if (_nextEmitRow + multiple >= posTex.Height) { return; }

            if (_useHV4)
            {
                
                HalfVector4[] tmpPos, tmpVel;
                tmpPos = new HalfVector4[multiple * posTex.Width];
                tmpVel = new HalfVector4[multiple * posTex.Width];

#if XBOX
                for (int i = 0; i < _toAddPositions.Count; i++) { tmpPos[i] = new HalfVector4(_toAddPositions[i].W, _toAddPositions[i].Z, _toAddPositions[i].Y, _toAddPositions[i].X); }
                for (int i = 0; i < _toAddVelocities.Count; i++) { tmpVel[i] = new HalfVector4(_toAddVelocities[i].W, _toAddVelocities[i].Z, _toAddVelocities[i].Y, _toAddVelocities[i].X); }
#else
                for (int i = 0; i < _toAddPositions.Count; i++) { tmpPos[i] = new HalfVector4(_toAddPositions[i]); }
                for (int i = 0; i < _toAddVelocities.Count; i++) { tmpVel[i] = new HalfVector4(_toAddVelocities[i]); }
#endif

                Rectangle r = new Rectangle(0, _nextEmitRow, posTex.Width, multiple);
                posTex.SetData<HalfVector4>(0, r, tmpPos, 0, tmpPos.Length, SetDataOptions.None);
                velTex.SetData<HalfVector4>(0, r, tmpVel, 0, tmpPos.Length, SetDataOptions.None);
            }
            else
            {
                Vector4[] tmpPos, tmpVel;
                tmpPos = new Vector4[multiple * posTex.Width];
                tmpVel = new Vector4[multiple * posTex.Width];

                _toAddPositions.CopyTo(tmpPos);
                _toAddVelocities.CopyTo(tmpVel);

                Rectangle r = new Rectangle(0, _nextEmitRow, posTex.Width, multiple);
                posTex.SetData<Vector4>(0, r, tmpPos, 0, tmpPos.Length, SetDataOptions.NoOverwrite);
                velTex.SetData<Vector4>(0, r, tmpVel, 0, tmpPos.Length, SetDataOptions.NoOverwrite);

            }


   
            // Increment for the next load of particles and clear the list
            _nextEmitRow += multiple;
            _toAddPositions.Clear();
            _toAddVelocities.Clear();     
        }

        public void SpawnParticles(List<Vector2> positions, List<Vector2> velocities, List<Color> colors)
        {
            //_toAddPositions.AddRange(positions);
            //_toAddVelocities.AddRange(velocities);
        }

        #region accuateParticleSpawn

        //public void SpawnParticles(List<Vector4> positions, List<Vector4> velocities)
        //{
        //    Texture2D posTex = _positions[_currentTargetIn].GetTexture();
        //    Texture2D velTex = _velocities[_currentTargetIn].GetTexture();

        //    int num = positions.Count;

        //    // Making sure we have enough particles to accomidate this explosion 
        //    // TODO: This could be made better to accomidate larger explosions!
        //    if (_nextEmitIndex + num >= _maxParticles) { _nextEmitIndex = 0; }
        //    if (_nextEmitIndex + num >= _maxParticles) { return; }

        //    int y = _nextEmitIndex / posTex.Width;
        //    int x = _nextEmitIndex - (y * posTex.Width);

        //    int onFirst = posTex.Width - x;
        //    num -= onFirst;
        //    int multiple = num / posTex.Width;
        //    num -= (multiple * posTex.Width);
        //    int onLast = num;

        //    int rows = 1 + multiple;
        //    if (onLast != 0) { rows++; }    


        //    // Do the first row first
        //    Vector4[] tmpPos = new Vector4[onFirst];
        //    Vector4[] tmpVel = new Vector4[onFirst];
        //    for (int i = 0; i < onFirst; i++) { tmpPos[i] = positions[i]; tmpVel[i] = velocities[i]; }
        //    posTex.SetData<Vector4>(0, new Rectangle(x, y, onFirst, 1), tmpPos, 0, tmpPos.Length, SetDataOptions.None);
        //    velTex.SetData<Vector4>(0, new Rectangle(x, y, onFirst, 1), tmpVel, 0, tmpVel.Length, SetDataOptions.None);

        //    // Do the middle rows next
        //    if (multiple > 0)
        //    {
        //        int numMiddleEls = multiple * posTex.Width;
        //        tmpPos = new Vector4[numMiddleEls];
        //        tmpVel = new Vector4[numMiddleEls];
        //        for (int i = 0; i < numMiddleEls; i++) { tmpPos[i] = positions[i + onFirst]; tmpVel[i] = velocities[i + onFirst]; }
        //        posTex.SetData<Vector4>(0, new Rectangle(0, y + 1, posTex.Width, multiple), tmpPos, 0, tmpPos.Length, SetDataOptions.None);
        //        velTex.SetData<Vector4>(0, new Rectangle(0, y + 1, posTex.Width, multiple), tmpVel, 0, tmpVel.Length, SetDataOptions.None);

        //        if (onLast != 0)
        //        {
        //            tmpPos = new Vector4[onLast];
        //            tmpVel = new Vector4[onLast];
        //            for (int i = 0; i < onLast; i++) { tmpPos[i] = positions[i + onFirst + numMiddleEls]; tmpVel[i] = velocities[i + onFirst + numMiddleEls]; }
        //            posTex.SetData<Vector4>(0, new Rectangle(0, y + 1 + multiple, onLast, 1), tmpPos, 0, tmpPos.Length, SetDataOptions.None);
        //            velTex.SetData<Vector4>(0, new Rectangle(0, y + 1 + multiple, onLast, 1), tmpVel, 0, tmpVel.Length, SetDataOptions.None);
        //        }
        //    }

        //    _nextEmitIndex += positions.Count;
        //}

        #endregion


        /// <summary>
        /// The vertex type used to render the particles
        /// </summary>
        protected struct sVTPointVertex
        {
            public Vector2 texture;       
            public float pointSize;

            public static VertexElement[] Elements =
                {                
                new VertexElement(0, 0, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(0, 8, VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.PointSize, 0),
                };

            public const int Size =  8 + 4;
        }
    }
}
