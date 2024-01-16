# Punishment Mod Settings

The **'Punishment'** tab in the settings editor will provide you with configurable options to control the punishment mod while playing as well as configure if and how it generates feedback with your confiured device.

It's **highly** recommended you tune these settings to ensure you are comfortable playing the mod.

**Note: This tab will only show up if you have already selected/configured a feedback device.**  

## **Warning!**
You should follow any warnings/recommendations related to the usage of your device (such as placement/intensity/usage warnings) so that you do not potentially injure yourself!

It's also highly recommended you experiment with your device before configuring/using this mod to find intensity/duration/distance values you are sure you will be comfortable with!

![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/4d7fc0f6-44ba-4110-941d-36686fb20d9e)

- **Enabled:** Ticking this means the Punishment mod is now enabled and will be used by Jump King. Untick if you do not want to use this portion of the mod.
- **Toggle Punishment Key:** Press this key during play to toggle the punishment mod functionality (toggling punishments and rewards).
  - **Note:** This will also clear your 'max progress' tracked by the mod used by the **'Reward on New Progress Only'** setting, meaning you can get rewarded for progress you have already made again if you toggle the mod off and back on.
- **Test Feedback Key:** Press this key during play to trigger a test feedback event to your feedback device. This is useful to test that your device is properly configured/working.
- **On Screen Display Behavior:** Controls how the mod displays messages on screen about punishments and rewards, see sections below for more information.
- **Round Durations:** Ticking this causes feedback event durations to be rounded when calculated.
  - This is useful when using the PiShock as it only supports durations that are whole seconds (no fractions)- you do not **need** to enable this as without this the duration sent to the PiShock API will still be rounded and it will still work, but enabling this makes any information displayed on screen match with your actual punishment/reward.
- **Punishment Enabled:** Ticking this enables the generation of punishment feedback events.
- **Minimum Punishment Duration:** The minimum duration (in seconds) for a punishment feedback event (the amount triggered after falling your minimum fall distance).
- **Minimum Punishment Intensity:** The minimum intensity (0-100) for a punishment feedback event (the amount triggered after falling your minimum fall distance).
- **Maximum Punishment Duration:** The maximum duration (in seconds) for a punishment feedback event (the amount triggered after falling your maximum fall distance).
- **Maximum Punishment Intensity:** The maximum intensity (0-100) for a punishment feedback event (the amount triggered after falling your maximum fall distance).
- **Minimum Punishment Fall Distance:** The minimum fall distance you must fall in order to receive a punishment feedback event.
  - For more information about distances/what might be good values see sections below.
- **Maximum Punishment Fall Distance:** The fall distance at which you will receive a maximum stength/duration punishment feedback event.
- **Punishment Easy Mode:** Ticking this enables 'easy mode' for punishments, changing the type (or potentially strength/duration) of feedback generated for punishments.
  - For the PiShock this causes vibrations to be sent instead of shocks.  
- **Rewards Enabled:** Ticking this enables the generation of reward feedback events.
- **Minimum Reward Duration:** The minimum duration (in seconds) for a reward feedback event (the amount triggered after gaining your minimum reward progress distance).
- **Minimum Reward Intensity:** The minimum intensity (0-100) for a reward feedback event (the amount triggered after gaining your minimum reward progress distance).
- **Maximum Reward Duration:** The maximum duration (in seconds) for a reward feedback event (the amount triggered after gaining your maximum reward progress distance).
- **Maximum Reward Intensity:** The maximum intensity (0-100) for a punishment feedback event (the amount triggered after gaining your maximum reward progress distance).
- **Minimum Reward Progress Distance:** The minimum amount of progress you must gain in order to receive a reward feedback event.
- **Maximum Reward Progress Distance:** The progress distance at which you will receive a maximum strength/duration reward feedback event.
- **Reward On New Progress Only:** Ticking this means you will only be rewarded the first time you make progress (based on height, not any displayed progress percentage). If enabled falling and re-making progress will not generate rewards.
  - **Note:** This value is not saved, restarting the game (or toggling the mod at runtime) will reset this value and you may be rewarded for progress you technically previously made already.

 ### On Screen Display Behaviors
 
- **None:** No on screen information will be displayed by the mod.
- **MessageOnly:** Only a message indication there is an incoming punishment, or that a punishment/reward was triggered will be displayed.
  - ![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/c32aa60e-1815-4ae7-adcc-8cf17227579c)
  - ![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/0676854c-c9e3-4f3f-a191-6201461881ff)
  - ![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/5e7f74e8-51d1-4ccd-ac2c-ff5221d60147)
