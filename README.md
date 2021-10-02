# Jump King Twitch Raven Mod
This Mod allows your Twitch Chat to lend words of encouragement whilst you climb to the babe! 

Ravens can fly onto the stage and relay messages from your community, triggered either on every chat message or via a configurable Channel Point Reward.

There's even an 'Insult' mode, where the Ravens will instead bully you every time you fall.

![birdchattest](https://user-images.githubusercontent.com/9095972/135728881-c4a61ccb-663b-4218-8f22-9ece0366592a.gif)

## Installation

**If you're not using Steam, make a copy of the MonoGame.Framework.dll in your Game's install directory first! It will make uninstalling easier!**

Check the Release page for the latest download
- Run the Installer.UI.exe inside the Installer Folder

![image](https://user-images.githubusercontent.com/9095972/135728412-5d00983e-8827-416d-8d55-3a87a5f9f6d7.png)
- Click the '...' next to the Game Directory text box and point this at your Jump King Install Directory (the place where JumpKing.exe is)
- Click the '...' next to the Mod Directory text box and point this at the Mod folder in the Install package
- Click 'Install', a pop-up should appear confirming it has succeeded
- You should fill in and Save the Settings via the Installer first, to ensure everything runs smoothly :)
- You can now launch the game normally from Steam/however you normally launch. You can be sure the mod has been installed correctly because a Console Window will open alongside

## Updating the Settings

- You can re-launch the Installer UI at any point, and click the 'Load Settings' button. The Application will attempt to load a valid settings file from the specified Game Directory
- After which you can edit the settings you desire, click 'Save Settings', and your changes will take effect next time you launch the game 

### Settings Info
Most of the settings in the Installer have ToolTips or extra text explaining what they do
- Raven Trigger Types:
  - **Chat Message:** Spawns a Raven for every Chat Message that appears
  - **Channel Point:** Spawns a Raven when a chatter redeems a specified Channel Point (Uses the text from that channel point!)
  - **Insult:** Spawns Ravens when you fall, they choose from a pre-determined list of insults

## Uninstallation

- To Uninstall the game, right click on the game in Steam, go to _Properties_, then _Local Files_, then click _Verify Integrity of game files..._
- These instructions are mirrored in the 'About' window in the installer
- For those not using Steam for the running of Jump King, you may need to redownload the game (Or if you made a copy of the MonoGame.Framework.dll you can just replace the one in the game directory with your copy!) 

# Contact
You can reach out to me on [Twitter](https://twitter.com/PhantomBadger_)

![heartchatmessage](https://user-images.githubusercontent.com/9095972/135729076-857302a4-7878-4654-b288-73283ae76090.png)

# Future Features

- [x] Support for Babe of Ascension & JK+
- [ ] Custom Insults for the Insult Ravens
- [ ] Text Wrapping for Longer Messages
- [ ] Runtime Sub-Only Toggles
- [ ] Twitch Emote Support
- [ ] Support for Different Raven Sprites
