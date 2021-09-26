using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.API
{
    public delegate void MessengerRavenTriggerArgs(string ravenName, Color ravenNameColour, string ravenMessage);

    public interface IMessengerRavenTrigger
    {
        event MessengerRavenTriggerArgs OnMessengerRavenTrigger;
    }
}
