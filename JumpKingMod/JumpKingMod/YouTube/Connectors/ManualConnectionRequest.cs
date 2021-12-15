using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.YouTube
{
    /// <summary>
    /// An aggregate class representing a request to connect the YouTube Client
    /// </summary>
    public class ManualConnectionRequest
    {
        public Action<bool> ResponseCallback { get; set; }
        public string ErrorMessages { get; set; }

        public ManualConnectionRequest(Action<bool> responseCallback)
        {
            ResponseCallback = responseCallback;
        }
    }
}
