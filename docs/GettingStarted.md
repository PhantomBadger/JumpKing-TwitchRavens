# Getting Started
_This information is accurate as of Release v4.1_

1. [Installation](https://github.com/PhantomBadger/JumpKing-TwitchRavens/blob/main/docs/GettingStarted.md#installation)
2. [Chat Ravens Mod](https://github.com/PhantomBadger/JumpKing-TwitchRavens/blob/main/docs/GettingStarted.md#chat-ravens-mod)
3. [Fall Damage Mod](https://github.com/PhantomBadger/JumpKing-TwitchRavens/blob/main/docs/GettingStarted.md#fall-damage-mod)
4. [Editing Settings](https://github.com/PhantomBadger/JumpKing-TwitchRavens/blob/main/docs/GettingStarted.md#editing-settings)

## Installation

Check the [Release](https://github.com/PhantomBadger/JumpKingMod/releases/) page for the latest download link! Click on the .zip file at the bottom to download

<img src="https://user-images.githubusercontent.com/9095972/233857807-74a2b146-bb56-4883-b574-9c6e3dd3ebad.png" width="75%"/>

Once downloaded, unzip the file. Inside you will find two folders: 'Installer' and 'Mod Files'

<img src="https://user-images.githubusercontent.com/9095972/233857851-aa60e0b6-0cd0-4cea-8b32-59f23d15d8fd.png" width="75%"/>

Open 'Installer' and run the 'JumpKingMod.Install.UI.exe' file

![image](https://user-images.githubusercontent.com/9095972/233857866-d0dad755-4200-43aa-b2d0-94bb79501a10.png)

![image](https://user-images.githubusercontent.com/9095972/233857878-10e09fe3-687c-4f86-908c-83a0cfc406f4.png)

This app is your one-stop-shop for Installing my mods and editing their settings in the future!

Click the '...' next to the **'Install Directory'** text box and navigate to your Jump King Install Directory (where your JumpKing.exe is located) and click 'Select Folder'. If you're not sure where this is, right click Jump King in Steam and click on 'Browse Local Files'

![image](https://user-images.githubusercontent.com/9095972/233857972-d969c9bc-7e19-4ed7-b03f-646d5ab16372.png)

<img src="https://user-images.githubusercontent.com/9095972/233857993-71f02eb3-cf9e-4bf7-8a17-f3c52e08076d.png" width="50%"/>

Click the '...' next to the **'Mod Directory'** text box and navigate to the 'Mod Files' folder in the .zip you downloaded and unpackaged

![image](https://user-images.githubusercontent.com/9095972/233858063-d6870cbe-84df-4296-af4e-668c90852d52.png)

Once complete there should be no more red text. You can now click **'Install'**

![image](https://user-images.githubusercontent.com/9095972/233858091-287f7960-ae52-4d40-a67f-f6144ef13e0d.png)

Once complete a pop-up confirming installation will appear! You can also look at the Console Window for additional feedback, and the settings will now be editable

![image](https://user-images.githubusercontent.com/9095972/233858176-8b7d8a3b-a581-49b8-b8e4-0609db305a4c.png)

## Chat Ravens Mod

The chat ravens mod allows Twitch/YouTube chat to spawn ravens/crows in game to display their messages of encouragement! It's very simple to set these up with the instructions below:

### Twitch Setup

First click the 'Twitch' side of the button, a **'Twitch'** tab will now be visible in the Settings Editor

![image](https://user-images.githubusercontent.com/9095972/233858244-5e4a74ff-8680-4889-bb35-12394b65847e.png)

Enter your Twitch Account Name and your Twitch OAuth token so the mod can connect to your Twitch Chat. For more instructions on how to generate a [Twitch OAuth token click here](https://github.com/PhantomBadger/JumpKing-TwitchRavens/blob/main/docs/HowToGenerateCredentials.md#twitch)

### YouTube Setup

First click the 'YouTube side of the button, a **'YouTube'** tab will now be visible in the Settings Editor

![image](https://user-images.githubusercontent.com/9095972/233858375-8253cc8c-f809-49ec-b758-c94f65c4f076.png)

Enter your YouTube Channel ID (Which can be found using [websites like this](https://commentpicker.com/youtube-channel-id.php) or through [your Channel Settings](https://support.google.com/youtube/answer/3250431?hl=en-GB)) and your API Key. For more instructions on how to generate a [YouTube API Key click here](https://github.com/PhantomBadger/JumpKing-TwitchRavens/blob/main/docs/HowToGenerateCredentials.md#youtube). 

For YouTube you will also need to specify a key to be pressed to connect to the chat once you are live. [More info on this process is available here](https://github.com/PhantomBadger/JumpKing-TwitchRavens/blob/main/docs/ConnectingToYouTube.md)

### For a breakdown of all Ravens Settings [click here](https://github.com/PhantomBadger/JumpKing-TwitchRavens/blob/main/docs/RavensModSettings.md)

## Fall Damage Mod
_Added in Release v4.1_
The Fall Damage Mod is currently packaged alongside the Ravens Mod released previously. However it can be used with or without the chat Ravens.

### For a breakdown of all Fall Damage Settings [click here](https://github.com/PhantomBadger/JumpKing-TwitchRavens/blob/main/docs/FallDamageModSettings.md)

### Nice Spawns
With 'Nice Spawns' enabled, a death in 'New Babe+' will spawn you here:

<img src="https://user-images.githubusercontent.com/9095972/233860256-df48c841-697c-4823-b918-2c602c781704.png" width="50%"/>

Whereas a death in 'Ghost of the Babe' will spawn you here:

<img src="https://user-images.githubusercontent.com/9095972/233860314-cc35726a-50cb-4c27-b55c-6b6eac727bd2.png" width="50%"/>

Without 'Nice Spawns' enabled, a death in any map will spawn you here:

<img src="https://user-images.githubusercontent.com/9095972/233860334-48041723-3f29-495a-8cd5-becb665d1f93.png" width="50%"/>

### Custom Maps

For custom maps using JK+, all deaths will spawn you at the start of the map

### Safe Falls

Any fall that lands in Deep Snow or the Bog won't harm you. Additionally the required fall in 'Ghost of the Babe' won't harm you either.

### Custom Death Text

You can add a 'FallDamageSubtexts.txt' file to the 'Content/Mods' folder of your Jump King install directory. If present the game will pick a random line from the file to display when the player has died

![image](https://user-images.githubusercontent.com/9095972/233860969-bfd4fb2f-2f9e-4c92-bfa0-3cf95b3a849e.png)

<img src="https://user-images.githubusercontent.com/9095972/233861040-6716c32c-c59a-459c-900a-98a084b100a2.png" width="50%"/>

## Editing Settings

To edit your mod settings, run the `JumpKingMod.Install.UI.exe` program and ensure the **'Install Directory'** is correct using the steps outlined above. Then click 'Load Settings'.

![image](https://user-images.githubusercontent.com/9095972/233861614-230aa22d-5480-44af-91f4-f76e9b268cb9.png)

After which your settings should be populated!

![image](https://user-images.githubusercontent.com/9095972/233861649-0fa2cadb-4269-47e8-81e0-8ecd1dc5e625.png)

Once finished editing, click 'Save Settings' - you will receive a small pop-up letting you know it has been done successfully.

![image](https://user-images.githubusercontent.com/9095972/233861679-6c384d86-c8bc-46ba-8689-fb8542e5b497.png)

You will now need to restart Jump King for these settings to take effect.





