using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using PrimalDevistation.Sprites;
using Microsoft.Xna.Framework.Input;
using PrimalDevistation.Particles;
using PrimalDevistation.Usefuls;
using System.Xml;
using System.IO;

namespace PrimalDevistation.Level
{
    

    public class cLevel : DrawableGameComponent
    {
        private cCollisionMap _collisionTile;
        private bool _showCollisionMap;
        private cTerrainMap _terrainMap;
        private List<Vector3> _explosions;
        private Dictionary<int, Texture2D> _explosionTextures;
        private Vector2 _size;
        private string _currentMap;
        private RenderTarget2D[] _explosionMaps;
        private int _currentTargetIn;
        private int _currentTargetOut;
        private Texture2D _background;
        private bool _showDestroyedBack;

        public Vector2 Size
        {
            get { return _size; }
            set { _size = value; }
        }	

        public cTerrainMap TerrainMap
        {
            get { return _terrainMap; }
            set { _terrainMap = value; }
        }
	
        public cCollisionMap CollisionMap
        {
            get { return _collisionTile; }
            set { _collisionTile = value; }
        }

        public cLevel(Game game, int tilesWide, int tilesHigh) : base(game)
        {
            _explosionTextures = new Dictionary<int, Texture2D>();
            _explosions = new List<Vector3>();
            _terrainMap = new cTerrainMap();
            _collisionTile = new cCollisionMap();
            _showCollisionMap = false;
            _currentTargetIn = 0;
            _currentTargetOut = 1;
            _showDestroyedBack = false;
        }

        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
                GraphicsDevice gd = GraphicsDevice;
                ContentManager cm = PrimalDevistation.Instance.CM;
                LoadMap("rock5");            

                _explosionTextures.Add(32, cm.Load<Texture2D>(@"ExplosionMasks/explosion32"));
                _explosionTextures.Add(64, cm.Load<Texture2D>(@"ExplosionMasks/explosion64"));
                _explosionTextures.Add(128, cm.Load<Texture2D>(@"ExplosionMasks/explosion128"));
                foreach (int size in _explosionTextures.Keys) { _collisionTile.defineExplosion(_explosionTextures[size], size); }
             
