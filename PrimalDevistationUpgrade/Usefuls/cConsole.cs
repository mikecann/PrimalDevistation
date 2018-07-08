using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace PrimalDevistation
{
    public delegate void ConsoleCommand(string[] arguments);

    public class cConsole : DrawableGameComponent
    {
        public class cCommand
        {
            public string _help;
            public ConsoleCommand _func;
        }

        private SpriteFont _font; 
        private Texture2D _backTex;
        private Color _backColor;
        private int _numLines;
        private List<string> _lines;
        private string _inputString;
        private KeyboardState _lastKS;
        private int _blinkTimer;
        private Dictionary<string,cCommand> _commands;
        private bool _open;
        private List<string> _previousInputs;
        private int _prevInputCounter;
        private float _FPS;
        private bool _showFPS;
        private int _fpsCounter;
        private int _fpsTimer;

        public bool ShowFPS
        {
            get { return _showFPS; }
            set { _showFPS = value; }
        }
	
        public bool Open
        {
            get { return _open; }
            set { _open = value; }
        }	

        public Dictionary<string,cCommand> Commands
        {
            get { return _commands; }
            set { _commands = value; }
        }

        public int DisplayLines
        {
            get { return _numLines; }
            set { _numLines = value; if (_numLines < 1) { _numLines = 1; } }
        }

        public Color BackColor
        {
            get { return _backColor; }
            set { _backColor = value; }
        }	

        public Texture2D BackTex
        {
            get { return _backTex; }
            set { _backTex = value; }
        }	      

        public SpriteFont Font
        {
            get { return _font; }
            set { _font = value; }
        }	

        public cConsole(Game game) : base(game)
        {
            _backColor = new Color(0, 0, 0, 160);
            _numLines = 20;
            _lines = new List<string>();
            _commands = new Dictionary<string, cCommand>();
            _previousInputs = new List<string>();
            _inputString = "";
            _prevInputCounter = 0;
            _FPS = 0;
            _showFPS = true;
            _open = false;
            AddCommand("clear", "clears the console window", delegate { _lines.Clear(); });
            AddCommand("fps", "toggles the display of the FPS", delegate { ShowFPS = !ShowFPS; });   
            AddCommand("help", "gets help on a command", HelpOnCommand);
            AddCommand("list", "lists all the commands", ListCommands);
        }

        public void AddLine(string line) { _lines.Add(line); }

        public void HelpOnCommand(string[] arguments)
        {
            if (arguments.Length <= 0) { _lines.Add("USAGE: 'help [command]'"); return; }
            if (!_commands.ContainsKey(arguments[0])) { _lines.Add("HELP CANNOT FIND COMMAND '" + arguments[0] + "'"); return; }
            cCommand com = _commands[arguments[0]];
            _lines.Add("HELP: '" + arguments[0] + "' - "+com._help);
        }

        public void ListCommands(string[] arguments)
        {
            foreach (string key in _commands.Keys)
	        {
                cCommand com = _commands[key];
        	    _lines.Add("-> '" + key + "' - " + com._help);
	        }           
        }
               
        public void AddCommand(string command, string help, ConsoleCommand func)
        {
            command = command.ToLower();            
            if (!_commands.ContainsKey(command))
            {
                cCommand com = new cCommand();
                com._help = help;
                com._func = func;
                _commands.Add(command, com);
            }
            else { Console.WriteLine("COMMAND " + command + "ALREADY EXISTS!"); } 
        }

      
        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadGraphicsContent(bool loadAllContent)
        {
            ContentManager cm = PrimalDevistation.Instance.CM;
            if (loadAllContent)
            {
                _font = cm.Load<SpriteFont>("Other/ConsoleFont");
                Vector2 sze = _font.MeasureString("Q");
                _numLines = (int)((GraphicsDevice.Viewport.Height / 2) / sze.Y);
                _backTex = cm.Load<Texture2D>("Other/2x2");
            }
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();

            if (_open)
            {
                _blinkTimer += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (_blinkTimer > 1000) { _blinkTimer = 0; }
                
                Keys[] keysNow = ks.GetPressedKeys();
                Keys[] keysLast = _lastKS.GetPressedKeys();
                if (keysNow.Length != 0)
                {
                    Keys keyNow = keysNow[0];
                    if (keysLast.Length == 0 || keysLast[0] != keyNow)
                    {
                        if (keyNow == Keys.Back && _inputString.Length > 0)
                        {
                            _inputString = _inputString.Substring(0, _inputString.Length - 1);
                        }
                        else if (keyNow == Keys.Space)
                        {
                            _inputString += " ";
                        }
                        else if (keyNow == Keys.Enter)
                        {
                            if (_inputString.Length != 0)
                            {
                                processInput(_inputString);
                                _previousInputs.Add(_inputString);
                                _inputString = "";
                                _prevInputCounter = _previousInputs.Count;
                            }
                        }
                        else if (keyNow == Keys.Subtract)
                        {
                            _inputString += "-";
                        }
                        else if (keyNow == Keys.Up)
                        {
                            if (_previousInputs.Count != 0) 
                            {
                                _prevInputCounter--;
                                if (_prevInputCounter < 0) { _prevInputCounter = 0; }
                                _inputString = _previousInputs[_prevInputCounter];    
                            }
                        }
                        else if (keyNow == Keys.Down)
                        {
                            if (_previousInputs.Count != 0)
                            {
                                _prevInputCounter++;
                                if (_prevInputCounter > _previousInputs.Count - 1) { _prevInputCounter = _previousInputs.Count - 1; }
                                _inputString = _previousInputs[_prevInputCounter];           
                            }
                        }
                        else if (keyNow == Keys.D0 || keyNow == Keys.D1 ||
                            keyNow == Keys.D2 || keyNow == Keys.D3 || keyNow == Keys.D4 || keyNow == Keys.D5 ||
                            keyNow == Keys.D6 || keyNow == Keys.D7 || keyNow == Keys.D8 || keyNow == Keys.D9)
                        {
                            _inputString += keyNow.ToString()[1];
                        }
                        else if (keyNow == Keys.A || keyNow == Keys.B || keyNow == Keys.C || keyNow == Keys.D ||
                                 keyNow == Keys.E || keyNow == Keys.F || keyNow == Keys.G || keyNow == Keys.H ||
                                 keyNow == Keys.I || keyNow == Keys.J || keyNow == Keys.K || keyNow == Keys.L ||
                                 keyNow == Keys.M || keyNow == Keys.N || keyNow == Keys.O || keyNow == Keys.P ||
                                 keyNow == Keys.Q || keyNow == Keys.R || keyNow == Keys.S || keyNow == Keys.T ||
                                 keyNow == Keys.U || keyNow == Keys.V || keyNow == Keys.W || keyNow == Keys.X ||
                                 keyNow == Keys.Y || keyNow == Keys.Z || keyNow == Keys.OemPeriod)
                        {
                            if (keyNow == Keys.OemPeriod) { _inputString += "."; }
                            else { _inputString += keyNow.ToString().ToLower(); }
                            
                        }
                    }
                }              
            }

            _fpsCounter++;
            _fpsTimer += (int)gameTime.ElapsedRealTime.TotalMilliseconds;
            if (_fpsTimer > 1000) { _fpsTimer -= 1000; _FPS = _fpsCounter; _fpsCounter=0; }

            if (ks.IsKeyDown(Keys.Escape) && !_lastKS.IsKeyDown(Keys.Escape)) { _open = !_open; }           

            _lastKS = ks;
        }

        private void processInput(string str)
        {            
            char[] space = new char[] { ' ' };
            string[] words = str.Split(space);
            if (words == null || words.Length < 1) { _lines.Add("UNKNOWN COMMAND '" + str + "'"); return; }
            if (!_commands.ContainsKey(words[0])) { _lines.Add("UNKNOWN COMMAND '" + str + "'"); return; }            
            if (_commands.ContainsKey(words[0]))
            {                
                string[] args = new string[words.Length - 1];
                for (int i = 1; i < words.Length; i++){  args[i-1]=words[i]; }       
                cCommand com = _commands[words[0]];
                com._func(args);
                _lines.Add("COMMAND EXECUTED '" + words[0] + "'");
            }       
        }

        /// <summary>
        /// This is where it all happens. Grabs a scene that has already been rendered,
        /// and uses postprocess magic to add a glowing bloom effect over the top of it.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch batch = PrimalDevistation.Instance.Batch;
            if (_open)
            {
                int w = GraphicsDevice.Viewport.Width;
                int h = (_numLines * _font.LineSpacing) + 2;

                batch.Begin();

                // Draw the background
                batch.Draw(_backTex, new Rectangle(0, 0, w, h), _backColor);

                // Draw the lines
                int y = h - _font.LineSpacing;
                for (int i = _lines.Count - 1; i >= 0; i--)
                {
                    batch.DrawString(_font, _lines[i], new Vector2(12, y), Color.White);
                    y -= _font.LineSpacing;
                }

                // Draw the input background
                batch.Draw(_backTex, new Rectangle(0, h + 2, w, _font.LineSpacing), _backColor);

                // Draw the input line 
                string blinkChar = "|";
                if (_blinkTimer < 500) { blinkChar = ""; }
                batch.DrawString(_font, "> " + _inputString + blinkChar, new Vector2(0, h + 2), Color.White);

                batch.End();
            }

            if (_showFPS)
            {
                batch.Begin();
                batch.DrawString(_font, "-> FPS: " + _FPS, new Vector2(60, 40), Color.White);
                batch.End();
            }
        }       
    }
}
