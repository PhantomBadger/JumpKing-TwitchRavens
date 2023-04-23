using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase
{
    /// <summary>
    /// An attribute identifying the class as being a Jump King Mod
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class JumpKingModAttribute : Attribute
    {
        public string ModName { get; private set; }
        public string EntryMethod { get; private set; }

        /// <summary>
        /// Constructor for creating a <see cref="JumpKingModAttribute"/>
        /// </summary>
        /// <param name="modName">The name of the mod</param>
        /// <param name="entryMethod">The entry method signature</param>
        public JumpKingModAttribute(string modName, string entryMethod)
        {
            ModName = modName;
            EntryMethod = entryMethod;
        }
    }
}
