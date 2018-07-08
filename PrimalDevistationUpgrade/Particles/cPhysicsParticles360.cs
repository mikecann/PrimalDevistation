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
    public class cPhysicsParticles360 : DrawableGameComponent, iPhysicsParticleSystem
    {
        private Effect _renderEffect;
        private Effect _updateEffect;
        private RenderTarget2D[] _positions;   
        private RenderTarget2D[] _velocities;
        private RenderTarget2D[] _ages;
        private Texture2D _colors;
        private Texture2D _particleTexture;
        private int _maxParticles;
        private int _currentTargetIn;
        private int _currentTargetOut;
        private int _textureWidth;
        private int _textureHeight;
        private int _nextEmitRow;
        private Random _random; 
        private List<Vector2> _toAddPositions;
        private List<Vector2> _toAddVelocities;
        private List<Color> _toAddColors;
        private sVTPointVertex[] _verts;
        private VertexDeclaration _dec;
        private VertexBuffer _vertexBuffer;
        private cSpriteBatch _batch;
        private int _nextForce;
        private Vector4[] _forces;
        private ResolveTexture2D _bbResolveTarget;
 
        public cPhysicsParticles360(Game game) : base(game)
        { 
            _maxParticles = 100000;
            _random = new Random();
            _currentTargetIn = 0;
            _currentTargetOut = 1;
            _nextEmitRow = 0;
            int multiple = _maxParticles / 768;
            _textureHeight = 768;
            _textureWidth = multiple;
            _maxParticles = _textureWidth * _textureHeight;
            _toAddPositions = new List<Vector2>();
            _toAddVelocities = new List<Vector2>();
            _toAddColors = new List<Color>();
            _forces = new Vector4[4];
            for (int i = 0; i < _forces.Length; i++) { _forces[i]=Vector4.Zero; }
            _nextForce = 0;            
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
        
        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
                ContentManager cm = PrimalDevistation.Instance.CM;
                GraphicsDevice gd = GraphicsDevice;

                _renderEffect = cm.Load<Effect>(@"Shaders/PointSpriteRender360");
                _updateEffect = cm.Load<Effect>(@"Shaders/ParticleUpdate360");             
                _particleTexture = cm.Load<Texture2D>(@"Sprites/particle2x2");      

                PresentationParameters pp = GraphicsDevice.PresentationParameters;
                _bbResolveTarget = new ResolveTexture2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, 1, pp.BackBufferFormat);
     
                // Create the sprite batch for render to textures
                _batch = new cSpriteBatch(gd);

                // Setting up some shader vars that will never change now
                _renderEffect.Parameters["View"].SetValue(Matrix.Identity);
                _renderEffect.Parameters["World"].SetValue(Matrix.Identity);

                _colors = new Texture2D(gd, _textureWidth, _textureHeight, 1, TextureUsage.None, SurfaceFormat.Color);

                _velocities = new RenderTarget2D[2];
                _positions = new RenderTarget2D[2];
                _ages = new RenderTarget2D[2];    
                _positions[0] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.Vector2);
                _positions[1] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.Vector2);
                _velocities[0] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.Vector2);
                _velocities[1] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.Vector2);
                _ages[0] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.Single);
                _ages[1] = new RenderTarget2D(gd, _textureWidth, _textureHeight, 1, SurfaceFormat.Single);  

                gd.SetRenderTarget(0, _positions[0]);
                gd.Clear(ClearOptions.Target, new Color(0,0,0,0), 1.0f, 0);
                gd.SetRenderTarget(0, _positions[1]);
                gd.Clear(ClearOptions.Target, new Color(0, 0, 0, 0), 1.0f, 0);
                gd.SetRenderTarget(0, _velocities[0]);
                gd.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);
                gd.SetRenderTarget(0, _velocities[1]);
                gd.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);
                gd.SetRenderTarget(0, _ages[0]);
                gd.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);
                gd.SetRenderTarget(0, _ages[1]);
                gd.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);
                gd.SetRenderTarget(0, null);


                //Vector2[] data = new Vector2[_maxParticles];
                //_positions[0].GetTexture().GetData<Vector2>(data);
                //Vector2 s = LieroXNA.Instance.Level.Size;
                //for (int i = 0; i < data.Length; i++) { data[i] = new Vector2(cMath.Rand(0, s.X / 2), cMath.Rand(0, s.Y / 2)); }
                //_positions[0].GetTexture().SetData<Vector2>(data);
                //_positions[1].GetTexture().SetData<Vector2>(data);     

                Texture2D[] particleTextures = new Texture2D[10];

                string part = @"Sprites/particles/part04";

                //particleTextures[0] = cm.Load<Texture2D>(part);
                //particleTextures[1] = cm.Load<Texture2D>(part);
                //particleTextures[2] = cm.Load<Texture2D>(part);
                //particleTextures[3] = cm.Load<Texture2D>(part);
                //particleTextures[4] = cm.Load<Texture2D>(part);
                //particleTextures[5] = cm.Load<Texture2D>(part);
                //particleTextures[6] = cm.Load<Texture2D>(part);
                //particleTextures[7] = cm.Load<Texture2D>(part);
                //particleTextures[8] = cm.Load<Texture2D>(part);
                //particleTextures[9] = cm.Load<Texture2D>(part);

                particleTextures[0] = cm.Load<Texture2D>(@"Sprites/particles/part01");
                particleTextures[1] = cm.Load<Texture2D>(@"Sprites/particles/part02");
                particleTextures[2] = cm.Load<Texture2D>(@"Sprites/particles/part03");
                particleTextures[3] = cm.Load<Texture2D>(@"Sprites/particles/part04");
                particleTextures[4] = cm.Load<Texture2D>(@"Sprites/particles/part05");
                particleTextures[5] = cm.Load<Texture2D>(@"Sprites/particles/part06");
                particleTextures[6] = cm.Load<Texture2D>(@"Sprites/particles/part07");
                particleTextures[7] = cm.Load<Texture2D>(@"Sprites/particles/part08");
                particleTextures[8] = cm.Load<Texture2D>(@"Sprites/particles/part09");
                particleTextures[9] = cm.Load<Texture2D>(@"Sprites/particles/part10");

                for (int i = 0; i < particleTextures.Length; i++) 
                {  
                    _renderEffect.Parameters["ParticleTex" + i].SetValue(particleTextures[i]); 
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
                        _verts[icounter].texIndex = cMath.Rand(0, particleTextures.Length-1);                   
                        _verts[icounter].pointSize = particleTextures[(int)_verts[icounter].texIndex].Width;               
                        icounter++;
                    }
                }
              
                _vertexBuffer = new VertexBuffer(gd, sVTPointVertex.Size*_maxParticles, BufferUsage.Points); 
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


            // Set the update params
            _updateEffect.Parameters["VelocityTexture"].SetValue(_velocities[_currentTargetIn].GetTexture());
            _updateEffect.Parameters["PositionTexture"].SetValue(_positions[_currentTargetIn].GetTexture());
            _updateEffect.Parameters["AgeTexture"].SetValue(_ages[_currentTargetIn].GetTexture());
            _updateEffect.Parameters["TerrainTexture"].SetValue(terrainTex);
            _updateEffect.Parameters["RatioWH"].SetValue(new Vector2(1f / (terrainTex.Width), 1f / (terrainTex.Height)));
            _updateEffect.Parameters["Gravity"].SetValue(PrimalDevistation.GRAVITY);
            _updateEffect.Parameters["Friction"].SetValue(0.5f);
            _updateEffect.Parameters["FrameDelta"].SetValue((float)gameTime.ElapsedGameTime.TotalSeconds);
            _updateEffect.Parameters["viewProjection"].SetValue(Matrix.Identity * PrimalDevistation.Instance.Camera.Projection);
            _updateEffect.Parameters["Forces"].SetValue(_forces);

            // Update the velocities
            gd.SetRenderTarget(0, _velocities[_currentTargetOut]);
            _updateEffect.CurrentTechnique = _updateEffect.Techniques["UpdateVel"];
            _batch.Effect = _updateEffect;
            _batch.RenderToTexture(_velocities[_currentTargetOut], gd);
            gd.SetRenderTarget(0, null);  

            // Update the positions
            gd.SetRenderTarget(0, _positions[_currentTargetOut]);
            _updateEffect.CurrentTechnique = _updateEffect.Techniques["UpdatePos"];
            _batch.Effect = _updateEffect;
            _batch.RenderToTexture(_positions[_currentTargetOut], gd);
            gd.SetRenderTarget(0, null);

            // Update the ages
            gd.SetRenderTarget(0, _ages[_currentTargetOut]);
            _updateEffect.CurrentTechnique = _updateEffect.Techniques["UpdateAge"];
            _batch.Effect = _updateEffect;
            _batch.RenderToTexture(_ages[_currentTargetOut], gd);
            gd.SetRenderTarget(0, null);

            _updateEffect.Parameters["TerrainTexture"].SetValue((Texture2D)null);

        }

        public override void  Draw(GameTime gameTime)
        {
            GraphicsDevice gd = GraphicsDevice;
            gd.ResolveBackBuffer(_bbResolveTarget);

            // Add the new spawns now 
            PushSpawnsToTexture();

            //Vector2[] dataPosOUT = new Vector2[_maxParticles];
            //_positions[_currentTargetIn].GetTexture().GetData<Vector2>(dataPosOUT);

            //Vector2[] dataVelOUT = new Vector2[_maxParticles];
            //_velocities[_currentTargetIn].GetTexture().GetData<Vector2>(dataVelOUT);
            
            // First update all the particles   
            UpdatePartcles(gd, gameTime);

            //dataPosOUT = new Vector2[_maxParticles];
            //_positions[_currentTargetOut].GetTexture().GetData<Vector2>(dataPosOUT);

            //dataVelOUT = new Vector2[_maxParticles];
            //_velocities[_currentTargetOut].GetTexture().GetData<Vector2>(dataVelOUT);

            PrimalDevistation.Instance.Batch.Begin();
            PrimalDevistation.Instance.Batch.Draw(_bbResolveTarget, new Rectangle(0, 0, gd.Viewport.Width, gd.Viewport.Height), Color.White);
            PrimalDevistation.Instance.Batch.End();                             

            // Set some shader vals
            _renderEffect.Parameters["Projection"].SetValue(PrimalDevistation.Instance.Camera.Projection);
            _renderEffect.Parameters["PositionsTexture"].SetValue(_positions[_currentTargetOut].GetTexture());
            _renderEffect.Parameters["VelocitiesTexture"].SetValue(_velocities[_currentTargetOut].GetTexture());
            _renderEffect.Parameters["ColorsTexture"].SetValue(_colors);
            _renderEffect.Parameters["AgeTexture"].SetValue(_ages[_currentTargetOut].GetTexture()); 
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
            Texture2D ageTex = _ages[_currentTargetIn].GetTexture();

            int num = _toAddPositions.Count;
            int multiple = (num / posTex.Width) + 1;

            // Making sure we have enough particles to accomidate this explosion 
            // TODO: This could be made better to accomidate larger explosions!
            if (_nextEmitRow + multiple >= posTex.Height) { _nextEmitRow = 0; }
            if (_nextEmitRow + multiple >= posTex.Height) { return; }            
                
            Vector2[] tmpPos, tmpVel;
            Color[] tmpCol;
            Single[] tmpAge;
            tmpPos = new Vector2[multiple * posTex.Width];
            tmpVel = new Vector2[multiple * posTex.Width];
            tmpCol = new Color[multiple * posTex.Width];
            tmpAge = new Single[multiple * posTex.Width];

            _toAddPositions.CopyTo(tmpPos);
            _toAddVelocities.CopyTo(tmpVel);
            _toAddColors.CopyTo(tmpCol);

            Rectangle r = new Rectangle(0, _nextEmitRow, posTex.Width, multiple);
            posTex.SetData<Vector2>(0, r, tmpPos, 0, tmpPos.Length, SetDataOptions.None);
            velTex.SetData<Vector2>(0, r, tmpVel, 0, tmpVel.Length, SetDataOptions.None);
            ageTex.SetData<Single>(0, r, tmpAge, 0, tmpVel.Length, SetDataOptions.None);
            _colors.SetData<Color>(0, r, tmpCol, 0, tmpCol.Length, SetDataOptions.None);  
              
            // Increment for the next load of particles and clear the list
            _nextEmitRow += multiple;
            _toAddPositions.Clear();
            _toAddVelocities.Clear();
            _toAddColors.Clear(); 
        }

        public void SpawnParticles(List<Vector2> positions, List<Vector2> velocities, List<Color> colors)
        {
            _toAddPositions.AddRange(positions);
            _toAddVelocities.AddRange(velocities);
            _toAddColors.AddRange(colors);
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
            public float texIndex;

            public static VertexElement[] Elements =
                {                
                new VertexElement(0, 0, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0),
                new VertexElement(0, 8, VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.PointSize, 0),
                new VertexElement(0, 12, VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.Depth, 0),
                };

            public const int Size =  8 + 4 + 4;
        }
    }
}
