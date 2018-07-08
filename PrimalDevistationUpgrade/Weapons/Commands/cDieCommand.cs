using System;
using System.Collections.Generic;
using System.Text;

namespace PrimalDevistation.Weapons.Commands
{
    class cDieCommand : cCommand
    {
        public override void FireCommand(cWeapon owner)
        {
            owner.KillFlag = true;
        }
    }
}
