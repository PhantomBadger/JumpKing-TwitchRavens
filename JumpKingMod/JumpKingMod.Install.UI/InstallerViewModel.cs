using Logging;
using Logging.API;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace JumpKingMod.Install.UI
{
    public class InstallerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The directory to install our mod to
        /// </summary>
        public string GameDirectory
        {
            get
            {
                return gameDirectory;
            }
            set
            {
                if (gameDirectory != value)
                {
                    gameDirectory = value;
                    RaisePropertyChanged(nameof(GameDirectory));
                    InstallCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private string gameDirectory;

        /// <summary>
        /// The directory of our install media
        /// </summary>
        public string ModDirectory
        {
            get
            {
                return modDirectory;
            }
            set
            {
                if (modDirectory != value)
                {
                    modDirectory = value;
                    RaisePropertyChanged(nameof(ModDirectory));
                    InstallCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private string modDirectory;

        public ICommand BrowseGameDirectoryCommand { get; private set; }
        public ICommand BrowseModDirectoryCommand { get; private set; }
        public DelegateCommand InstallCommand { get; private set; }

        private readonly ILogger logger;

        private const string ExpectedFrameworkDllName = "MonoGame.Framework.dll";
        private const string ExpectedModDllName = "JumpKingModLoader.dll";

        /// <summary>
        /// Ctor for creating a <see cref="InstallerViewModel"/>
        /// </summary>
        public InstallerViewModel()
        {
            logger = new ConsoleLogger();
            InitialiseCommands();
        }

        /// <summary>
        /// Initialises the commands used for this window
        /// </summary>
        private void InitialiseCommands()
        {
            BrowseGameDirectoryCommand = new DelegateCommand(_ => { BrowseForGameDirectory(); });
            BrowseModDirectoryCommand = new DelegateCommand(_ => { BrowseForModDirectory(); });
            InstallCommand = new DelegateCommand(_ => { InstallMod(); }, _ => { return CanInstallMod(); });
        }

        /// <summary>
        /// Asks the user to select the game's install directory
        /// </summary>
        private void BrowseForGameDirectory()
        {
            string userPickedVal = AskUserForFolder(GameDirectory);
            if (!string.IsNullOrWhiteSpace(userPickedVal))
            {
                GameDirectory = userPickedVal;
            }
        }

        /// <summary>
        /// Asks the user to select the Mod's install directory
        /// </summary>
        private void BrowseForModDirectory()
        {
            string userPickedVal = AskUserForFolder(ModDirectory);
            if (!string.IsNullOrWhiteSpace(userPickedVal))
            {
                ModDirectory = userPickedVal;
            }
        }

        /// <summary>
        /// Attempts to copy the local mod folder to the remote location, then attempts to install the mod
        /// </summary>
        private void InstallMod()
        {
            // Copy over local mods to destination mods
            string expectedRemoteModFolder = Path.Combine(GameDirectory, "Content", "Mods");
            bool result = true;
            string errorText = string.Empty;
            if (Directory.Exists(ModDirectory))
            {
                try
                {
                    Directory.CreateDirectory(expectedRemoteModFolder);

                    string[] dllFiles = Directory.GetFiles(ModDirectory);
                    if (dllFiles.Length > 0)
                    {
                        for (int i = 0; i < dllFiles.Length; i++)
                        {
                            string dstFilePath = Path.Combine(expectedRemoteModFolder, Path.GetFileName(dllFiles[i]));
                            File.Copy(dllFiles[i], dstFilePath, true);
                        }
                    }
                    else
                    {
                        errorText = $"Failed to identify any files in our Install Directory at '{ModDirectory}'";
                        result = false;
                        logger.Error(errorText);
                    }
                }
                catch (Exception e)
                {
                    errorText = $"Failed to Install Mod due to Exception: {e.ToString()}";
                    result = false;
                    logger.Error(errorText);
                }
            }
            else
            {
                result = false;
                errorText = $"Failed to Install Mod as we couldn't find our local install media in '{ModDirectory}'!";
            }

            // Do the Install
            if (result)
            {
                Installer installer = new Installer();

                string frameworkDllPath = Path.Combine(GameDirectory, ExpectedFrameworkDllName);
                string expectedModDllPath = Path.Combine(expectedRemoteModFolder, ExpectedModDllName);
                ModEntrySettings modEntrySettings = new ModEntrySettings()
                {
                    EntryClassTypeName = "JumpKingModLoader.Loader",
                    EntryMethodName = "Init"
                };
                result = installer.InstallMod(frameworkDllPath, expectedModDllPath, modEntrySettings, out string error);
                errorText = error;
            }

            // Report the result
            if (result)
            {
                MessageBox.Show("JumpKingMod Installed Correctly!");
            }
            else
            {
                MessageBox.Show($"JumpKingMod Failed to Install! Error: {errorText}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Called by <see cref="InstallCommand"/> to determine whether the command can execute
        /// </summary>
        private bool CanInstallMod()
        {
            return CheckValidDirectories();
        }

        /// <summary>
        /// Checks if the install directory exists and contains the expected .dll
        /// </summary>
        private bool CheckValidDirectories()
        {
            bool validGameDir = false;
            if (Directory.Exists(GameDirectory))
            {
                string dllPath = Path.Combine(GameDirectory, ExpectedFrameworkDllName);
                validGameDir = File.Exists(dllPath);
            }

            bool validModDir = false;
            if (Directory.Exists(ModDirectory))
            {
                string dllPath = Path.Combine(ModDirectory, ExpectedModDllName);
                validModDir = File.Exists(dllPath);
            }

            return validGameDir && validModDir;
        }

        /// <summary>
        /// Utility function which wraps a CommonOpenFileDialog which is used to ask the user for a folder
        /// </summary>
        private string AskUserForFolder(string initialDirectory)
        {
            CommonOpenFileDialog openDialog = new CommonOpenFileDialog();
            openDialog.IsFolderPicker = true;
            if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
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
        /// Invokes the <see cref="PropertyChanged"/> event
        /// </summary>
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
