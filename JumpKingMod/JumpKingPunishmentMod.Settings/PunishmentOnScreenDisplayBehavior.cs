using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingPunishmentMod.Settings
{
    /// <summary>
    /// An enum representing potential on screen display behaviors for the punishment mod
    /// </summary>
    public enum PunishmentOnScreenDisplayBehavior
    {
        /// <summary>
        /// No information will be displayed on screen
        /// </summary>
        None,

        /// <summary>
        /// Only a simple message will be displayed on screen with no information about the strength of the feedback
        /// </summary>
        MessageOnly,

        /// <summary>
        /// A simple message plus a 1-100% will be displayed indicating the strength of the feedback based on your min/max distances
        /// </summary>
        DistanceBasedPercentage,

        /// <summary>
        /// A simple message plus specifics about the intensity and duration of the feedback being triggered will display on screen
        /// </summary>
        FeedbackIntensityAndDuration,
    }
}