                PrimalDevistation.Instance.Console.AddCommand("showCollision", "shows the collision map for this level", delegate { _showCollisionMap = !_showCollisionMap; });
                PrimalDevistation.Instance.Console.AddCommand("resetMap", "resets the current map", delegate { LoadMap(_currentMap); });
                PrimalDevistation.Instance.Console.AddCommand("loadMap", "loads a given map", consoleLoadMap);
                PrimalDevistation.Instance.Console.AddCommand("showDestrBk", "loads a given map", delegate { _showDestroyedBack = !_showDestroyedBack; });

                
            }
        }

        public void consoleLoadMap(string[] args)
        {
            if (args.Length <= 0) { PrimalDevistation.Instance.Console.AddLine("USAGE: 'loadMap [level]'"); return; }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(@"Resources/Maps/" + args[0] + ".xml");                
            }
            catch (FileNotFoundException e)
            {
                PrimalDevistation.Instance.Console.AddLine("MAP NOT FOUND '" + args[0] + "'");
                return;
            }

            LoadMap(args[0]);
        }

        public void LoadMap(string mapName)
        {
            
            GraphicsDevice gd = GraphicsDevice;
            ContentManager cm = PrimalDevistation.Instance.CM;

            XmlDocument doc = new XmlDocument();
            
            doc.Load(@"Resources/Maps/" + mapName+ ".xml");
            XmlNode mapNode = doc.SelectSingleNode("map");
            XmlNode destrNode = mapNode.SelectSingleNode("destructableMap");             
            XmlNode indestrNode = mapNode.SelectSingleNode("indestructableMap");
            XmlNode bgNode = mapNode.SelectSingleNode("background");
            XmlNode destrMaskNode = mapNode.SelectSingleNode("destrMask");

            _terrainMap.InitialTexture = cm.Load<Texture2D>(@"Maps/" + destrNode.InnerText);

            Texture2D expMask=null;
            if (indestrNode != null) { _terrainMap.IndestructableTexture = cm.Load<Texture2D>(@"Maps/" + indestrNode.InnerText); }
            if (bgNode != null) { _background = cm.Load<Texture2D>(@"Maps/" + bgNode.InnerText); }
            if (destrMaskNode != null) { expMask = cm.Load<Texture2D>(@"Maps/" + destrMaskNode.InnerText); }
          
            _terrainMap.Reset(GraphicsDevice);
            _size.X = _terrainMap.InitialTexture.Width;
            _size.Y = _terrainMap.InitialTexture.Height;
            _explosionMaps = new RenderTarget2D[2];
            _explosionMaps[0] = new RenderTarget2D(gd, (int)_size.X, (int)_size.Y, 1, SurfaceFormat.Color);
            _explosionMaps[1] = new RenderTarget2D(gd, (int)_size.X, (int)_size.Y, 1, SurfaceFormat.Color);

            cGraphics.ClearRenderTarget(gd, _explosionMaps[0], Color.White);
            cGraphics.ClearRenderTarget(gd, _explosionMaps[1], Color.White);
            //cGraphics.ClearRenderTargetSlow(_explosionMaps[0], Color.White);
            //cGraphics.ClearRenderTargetSlow(_explosionMaps[1], Color.White);


            if (expMask == null) { expMask = cm.Load<Texture2D>(@"Maps/blankMask"); }
           
            Color[] data = new Color[expMask.Width * expMask.Height];
            expMask.GetData<Color>(data);
            _explosionMaps[0].GetTexture().SetData<Color>(data);
            _explosionMaps[1].GetTexture().SetData<Color>(data);
         
            
            //cGraphics.RenderToTex(gd, tex, _explosionMaps[0]);
            //cGraphics.RenderToTex(gd, tex, _explosionMaps[1]);

            //cGraphics.ClearRenderTarget(gd, _explosionMap, Color.White);
            _collisionTile.Init(_terrainMap.InitialTexture, _terrainMap.IndestructableTexture, _explosionMaps[0].GetTexture());
            _currentMap = mapName;
             
        }

        public void AddExplosion(int size, Vector2 where, Vector2 direction)
        {
            _explosions.Add(new Vector3(where.X, where.Y, size));
            _collisionTile.Explode(where, direction, size);
        }

        private void GenerateExplosionMap()
        {
            GraphicsDevice gd = GraphicsDevice;
            DepthStencilBuffer dsb = gd.DepthStencilBuffer;
            gd.DepthStencilBuffer = null;

            gd.SetRenderTarget(0, _explosionMaps[_currentTargetOut]);
            SpriteBatch batch = PrimalDevistation.Instance.Batch;
            
            batch.Begin();
            batch.Draw(_explosionMaps[_currentTargetIn].GetTexture(), new Rectangle(0, 0, (int)_size.X, (int)_size.Y), Color.White);
            batch.End();     

            batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            Vector2 cam = PrimalDevistation.Instance.Camera.Position;
            for (int i = 0; i < _explosions.Count; i++)
            {
                int size = (int)(_explosions[i].Z);
                int hSize = size / 2;
                Texture2D tex = _explosionTextures[size];
                batch.Draw(tex, new Rectangle((int)(_explosions[i].X - hSize), (int)(_explosions[i].Y - hSize), size, size), Color.White);
            }
            batch.End();
            gd.SetRenderTarget(0, null);
            gd.DepthStencilBuffer = dsb;
            _explosions.Clear();

            int tmp = _currentTargetOut;
            _currentTargetOut = _currentTargetIn;
            _currentTargetIn = tmp;
        }
              
        public override void Draw(GameTime gameTime)
        {
            // Add the list of explosions this frame to the backround
            GenerateExplosionMap();
            _terrainMap.GenerateMap(GraphicsDevice, _explosionMaps[_currentTargetOut].GetTexture());            

            if (_showCollisionMap) { _collisionTile.Draw(gameTime); }
            else
            {
                SpriteBatch batch = PrimalDevistation.Instance.Batch;
                cCamera2D cam = PrimalDevistation.Instance.Camera;
                Vector2 tmpPos = cam.ZoomPoint - (cam.Position*cam.Zoom);
                Vector2 tmpPos2 = cam.ZoomPoint - ((cam.Position - new Vector2(-5, 5)) * cam.Zoom);
                Vector2 tmpPos3 = cam.ZoomPoint - (((cam.Position*0.5f)+new Vector2(500, 500)) * cam.Zoom);

                PrimalDevistation.Instance.GD.Clear(Color.Black);

                
                batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
                if (_background != null) { batch.Draw(_background, tmpPos3, null, Color.White, 0, Vector2.Zero, cam.Zoom, SpriteEffects.None, 0); }
                //batch.Draw(_explosionMaps[_currentTargetOut].GetTexture(), tmpPos, null, Color.White, 0, Vector2.Zero, cam.Zoom, SpriteEffects.None, 0);
                if (_showDestroyedBack) { batch.Draw(_terrainMap.InitialTexture, tmpPos, null, new Color(100, 100, 100), 0, Vector2.Zero, cam.Zoom, SpriteEffects.None, 0); }
                batch.Draw(_terrainMap.Texture, tmpPos2, null, new Color(70, 70, 70), 0, Vector2.Zero, cam.Zoom, SpriteEffects.None, 0);
                batch.Draw(_terrainMap.Texture, tmpPos, null, Color.White, 0, Vector2.Zero, cam.Zoom, SpriteEffects.None, 0);      
                batch.End();                          
            }
        }
              


    }
}
