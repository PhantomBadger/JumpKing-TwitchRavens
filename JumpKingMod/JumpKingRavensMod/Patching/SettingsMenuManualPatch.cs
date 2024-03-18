using BehaviorTree;
using HarmonyLib;
using JumpKing.PauseMenu;
using JumpKing.PauseMenu.BT;
using JumpKingRavensMod.Button;
using JumpKingRavensMod.Settings.Editor;
using Logging.API;
using Microsoft.Xna.Framework;
using PBJKModBase.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JumpKingRavensMod.Patching
{
    /// <summary>
    /// An implementation of <see cref="IManualPatch"/> that adds a menu button for editing the raven settings
    /// </summary>
    public class SettingsMenuManualPatch : IManualPatch
    {
        private static ILogger logger;

        public static event EventHandler OnSettingsChanged;

        /// <summary>
        /// Ctor for creating a <see cref="SettingsMenuManualPatch"/>
        /// </summary>
        /// <param name="logger">An <see cref="ILogger"/> implementation for logging</param>
        public SettingsMenuManualPatch(ILogger logger)
        {
            SettingsMenuManualPatch.logger = logger;
        }

        /// <inheritdoc/>
        public void SetUpManualPatch(Harmony harmony)
        {
            var createOptionsMenuMethod = AccessTools.Method("JumpKing.PauseMenu.MenuFactory:CreateOptionsMenu");
            var postfixMethod = AccessTools.Method($"{this.GetType().Name}:PostfixPatchMethod");
            harmony.Patch(createOptionsMenuMethod, postfix: new HarmonyMethod(postfixMethod));
        }

        /// <summary>
        /// Called after JumpKing.PauseMenu.MenuFactory:CreateOptionsMenu to add our own button before the 'back' button at the end
        /// </summary>
        public static void PostfixPatchMethod(ref MenuSelector __result)
        {
            // Gotta get the back button, replace it, then re-add it, and re-call reinitialise 
            // to ensure our button and the back button are both picked up
            var backButton = __result.Children.Last();
            __result.Children[__result.Children.Length - 1] =
                new DelegateButton("Raven Settings", new Color(0, 255, 0), displayExploreTexture: true, () => OpenSettings());
            __result.Children.AddItem(backButton);
            __result.Initialize();
        }

        /// <summary>
        /// Opens the settings editor, calls <see cref="OnSettingsChanged"/> when exiting
        /// </summary>
        public static void OpenSettings()
        {
            SettingsEditor editor = new SettingsEditor();
            SettingsEditorViewModel vm = new SettingsEditorViewModel(logger, Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

            editor.DataContext = vm;

            bool? response = editor.ShowDialog();
            OnSettingsChanged?.Invoke(null, EventArgs.Empty);
        }
    }
}
