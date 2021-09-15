using JumpKingMod.Settings;
using Logging;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JumpKingMod.Install.UI
{
    public partial class InstallerForm : Form
    {
        private readonly UserSettings installerSettings;

        private const string ExpectedModDllName = "JumpKingMod.dll";
        private const string ExpectedFrameworkDllName = "MonoGame.Framework.dll";
        private const string InstallerSettingsFileName = "JumpKingInstaller.settings";
        private readonly Dictionary<string, string> DefaultUserSettings = new Dictionary<string, string>()
        {
            { "InstallDir", "" },
            { "ModDir", "" }
        };

        public InstallerForm()
        {
            InitializeComponent();
            installerSettings = new UserSettings(InstallerSettingsFileName, DefaultUserSettings, new ConsoleLogger());
        }

        /// <summary>
        /// Called from the 'File' Tool Strip. Shows the 'About' form.
        /// </summary>
        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            AboutForm aboutForm = new AboutForm();
            aboutForm.ShowDialog();
        }

        /// <summary>
        /// Called from the 'File' Tool Strip. Exits the application.
        /// </summary>
        private void tlstrpExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Called on Load, attempts to get the last settings used, makes best guesses where possible if not present
        /// </summary>
        private void InstallerForm_Load(object sender, EventArgs e)
        {
            // Bind the check method to the text updating
            txtInstallDir.TextChanged += CheckValidDirectories;
            txtModDir.TextChanged += CheckValidDirectories;

            // Load the settings
            string lastInstallDir = installerSettings.GetSettingOrDefault("InstallDir", string.Empty);

            if (string.IsNullOrWhiteSpace(lastInstallDir))
            {
                // Take a guess at the install directory
                string programFiles = Environment.GetEnvironmentVariable("ProgramFiles");
                if (!string.IsNullOrWhiteSpace(programFiles))
                {
                    string possibleInstallDir = Path.Combine(programFiles, "Steam", "steamapps", "common", "Jump King");
                    txtInstallDir.Text = possibleInstallDir;
                }
            }
            else
            {
                txtInstallDir.Text = lastInstallDir;
            }

            string lastModDir = installerSettings.GetSettingOrDefault("ModDir", string.Empty);
            txtModDir.Text = lastModDir;
        }

        /// <summary>
        /// Called on close, updates the settings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InstallerForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            installerSettings.SetOrCreateSetting("InstallDir", txtInstallDir.Text);
            installerSettings.SetOrCreateSetting("ModDir", txtModDir.Text);
        }

        /// <summary>
        /// Called when a user clicks the '...' button, opens a dialog box to find the Jump King .exe
        /// </summary>
        private void btnInstallDir_Click(object sender, EventArgs e)
        {
            string userPickedVal = AskUserForFolder(txtInstallDir.Text);
            if (!string.IsNullOrWhiteSpace(userPickedVal))
            {
                txtInstallDir.Text = userPickedVal;
            }
        }

        /// <summary>
        /// Called when a user clicks the '...' button, opens a dialog box to find the mod .dll
        /// </summary>
        private void btnModDir_Click(object sender, EventArgs e)
        {
            string userPickedVal = AskUserForFolder(txtModDir.Text);
            if (!string.IsNullOrWhiteSpace(userPickedVal))
            {
                txtModDir.Text = userPickedVal;
            }
        }

        /// <summary>
        /// Called when a user clicks the 'Install' button, attempts to install the Mod Hook
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInstall_Click(object sender, EventArgs e)
        {
            Installer installer = new Installer();

            string frameworkDllPath = Path.Combine(txtInstallDir.Text, ExpectedFrameworkDllName);
            string modDllPath = Path.Combine(txtModDir.Text, ExpectedModDllName);
            ModEntrySettings modEntrySettings = new ModEntrySettings()
            {
                EntryClassTypeName = "JumpKingMod.JumpKingModEntry",
                EntryMethodName = "Init"
            };
            bool result = installer.InstallMod(frameworkDllPath, modDllPath, modEntrySettings, out string error);

            if (result)
            {
                MessageBox.Show("JumpKingMod Installed Correctly!");
            }
            else
            {
                MessageBox.Show($"JumpKingMod Failed to Install! Error: {error}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Utility function which wraps a CommonOpenFileDialog which is used to ask the user for a folder
        /// </summary>
        private string AskUserForFolder(string initialDirectory)
        {
            CommonOpenFileDialog openDialog = new CommonOpenFileDialog();
            openDialog.IsFolderPicker = true;
            if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(txtInstallDir.Text))
            {
                openDialog.InitialDirectory = initialDirectory;
            }

            if (openDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return openDialog.FileName;
            }
            return null;
        }

        /// <summary>
        /// Checks if the install directory exists and contains the expected .dll
        /// </summary>
        private void CheckValidDirectories(object sender, EventArgs e)
        {
            bool validInstallDir = false;
            bool validModDir = false;
            string installDir = txtInstallDir.Text;
            string modDir = txtModDir.Text;

            if (Directory.Exists(installDir))
            {
                string dllPath = Path.Combine(installDir, ExpectedFrameworkDllName);
                validInstallDir = File.Exists(dllPath);
            }

            if (Directory.Exists(modDir))
            {
                string expectedDLLPath = Path.Combine(modDir, ExpectedModDllName);
                validModDir = File.Exists(expectedDLLPath);
            }
            btnInstall.Enabled = validInstallDir && validModDir;
        }
    }
}
