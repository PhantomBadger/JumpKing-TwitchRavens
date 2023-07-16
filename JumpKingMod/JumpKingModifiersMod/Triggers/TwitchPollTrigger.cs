using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Settings;
using JumpKingModifiersMod.Triggers.Poll;
using Logging.API;
using Microsoft.Xna.Framework;
using PBJKModBase.API;
using PBJKModBase.Entities;
using Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;

namespace JumpKingModifiersMod.Triggers
{
    /// <summary>
    /// An implementation of <see cref="IModifierTrigger"/> which triggers the effects based on a twitch poll.
    /// Will select a random subst of modifiers, ask the users to vote on it, then enables it for a set amount of time
    /// </summary>
    public class TwitchPollTrigger : IModifierPollTrigger, IModEntity, IDisposable
    {
        public event ModifierEnabledDelegate OnModifierEnabled;
        public event ModifierDisabledDelegate OnModifierDisabled;
        public event PollStartedDelegate OnPollStarted;
        public event PollClosedDelegate OnPollClosed;
        public event PollEndedDelegate OnPollEnded;

        private readonly ILogger logger;
        private readonly IGameStateObserver gameStateObserver;
        private readonly List<IModifier> availableModifiers;
        private readonly List<ActiveModifierCountdown> activeModifiers;
        private readonly List<ActiveModifierCountdown> previouslyActiveModifiers;
        private readonly BlockingCollection<Tuple<string, int>> relayRequestQueue;
        private readonly ModEntityManager modEntityManager;
        private readonly Random random;
        private readonly Thread processingThread;
        private readonly ConcurrentDictionary<string, byte> alreadyVotedChatters;
        private readonly UserSettings userSettings;
        private readonly IPollChatProvider chatProvider;

        private PollTriggerState triggerState;
        private ModifierPoll currentPoll;
        private float pollTimeCounter;
        private float closingPollTimeCounter;
        private float timeBetweenPollsCounter;
        private bool isEnabled;

        private float BasePollTimeInSeconds;
        private float PollClosedTimeInSeconds;
        private float BaseActiveModifierDurationInSeconds;
        private float TimeBetweenPollsInSeconds;

        private const int NumberOfModifiersInPoll = 4;
        internal const float DefaultPollTimeModifier = 1.0f;
        internal const float DefaultActiveModifierDurationModifier = 1.0f;

        /// <summary>
        /// The Base Poll Time multiplied by the changeable modifier
        /// </summary>
        public float PollTimeInSeconds
        {
            get { return BasePollTimeInSeconds * PollTimeModifier; }
        }

        /// <summary>
        /// The modifier to be applied to produce the <see cref="PollTimeInSeconds"/>
        /// </summary>
        public float PollTimeModifier
        {
            get
            {
                return pollTimeModifier;
            }
            set
            {
                pollTimeModifier = Math.Max(0, value);
            }
        }
        private float pollTimeModifier;

        /// <summary>
        /// The Base Active Modifier Duration multiplied by the changeable modifier
        /// </summary>
        public float ActiveModifierDurationInSeconds
        {
            get { return BaseActiveModifierDurationInSeconds * ActiveModifierDurationModifier; }
        }

        /// <summary>
        /// The modifier to be applied to produce the <see cref="ActiveModifierDurationInSeconds"/>
        /// </summary>
        public float ActiveModifierDurationModifier
        {
            get
            {
                return activeModifierDurationModifier;
            }
            set
            {
                activeModifierDurationModifier = Math.Max(0, value);
            }
        }
        private float activeModifierDurationModifier;

