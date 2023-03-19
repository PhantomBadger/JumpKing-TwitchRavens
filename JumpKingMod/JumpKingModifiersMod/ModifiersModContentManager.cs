using HarmonyLib;
using Logging.API;
using Microsoft.Xna.Framework.Graphics;
using PBJKModBase;
using System;
using System.Reflection;
using static JumpKing.JKContentManager.RavenSprites;

namespace JumpKingModifiersMod
{
    /// <summary>
    /// A static class which loads the appropriate resources the mod needs
    /// </summary>
    public static class ModifiersModContentManager
    {
        public static Texture2D YouDiedTexture;

        /// <summary>
        /// Load all the Mod-specific content
        /// </summary>
        public static void LoadContent(ILogger logger)
        {
            try
            {
                YouDiedTexture = JumpKing.Game1.instance.Content.Load<Texture2D>("Mods/Resources/youdied");
                logger.Information($"Loaded 'You Died' Texture");
            }
            catch (Exception e)
            {
                logger.Error($"Failed to initialise ModifiersModContentManager: {e.ToString()}");
            }
        }
    }
}
