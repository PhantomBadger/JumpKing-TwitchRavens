using PBJKModBase.PiShock.Settings;
using Logging.API;
using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.PiShock
{
    /// <summary>
    /// A class whose purpose is to make a <see cref="PiShockDevice"/>
    /// </summary>
    public class PiShockDeviceFactory
    {
        private readonly ILogger logger;
        private readonly UserSettings userSettings;

        private static PiShockDevice piShockDevice;
        private static object deviceLock = new object();

        /// <summary>
        /// Constructor for creating a <see cref="PiShockDeviceFactory"/>
        /// </summary>
        /// <param name="userSettings">A <see cref="UserSettings"/> class to get settings info from</param>
        /// <param name="logger">An <see cref="ILogger"/> implementation for logging</param>
        /// <exception cref="ArgumentNullException"></exception>
        public PiShockDeviceFactory(UserSettings userSettings, ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));

            piShockDevice = null;
        }

        /// <summary>
        /// Gets a <see cref="PiShockDevice"/> instance for interfacing with the device
        /// </summary>
        public PiShockDevice GetPiShockDevice()
        {
            lock (deviceLock)
            {
                if (piShockDevice != null)
                {
                    return piShockDevice;
                }
                else
                {
                    string apiUsername = userSettings.GetSettingOrDefault(PBJKModBasePiShockSettingsContext.UsernameKey, string.Empty);
                    string apiKey = userSettings.GetSettingOrDefault(PBJKModBasePiShockSettingsContext.APIKeyKey, string.Empty);
                    string apiShareCode = userSettings.GetSettingOrDefault(PBJKModBasePiShockSettingsContext.ShareCodeKey, string.Empty);
                    
                    if (string.IsNullOrWhiteSpace(apiUsername))
                    {
                        logger.Error($"No valid API Username found in the {PBJKModBasePiShockSettingsContext.SettingsFileName} file!");
                        return null;
                    }
                    if (string.IsNullOrWhiteSpace(apiKey))
                    {
                        logger.Error($"No valid API Key found in the {PBJKModBasePiShockSettingsContext.SettingsFileName} file!");
                        return null;
                    }
                    if (string.IsNullOrWhiteSpace(apiShareCode))
                    {
                        logger.Error($"No valid API Share Code found in the {PBJKModBasePiShockSettingsContext.SettingsFileName} file!");
                        return null;
                    }

                    apiUsername.Trim();
                    apiKey.Trim();
                    apiShareCode.Trim();

                    try
                    {
                        piShockDevice = new PiShockDevice(logger, apiUsername, apiKey, apiShareCode);
                        return piShockDevice;
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Encountered error when trying to create PiShock Device: {e.ToString()}");
                        return null;
                    }
                }
            }
        }
    }
}
