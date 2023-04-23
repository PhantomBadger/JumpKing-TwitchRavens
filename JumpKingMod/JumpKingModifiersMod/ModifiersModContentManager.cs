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
        public static Texture2D BloodSplatterRawTexture;
        public static Sprite[] BloodSplatterSprites;
        public static Texture2D LowVisibilityOverlayTexture;
        public static Texture2D LavaTexture;
        public static Texture2D KingDeathTexture;
        public static Texture2D CutoutRawTexture;
        public static Sprite[] CutoutSprites;

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

                BloodSplatterRawTexture = JumpKing.Game1.instance.Content.Load<Texture2D>("Mods/Resources/bloodsplat");
                BloodSplatterSprites = SpriteChopUtilGrid(BloodSplatterRawTexture, new Point(2, 4), Vector2.Zero, BloodSplatterRawTexture.Bounds);
                logger.Information($"Loaded 'Bloodsplat' Textures");

                LowVisibilityOverlayTexture = JumpKing.Game1.instance.Content.Load<Texture2D>("Mods/Resources/lowVisibilityOverlay");
                logger.Information($"Loaded 'Low Visibility Overlay' Texture");

                LavaTexture = JumpKing.Game1.instance.Content.Load<Texture2D>("Mods/Resources/lava");
                logger.Information($"Loaded 'Lava' Texture");

                KingDeathTexture = JumpKing.Game1.instance.Content.Load<Texture2D>("Mods/Resources/king_death");
                logger.Information($"Loaded 'King Death' Texture");

                CutoutRawTexture = JumpKing.Game1.instance.Content.Load<Texture2D>("Mods/Resources/cutout");
                CutoutSprites = SpriteChopUtilGrid(CutoutRawTexture, new Point(4, 6), Vector2.Zero, CutoutRawTexture.Bounds);
                logger.Information($"Loaded 'Cutout' Textures");
            }
            catch (Exception e)
            {
                logger.Error($"Failed to initialise ModifiersModContentManager: {e.ToString()}");
            }
        }

        /// <summary>
        /// Copied from JumpKing.JKContentManager.Util to save us needing to worry about accessing it
        /// via reflection. This is internal in the base game but public in JK+
        /// </summary>
        internal static Sprite[] SpriteChopUtilGrid(Texture2D p_texture, Point p_cells, Vector2 p_center, Rectangle p_source)
        {
            int num = p_source.Width / p_cells.X;
            int num2 = p_source.Height / p_cells.Y;
            Sprite[] array = new Sprite[p_cells.X * p_cells.Y];
            for (int i = 0; i < p_cells.X; i++)
            {
                for (int j = 0; j < p_cells.Y; j++)
                {
                    array[i + j * p_cells.X] = Sprite.CreateSpriteWithCenter(p_texture, new Rectangle(p_source.X + num * i, p_source.Y + num2 * j, num, num2), p_center);
                }
            }
            return array;
        }
    }
}