- **DistanceBasedPercentage:** An on screen message along with a percentage indicating the strength of the punishment/reward (based on your min/max distance settings) will be displayed. This means someone cannot know the exact strength/duration of the feedback triggered on your device unless they know your settings.
  - ![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/b967f068-4bb3-499f-966e-85c8aced15b2)
  - ![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/471d1814-68fb-416c-8935-863921b35446)
  - ![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/ead14b60-c1ad-4b5d-b4f1-3b3421cdf357)
- **FeedbackIntensityAndDuration:** An on screen message along with the exact strength percentage and second duration of the punishment/reward (based on your settings) will be displayed on screen.
  - ![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/ec790f70-8aff-4661-91be-0187b1ae88a6)
  - ![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/eee1235c-9800-4d91-a6f2-1c9ca12155c4)
  - ![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/82fd63e4-44cb-4a78-8ab6-e73747bd47a6)

## Distance settings tips

Determining what distance settings are right for you might take some tuning (and potentially an understanding of how the settings work), this section is here to help.

### How are distances calculated?

Distances are calculated based on the Y location (verticle position) of the player (the King himself). They are based on your last known position on the ground (sand and ice is considered ground, even if you are moving/sliding) versus your new location on the ground when landing- it is **not** based on splatting, the height you reach when jumping, or any kind of progress percentage the game tracks.

For example if the King jumps from the top location to the bottom location in the following image:  
![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/5714c819-cb5c-4735-8b73-8187e8855694)  
The fall distance used for the punishment calculation is represented by the red arrow (just the difference in the verticle position, not the actual length of the arrow here)- in this particular example the fall distance is 88 units.

Rewards are calculated in the exact same way, but are caused by upward progress.  
Note that for the 'Reward On New Progress Only' setting the highest Y value you reach is used to calculate your max progress. This means if you are in an area that forces a fall and you need to work your way back up (even if it's through new content) you will not receive new rewards until you are higher than you were before the fall.

#### What about teleports, user maps, or other mods?

Sometimes when changing between screens in Jump King the player is teleported as the map physically cannot exist as it's laid out. This **should be supported and work correctly** (at least with teleports done in the original game/DLC). If user maps use the same functionality to do teleports as the original game they should be supported as well (note however no testing has been done with user maps, you may want to test stuff yourself before committing to anything).

Other mods will likely work in tandem with the punishment mod, however you should be cautious of mods that move/teleport the player- there is a good chance they will not work correctly and may incorrectly trigger punishments/rewards. If you would like to use other such mods in tandem with the punishment mod you should probably do some basic testing with them before using them.

### What can I do to find distances that work for me?

One screen in Jump King should be 360 units tall, this means that if you wanted a distance setting of 3 screens you would use a distance of 1080 (3 * 360). This is a good rough guide for most players to work with.

If you would like to experiment more with distances/confirm distances yourself the distance being used to calculate a punishment/reward are printed out to the console when playing with the mod enabled- this can help you tune distances that work for you.  
![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/ec4e916b-ecda-4fe2-b95b-3c80ea89ff37)

### How does the min/max distance relate to the reward/punishment?

When you fall (or progress) your set minimum distance a punishment (or reward) will be triggered at your defined minimum duration/intensity.  
When you fall (or progress) your set maximum distance (or beyond) a punishment (or reward) will be triggered at your defined maximum duration/intensity.

If you fall (or progress) any amount in between your minimum and maximum distance a feedback event between your minimum and maximum settings will be calculated and triggered based on your settings and the distance.  
For example if you have a minimum fall distance of 0, a maximum fall distance of 1000, and fall 500 units a punishment will trigger half way between your minimum and maximum duration/intensity settings. If in this case your minimum punshiment intensity is 10, and your maximum punsihment intensity is 100, a punishment of intensity 55 (((100 - 10) * .5) + 10) would be triggered.

If you would like to test various distances/durations/intensities the results of any punishment/reward calculations can be seen in the console while the mod is active.  
![image](https://github.com/zarradeth/JumpKing-TwitchRavens/assets/20621507/7e094ec1-5486-47ed-a334-bb07f0e4b55f)

### What if I don't want any variation in the punishment/reward?
If you do not want any variation in the punishment/reward you can set the min and max fall/progress distance to the same value (the value at which a feedback event should trigger)- in this case **the maximum duration/intensity settings will always trigger** (the minimum duration/intensity is effectively unused). It's recommended you also just set your minimum and maximum duration/intensity settings to match if you do this.
