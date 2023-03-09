using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.API
{
    /// <summary>
    /// An interface representing a custom entity used by the mod which draws in the foreground
    /// </summary>
    public interface IForegroundModEntity
    {
        void ForegroundDraw();

        void Update(float p_delta);
    }
}
