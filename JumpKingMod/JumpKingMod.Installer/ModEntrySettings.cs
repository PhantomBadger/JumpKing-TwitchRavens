using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Install
{
    /// <summary>
    /// An aggregate class containing info about our Mod's entry point
    /// </summary>
    public class ModEntrySettings
    {
        /// <summary>
        /// The full name of the class type the entry method is in, includes namespaces
        /// </summary>
        public string EntryClassTypeName { get; set; }

        /// <summary>
        /// The name of the method within the above class that we hook into, it's expected to be a static void with no parameters
        /// </summary>
        public string EntryMethodName { get; set; }
    }
}
