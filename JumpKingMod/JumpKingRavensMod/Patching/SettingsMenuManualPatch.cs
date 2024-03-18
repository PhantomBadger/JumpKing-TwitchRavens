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
    public class SettingsMenuManualPatch : IManualPatch
    {
        private static ILogger logger;

        public static event EventHandler OnSettingsChanged;

        public SettingsMenuManualPatch(ILogger logger)
        {
            SettingsMenuManualPatch.logger = logger;
        }

        public void SetUpManualPatch(Harmony harmony)
        {
            var createOptionsMenuMethod = AccessTools.Method("JumpKing.PauseMenu.MenuFactory:CreateOptionsMenu");
            var postfixMethod = AccessTools.Method($"{this.GetType().Name}:PostfixPatchMethod");
            harmony.Patch(createOptionsMenuMethod, postfix: new HarmonyMethod(postfixMethod));
        }

        public static void PostfixPatchMethod(ref MenuSelector __result)
        {
            var backButton = __result.Children.Last();
            __result.Children[__result.Children.Length - 1] =
                new DelegateButton("Raven Settings", displayExploreTexture: true, () => OpenSettings());
            __result.Children.AddItem(backButton);
            __result.Initialize();
        }

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
