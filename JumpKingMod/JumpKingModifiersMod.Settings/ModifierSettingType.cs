using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Settings
{
    /// <summary>
    /// An enum representing the possible types of Modifier Settings
    /// </summary>
    public enum ModifierSettingType
    {
        /// <summary>
        /// <see cref="bool"/>
        /// </summary>
        Bool,

        /// <summary>
        /// <see cref="string"/>
        /// </summary>
        String,

        /// <summary>
        /// <see cref="int"/>
        /// </summary>
        Int,

        /// <summary>
        /// <see cref="float"/>
        /// </summary>
        Float,

        /// <summary>
        /// <see cref="Enum"/>
        /// </summary>
        Enum,
    }
}
