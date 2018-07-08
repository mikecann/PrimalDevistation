using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PrimalDevistation.Particles;
using PrimalDevistation.Objects;

namespace PrimalDevistation.Level
{
    public class cCollisionMap
    {
        public class cExplosion 
        {
            public Vector4[,] _partVel;
            public byte[,] _data;      
        }

        private Color[,] _destructableData;
        private Color[,] _indestructableData;
        private bool _explosionAdded;
        private Texture2D _debugDrawTexture;
        private Dictionary<int, cExplosion> _explosions;

        public Dictionary<int, cExplosion> Explosions
        {
            get { return _explosions; }
            set { _explosions = value; }
        }
	

        public cCollisionMap()
        {  
            _explosions = new Dictionary<int, cExplosion>();
        }

        public bool CheckCollision(int x, int y) { return CheckCollision(new Point(x, y)); }   
        public bool CheckCollision(Vector2 v) { return CheckCollision(new Point((int)v.X, (int)v.Y)); }         
        public bool CheckCollision(Point p)
        {
            if (p.X < 0 || p.X > _destructableData.GetLength(0)-1) { return true; }
            if (p.Y < 0 || p.Y > _destructableData.GetLength(1) - 1) { return true; }
            Color data = _destructableData[p.X, p.Y];
            Color data2 = _indestructableData[p.X, p.Y];
            if (data.A != 0 || data2.A!=0) { return true; }
            return false;
        }

        public bool CheckCollision(int intX, int intY, Rectangle bounds)
        {
            intX += bounds.X;
            intY += bounds.Y;

            // Loop through all y values
            for (int y = intY; y < intY + bounds.Height - 1; y++)
            {
                // If its outside the collision area then we go no further
                if (y<0 || y >= _destructableData.GetLength(1) - 1) { return true; }  
         
                // Loop through all the y values
                for (int x = intX; x < intX + bounds.Width - 1; x++)
                {
                    // If its outside the y area go to the next x
                    if (x<0 || x >= _destructableData.GetLength(0) - 1) { return true; }

                    if (_destructableData[x, y].A != 0 || _indestructableData[x, y].A != 0) { return true; }           
                }
            }

            return false;
        }

        public bool CheckCollisionDestructable(int intX, int intY, byte[,] ppColData, SpriteEffects flips)
        {
            // Move to the top left of the explosion        
            if (intX < 0 || intY < 0) { return true; }
            int ex = 0;
            int ey = 0;
            int h = ppColData.GetLength(0);
            int w = ppColData.GetLength(1);
            
            // Loop through all y values
            for (int y = intY; y < intY + h - 1; y++)
            {
                // If its outside the collision area then we go no further
                if (y<0 || y >= _destructableData.GetLength(1) - 1) { return true; }

                ex = 0;

                // Loop through all the y values
                for (int x = intX; x < intX + w - 1; x++)
                {
                    // If its outside the y area go to the next x
                    if (x<0 || x >= _destructableData.GetLength(0) - 1) { return true; }
                    if (_destructableData[x, y].A != 0)
                    {
                        if (flips==SpriteEffects.FlipHorizontally)
                        {
                            if (ppColData[ey, w-1-ex] != 0) { return true; }
                        }
                        else
                        {
                            if (ppColData[ey, ex] != 0) { return true; }
                        }                        
                    }
                    ex++;
                }

                ey++;
            }
            
            return false;
        }

        public bool CheckCollisionIndestructable(int intX, int intY, byte[,] ppColData, SpriteEffects flips)
        {
            // Move to the top left of the explosion        
            if (intX < 0 || intY < 0) { return true; }
            int ex = 0;
            int ey = 0;
            int h = ppColData.GetLength(0);
            int w = ppColData.GetLength(1);

            // Loop through all y values
            for (int y = intY; y < intY + h - 1; y++)
            {
                // If its outside the collision area then we go no further
                if (y<0 || y >= _indestructableData.GetLength(1) - 1) { return true; }

                ex = 0;

                // Loop through all the y values
                for (int x = intX; x < intX + w - 1; x++)
                {
                    // If its outside the y area go to the next x
                    if (x<0 || x >= _indestructableData.GetLength(0) - 1) { return true; }
                    if (_indestructableData[x, y].A != 0)
                    {
                        if (flips == SpriteEffects.FlipHorizontally)
                        {
                            if (ppColData[ey, w - 1 - ex] != 0) { return true; }
                        }
                        else
                        {
                            if (ppColData[ey, ex] != 0) { return true; }
                        }
                    }
                    ex++;
                }

                ey++;
            }

            return false;
        }

        public void Init(Texture2D destruct, Texture2D indestruct, Texture2D expMap)
        {
            Color[] expData = new Color[destruct.Width * destruct.Height];
            expMap.GetData<Color>(expData);

            Color[] destructData = new Color[destruct.Width * destruct.Height];
            _destructableData = new Color[destruct.Width, destruct.Height];
            destruct.GetData<Color>(destructData);

            _indestructableData = new Color[destruct.Width, destruct.Height];
            Color[] indestructData = null;
            if (indestruct!=null) 
            {
                indestructData = new Color[destruct.Width * destruct.Height];      
                indestruct.GetData<Color>(indestructData);
            }        

            int x = 0;
            int y = 0;

            for (int i = 0; i < destructData.Length; i++)
            {
                if (expData[i].R != 0) { _destructableData[x, y] = destructData[i]; }
                if (indestruct != null) { _indestructableData[x, y] = indestructData[i]; } else { _indestructableData[x, y] = new Color(0,0,0,0); }
                x++;
                if (x >= destruct.Width) { x = 0; y++; }
            }
        }

