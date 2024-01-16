# Connecting to YouTube
YouTube's livestreaming services differ from how Twitch handles things a fair bit, as such there are some slight deviations to how things work for you.

- Once your settings have been correctly updated to support [YouTube connection](HowToGenerateCredentials.md#youtube), you can launch Jump King before going live. **However, you will be unable to connect to the chat until you have gone live.**
- Once you have launched the game and have started, you will see a "Disconnected" message in the top right - this is the status of your connection. Since your YouTube Stream's chat does not exist until you are live we are unable to connect to it until you are live.

![image](https://user-images.githubusercontent.com/9095972/147679343-48120272-bbef-4f14-8718-c03f077e405c.png)
- Press the appropriate hotkey mentioned in the connection message to trigger a connection process, you will see the text change to "Attempting to Connect" and eventually to "Connected". If the game fails to connect, please wait 30 seconds before trying again - YouTube's API is a bit slow to update for these queries, and rapidly polling it may result in your queries getting blocked.
- Once you have successfully connected, chat messages will start to be received and handled appropriately by the mod. You can press the same hotkey to disconnect/re-connect if you wish (If your stream goes down for example)

![image](https://user-images.githubusercontent.com/9095972/147679377-8d051556-ade9-480e-b3b4-668fea931f1e.png)
- You may have a small delay in the initial messages coming through, this is because YouTube's API gives us a large batch of previous messages when we first connect, instead of the newest ones. We will filter these out automatically to not show any messages from before the mod connected.
