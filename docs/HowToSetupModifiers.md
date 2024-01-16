# How to Setup Modifiers/Chaos Mode
The Jump King Modifiers Mod provides a selection of ways to change how the base game plays. You have full control over which modifiers are used and how they are triggered.

## Triggering a Modifier
There are two main ways of triggering the effects, "Toggle" and "Chat Poll" (and a third "None" option)

![image](https://github.com/PhantomBadger/JumpKingMod-PrivateMirror/assets/9095972/cc8a4c37-f641-495e-9f17-a87007999036)

- **Toggle:** Each modifier can be manually turned on (and off) whilst playing the game using a button of your choice. Use this mode for challenge runs or to experiment with the modifiers.
- **Chat Poll:** Chat will decide which modifier to enable at set increments by typing a number in chat. This is the 'Chaos Mode' version.
- **None:** The Modifiers will not be activated ever. This is a quick way to "turn off" the mod.

## Toggle Mode

With the trigger type set to "Toggle" all the available modifiers will be displayed. Each one will contain a "Toggle Key" and an "Enabled" option. 

If you want a modifier to be active, you must tick the "Enabled" box to the right of it, and choose a [Key Code](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.keys?view=windowsdesktop-7.0) for the game to listen out for.
Once the game is running, the Mod will listen for the keys of any enabled modifiers, and toggle them as requested.

If a modifier has additional settings, they will become available once enabled.

For example, in the below screenshot, the "Fall Damage" mod is enabled to the '5' key at the top of the keyboard, and has it's settings configured to enable blood splatters which can be cleared by pressing F14:

![image](https://github.com/PhantomBadger/JumpKingMod-PrivateMirror/assets/9095972/88c9cef6-bd3b-4b4c-bfa9-ae7db2f4a222)

The "Meta" modifiers will do nothing in this mode.

## Chat Poll

With the trigger type set to "Chat Poll" all the available modifiers will be displayed. Each one will contain an "Enabled" checkbox. In addition, there will be four more settings
at the top of the window. These settings will let you configure how the poll works in game.
- **Poll Duration:** This is how long, in seconds, the poll will count down from before choosing a winner
- **Poll Closed:** This is how long, in seconds, the poll will *show* the winner before activating it
- **Poll Cooldown:** This is how long, in seconds, the mod will wait before starting a new poll
- **Modifier Duration:** This is how long, in seconds, the winning modifier will remain active for

![image](https://github.com/PhantomBadger/JumpKingMod-PrivateMirror/assets/9095972/5eb402a4-5005-448d-b6d8-8082764f4600)

The mod will choose four modifier options from the pool at random and display a poll on screen for the chat to vote. Chat will type "1", "2", "3", or "4", to choose the option they want. Each chatter can only vote once per poll. Both YouTube and Twitch chats are supported. 
For help setting up YouTube/Twitch integration, please [read our guide](HowToGenerateCredentials.md#youtube).

![image](https://github.com/PhantomBadger/JumpKingMod-PrivateMirror/assets/9095972/7ecc82fe-f459-46c0-bb00-4d4d18aa420b)

For a modifier to be active for selection, it must have the "Enabled" checkbox selected. If there are additional settings for that modifier, they will become visible once "Enabled" is checked.

It is not advised to enable "Fall Damage" for this mode, it works - but it's not the best experience.

Check out the video below by Rainhoe to see thie mod in action!

<a href="https://www.youtube.com/watch?v=HMqxvmZy1tQ">
     <img 
      src="https://img.youtube.com/vi/HMqxvmZy1tQ/0.jpg" 
      alt="Jump King, but Twitch Chat controls my Game" 
      style="width:60%;">
</a>

### Low Gravity

The "Low Gravity" modifier requires JK+ to be installed. If you do not have this installed the modifier will not be included even if you enable it.



