using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using PrimalDevistation.Level;
using PrimalDevistation.Weapons;
using PrimalDevistation.Usefuls;
using PrimalDevistation.Objects;

namespace PrimalDevistation
{
    public class cDebugMode : DrawableGameComponent
    {
        private Texture2D _flatTex;
        private SpriteBatch _batch;
        private List<string> _tools;
        private string _currentTool;

#if !XBOX360
        private MouseState _lastMS;
        private MouseState _mouseDownMS;
#endif

        private KeyboardState _lastKS;
        private string _spawnWeapon;
        private Random _rand;
   
        public cDebugMode(Game game)
            : base(game)
        {
            _rand = new Random();

            PrimalDevistation.Instance.Console.AddCommand("setTool", "sets the current tool, use listtools to get all the available tools", SetTool);
            PrimalDevistation.Instance.Console.AddCommand("ListTools", "lists all the available tools", ListTools);
            PrimalDevistation.Instance.Console.AddCommand("ListWeapons", "lists all the available weapons", ListWeapons);
            PrimalDevistation.Instance.Console.AddCommand("wep", "sets the weapon that is spawned on click", WeaponSpawn);
            
            _tools = new List<string>();
            _tools.Add("explosion32");
            _tools.Add("explosion64");
            _tools.Add("explosion128");
            _tools.Add("wep");
            _currentTool = "explosion128";
            _spawnWeapon = "rocket";
        }

        private void SetTool(string[] args)
        {
            if (args.Length <= 0) { PrimalDevistation.Instance.Console.AddLine("USAGE: 'setTool [tool]' use 'listtools' to get available tools"); return; }
            if (!_tools.Contains(args[0])) { PrimalDevistation.Instance.Console.AddLine("TOOL NOT FOUND '" + args[0] + "'"); return; }
            _currentTool = args[0];
            PrimalDevistation.Instance.Console.AddLine("TOOL SET TO '" + _currentTool + "'");
        }

        private void ListTools(string[] args)
        {
            PrimalDevistation.Instance.Console.AddLine("-- Debug Tools Are --");
            for (int i = 0; i < _tools.Count; i++)
			{
                PrimalDevistation.Instance.Console.AddLine("'" + _tools[i] + "'" );
			}            
        }

        private void ListWeapons(string[] args)
        {
            cWeaponFactory ws = PrimalDevistation.Instance.Weapons;
            PrimalDevistation.Instance.Console.AddLine("-- Weapons Are --");

            for (int i = 0; i < ws.WeaponBlueprints.Count; i++)
            {
                PrimalDevistation.Instance.Console.AddLine("'" + ws.WeaponBlueprints[i].Name + "'");
            }
                            
        }

        private void WeaponSpawn(string[] args)
        {
            cWeaponFactory ws = PrimalDevistation.Instance.Weapons;
            if (args.Length <= 0) { PrimalDevistation.Instance.Console.AddLine("USAGE: 'weaponspawn [weapon]' use 'ListWeapons' to get available weapons"); return; }
            //if (!ws.WeaponBlueprints.ContainsKey(args[0])) { LieroXNA.Instance.LieroConsole.AddLine("WEAPON NOT FOUND '" + args[0] + "'"); return; }
            _spawnWeapon = args[0];
            PrimalDevistation.Instance.Console.AddLine("WEAPON SPAWN SET TO '" + _spawnWeapon + "'");
        }

        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            if (loadAllContent)
            {
                _batch = new SpriteBatch(GraphicsDevice);
                _flatTex = PrimalDevistation.Instance.CM.Load<Texture2D>(@"Sprites/particle2x2"); 
            }
            base.LoadGraphicsContent(loadAllContent);
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();
            cCamera2D cam = PrimalDevistation.Instance.Camera;
#if !XBOX360
            MouseState ms = Mouse.GetState();            

            
            Vector2 where, dir;
            where = new Vector2(ms.X, ms.Y);
            where = (where - cam.ZoomPoint) / cam.Zoom;     
            where += cam.Position;
 
