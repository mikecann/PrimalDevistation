using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace PrimalDevistation.Weapons.Events
{
    public class cOnCollisionEvent
    {
        private List<cCommand> _commands; 

        public List<cCommand> Commands
        {
            get { return _commands; }
            set { _commands = value; }
        }

        public cOnCollisionEvent(XmlNode node)
        {
            _commands = new List<cCommand>();
            XmlNodeList commands = node.ChildNodes;
            foreach (XmlNode command in commands) { _commands.Add(cCommand.GetCommand(command)); }
        }

        public void Collision(cWeapon owner)
        {
            owner.Position -= owner.Velocity;
            for (int i = 0; i < _commands.Count; i++) { _commands[i].FireCommand(owner); }       
        }
    }
}
