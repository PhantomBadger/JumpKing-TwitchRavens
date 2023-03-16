using HarmonyLib;
using Logging.API;
using Microsoft.Xna.Framework.Graphics;
using PBJKModBase;
using System;
using System.Reflection;
using static JumpKing.JKContentManager.RavenSprites;

namespace JumpKingRavensMod
{
    /// <summary>
    /// A static class which loads the appropriate resources the mod needs
    /// </summary>
    public static class RavensModContentManager
    {
        public static RavenContent Raven;

        public static Texture2D ScopeTexture;

        public static Texture2D RavenStunnedTexture;

        public static Texture2D RavenFallingTexture;

        /// <summary>
        /// Load all the Mod-specific content
        /// </summary>
        public static void LoadContent(ILogger logger)
        {
            try
            {
                ScopeTexture = JumpKing.Game1.instance.Content.Load<Texture2D>("Mods/Resources/scope");
                logger.Information($"Loaded Scope Texture");

                RavenStunnedTexture = JumpKing.Game1.instance.Content.Load<Texture2D>("Mods/Resources/raven_stunned");
                logger.Information($"Loaded Raven Stun Texture");

                RavenFallingTexture = JumpKing.Game1.instance.Content.Load<Texture2D>("Mods/Resources/raven_falling");
                logger.Information($"Loaded Raven Fall Texture");

                // Constructor for Raven Content is internal, we have to use Activator and reflection to create it
                // Pass in our raven version and it will split it for us
                Type ravenContenType = AccessTools.TypeByName("JumpKing.JKContentManager+RavenSprites+RavenContent")
                    ?? throw new InvalidOperationException($"Cannot find 'JumpKing.JKContentManager+RavenSprites+RavenContent' type in Jump King");

                // The RavenContent ctor is internal for base JK but public for JK+, so we need to try both before we 
                // deem this a failure
                JumpKingModUtilities.AttemptMultipleActions(
                    () =>
                    {
                        logger.Error($"Failed to Load Raven Content from any of the known endpoints!");
                    },
                    new Tuple<Action, Action<Exception>>(
                        () =>
                        {
                            Raven = (RavenContent)Activator.CreateInstance(ravenContenType,
                                BindingFlags.NonPublic | BindingFlags.Instance,
                                binder: null,
                                new object[] { JumpKing.Game1.instance.Content.Load<Texture2D>("Mods/Resources/raven") },
                                culture: null);
                        },
                        (Exception e) =>
                        {
                            logger.Warning($"Attempted to Load Raven Content using the base game's endpoint and failed! If you're using JK+ then ignore this!");
                        }),
                    new Tuple<Action, Action<Exception>>(
                        () =>
                        {
                            Raven = (RavenContent)Activator.CreateInstance(ravenContenType, JumpKing.Game1.instance.Content.Load<Texture2D>("Mods/Resources/raven"));
                        },
                        (Exception e) =>
                        {
                            logger.Warning($"Attempted to Load Raven Content using JK+'s endpoint and failed!");
                        }));

                if (Raven != null)
                {
                    logger.Information($"Loaded Raven Content!");
                }
                else
                {
                    logger.Information($"Failed to Load Raven Content!");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to initialise RavensModContentManager: {e.ToString()}");
            }
        }
    }
}
