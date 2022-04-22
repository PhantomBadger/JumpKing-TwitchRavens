# For JK+ v1.8.0 please use [Release v3.0](https://github.com/PhantomBadger/JumpKing-TwitchRavens/releases/tag/v3.0)

# Jump King Chat Raven Mod
This Mod allows your Twitch or YouTube Chat to lend words of encouragement whilst you climb to the babe! 

## Features
### Twitch & YouTube Ravens
Both Twitch and YouTube Chat can now communicate with you within the world of Jump King! Messages from chat will be parroted by small ravens that land on the stage as you traverse it. You have the option of gating this behind a Channel Point reward for busier chats!
<p align="center">
  <img src="https://user-images.githubusercontent.com/9095972/135728881-c4a61ccb-663b-4218-8f22-9ece0366592a.gif" width="75%" height="75%" alt="Ravens land on the stage to relay messages from Stream Chat!"/>
</p>

The Ravens can be triggered through different ways to suit your chat
- **Chat Message:** Spawns a Raven for every Chat Message that appears
- **Channel Point:** Spawns a Raven when a chatter redeems a specified Channel Point (Uses the text from that channel point!) *(Twitch Only)*
- **Insult:** Spawns Ravens when you fall, they choose from a pre-determined list of insults *(Twitch Only)*

### Gun Mode
Are your chat ravens dunking on you a bit _too_ hard for that last fall? Use the mouse-controlled Ravens Gun to blast them out of the sky and take out some of your rage!
<p align="center">
  <img src="https://user-images.githubusercontent.com/9095972/164544496-50060983-da29-4b0c-89bb-7bdcc1011a25.gif" width="75%" height="75%" alt="Shoot Ravens out fo the sky using your mouse to control a gun for a bit of stress relief!"/>
</p>

### Free Flying
Toggle a 'free flying' mode for the Jump King, which will let you explore or practice jumps without consequence!

_**Note:** Enabling this aspect of the mod will disable achievements in the game until the Free Flying mod is turned off in the settings!_

## Installation

[Check out the Instructions here!](https://github.com/PhantomBadger/JumpKing-TwitchRavens/blob/main/docs/Installation.md)

For YouTube Support, check out [this guide](https://github.com/PhantomBadger/JumpKing-TwitchRavens/blob/main/docs/ConnectingToYouTube.md)

### Common Issues
<details>
  <summary>Operation is not supported. HRESULT: 0x80131515</summary>
  
  <p align="center">
    <img src="https://user-images.githubusercontent.com/9095972/137400957-8d5399be-3e28-46de-b589-d8fea48cbe2b.png" width="75%" height="75%"/>
  </p>

  Occurs when running the game after installing.
  This is because your computer is blocking the .dlls from being dynamically loaded by Jump King. You can either right click each .dll in the mods folder, go to 'properties' and then click 'Unblock'. Or alternatively, you can run a powershell command in the mod's directory such as `dir -Recurse | Unblock-File` to unblock the files all at once. 
</details>
  
## Updating the Settings

- You can re-launch the Installer UI at any point, and click the 'Load Settings' button. The Application will attempt to load a valid settings file from the specified Game Directory
- After which you can edit the settings you desire, click 'Save Settings', and your changes will take effect next time you launch the game 

## Uninstallation

- To Uninstall the game, right click on the game in Steam, go to _Properties_, then _Local Files_, then click _Verify Integrity of game files..._
- These instructions are mirrored in the 'About' window in the installer
- For those not using Steam for the running of Jump King, you may need to redownload the game (Or if you made a copy of the MonoGame.Framework.dll you can just replace the one in the game directory with your copy!) 

# Contact
You can reach out to me on [Twitter](https://twitter.com/PhantomBadger_)

![heartchatmessage](https://user-images.githubusercontent.com/9095972/135729076-857302a4-7878-4654-b288-73283ae76090.png)

# Future Features

- [x] Configurable exclusion word list [Supported in v1.2](https://github.com/PhantomBadger/JumpKing-TwitchRavens/releases/tag/v1.2)
- [x] Custom Insults for the Insult Ravens [Supported in v1.2](https://github.com/PhantomBadger/JumpKing-TwitchRavens/releases/tag/v1.2)
- [x] Text Wrapping for Longer Messages [Supported in v1.2](https://github.com/PhantomBadger/JumpKing-TwitchRavens/releases/tag/v1.2)
- [x] Runtime Sub-Only Toggles [Supported in v1.2](https://github.com/PhantomBadger/JumpKing-TwitchRavens/releases/tag/v1.2)
- [ ] Twitch Emote Support
- [ ] Support for Different Raven Sprites
- [x] YouTube Chat Support [Supported in v2.0](https://github.com/PhantomBadger/JumpKing-TwitchRavens/releases/tag/v2.0)
