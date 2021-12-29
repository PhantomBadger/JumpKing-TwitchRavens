# How to generate credentials
Depending on your streaming platform of choice, the Chat Ravens Mod will require a form of authorization token that let's it tell Twitch/YouTube that it is allowed to perform the desired actions.

**For both platforms it is important that you keep these tokens private, you should avoid showing them on stream if possible. If you do accidentally show the token, regenerate a new one as soon as possible. Each site linked below will contain additional steps for disabling an existing token**
## Twitch

 - To generate your Twitch OAuth token, go to this link: https://twitchapps.com/tmi/
   *(This is also accessible via the 'click here' option in the installer)*
 - On the website, click 'Connect' and follow any additional instructions required to log in to your Twitch Account
 - Once connected, a token will be provided, it will look something like `oauth:6xx327ztkga11s32eza5w0ttwzg5yn`
 - Copy and paste this token into the "Twitch OAuth" section of the Installer

## YouTube

 - To generate your YouTube API Key, go to this link: https://console.cloud.google.com/apis/credentials 
 *(This is also accessible via the 'click here' option in the installer)*
 - Click "Create Project" in the top right of the screen
 - Select a project name, this will represent your session of the Ravens mod to YouTube. I suggest naming it something like "YourChannelName-JumpKingRavens" to make it easy to keep track of.
	 - You do not need to enter an organisation with a project, and can leave it blank.
 - Once a project has been created, make sure it is selected then click 'Create Credentials' at the top of the screen, and select 'API Key' from the drop down
 - An API Key will be generated for you, it will look something like `AIzaSyDchfFGogUxIprKhZYDo0ku59zTC58s5uc`
 - Copy and paste this token into the "YouTube API Key" section of the Installer
