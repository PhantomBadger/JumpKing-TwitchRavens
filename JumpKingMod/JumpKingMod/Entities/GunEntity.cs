using HarmonyLib;
using JumpKing;
using JumpKing.PlayerPreferences.Persocom;
using JumpKingMod.API;
using JumpKingMod.Entities.Raven;
using Logging.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Entities
{
    /// <summary>
    /// An implementation of <see cref="IForegroundModEntity"/> which draws a scope on screen when active and destroys the ravens
    /// </summary>
    public class GunEntity : IForegroundModEntity
    {
        private readonly MessengerRavenSpawningEntity spawningEntity;
        private readonly ModEntityManager modEntityManager;
        private readonly Keys toggleGunKey;
        private readonly ILogger logger;
        private readonly Sprite scopeSprite;
        private readonly MethodInfo getPrefsMethodInfo;
        private readonly FieldInfo graphicsPrefInstanceFieldInfo;

        private bool isGunActive;
        private bool gunToggleCooldown;
        private bool clickCooldown;

        /// <summary>
        /// Constructor for creating a <see cref="GunEntity"/>
        /// </summary>
        /// <param name="spawningEntity">The <see cref="MessengerRavenSpawningEntity"/> to use to poll existing ravens</param>
        /// <param name="modEntityManager">The <see cref="ModEntityManager"/> to register to</param>
        /// <param name="logger">An <see cref="ILogger"/> implementation to log to</param>
        public GunEntity(MessengerRavenSpawningEntity spawningEntity, ModEntityManager modEntityManager, ILogger logger)
        {
            this.spawningEntity = spawningEntity ?? throw new ArgumentNullException(nameof(spawningEntity));
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            isGunActive = false;
            gunToggleCooldown = false;
            toggleGunKey = Keys.F8;

            scopeSprite = Sprite.CreateSprite(ModContentManager.ScopeTexture);
            scopeSprite.center = Vector2.One / 2f;

            modEntityManager.AddForegroundEntity(this);

            // Attempt to be able to assess the screen scale via reflection as this changes our mouse positions
            try
            {
                Type graphicsPrefType = AccessTools.TypeByName("JumpKing.PlayerPreferences.Persocom.GraphicsPreferencesRuntime");
                graphicsPrefInstanceFieldInfo = graphicsPrefType.BaseType.GetField("instance", BindingFlags.Public | BindingFlags.Static);
                getPrefsMethodInfo = graphicsPrefType.BaseType.GetMethod("GetPrefs");
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }
        }

        /// <summary>
        /// Draw the scope in the foreground when active
        /// </summary>
        public void ForegroundDraw()
        {
            if (isGunActive)
            {
                MouseState mouseState = Mouse.GetState();
                Point mousePosition = (mouseState.Position.ToVector2() / GetScreenScale()).ToPoint();
                scopeSprite.Draw(mousePosition, SpriteEffects.None);

                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (!clickCooldown)
                    {
                        MessengerRavenEntity raven = spawningEntity.TryGetMessengerRaven(mousePosition);
                        if (raven != null)
                        {
                            logger.Information($"Killing Raven");
                            raven.SetKillState();
                        }
                        clickCooldown = true;
                    }
                }
                else
                {
                    clickCooldown = false;
                }
            }
        }

        /// <summary>
        /// Identify if the gun is currently active or not
        /// </summary>
        public void Update(float p_delta)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            // Toggle the gun behaviour
            if (keyboardState.IsKeyDown(toggleGunKey))
            {
                if (!gunToggleCooldown)
                {
                    logger.Information($"{toggleGunKey.ToString()} Pressed - Toggling Gun State to {!isGunActive}");
                    isGunActive = !isGunActive;

                    gunToggleCooldown = true;
                }
            }
            else
            {
                gunToggleCooldown = false;
            }
        }

        /// <summary>
        /// Uses reflection to get the current graphics prefs, and sets the screen scale appropriately
        /// </summary>
        private float GetScreenScale()
        {
            try
            {
                GraphicPrefs gp = (GraphicPrefs)getPrefsMethodInfo.Invoke(graphicsPrefInstanceFieldInfo.GetValue(null), null);
                switch (gp.size_mode)
                {
                    case SizeMode.x1:
                        return 1f;
                    case SizeMode.x2:
                        return 2f;
                    case SizeMode.x3:
                        return 3f;
                    default:
                        logger.Warning($"Unknown size mode of {gp.size_mode} identified, using 1 instead");
                        return 1f;
                }
            }
            catch (Exception e)
            {
                return 1f;
            }
        }
    }
}
