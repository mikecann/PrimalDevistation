using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;

namespace PrimalDevistation.Weapons.Events
{
    public class cOnAgeEvent
    {
        private int _atAge;
        private List<cCommand> _commands;

        public List<cCommand> Commands
        {
            get { return _commands; }
            set { _commands = value; }
        }

        public int AtAge
        {
            get { return _atAge; }
            set { _atAge = value; }
        }

        public cOnAgeEvent(XmlNode node)
        {
            _atAge = 1000;
            if (node.Attributes["equals"] != null) { _atAge = int.Parse(node.Attributes["equals"].Value); }

            _commands = new List<cCommand>();
            XmlNodeList commands = node.ChildNodes;
            foreach (XmlNode command in commands) { _commands.Add(cCommand.GetCommand(command)); }
        }

        public void Update(cWeapon owner, GameTime gameTime)
        {  
            if (owner.Age >= _atAge)
            {            
                for (int i = 0; i < _commands.Count; i++) { _commands[i].FireCommand(owner); }
            }
        }
    }
}
