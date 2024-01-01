using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingPunishmentMod.API
{
    /// <summary>
    /// An interface representing a device that can punish the user
    /// </summary>
    public interface IPunishmentDevice
    {
        /// <summary>
        /// Update the state of the device
        /// Called by the <see cref="PunishmentManagerEntity"/> as long as it has been registered
        /// <param name="p_delta">The number of seconds that have passed since the last update</param>
        /// </summary>
        void Update(float p_delta);

        /// <summary>
        /// Called when the device should punish the user
        /// <param name="intensity">A 1-100 number that should control the strength of the feedback</param>
        /// <param name="duration">The number of seconds the feedback should occur for</param>
        /// <param name="easyMode">Whether 'easy mode' punishment is enabled, which should reduce or change the type of feedback</param>
        /// </summary>
        void Punish(float intensity, float duration, bool easyMode);

        /// <summary>
        /// Called when the device should reward the user
        /// <param name="intensity">A 1-100 number that should control the strength of the feedback</param>
        /// <param name="duration">The number of seconds the feedback should occur for</param>
        /// </summary>
        void Reward(float intensity, float duration);

        /// <summary>
        /// Called when we want to trigger test feedback from the device
        /// <param name="intensity">A 1-100 number that should control the strength of the feedback</param>
        /// <param name="duration">The number of seconds the feedback should occur for</param>
        /// </summary>
        void Test(float intensity, float duration);
    }
}
