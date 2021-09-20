using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.API
{
    /// <summary>
    /// An interface representing a custom entity used by the mod which draws in the foreground
    /// </summary>
    public interface IForegroundModEntity
    {
        void ForegroundDraw();
    }
}
