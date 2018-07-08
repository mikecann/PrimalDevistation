using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using PrimalDevistation.Level;

namespace PrimalDevistation.Particles
{
    class cPhysicsParticlesCPU : DrawableGameComponent, iPhysicsParticleSystem
    {
        private class cCPUParticle
        {
            public float _velX;
            public float _velY;           
        }

        private PointVertex[] _pointVertices;
        private VertexBuffer _vertexBuffer;
        private VertexDeclaration _vertexDeclaration;
        private cCPUParticle[] _particles;
        private Effect _renderEffect;
        private int _spawnIndex;
        private int _maxParticles;
        private Matrix _proj;
        private Texture2D _texture;
        private Color[] _colors = new Color[] { Color.Red, Color.Orange, Color.Red, Color.Yellow };
        private int _particleSize;

        public cPhysicsParticlesCPU(Game game)
            : base(game)
        {
            _maxParticles = 10000;
            _spawnIndex = 0;
            PrimalDevistation.Instance.Console.AddCommand("psize", "sets the particle size", SetPSize);
        }

        private void SetPSize(string[] args)
        {
            if (args.Length <= 0) { PrimalDevistation.Instance.Console.AddLine("USAGE: 'psize [size]'"); return; }
            int res = int.Parse(args[0]);         
            //if (!int.TryParse(args[0], out res)) { LieroXNA.Instance.LieroConsole.AddLine("Size is not a valid integer"); return; }

            _particleSize = res;
        }

        public void AddForce(Vector2 where, float strength, float holeSize)
        {
        }

        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
                ContentManager cm = PrimalDevistation.Instance.CM;
                GraphicsDevice gd = GraphicsDevice;
                Random r = new Random();

                // Load the standard point sprite render effect
                _renderEffect = cm.Load<Effect>(@"Shaders/PointSpriteRender");
                _texture = cm.Load<Texture2D>(@"Sprites/particle2x2");

                // Create the vertex buffer
                _vertexBuffer = new VertexBuffer(gd, _maxParticles * PointVertex.Size, ResourceUsage.Points | ResourceUsage.WriteOnly, ResourceManagementMode.Automatic);

                // Create the declaration
                _vertexDeclaration = new VertexDeclaration(gd, PointVertex.Elements);

                // init the points
                _pointVertices = new PointVertex[_maxParticles];
                _particles = new cCPUParticle[_maxParticles];
                for (int i = 0; i < _maxParticles; i++)
                {
                    _pointVertices[i] = new PointVertex();
                    _pointVertices[i].position = new Vector3(-100,-100,0);
                    _pointVertices[i].color = _colors[r.Next(0, _colors.Length)].ToVector4();
                    _pointVertices[i].pointSize = 2;
                    _particles[i] = new cCPUParticle();                  
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
        }

        public override void Update(GameTime gameTime)
        {

            cCollisionMap cm = PrimalDevistation.Instance.Level.CollisionMap;
            float fpOldX, fpOldY, fpNewX, fpNewY;
            int oldX, oldY, newX, newY;        

            // Update the particles in a fountain type of way
            for (int i = 0; i < _maxParticles; i++)
            {
                cCPUParticle p = _particles[i];
                p._velX += PrimalDevistation.GRAVITY.X;
                p._velY += PrimalDevistation.GRAVITY.Y;

                fpOldX = _pointVertices[i].position.X;
                fpOldY = _pointVertices[i].position.Y;
                fpNewX = fpOldX + p._velX;
                fpNewY = fpOldY + p._velY;
                oldX = (int)fpOldX;
                oldY = (int)fpOldY;
                newX = (int)fpNewX;
                newY = (int)fpNewY;

                // If we have actually moved a pixel in the last frame (could have been less!)
                if (oldX != newX)
                {
                    // If we collide at this new X position we need to rebound
                    if (cm.CheckCollision(newX, oldY))
                    {
                        p._velX = -p._velX * 0.5f;
                        fpNewX = fpOldX;
                    }
                }

                // If we have actually moved a pixel in the last frame (could have been less!)
                if (oldY != newY)
                {
                    // If we collide at this new X position we need to rebound
                    if (cm.CheckCollision(oldX, newY))
                    {
                        p._velY = -p._velY * 0.5f;
                        fpNewY = fpOldY;
                    }
                }
                
                // Finally set the new position 
                _pointVertices[i].position.X = fpNewX;
                _pointVertices[i].position.Y = fpNewY;  
            }
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice gd = GraphicsDevice;

            //gd.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.Black, 1.0f, 0);

            _vertexBuffer.SetData(_pointVertices, 0, _maxParticles, SetDataOptions.None);
            gd.Vertices[0].SetSource(_vertexBuffer, 0, PointVertex.Size);

            gd.VertexDeclaration = _vertexDeclaration;
            _renderEffect.Parameters["View"].SetValue(Matrix.Identity);
            _renderEffect.Parameters["Projection"].SetValue(_proj);
            _renderEffect.Parameters["World"].SetValue(Matrix.Identity);
            _renderEffect.Parameters["ParticleSize"].SetValue(_particleSize);
            _renderEffect.Parameters["CameraPos"].SetValue(PrimalDevistation.Instance.Camera.Position);
            _renderEffect.Parameters["ZoomCentre"].SetValue(PrimalDevistation.Instance.Camera.ZoomPoint);
            _renderEffect.Parameters["Zoom"].SetValue(PrimalDevistation.Instance.Camera.Zoom);

            _renderEffect.CurrentTechnique = _renderEffect.Techniques[0];
            
            _renderEffect.Begin();
            gd.RenderState.AlphaBlendEnable = true;
            foreach (EffectPass pass in _renderEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                gd.Textures[0] = _texture;
                gd.DrawPrimitives(PrimitiveType.PointList, 0, _maxParticles);
                pass.End();

            }
            _renderEffect.End();
        }

        public void SpawnParticles(List<Vector2> positions, List<Vector2> velocities, List<Color> colors)
        {
            //for (int i = 0; i < positions.Count; i++)
            //{
            //    cCPUParticle p = _particles[_spawnIndex];
            //    p._velX = velocities[i].X;
            //    p._velY = velocities[i].Y;
            //    _pointVertices[_spawnIndex].position.X = positions[i].X;
            //    _pointVertices[_spawnIndex].position.Y = positions[i].Y;
            //    _pointVertices[_spawnIndex].color.X = positions[i].Z;
            //    _pointVertices[_spawnIndex].color.Y = positions[i].W;
            //    _pointVertices[_spawnIndex].color.Z = velocities[i].Z;
            //    _spawnIndex++;
            //    if (_spawnIndex >= _maxParticles) { _spawnIndex = 0; }
            //}
        }

        /// <summary>
        /// The vertex type used to render the particles
        /// </summary>
        protected struct PointVertex
        {

            public Vector3 position;
            public Vector4 color;
            public float pointSize;

            public static VertexElement[] Elements =
                {
                new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0),
                new VertexElement(0, 12, VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.Color, 0),
                new VertexElement(0, 28, VertexElementFormat.Single, VertexElementMethod.Default, VertexElementUsage.PointSize, 0),
                };

            public const int Size = 12 + 16 + 4;
        }
    }    
}