        /// <summary>
        /// Ctor for creating a <see cref="TwitchPollTrigger"/>
        /// </summary>
        public TwitchPollTrigger(IPollChatProvider chatProvider, List<IModifier> availableModifiers, ModEntityManager modEntityManager, IGameStateObserver gameStateObserver, UserSettings userSettings, ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.gameStateObserver = gameStateObserver ?? throw new ArgumentNullException(nameof(gameStateObserver));
            this.chatProvider = chatProvider ?? throw new ArgumentNullException(nameof(chatProvider));
            this.availableModifiers = availableModifiers ?? throw new ArgumentNullException(nameof(availableModifiers));
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
            this.random = new Random(DateTime.Now.Second + DateTime.Now.Millisecond);

            alreadyVotedChatters = new ConcurrentDictionary<string, byte>();
            activeModifiers = new List<ActiveModifierCountdown>();
            previouslyActiveModifiers = new List<ActiveModifierCountdown>();
            relayRequestQueue = new BlockingCollection<Tuple<string, int>>();
            this.chatProvider.OnChatVote += OnMessageReceived;
            triggerState = PollTriggerState.CreatingPoll;
            currentPoll = null;
            isEnabled = false;

            BasePollTimeInSeconds = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.PollDurationInSecondsKey, JumpKingModifiersModSettingsContext.DefaultBasePollTimeInSeconds);
            PollClosedTimeInSeconds = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.PollClosedDurationInSecondsKey, JumpKingModifiersModSettingsContext.DefaultPollClosedTimeInSeconds);
            BaseActiveModifierDurationInSeconds = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.ModifierDurationInSecondsKey, JumpKingModifiersModSettingsContext.DefaultBaseActiveModifierDurationInSeconds);
            TimeBetweenPollsInSeconds = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.TimeBetweenPollsInSecondsKey, JumpKingModifiersModSettingsContext.DefaultTimeBetweenPollsInSeconds);

            PollTimeModifier = DefaultPollTimeModifier;
            ActiveModifierDurationModifier = DefaultActiveModifierDurationModifier;

            for (int i = 0; i < availableModifiers.Count; i++)
            {
                this.logger.Information($"[Twitch Poll Trigger] '{availableModifiers[i].DisplayName}' registered as a poll option");
            }

            gameStateObserver.OnGameLoopRunning += OnGameLoopRunning;
            gameStateObserver.OnGameLoopNotRunning += OnGameLoopNotRunning;

            // Kick off the processing thread
            processingThread = new Thread(ProcessRelayRequests);
            processingThread.Start();
        }

        /// <summary>
        /// Implementation of <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            gameStateObserver.OnGameLoopRunning -= OnGameLoopRunning;
            gameStateObserver.OnGameLoopNotRunning -= OnGameLoopNotRunning;
            chatProvider.OnChatVote -= OnMessageReceived;
            chatProvider.Dispose();
        }

        /// <summary>
        /// Called by <see cref="IGameStateObserver.OnGameLoopNotRunning"/> we cache all our known active modifiers and disable them
        /// </summary>
        private void OnGameLoopNotRunning()
        {
            //CacheActiveModifiers();
            if (activeModifiers != null && activeModifiers.Count > 0)
            {
                for (int i = 0; i < activeModifiers.Count; i++)
                {
                    activeModifiers[i]?.Modifier?.DisableModifier();
                }
                activeModifiers.Clear();
            }
        }

        /// <summary>
        /// Called by <see cref="IGameStateObserver.OnGameLoopRunning"/> we restore any cached active modifiers and renable them
        /// </summary>
        private void OnGameLoopRunning()
        {
            //RestoreCachedModifiers();
        }

        /// <summary>
        /// Caches any active modifiers and disables them
        /// </summary>
        private void CacheActiveModifiers()
        {
            previouslyActiveModifiers.Clear();
            // If the game loop is stopped, we disable all modifiers we know to be active and keep a cache of them
            if (activeModifiers != null & activeModifiers.Count > 0)
            {
                logger.Information($"Caching {activeModifiers.Count} active modifiers to be re-enabled later");
                previouslyActiveModifiers.AddRange(activeModifiers);
                activeModifiers.Clear();
                for (int i = 0; i < previouslyActiveModifiers.Count; i++)
                {
                    previouslyActiveModifiers[i].Modifier.DisableModifier();
                }
            }
        }

        /// <summary>
        /// Restores any cached modifiers and enables them
        /// </summary>
        private void RestoreCachedModifiers()
        {
            activeModifiers.Clear();
            if (previouslyActiveModifiers != null && previouslyActiveModifiers.Count > 0)
            {
                logger.Information($"Restoring {previouslyActiveModifiers.Count} cached active modifiers");
                // If we have a cache of any previously active modifiers, we enable them again now
                activeModifiers.AddRange(previouslyActiveModifiers);
                previouslyActiveModifiers.Clear();
                for (int i = 0; i < activeModifiers.Count; i++)
                {
                    activeModifiers[i].Modifier.EnableModifier();
                }
            }
        }

        /// <inheritdoc/>
        public bool EnableTrigger()
        {
            if (!modEntityManager.AddEntity(this, zOrder: 0))
            {
                logger.Information($"Failed to enable '{this.GetType().Name}' Modifier Trigger as it didn't add to the entity managed correctly");
                return false;
            }

            if (gameStateObserver == null || !gameStateObserver.IsGameLoopRunning())
            {
                logger.Information($"Failed to enable '{this.GetType().Name}' Modifier Trigger as the game loop is not currently running");
                return false;
            }

            isEnabled = true;
            currentPoll = null;
            chatProvider.EnableChatProvider();
            previouslyActiveModifiers.Clear();
            activeModifiers.Clear();
            triggerState = PollTriggerState.CreatingPoll;

            logger.Information($"Enabled '{this.GetType().Name}' Modifier Trigger");
            return true;
        }

        /// <inheritdoc/>
        public bool DisableTrigger()
        {
            if (!modEntityManager.RemoveEntity(this))
            {
                logger.Information($"Failed to disable '{this.GetType().Name}' Modifier Trigger as it didn't remove from the entity managed correctly");
                return false;
            }

            // End the poll prematurely if we had one active
            if (currentPoll != null)
            {
                OnPollEnded?.Invoke(currentPoll);
                currentPoll = null;
            }

            if (activeModifiers != null && activeModifiers.Count > 0)
            {
                for (int i = 0; i < activeModifiers.Count; i++)
                {
                    activeModifiers[i]?.Modifier?.DisableModifier();
                }
            }

            isEnabled = true;
            chatProvider.DisableChatProvider();
            previouslyActiveModifiers.Clear();
            activeModifiers.Clear();
            logger.Information($"Disabled '{this.GetType().Name}' Modifier Trigger");
            return true;
        }

        /// <inheritdoc/>
        public bool IsTriggerEnabled()
        {
            return isEnabled;
        }

        /// <summary>
        /// Called by the underlying Chat Provider when a message is received, updates the UI Entity
        /// </summary>
        private void OnMessageReceived(string chatName, int pollOption)
        {
            try
            {
                if (!isEnabled)
                {
                    return;
                }

                // Queue up the request
                if (triggerState == PollTriggerState.CollectingVotes)
                {
                    relayRequestQueue.Add(new Tuple<string, int>(chatName, pollOption));
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Encountered Exception during OnMessageReceived: {ex.ToString()}");
            }
        }

        /// <summary>
        /// Runs a loop which processes the chat relay requests
        /// </summary>
        private void ProcessRelayRequests()
        {
            while (true)
            {
                try
                {
                    // Pop off a request
                    Tuple<string, int> e = relayRequestQueue.Take();
                    string chatName = e.Item1;
                    int choiceNumber = e.Item2;

                    // If the trigger isn't active then dip
                    if (!isEnabled)
                    {
                        continue;
                    }

                    // If the game loop isn't running we drop the messages
                    if (gameStateObserver == null || !gameStateObserver.IsGameLoopRunning())
                    {
                        continue;
                    }

                    // Get the current poll, if it's null then we drop the message and continue
                    // Since we have a local copy we dont need to worry about it turning null whilst
                    // we work
                    ModifierPoll currentPoll = this.currentPoll;
                    if (currentPoll == null)
                    {
                        continue;
                    }

                    // Find that option in the poll
                    if (!currentPoll.Choices.ContainsKey(choiceNumber))
                    {
                        continue;
                    }

                    // Increment that value in the poll
                    ModifierPollOption option = currentPoll.Choices[choiceNumber];
                    option.IncrementCount();
                }
                catch (Exception ex)
                {
                    logger.Error($"Encountered Exception during ProcessRelayRequests: {ex.ToString()}");
                }
            }
        }

        /// <summary>
        /// Try to parse a number for the poll from the provided message
        /// </summary>
        private bool TryGetPollChoiceNumberFromMessage(string message, out int choiceNumber)
        {
            choiceNumber = -1;
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }
            string trimmedMessage = message.Trim();
            if (trimmedMessage.Length == 0)
            {
                return false;
            }

            if (int.TryParse(trimmedMessage[0].ToString(), out choiceNumber))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a list of the active modifiers and their countdowns
        /// </summary>
        public IReadOnlyList<ActiveModifierCountdown> GetActiveModifierCountdowns()
        {
            return activeModifiers;
        }

        /// <summary>
        /// Called by the <see cref="ModEntityManager"/>
        /// </summary>
        /// <param name="p_delta"></param>
        public void Update(float p_delta)
        {
            try
            {
                // If the game loop isn't running we drop what we're doing
                if (gameStateObserver == null || !gameStateObserver.IsGameLoopRunning())
                {
                    return;
                }

                // Check duration of the active modifiers and disable any that are done
                activeModifiers.RemoveAll(modifierCountdown =>
                {
                    if (modifierCountdown.DecreaseCounter(p_delta) < 0)
                    {
                        modifierCountdown.Modifier.DisableModifier();
                        OnModifierDisabled?.Invoke(modifierCountdown.Modifier);
                        return true;
                    }
                    return false;
                });

                // Process our current state
                switch (triggerState)
                {
                    case PollTriggerState.CreatingPoll:
                        {
                            // Identify what four modifiers we want to choose from and start the query
                            List<IModifier> possibleModifiers = availableModifiers.Where(modifier => !modifier.IsModifierEnabled()).ToList();
                            if (possibleModifiers.Count == 0)
                            {
                                return;
                            }

                            List<IModifier> modifiersToChooseBetween = new List<IModifier>();
                            if (possibleModifiers.Count < NumberOfModifiersInPoll)
                            {
                                // Run the poll with just these
                                modifiersToChooseBetween.AddRange(possibleModifiers);
                            }
                            else
                            {
                                // Randomly select some
                                for (int i = 0; i < NumberOfModifiersInPoll; i++)
                                {
                                    int index = random.Next(possibleModifiers.Count);
                                    modifiersToChooseBetween.Add(possibleModifiers[index]);
                                    possibleModifiers.RemoveAt(index);
                                }
                            }

                            // Set the active poll object
                            currentPoll = new ModifierPoll(modifiersToChooseBetween);
                            chatProvider.ClearPerPollData();

                            StringBuilder logBuilder = new StringBuilder();
                            logBuilder.Append($"Creating a Twitch Poll with '{modifiersToChooseBetween.Count}' choices for '{PollTimeInSeconds.ToString()}' seconds: ");
                            foreach (var choice in currentPoll.Choices)
                            {
                                logBuilder.Append($"{choice.Value.Modifier.DisplayName}, ");
                            }
                            logger.Information(logBuilder.ToString().Trim().TrimEnd(','));

                            triggerState = PollTriggerState.CollectingVotes;
                            pollTimeCounter = 0;

                            OnPollStarted?.Invoke(currentPoll);
                            logger.Information($"Changing Twitch Poll State to '{triggerState.ToString()}'");
                            break;
                        }
                    case PollTriggerState.CollectingVotes:
                        {
                            if (currentPoll == null)
                            {
                                logger.Warning($"Current Poll is null, but we're in the '{triggerState.ToString()}' state, moving back to '{PollTriggerState.CreatingPoll.ToString()}'");
                                triggerState = PollTriggerState.CreatingPoll;
                                return;
                            }

                            pollTimeCounter += p_delta;
                            currentPoll.TimeRemainingInSeconds = Math.Max(0, (PollTimeInSeconds - pollTimeCounter));

                            if (pollTimeCounter > PollTimeInSeconds)
                            {
                                closingPollTimeCounter = 0;
                                triggerState = PollTriggerState.ClosingPoll;
                                OnPollClosed?.Invoke(currentPoll);
                                chatProvider.ClearPerPollData();
                                logger.Information($"Changing Twitch Poll State to '{triggerState.ToString()}'");
                            }
                            break;
                        }
                    case PollTriggerState.ClosingPoll:
                        {
                            if ((closingPollTimeCounter += p_delta) > PollClosedTimeInSeconds)
                            {
                                triggerState = PollTriggerState.ExecutingWinningOption;
                                logger.Information($"Changing Twitch Poll State to '{triggerState.ToString()}'");
                            }
                            break;
                        }
                    case PollTriggerState.ExecutingWinningOption:
                        {
                            if (currentPoll == null)
                            {
                                logger.Warning($"Current Poll is null, but we're in the '{triggerState.ToString()}' state, moving back to '{PollTriggerState.CreatingPoll.ToString()}'");
                                triggerState = PollTriggerState.CreatingPoll;
                                return;
                            }

                            // Get the winning trigger
                            ModifierPollOption winningModifier = currentPoll.FindWinningModifier();
                            if (winningModifier == null)
                            {
                                logger.Warning($"No winning modifier was found, but we're in the '{triggerState.ToString()}' state, moving back to '{PollTriggerState.CreatingPoll.ToString()}'");
                                triggerState = PollTriggerState.CreatingPoll;
                                return;
                            }
                            logger.Information($"Poll won by '{winningModifier.Modifier.DisplayName}' with '{winningModifier.Count}' votes");


                            // Enable the trigger
                            winningModifier.Modifier.EnableModifier();
                            OnModifierEnabled?.Invoke(winningModifier.Modifier);
                            activeModifiers.Add(new ActiveModifierCountdown(winningModifier.Modifier, ActiveModifierDurationInSeconds));

                            // Clear the current poll and queue up a next one
                            OnPollEnded?.Invoke(currentPoll);
                            triggerState = PollTriggerState.DownTimeBetweenPolls;
                            currentPoll = null;
                            timeBetweenPollsCounter = 0;
                            logger.Information($"Changing Twitch Poll State to '{triggerState.ToString()}'");
                            break;
                        }
                    case PollTriggerState.DownTimeBetweenPolls:
                        {
                            if ((timeBetweenPollsCounter += p_delta) > TimeBetweenPollsInSeconds)
                            {
                                triggerState = PollTriggerState.CreatingPoll;
                                logger.Information($"Changing Twitch Poll State to '{triggerState.ToString()}'");
                            }
                            break;
                        }

                }
            }
            catch (Exception ex)
            {
                logger.Error($"Encountered Exception during Trigger Update: {ex.ToString()}");
            }
        }

        /// <inheritdoc/>
        public void Draw()
        {
            // Do nothing
        }

        /// <summary>
        /// Adds a new modifier to the collection of modifiers
        /// </summary>
        public void AddModifier(IModifier modifier)
        {
            availableModifiers.Add(modifier);
        }
    }
}
