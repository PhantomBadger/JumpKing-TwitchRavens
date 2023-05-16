using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Install.UI.API
{
    /// <summary>
    /// An interface representing a ViewModel for settings in the installer
    /// </summary>
    public interface IInstallerSettingsViewModel
    {
        /// <summary>
        /// Returns whether the settings are loaded or not
        /// </summary>
        bool AreSettingsLoaded();

        /// <summary>
        /// Attempts to load the settings for this ViewModel
        /// </summary>
        bool LoadSettings(string gameDirectory, bool createIfDoesntExist);

        /// <summary>
        /// Attempts to save the settings for this ViewModel
        /// </summary>
        bool SaveSettings(string gameDirectory);
    }
}
