using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.API
{
    /// <summary>
    /// An interface representing a custom entity used by the mod
    /// </summary>
    public interface IModEntity
    {
        void Draw();

        void Update(float p_delta);
    }
}
