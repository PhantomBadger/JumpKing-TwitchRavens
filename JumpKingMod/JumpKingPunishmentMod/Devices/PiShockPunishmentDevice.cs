using JumpKingPunishmentMod.API;
using PBJKModBase.PiShock;
using Logging.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingPunishmentMod.Devices
{
    /// <summary>
    /// An implementation of a punishment device backed by a PiShock
    /// </summary>
    public class PiShockPunishmentDevice : IPunishmentDevice
    {
        private readonly PiShockDevice device;
        private readonly ILogger logger;

        /// <summary>
        /// Ctor for creating a <see cref="PiShockPunishmentDevice"/>
        /// </summary>
        /// <param name="device">An instance of a <see cref="PiShockDevice"/> to trigger feedback on</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to log to</param>
        /// <exception cref="ArgumentNullException"></exception>
        public PiShockPunishmentDevice(PiShockDevice device, ILogger logger)
        {
            this.device = device ?? throw new ArgumentNullException(nameof(device));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            // Nothing to do here
        }

        /// <inheritdoc/>
        public void Punish(float intensity, float duration, bool easyMode)
        {
            if (!easyMode)
            {
                device.Shock(Round(duration), Round(intensity));
            }
            else
            {
                // Send a vibration instead for easy mode
                device.Vibrate(Round(duration), Round(intensity));
            }
        }

        /// <inheritdoc/>
        public void Reward(float intensity, float duration)
        {
            // PiShock only works with ints
            device.Vibrate(Round(duration), Round(intensity));
        }

        /// <inheritdoc/>
        public void Test(float intensity, float duration)
        {
            // Send a beep for testing, beeps don't support intensity, which is fine
            device.Beep(Round(duration));
        }

        /// <summary>
        /// The PiShock API only works with ints so we need to round floats,
        /// but we also want to round 0 values up so we still get some feedback
        /// </summary>
        private int Round(float value)
        {
            if (value > 0.0f && value <= 0.5f)
            {
                value += 0.5f;
            }
            return Convert.ToInt32(value);
        }
    }
}
