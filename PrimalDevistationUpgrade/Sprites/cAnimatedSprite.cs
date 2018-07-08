using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using PrimalDevistation.Usefuls;

namespace PrimalDevistation.Sprites
{
    public class cAnimatedSprite
    {
        public class cAnim
        {
            public enum eAnimTypes { LOOP }
            public eAnimTypes AnimType;
            public string Name;
            public List<Texture2D> Frames;
            public List<byte[,]> FramesColData;
            public int AnimLength;
            public cAnim() 
            {
                Frames = new List<Texture2D>();
                FramesColData = new List<byte[,]>(); 
                AnimType = eAnimTypes.LOOP; 
                AnimLength = 1000; 
            }
        }        

        private string _name;
        private string _xmlURL;
        private Dictionary<string,cAnim> _animations;
        private cAnim _currentAnim;
        private Texture2D _currentFrame;        
        private int _animTimer;
        private byte[,] _currentFrameData;

        public byte[,] CurrentFrameCollisionData
        {
            get { return _currentFrameData; }
            set { _currentFrameData = value; }
        }	

        public Texture2D CurrentFrame
        {
            get { return _currentFrame; }
            set { _currentFrame = value; }
        }	

        public cAnim CurrentAnim
        {
            get { return _currentAnim; }
            set { _currentAnim = value; }
        }	

        public Dictionary<string,cAnim> Animations
        {
            get { return _animations; }
            set { _animations = value; }
        }

        public string XMLURL
        {
            get { return _xmlURL; }
            set { _xmlURL = value; }
        }
	
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public cAnimatedSprite(string XMLURL)
        {
            _xmlURL = XMLURL;
            _animations = new Dictionary<string, cAnim>();
            ParseAnim(XMLURL);
            _animTimer = 0;
        }

        public cAnimatedSprite(cAnimatedSprite blueprint)
        {
            _xmlURL = blueprint.XMLURL;
            _name = blueprint.Name;
            _animations = blueprint.Animations;
            _animTimer = 0;
        }        

        private void ParseAnim(string url)
        {
            GraphicsDevice gd = PrimalDevistation.Instance.GD;
            ContentManager cm = PrimalDevistation.Instance.CM;

            XmlDocument doc = new XmlDocument();
            doc.Load(url);

            // Create the weapon and set its name       
            XmlNode spriteNode = doc.SelectSingleNode("sprite");
            string imageURL = spriteNode.Attributes["img"].Value;           
            Texture2D tex = cm.Load<Texture2D>(imageURL);           
            Color[,] texData = GetFramesetData(tex);

            XmlNodeList animNodes = spriteNode.SelectNodes("animation");
            foreach (XmlNode node in animNodes)
            {
                cAnim a = new cAnim();
                a.Name = node.Attributes["name"].Value;
                if (node.Attributes["loop"] != null) { a.AnimType = GetAnimType(node.Attributes["loop"].Value); }
                if (node.Attributes["animLength"] != null) { a.AnimLength = int.Parse(node.Attributes["animLength"].Value); }
                _animations.Add(a.Name, a);

                XmlNodeList frameNodes = node.SelectNodes("frame");
                foreach (XmlNode frameNode in frameNodes)
                {
                    int x = int.Parse(frameNode.Attributes["x"].Value);
                    int y = int.Parse(frameNode.Attributes["y"].Value);
                    int w = int.Parse(frameNode.Attributes["w"].Value);
                    int h = int.Parse(frameNode.Attributes["h"].Value);
                    Texture2D frameTex = new Texture2D(gd, w, h, 1, TextureUsage.None, SurfaceFormat.Color);
                    frameTex.SetData<Color>(GetArrayForFrame(texData, x, y, w, h));
                    a.FramesColData.Add(cGraphics.GetCollisionData(frameTex));
                    a.Frames.Add(frameTex);
                    
                }
            }
        }

        private Color[,] GetFramesetData(Texture2D framesetTex)
        {
            Color[,] dataOut = new Color[framesetTex.Height, framesetTex.Width];
            Color[] texData = new Color[framesetTex.Width * framesetTex.Height];
            framesetTex.GetData<Color>(texData);
            int x = 0; int y = 0;         
            for (int i = 0; i < texData.Length; i++)
            {
                dataOut[y, x] = texData[i];
                x++;
                if (x >= framesetTex.Width) { x = 0; y++; }
            }
            return dataOut;
        }

        private Color[] GetArrayForFrame(Color[,] framesetData, int x, int y, int w, int h)
        {
            Color[] dataOut = new Color[w * h];
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    int indx = (i * w) + j;
                    dataOut[indx] = framesetData[y + i, x + j];
                }
            }
            return dataOut;
        }
               
        private cAnim.eAnimTypes GetAnimType(string name)
        {
            if (name == "loop") { return cAnim.eAnimTypes.LOOP;  }
            return cAnim.eAnimTypes.LOOP;
        }

        public void Update(GameTime gameTime)
        {
            if (_currentAnim != null)
            {
                int frame = (int)MathHelper.Lerp(0, _currentAnim.Frames.Count, (1f / _currentAnim.AnimLength) * _animTimer);
                _currentFrame = _currentAnim.Frames[frame];
                _currentFrameData = _currentAnim.FramesColData[frame];

                _animTimer += gameTime.ElapsedGameTime.Milliseconds;
                if (_animTimer >= _currentAnim.AnimLength)
                {
                    if (_currentAnim.AnimType == cAnim.eAnimTypes.LOOP)
                    {
                        int multiple = _animTimer / _currentAnim.AnimLength;
                        _animTimer -= (multiple * _currentAnim.AnimLength);
                    }
                }           
            }
        }

        public void Play(string animName)
        {
            _currentAnim = _animations[animName];
            _animTimer = 0;
        }
    }
}
