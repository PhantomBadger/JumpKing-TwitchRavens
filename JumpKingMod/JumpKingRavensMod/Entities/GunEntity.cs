using HarmonyLib;
using JumpKing;
using JumpKing.PlayerPreferences.Persocom;
using JumpKingRavensMod.API;
using JumpKingRavensMod.Entities.Raven;
using JumpKingRavensMod.Settings;
using Logging.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PBJKModBase.API;
using PBJKModBase.Entities;
using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.Entities
{
    /// <summary>
    /// An implementation of <see cref="IForegroundModEntity"/> which draws a scope on screen when active and destroys the ravens
    /// </summary>
    public class GunEntity : IForegroundModEntity
    {
        private readonly MessengerRavenSpawningEntity spawningEntity;
        private readonly ModEntityManager modEntityManager;
        private readonly UserSettings userSettings;
        private Keys toggleGunKey;
        private readonly ILogger logger;
        private readonly Sprite scopeSprite;
        private readonly MethodInfo getPrefsMethodInfo;
        private readonly FieldInfo graphicsPrefInstanceFieldInfo;

        private bool isGunActive;
        private bool gunToggleCooldown;
        private bool clickCooldown;
        private float shootCooldownCounter;
        private Point currentMousePosition;
        private float score;
        private UITextEntity scoreTextEntity;

        private const float CooldownMaxInSeconds = 0.5f;
        private const float ScopeSpriteMaxScale = 1.25f;
        private const float ScopeSpriteMinScale = 1.0f;
        private const float ScoreIncrement = 100;
        private const float MaxScore = 99999;

        /// <summary>
        /// Constructor for creating a <see cref="GunEntity"/>
        /// </summary>
        /// <param name="spawningEntity">The <see cref="MessengerRavenSpawningEntity"/> to use to poll existing ravens</param>
        /// <param name="modEntityManager">The <see cref="ModEntityManager"/> to register to</param>
        /// <param name="logger">An <see cref="ILogger"/> implementation to log to</param>
        public GunEntity(MessengerRavenSpawningEntity spawningEntity, ModEntityManager modEntityManager, UserSettings userSettings, ILogger logger)
        {
            this.spawningEntity = spawningEntity ?? throw new ArgumentNullException(nameof(spawningEntity));
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            isGunActive = false;
            gunToggleCooldown = false;

            ReadSettings();
            userSettings.OnSettingsInvalidated += (o, e) => ReadSettings();

            shootCooldownCounter = CooldownMaxInSeconds;

            scopeSprite = Sprite.CreateSpriteWithCenter(RavensModContentManager.ScopeTexture,
                new Rectangle(0, 0, RavensModContentManager.ScopeTexture.Width, RavensModContentManager.ScopeTexture.Height),
                new Vector2(0.5f, 0.5f));
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

        private void ReadSettings()
        {
            toggleGunKey = userSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.GunToggleKeyKey, Keys.F8);
        }

        /// <summary>
        /// Draw the scope in the foreground when active
        /// </summary>
        public void ForegroundDraw()
        {
            // Render the gun at the mouse position
            if (isGunActive)
            {
                // Scale the sprite depending on the current cooldown
                float scale = MathHelper.Lerp(ScopeSpriteMaxScale, ScopeSpriteMinScale, shootCooldownCounter / CooldownMaxInSeconds);
                Game1.spriteBatch.Draw(scopeSprite.texture,
                    currentMousePosition.ToVector2() - (scopeSprite.source.Size.ToVector2() * scopeSprite.center * scale),
                    scopeSprite.source,
                    scopeSprite.GetColor(),
                    0f,
                    Vector2.Zero,
                    new Vector2(scale, scale),
                    SpriteEffects.None, 0f);
            }
        }

        /// <summary>
        /// Identify if the gun is currently active or not
        /// </summary>
        public void Update(float delta)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            // Toggle the gun behaviour
            if (keyboardState.IsKeyDown(toggleGunKey))
            {
                if (!gunToggleCooldown)
                {
                    logger.Information($"{toggleGunKey.ToString()} Pressed - Toggling Gun State to {!isGunActive}");
                    isGunActive = !isGunActive;

                    if (isGunActive)
                    {
                        scoreTextEntity = new UITextEntity(modEntityManager, 
                            new Vector2(450, 20), 
                            "0", 
                            Color.White, 
                            UIEntityAnchor.Center);
                    }
                    else
                    {
                        scoreTextEntity.Dispose();
                        scoreTextEntity = null;
                    }

                    gunToggleCooldown = true;
                }
            }
            else
            {
                gunToggleCooldown = false;
            }

            MouseState mouseState = Mouse.GetState();
            if (isGunActive)
            {
                currentMousePosition = (mouseState.Position.ToVector2() / GetScreenScale()).ToPoint();

                // Handle the 'Shooting' logic
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (!clickCooldown)
                    {
                        MessengerRavenEntity raven = spawningEntity.TryGetMessengerRaven(currentMousePosition);
                        if (raven != null)
                        {
                            logger.Information($"Killing Raven");
                            raven.SetKillState();

                            score += ScoreIncrement;
                            score = Math.Min(score, MaxScore);
                        }
                        clickCooldown = true;
                        shootCooldownCounter = 0;
                    }
                }

                // Update the score
                if (scoreTextEntity != null)
                {
                    scoreTextEntity.TextValue = score.ToString().PadLeft(5, '0');
                }
            }

            // Handle the shooting cooldown
            if (clickCooldown && (shootCooldownCounter += delta) > CooldownMaxInSeconds)
            {
                if (mouseState.LeftButton == ButtonState.Released)
                {
                    clickCooldown = false;
                    shootCooldownCounter = CooldownMaxInSeconds;
                }
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
