using HarmonyLib;
using JumpKingMod.API;
using JumpKingMod.Patching;
using Logging.API;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Patching
{
    /// <summary>
    /// An implementation of <see cref="IMessengerRavenTrigger"/> which triggers when the player falls
    /// </summary>
    public class PlayerFallMessengerRavenTrigger : IMessengerRavenTrigger, IManualPatch
    {
        public event MessengerRavenTriggerArgs OnMessengerRavenTrigger;

        private static PlayerFallMessengerRavenTrigger instance;
        private string[] insults;

        private readonly Random random;

        /// <summary>
        /// Constructor for creating a <see cref="PlayerFallMessengerRavenTrigger"/>
        /// </summary>
        /// <param name="logger">an <see cref="ILogger"/> implementation for logging</param>
        public PlayerFallMessengerRavenTrigger(ILogger logger)
        {
            instance = this;
            random = new Random();

            insults = new string[]
            {
                "lmao",
                "OMEGADOWN",
                "LOL",
                "Fall King",
                "fucking idiot",
                "lmfao",
                "back to the old man",
                "LETS GOOOO",
                "stop playing",
                "you suck",
                "KEKW",
                ":(",
                "YEP FALL",
                "Imagine being EU",
                "Deez Nuts",
                "Get got",
                "kek wow",
                "Almost as bad as Fost",
                "THIS DUDE",
                "ravenJAM",
                "CinnaMoment",
                "Hi YouTube",
                "Go back to chess",
                "L",
                "It's like you don't even try",
            };
        }

        /// <summary>
        /// Sets up the patching required for this trigger
        /// </summary>
        public void SetUpManualPatch(Harmony harmony)
        {
            var onPlayerFallMethod = AccessTools.Method("JumpKing.MiscSystems.Achievements.AchievementManager:OnPlayerFall");
            var postfixMethod = AccessTools.Method($"JumpKingMod.Patching.{this.GetType().Name}:PostfixTriggerMethod");
            harmony.Patch(onPlayerFallMethod, postfix: new HarmonyMethod(postfixMethod));
        }

        /// <summary>
        /// Called by harmony after <see cref="JumpKing.MiscSystems.Achievements.AchievementManager"/> 'OnPlayerFall' private method
        /// </summary>
        public static void PostfixTriggerMethod(object __instance)
        {
            Task.Run(() =>
            {
                for (int i = 0; i < 3; i++)
                {
                    Task.Delay(500).Wait();
                    instance.OnMessengerRavenTrigger?.Invoke(null, Color.White, instance.insults[instance.random.Next(0, instance.insults.Length)]);
                }
            });
        }
    }
}
