# How to setup force feedback devices

Depending on the force feedback device you are using the Punishment Mod will need information to allow external control of/iteraction with the device.  
If your device is properly configured and the Punishment mod is enabled a test feedback event should trigger on the device upon starting Jump King (you can also [refer to the console window](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/a183eb79-4e89-4c3c-82b0-a46f07106c27) for more information/see if requests are going through).

**You should generally keep this information private as it may allow others to control these devices as well. If you accidentally leak this information the tools linked to below should allow deleting/regenerating keys/codes such that the old ones no longer work.**

## PiShock

This guide assumes you already have the PiShock device generally setup, if you do not follow guides on the [PiShock website](https://pishock.com/) to do this. You should confirm you can manually control your device when logged in (meaning everything is setup properly) [here](https://pishock.com/#/control). 

![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/72357a02-6814-463f-8d94-5fcb3e4be4ab)

There are 3 unique pieces of information needed for the Puishment Mod to control the PiShock:  
- Your username on the PiShock website, [visible here if logged in](https://pishock.com/#/account).
- An API Key generated on the PiShock website (generated from the [accounts page](https://pishock.com/#/account)).
  - Note: If you have previously generated an API Key you can/should re-use it, your account can only have one active API Key and generating a new one will invalidate the old one (also useful if you accidentally share the key).
  - ![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/96b3b348-d707-47b6-8409-3e739340a1a8)
- A share code for the shocker you wish to use- generated from the [control page](https://pishock.com/#/control).
  - ![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/d1f984cb-dbad-4c1b-8b9f-b76d726d0c05)
  - ![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/ed0fddcf-c06f-4afd-a6cf-f3309fdd7e74)
  - ![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/a25c74ac-eef6-48ec-ab6c-83fb81ffb288)
    - Note you can also delete previously generated codes (so they can no longer be used), limit the duration/intensity that can be triggered via this share code, or pause its functionality from here as well.


