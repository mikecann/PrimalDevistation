using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using PrimalDevistation.Weapons.Commands;

namespace PrimalDevistation.Weapons
{
    public abstract class cCommand
    {
        public abstract void FireCommand(cWeapon owner);

        public static cCommand GetCommand(XmlNode node)
        {
            string name = node.Name;
            if (name == "die") { return new cDieCommand(); }
            else if (name == "spawn") { return new cSpawnCommand(node); }
            throw new Exception("command '" + name + "' not found");
        }
    }
}
