using HarmonyLib;
using JumpKing;
using Logging.API;
using Microsoft.Xna.Framework;
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

        public static Texture2D HealthBarBackTexture;

        public static Texture2D HealthBarFrontTexture;

        /// <summary>
        /// Load all the Mod-specific content
        /// </summary>
        public static void LoadContent(ILogger logger)
        {
            try
            {
                YouDiedTexture = JumpKing.Game1.instance.Content.Load<Texture2D>("Mods/Resources/youdied");
                logger.Information($"Loaded 'You Died' Texture");

                HealthBarBackTexture = JumpKing.Game1.instance.Content.Load<Texture2D>("Mods/Resources/healthbar_back");
                logger.Information($"Loaded 'Health Bar Back' Texture");

                HealthBarFrontTexture = JumpKing.Game1.instance.Content.Load<Texture2D>("Mods/Resources/healthbar_front");
                logger.Information($"Loaded 'Health Bar Front' Texture");
            }
            catch (Exception e)
            {
                logger.Error($"Failed to initialise ModifiersModContentManager: {e.ToString()}");
            }
        }
    }
}
