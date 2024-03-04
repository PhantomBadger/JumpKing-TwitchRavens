using Logging.API;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace PBJKModBase
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

        /// <summary>
        /// Load all the Mod-specific content
        /// </summary>
        public static void LoadContent(ILogger logger)
        {
            try
            {
                ArialUnicodeMS = JumpKing.Game1.instance.Content.Load<SpriteFont>("Content/JKMods/Resources/arial-unicode-ms");
                ArialUnicodeMS.DefaultCharacter = '#';
                logger.Information($"Loaded Arial Unicode MS Fallback Font");
            }
            catch (Exception e)
            {
                logger.Error($"Failed to initialise ModContentManager: {e.ToString()}");
            }
        }
    }
}