            if (ms.LeftButton == ButtonState.Pressed)
            {
                if (_lastMS.LeftButton != ButtonState.Pressed) { _mouseDownMS = ms; }

                dir = new Vector2(ms.X - _lastMS.X, ms.Y - _lastMS.Y) / 10f;
                if (_currentTool == "explosion32") { PrimalDevistation.Instance.Explode(32, where, dir, null, false); }
                else if (_currentTool == "explosion64") { PrimalDevistation.Instance.Explode(64, where, dir, null, false); }
                else if (_currentTool == "explosion128") { PrimalDevistation.Instance.Explode(128, where, dir, null, false); }
            }
            else
            {
                if (_currentTool == "wep" && _lastMS.LeftButton == ButtonState.Pressed)
                {             
                    dir = new Vector2(ms.X - _mouseDownMS.X, ms.Y - _mouseDownMS.Y) / 20f;
                    PrimalDevistation.Instance.Weapons.SpawnWeapon(_spawnWeapon, where, dir, null);
                }
            }

            if (ms.ScrollWheelValue != _lastMS.ScrollWheelValue)
            {
                int wheelDiff = ms.ScrollWheelValue - _lastMS.ScrollWheelValue;
                cam.Zoom = cam.Zoom + (wheelDiff / 5000f);
                //cam.ZoomPoint = new Vector2(ms.X,ms.Y);          
            }

            _lastMS = ms;
            _lastKS = ks;
#endif

            if (ks.IsKeyDown(Keys.Left)) { cam.Move(-8f, 0f); }
            if (ks.IsKeyDown(Keys.Right)) { cam.Move(8f, 0f); }
            if (ks.IsKeyDown(Keys.Up)) { cam.Move(0f, -8f); }
            if (ks.IsKeyDown(Keys.Down)) { cam.Move(0f, 8f); }
        }

        public override void Draw(GameTime gameTime)
        {
            int w = GraphicsDevice.Viewport.Width;
            int h = GraphicsDevice.Viewport.Height;
            _batch.Begin();
            _batch.Draw(_flatTex, new Rectangle(0, 0, w, 6), Color.Red);
            _batch.Draw(_flatTex, new Rectangle(0, 0, 6, h), Color.Red);
            _batch.Draw(_flatTex, new Rectangle(w-6, 0, 6, h), Color.Red);
            _batch.Draw(_flatTex, new Rectangle(0, h-6, w, 6), Color.Red);
            _batch.End();
            base.Draw(gameTime);
        }

        //public void DrawObjectBounds(cPhysicsObject obj)
        //{
        //    cCamera2D cam = LieroXNA.Instance.Camera;       

        //    Vector2 topLeft = new Vector2(obj.Position.X - obj.CollisionBounds.X, obj.Position.X - obj.CollisionBounds.Y);
        //    Vector2 topRight = new Vector2(obj.Position.X + obj.CollisionBounds.X, obj.Position.X - obj.CollisionBounds.Y);
        //    Vector2 bottomRight = new Vector2(obj.Position.X + obj.CollisionBounds.X, obj.Position.X + obj.CollisionBounds.Y);
        //    Vector2 bottomLeft = new Vector2(obj.Position.X - obj.CollisionBounds.X, obj.Position.X + obj.CollisionBounds.Y);

        //    topLeft = cam.GetScreenPos(ref topLeft);
        //    topRight = cam.GetScreenPos(ref topRight);
        //    bottomRight = cam.GetScreenPos(ref bottomRight);
        //    bottomLeft = cam.GetScreenPos(ref bottomLeft);

        //    _batch.Begin();
        //    _batch.Draw(_flatTex, new Rectangle(topLeft.X, topLeft.Y, topLeft.X - topRight.X, 2), Color.Red);
        //    _batch.Draw(_flatTex, new Rectangle(topRight.X, topRight.Y, 2, topRight.Y - bottomRight.Y), Color.Red);
        //    _batch.Draw(_flatTex, new Rectangle(w - 6, 0, 6, h), Color.Red);
        //    _batch.Draw(_flatTex, new Rectangle(0, h - 6, w, 6), Color.Red);
        //    _batch.End();
        //}
    }
}
