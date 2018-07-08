using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PrimalDevistation.Player;

namespace PrimalDevistation
{
    public class cCamera2D
    {
        private const float _zoomMax = 1.5f;
        private Vector2 _position;
        private float _zoom;
        private Vector2 _zoomPoint;
        private Matrix _proj;
        private Viewport _viewport;

        public Viewport Viewport
        {
            get { return _viewport; }
            set { _viewport = value; }
        }

        public Matrix Projection
        {
            get { return _proj; }
            set { _proj = value; }
        }
	
        public Vector2 ZoomPoint
        {
            get { return _zoomPoint; }
            set { _zoomPoint = value; }
        }
	
        public float Zoom
        {
            get { return _zoom; }
            set 
            {
                _zoom = value;
                if (_zoom > _zoomMax) { _zoom = _zoomMax; }
                Vector2 size = PrimalDevistation.Instance.Level.Size;
                //if (size.X * _zoom < _viewport.Width) { _zoom = _viewport.Width / size.X; }
                if (size.Y * _zoom < _viewport.Height) { _zoom = _viewport.Height / size.Y; }
                ClampToBounds();
            }
        }

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; ClampToBounds(); }
        }      

        public cCamera2D(Viewport viewport)
        {
            _viewport = viewport;
            _position = Vector2.Zero;
            _zoom = _zoomMax;
            _zoomPoint = new Vector2(viewport.Width / 2, viewport.Height / 2);

            float scaleX = 1.0f / (viewport.Width / 2);
            float scaleY = 1.0f / (viewport.Height / 2);
            _proj = Matrix.CreateScale(scaleX, scaleY, 1) *
                         Matrix.CreateScale(1, -1, 1) *
                         Matrix.CreateTranslation(-1, 1, 0);
        }

        public Vector2 GetScreenPos(ref Vector2 worldPos)
        {
            Vector2 vOut = new Vector2((worldPos.X - _position.X) * _zoom, (worldPos.Y - _position.Y) * _zoom);
            vOut.X += _zoomPoint.X;
            vOut.Y += _zoomPoint.Y;
            return vOut;       
        }

        public void Move(float x, float y)
        {
            _position.X += x;
            _position.Y += y;
            ClampToBounds();
        }       

        public void ClampToBounds()
        {
            Vector2 size = PrimalDevistation.Instance.Level.Size;
            Vector2 min = (_zoomPoint / _zoom);     
            _position.X = MathHelper.Clamp(_position.X, min.X, size.X + min.X - _viewport.Width/_zoom);
            _position.Y = MathHelper.Clamp(_position.Y, min.Y, size.Y + min.Y - _viewport.Height/_zoom);        
        }

        public void Update(GameTime gameTime)
        {
            List<cPlayer> activePlayers = PrimalDevistation.Instance.PlayerManager.ActivePlayers;
            if (activePlayers.Count == 0) { return; }
            
            // If there is only one player we just centre the screen on it
            if (activePlayers.Count == 1)
            {
                cPlayer p = activePlayers[0];
                _position.X = p.Position.X;
                _position.Y = p.Position.Y;
                ClampToBounds();
            }
            else // Else there is more than one player we need to do some fancy bounding code
            {     
                Vector2 topLeft = new Vector2(float.MaxValue, float.MaxValue);
                Vector2 bottomRight = new Vector2(float.MinValue, float.MinValue);

                for (int i = 0; i < activePlayers.Count; i++)
                {
                    topLeft.X = Math.Min(topLeft.X, activePlayers[i].Position.X);
                    topLeft.Y = Math.Min(topLeft.Y, activePlayers[i].Position.Y);
                    bottomRight.X = Math.Max(bottomRight.X, activePlayers[i].Position.X);
                    bottomRight.Y = Math.Max(bottomRight.Y, activePlayers[i].Position.Y);
                }

                Vector2 dist = bottomRight - topLeft;

                Vector2 centre = topLeft + (dist / 2);
                _position.X = centre.X;
                _position.Y = centre.Y;

                dist.X = Math.Abs(dist.X) + 300;
                dist.Y = Math.Abs(dist.Y) + 300;

                float ratioX = (_viewport.Width / dist.X);
                float ratioY = (_viewport.Height / dist.Y);   
               
                Zoom = Math.Min(ratioX, ratioY);
                ClampToBounds();
            }
        }
    }


}
