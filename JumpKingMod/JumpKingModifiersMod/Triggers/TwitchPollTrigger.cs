using JumpKingModifiersMod.API;
using Logging.API;
using Microsoft.Xna.Framework;
using PBJKModBase.API;
using PBJKModBase.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    public class TwitchPollTrigger : IModifierTrigger, IModEntity
    {
        public delegate void TwitchPollStartedDelegate(ModifierTwitchPoll poll);
        public delegate void TwitchPollClosedDelegate(ModifierTwitchPoll poll);
        public delegate void TwitchPollEndedDelegate(ModifierTwitchPoll poll);

        private enum TwitchPollTriggerState
        {
            CreatingPoll,
            CollectingVotes,
            ClosingPoll,
            ExecutingWinningOption,
            DownTimeBetweenPolls,
        }

        public event ModifierEnabledDelegate OnModifierEnabled;
        public event ModifierDisabledDelegate OnModifierDisabled;
        public event TwitchPollStartedDelegate OnTwitchPollStarted;
        public event TwitchPollClosedDelegate OnTwitchPollClosed;
        public event TwitchPollEndedDelegate OnTwitchPollEnded;

        private readonly ILogger logger;
        private readonly List<IModifier> availableModifiers;
        private readonly List<ActiveModifierCountdown> activeModifiers;
        private readonly BlockingCollection<OnMessageReceivedArgs> relayRequestQueue;
        private readonly TwitchClient twitchClient;
        private readonly ModEntityManager modEntityManager;
        private readonly Random random;
        private readonly Thread processingThread;
        private readonly ConcurrentDictionary<string, byte> alreadyVotedChatters;

        private TwitchPollTriggerState triggerState;
        private ModifierTwitchPoll currentPoll;
        private float pollTimeCounter;
        private float closingPollTimeCounter;
        private float timeBetweenPollsCounter;
        private bool isEnabled;

        private const int NumberOfModifiersInPoll = 4;
        private const float PollTimeInSeconds = 30.0f;
        private const float PollClosedTimeInSeconds = 5.0f;
        private const float ActiveModifierDurationInSeconds = 20f;
        private const float TimeBetweenPollsInSeconds = 5.0f;

        /// <summary>
        /// Ctor for creating a <see cref="TwitchPollTrigger"/>
        /// </summary>
        /// <param name="twitchClient"></param>
        /// <param name="logger"></param>
        public TwitchPollTrigger(TwitchClient twitchClient, List<IModifier> availableModifiers, ModEntityManager modEntityManager, ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.twitchClient = twitchClient ?? throw new ArgumentNullException(nameof(twitchClient));
            this.availableModifiers = availableModifiers ?? throw new ArgumentNullException(nameof(availableModifiers));
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.random = new Random(DateTime.Now.Second + DateTime.Now.Millisecond);

            alreadyVotedChatters = new ConcurrentDictionary<string, byte>();
            activeModifiers = new List<ActiveModifierCountdown>();
            relayRequestQueue = new BlockingCollection<OnMessageReceivedArgs>();
            this.twitchClient.OnMessageReceived += OnMessageReceived;
            triggerState = TwitchPollTriggerState.CreatingPoll;
            currentPoll = null;
            isEnabled = false;

            // Kick off the processing thread
            processingThread = new Thread(ProcessRelayRequests);
            processingThread.Start();
        }

        /// <inheritdoc/>
        public bool EnableTrigger()
        {
            if (!modEntityManager.AddEntity(this, zOrder: 0))
            {
                logger.Information($"Failed to enabled '{this.GetType().Name}' Modifier Trigger as it didn't add to the entity managed correctly");
                return false;
            }
            isEnabled = true;
            currentPoll = null;
            alreadyVotedChatters.Clear();
            triggerState = TwitchPollTriggerState.CreatingPoll;

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
                OnTwitchPollEnded?.Invoke(currentPoll);
                currentPoll = null;
            }

            isEnabled = true;
            alreadyVotedChatters.Clear();
            logger.Information($"Disabled '{this.GetType().Name}' Modifier Trigger");
            return true;
        }

        /// <inheritdoc/>
        public bool IsTriggerEnabled()
        {
            return isEnabled;
        }

        /// <summary>
        /// Called by the underlying Twitch Client when a message is received, updates the UI Entity
        /// </summary>
        private void OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            try
            {
                if (!isEnabled || e == null || e.ChatMessage == null || string.IsNullOrWhiteSpace(e.ChatMessage.Message))
                {
                    return;
                }

                // Queue up the request
                if (triggerState == TwitchPollTriggerState.CollectingVotes)
                {
                    relayRequestQueue.Add(e);
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
                    OnMessageReceivedArgs e = relayRequestQueue.Take();

                    // If the trigger isn't active then dip
                    if (!isEnabled)
                    {
                        continue;
                    }

                    // Get the current poll, if it's null then we drop the message and continue
                    // Since we have a local copy we dont need to worry about it turning null whilst
                    // we work
                    ModifierTwitchPoll currentPoll = this.currentPoll;
                    if (currentPoll == null)
                    {
                        continue;
                    }

                    // If they have already voted, then dip
                    if (alreadyVotedChatters.ContainsKey(e.ChatMessage.UserId))
                    {
                        continue;
                    }

                    // Get the choice number from the message
                    if (!TryGetPollChoiceNumberFromMessage(e.ChatMessage.Message, out int choiceNumber))
                    {
                        continue;
                    }

                    // Find that option in the poll
                    if (!currentPoll.Choices.ContainsKey(choiceNumber))
                    {
                        continue;
                    }

                    // Increment that value in the poll
                    ModifierTwitchPollOption option = currentPoll.Choices[choiceNumber];
                    option.IncrementCount();
                    alreadyVotedChatters.TryAdd(e.ChatMessage.UserId, 0);
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
        /// Called by the <see cref="ModEntityManager"/>
        /// </summary>
        /// <param name="p_delta"></param>
        public void Update(float p_delta)
        {
            try
            {
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
                    case TwitchPollTriggerState.CreatingPoll:
                        {
                            // Identify what four modifiers we want to choose from and start the query
                            List<IModifier> possibleModifiers = availableModifiers.Where(modifier => !modifier.IsModifierEnabled()).ToList();

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
                            currentPoll = new ModifierTwitchPoll(modifiersToChooseBetween);
                            alreadyVotedChatters.Clear();

                            StringBuilder logBuilder = new StringBuilder();
                            logBuilder.Append($"Creating a Twitch Poll with '{modifiersToChooseBetween.Count}' choices for '{PollTimeInSeconds.ToString()}' seconds: ");
                            foreach (var choice in currentPoll.Choices)
                            {
                                logBuilder.Append($"{choice.Value.Modifier.DisplayName}, ");
                            }
                            logger.Information(logBuilder.ToString().Trim().TrimEnd(','));

                            triggerState = TwitchPollTriggerState.CollectingVotes;
                            pollTimeCounter = 0;

                            OnTwitchPollStarted?.Invoke(currentPoll);
                            logger.Information($"Changing Twitch Poll State to '{triggerState.ToString()}'");
                            break;
                        }
                    case TwitchPollTriggerState.CollectingVotes:
                        {
                            if (currentPoll == null)
                            {
                                logger.Warning($"Current Poll is null, but we're in the '{triggerState.ToString()}' state, moving back to '{TwitchPollTriggerState.CreatingPoll.ToString()}'");
                                triggerState = TwitchPollTriggerState.CreatingPoll;
                                return;
                            }

                            pollTimeCounter += p_delta;
                            currentPoll.TimeRemainingInSeconds = Math.Max(0, (PollTimeInSeconds - pollTimeCounter));

                            if (pollTimeCounter > PollTimeInSeconds)
                            {
                                closingPollTimeCounter = 0;
                                triggerState = TwitchPollTriggerState.ClosingPoll;
                                OnTwitchPollClosed?.Invoke(currentPoll);
                                alreadyVotedChatters.Clear();
                                logger.Information($"Changing Twitch Poll State to '{triggerState.ToString()}'");
                            }
                            break;
                        }
                    case TwitchPollTriggerState.ClosingPoll:
                        {
                            if ((closingPollTimeCounter += p_delta) > PollClosedTimeInSeconds)
                            {
                                triggerState = TwitchPollTriggerState.ExecutingWinningOption;
                                logger.Information($"Changing Twitch Poll State to '{triggerState.ToString()}'");
                            }
                            break;
                        }
                    case TwitchPollTriggerState.ExecutingWinningOption:
                        {
                            if (currentPoll == null)
                            {
                                logger.Warning($"Current Poll is null, but we're in the '{triggerState.ToString()}' state, moving back to '{TwitchPollTriggerState.CreatingPoll.ToString()}'");
                                triggerState = TwitchPollTriggerState.CreatingPoll;
                                return;
                            }

                            // Get the winning trigger
                            ModifierTwitchPollOption winningModifier = currentPoll.FindWinningModifier();
                            if (winningModifier == null)
                            {
                                logger.Warning($"No winning modifier was found, but we're in the '{triggerState.ToString()}' state, moving back to '{TwitchPollTriggerState.CreatingPoll.ToString()}'");
                                triggerState = TwitchPollTriggerState.CreatingPoll;
                                return;
                            }
                            logger.Information($"Poll won by '{winningModifier.Modifier.DisplayName}' with '{winningModifier.Count}' votes");


                            // Enable the trigger
                            winningModifier.Modifier.EnableModifier();
                            OnModifierEnabled?.Invoke(winningModifier.Modifier);
                            activeModifiers.Add(new ActiveModifierCountdown(winningModifier.Modifier, ActiveModifierDurationInSeconds));

                            // Clear the current poll and queue up a next one
                            OnTwitchPollEnded?.Invoke(currentPoll);
                            triggerState = TwitchPollTriggerState.DownTimeBetweenPolls;
                            currentPoll = null;
                            timeBetweenPollsCounter = 0;
                            logger.Information($"Changing Twitch Poll State to '{triggerState.ToString()}'");
                            break;
                        }
                    case TwitchPollTriggerState.DownTimeBetweenPolls:
                        {
                            if ((timeBetweenPollsCounter += p_delta) > TimeBetweenPollsInSeconds)
                            {
                                triggerState = TwitchPollTriggerState.CreatingPoll;
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
    }
}
