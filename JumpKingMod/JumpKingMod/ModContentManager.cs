using HarmonyLib;
using Logging.API;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static JumpKing.JKContentManager.RavenSprites;

namespace JumpKingMod
{
    /// <summary>
    /// A static class which loads the appropriate resources the mod needs
    /// </summary>
    public static class ModContentManager
    {
        /// <summary>
        /// An 'Arial-Unicode-MS' font we use for fallbacks
        /// </summary>
        public static SpriteFont ArialUnicodeMS;

        public static RavenContent Raven;

        /// <summary>
        /// Load all the Mod-specific content
        /// </summary>
        public static void LoadContent(ILogger logger)
        {
            try
            {
                ArialUnicodeMS = JumpKing.Game1.instance.Content.Load<SpriteFont>("Mods/Resources/arial-unicode-ms");
                ArialUnicodeMS.DefaultCharacter = '#';
                logger.Information($"Loaded Arial Unicode MS Fallback Font");

                // Constructor for Raven Content is internal, we have to use Activator and reflection to create it
                // Pass in our raven version and it will split it for us
                Type ravenContenType = AccessTools.TypeByName("JumpKing.JKContentManager+RavenSprites+RavenContent")
                    ?? throw new InvalidOperationException($"Cannot find 'JumpKing.JKContentManager+RavenSprites+RavenContent' type in Jump King");
                Raven = (RavenContent)Activator.CreateInstance(ravenContenType, 
                    BindingFlags.NonPublic | BindingFlags.Instance, 
                    binder: null, 
                    new object[] { JumpKing.Game1.instance.Content.Load<Texture2D>("Mods/Resources/raven") }, 
                    culture: null);
                logger.Information($"Loaded Raven Content!");
            }
            catch (Exception e)
            {
                logger.Error($"Failed to initialise ModContentManager: {e.ToString()}");
            }
        }
    }
}