        public Color GetTerrainColorAt(int x, int y)
        {
            if (x < 0 || x > _destructableData.GetLength(0) - 1) { return Color.Black; }
            if (y < 0 || y > _destructableData.GetLength(1) - 1) { return Color.Black; }
            if (_indestructableData[x, y].A == 0) { return _destructableData[x, y]; }
            else { return _indestructableData[x, y]; }
        }

        public void Explode(Vector2 where, Vector2 direction, int size)
        {
            if (!_explosions.ContainsKey(size)) { return; }
            cExplosion e = _explosions[size];
            byte[,] data = e._data;

            // Move to the top left of the explosion
            where.X -= size/2;
            where.Y -= size/2;      
            int intX = (int)where.X;
            int intY = (int)where.Y;
            if (intX < 0 || intY < 0) { return; }      
            int ex = 0;
            int ey = 0;
            _explosionAdded = true;

            List<Vector2> positions = new List<Vector2>();
            List<Vector2> velocities = new List<Vector2>();
            List<Color> colors = new List<Color>();
            Vector2 pos, vel;

            int maxP = 50;
            int part = maxP;

            // Loop through all y values
            for (int y = intY; y < intY + size-1; y++)
            {
                // If its outside the collision area then we go no further
                if (y >= _destructableData.GetLength(1)-1) { break; }

                ex = 0;

                // Loop through all the y values
                for (int x = intX; x < intX + size-1; x++)
                {
                    // If its outside the y area go to the next x
                    if (x >= _destructableData.GetLength(0)-1) { break; }
                    if (data[ex, ey] != 0 && _destructableData[x, y].A != 0 && _indestructableData[x,y].A==0) 
                    {
                        if (part >= maxP)
                        {
                            part = 0;
                            Color vCol = _destructableData[x, y];
                            pos = new Vector2(x, y);
                            positions.Add(pos);

                            vel = new Vector2(e._partVel[ex, ey].X +direction.X, e._partVel[ex, ey].Y +direction.Y);
                            velocities.Add(vel);

                            colors.Add(vCol);             
                        }             
                        _destructableData[x, y] = new Color();
                    }
                    ex++;
                    part++;
                }

                ey++;
            }

            PrimalDevistation.Instance.PhysicsParticles.SpawnParticles(positions, velocities, colors);
        }

        public void defineExplosion(Texture2D tex, int size)
        {
            // Create the explosion object
            cExplosion e = new cExplosion();
            e._data = new byte[tex.Width, tex.Height];
            e._partVel = new Vector4[tex.Width, tex.Height];

            // Grab the texture data
            Color[] data = new Color[tex.Width * tex.Height];
            tex.GetData<Color>(data);             

            // Init some vars
            int x = 0;
            int y = 0;
            Vector2 diff = new Vector2();
            Random r = new Random();
           
            // Loop through each pixel
            for (int i = 0; i < data.Length; i++)
            {
                diff = new Vector2(x - (size / 2), y-(size / 2));
                float mag = diff.Length();
                if (mag < 8) { mag = 8; }
                diff.Normalize();
                diff.X *= (size/2) / (mag*0.6f);
                diff.Y *= (size/2) / (mag*0.6f);
                diff.Y += r.Next(-1 * size, 1 * size) / 100f;
                diff.X += r.Next(-1 * size, 1 * size) / 100f;
                e._partVel[x, y] = new Vector4(diff.X, diff.Y, 0, 0);
                e._data[x, y] = data[i].A;
                x++;
                if (x >= tex.Width) { x = 0; y++; }
            }



            _explosions.Add(size, e);
        }

        public void Draw(GameTime gameTime)
        {
            GraphicsDevice gd = PrimalDevistation.Instance.GD;

            if (_explosionAdded || _debugDrawTexture==null)
            {
                _debugDrawTexture = new Texture2D(gd, _destructableData.GetLength(0), _destructableData.GetLength(1), 1, TextureUsage.None, SurfaceFormat.Color);
                Color[] data = new Color[_destructableData.GetLength(0) * _destructableData.GetLength(1)];

                int element = 0;

                for (int y = 0; y < _destructableData.GetLength(1); y++)
                {
                    for (int x = 0; x < _destructableData.GetLength(0); x++)
                    {
                        if (_destructableData[x, y].A != 0) { data[element] = Color.Red; }
                        else { data[element] = Color.CornflowerBlue; }
                        element++;
                    }
                }
                _debugDrawTexture.SetData<Color>(data);
                _explosionAdded = false;
            }

            //Vector2 cam = LieroXNA.Instance.Camera.Position;

            //SpriteBatch batch = LieroXNA.Instance.Batch;
            //batch.Begin();
            //batch.Draw(_debugDrawTexture, new Rectangle((int)-cam.X, (int)-cam.Y, _debugDrawTexture.Width, _debugDrawTexture.Height), Color.White);
            //batch.End();
        }

        
    }
}
