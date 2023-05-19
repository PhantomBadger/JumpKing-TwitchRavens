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
    public class TwitchPollTrigger : IModifierTrigger, IForegroundModEntity
    {
        private enum TwitchPollTriggerState
        {
            CreatingPoll,
            CollectingVotes,
            ExecutingPoll,
        }

        public event ModifierEnabledDelegate OnModifierEnabled;
        public event ModifierDisabledDelegate OnModifierDisabled;

        private readonly ILogger logger;
        private readonly List<IModifier> availableModifiers;
        private readonly List<ActiveModifierCountdown> activeModifiers;
        private readonly BlockingCollection<OnMessageReceivedArgs> relayRequestQueue;
        private readonly TwitchClient twitchClient;
        private readonly ModEntityManager modEntityManager;
        private readonly Random random;

        private TwitchPollTriggerState triggerState;
        private ModifierTwitchPoll currentPoll;
        private float pollTimeCounter;

        private const int NumberOfModifiersInPoll = 4;
        private const float PollTimeInSeconds = 5.0f;
        private const float ActiveModifierDurationInSeconds = 7.5f;

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

            this.twitchClient.OnMessageReceived += OnMessageReceived;
            triggerState = TwitchPollTriggerState.CreatingPoll;
            currentPoll = null;
        }

        public bool EnableTrigger()
        {
            modEntityManager.AddForegroundEntity(this);
            currentPoll = null;
            triggerState = TwitchPollTriggerState.CreatingPoll;
            return true;
        }

        public bool DisableTrigger()
        {
            modEntityManager?.RemoveForegroundEntity(this);
            return false;
        }

        public bool IsTriggerEnabled()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called by the underlying Twitch Client when a message is received, updates the UI Entity
        /// </summary>
        private void OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            try
            {
                if (e == null || e.ChatMessage == null || string.IsNullOrWhiteSpace(e.ChatMessage.Message))
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
                            triggerState = TwitchPollTriggerState.CollectingVotes;
                            pollTimeCounter = 0;
                            break;
                        }
                    case TwitchPollTriggerState.CollectingVotes:
                        {
                            if (currentPoll == null)
                            {
                                // TODO: throw a wobbly
                                triggerState = TwitchPollTriggerState.CreatingPoll;
                                return;
                            }

                            if ((pollTimeCounter += p_delta) > PollTimeInSeconds)
                            {
                                // Wait the set amount of time and collect votes
                                // TODO: Sort this
                                // Pop off a request
                                OnMessageReceivedArgs e = relayRequestQueue.Take();
                            }
                            else
                            {
                                triggerState = TwitchPollTriggerState.ExecutingPoll;
                            }
                            break;
                        }
                    case TwitchPollTriggerState.ExecutingPoll:
                        {
                            if (currentPoll == null)
                            {
                                // TODO: throw a wobbly
                                triggerState = TwitchPollTriggerState.CreatingPoll;
                                return;
                            }

                            // Get the winning trigger
                            IModifier winningModifier = currentPoll.FindWinningModifier();

                            // Enable the trigger
                            winningModifier.EnableModifier();
                            activeModifiers.Add(new ActiveModifierCountdown(winningModifier, ActiveModifierDurationInSeconds));

                            // Clear the current poll and queue up a next one
                            currentPoll = null;
                            triggerState = TwitchPollTriggerState.CreatingPoll;
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Encountered Exception during Trigger Update: {ex.ToString()}");
            }
        }

        public void ForegroundDraw()
        {
            // Do nothing
        }
    }
}
