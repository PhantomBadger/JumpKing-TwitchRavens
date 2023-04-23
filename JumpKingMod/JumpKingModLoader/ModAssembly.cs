using PBJKModBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModLoader
{
    /// <summary>
    /// A class which aggregates data about the Mod Assembly
    /// </summary>
    public class ModAssembly
    {
        /// <summary>
        /// The assembly containing the mod
        /// </summary>
        public Assembly Assembly { get; private set; }

        /// <summary>
        /// The type containing the entry method
        /// </summary>
        public Type EntryType { get; private set; }

        /// <summary>
        /// The attribute describing the mod
        /// </summary>
        public JumpKingModAttribute ModAttribute {get; private set;}

        /// <summary>
        /// Constructor for creating a <see cref="ModAssembly"/>
        /// </summary>
        /// <param name="assembly">The assembly containing the mod</param>
        /// <param name="entryType">The type containing the entry method</param>
        /// <param name="jumpKingModAttribute">The attribute describing the mod</param>
        public ModAssembly(Assembly assembly, Type entryType, JumpKingModAttribute jumpKingModAttribute)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            EntryType = entryType ?? throw new ArgumentNullException(nameof(entryType));
            ModAttribute = jumpKingModAttribute ?? throw new ArgumentNullException(nameof(jumpKingModAttribute));
        }
    }
}
