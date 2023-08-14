using JumpKing;
using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Triggers;
using Logging.API;
using Microsoft.Xna.Framework;
using PBJKModBase.API;
using PBJKModBase.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Visuals
{
    /// <summary>
    /// An implementation of <see cref="IModEntity"/> which acts as the visual component to a provided
    /// <see cref="PollTrigger"/>, creating and managing UI texts to display the poll state to the chat
    /// </summary>
    public class PollVisual : IModEntity, IDisposable
    {
        private readonly ModEntityManager modEntityManager;
        private readonly IModifierPollTrigger trigger;
        private readonly IGameStateObserver gameStateObserver;
        private readonly ILogger logger;

        private ModifierPoll currentPoll;
        private UITextEntity pollDescriptionEntity;
        private UITextEntity pollCountdownEntity;
        private List<Tuple<ModifierPollOption, UITextEntity>> pollOptionEntities;
        private List<UITextEntity> activeModifierEntitiesPool;
        private float bottomOfOptionsYValue;
        private float initialYOffset;

        private const float YPadding = 1;
        private const float CountdownYPadding = 5;
        private const float InitialPositionXPadding = 8f;
        private const int MaxActiveModifiersOnDisplay = 5;
        private const float ActiveModifierYPadding = 5;

        /// <summary>
        /// Ctor for creating a <see cref="PollVisual"/>
        /// </summary>
        /// <param name="modEntityManager">The <see cref="ModEntityManager"/> to register to</param>
        /// <param name="trigger">The <see cref="PollTrigger"/> to act as a visual for</param>
        /// <param name="gameStateObserver">An implementation of <see cref="IGameStateObserver"/> to determine when we should draw</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to use for logging</param>
        public PollVisual(ModEntityManager modEntityManager, IModifierPollTrigger trigger, IGameStateObserver gameStateObserver, ILogger logger, int initialYOffset = 0)
        {
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
            this.gameStateObserver = gameStateObserver ?? throw new ArgumentNullException(nameof(gameStateObserver));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.initialYOffset = initialYOffset;

            pollOptionEntities = new List<Tuple<ModifierPollOption, UITextEntity>>();

            this.gameStateObserver.OnGameLoopNotRunning += OnGameLoopNotRunning;
            this.gameStateObserver.OnGameLoopRunning += OnGameLoopRunning;
            this.trigger.OnPollStarted += OnTwitchPollStarted;
            this.trigger.OnPollClosed += OnTwitchPollClosed;
            this.trigger.OnPollEnded += OnTwitchPollEnded;
            this.modEntityManager.AddEntity(this, zOrder: 0);
        }

        /// <summary>
        /// An implementation of <see cref="IDisposable.Dispose"/> to clean up events
        /// </summary>
        public void Dispose()
        {
            gameStateObserver.OnGameLoopNotRunning -= OnGameLoopNotRunning;
            gameStateObserver.OnGameLoopRunning -= OnGameLoopRunning;
            trigger.OnPollStarted -= OnTwitchPollStarted;
            trigger.OnPollClosed -= OnTwitchPollClosed;
            trigger.OnPollEnded -= OnTwitchPollEnded;
            modEntityManager.RemoveEntity(this);
            CleanUpUIEntities();
        }

        /// <summary>
        /// Called by <see cref="IGameStateObserver.OnGameLoopNotRunning"/> hides all active UI entities
        /// </summary>
        private void OnGameLoopNotRunning()
        {
            logger.Information($"Called OnGameLoopNotRunning in TwitchPollVisual - Hiding all UI");
            HideAllUI();
        }

        /// <summary>
        /// Called by <see cref="IGameStateObserver.OnGameLoopRunning"/> shows all active UI entities
        /// </summary>
        private void OnGameLoopRunning()
        {
            logger.Information($"Called OnGameLoopRunning in TwitchPollVisual - Showing all UI");
            ShowAllUI();
        }

        /// <summary>
        /// Hides all UI entities
        /// </summary>
        private void HideAllUI()
        {
            if (pollDescriptionEntity != null)
            {
                pollDescriptionEntity.IsEnabled = false;
            }
            if (pollCountdownEntity != null)
            {
                pollCountdownEntity.IsEnabled = false;
            }
            if (pollOptionEntities != null)
            {
                for (int i = 0; i < pollOptionEntities.Count; i++)
                {
                    if (pollOptionEntities[i].Item2 != null)
                    {
                        pollOptionEntities[i].Item2.IsEnabled = false;
                    }
                }
            }
            if (activeModifierEntitiesPool == null)
            {
                for (int i = 0; i < activeModifierEntitiesPool.Count; i++)
                {
                    activeModifierEntitiesPool[i].IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// Shows all UI entities
        /// </summary>
        private void ShowAllUI()
        {
            if (pollDescriptionEntity != null)
            {
                pollDescriptionEntity.IsEnabled = true;
            }
            if (pollCountdownEntity != null)
            {
                pollCountdownEntity.IsEnabled = true;
            }
            if (pollOptionEntities != null)
            {
                for (int i = 0; i < pollOptionEntities.Count; i++)
                {
                    if (pollOptionEntities[i].Item2 != null)
                    {
                        pollOptionEntities[i].Item2.IsEnabled = true;
                    }
                }
            }
            if (activeModifierEntitiesPool == null)
            {
                for (int i = 0; i < activeModifierEntitiesPool.Count; i++)
                {
                    activeModifierEntitiesPool[i].IsEnabled = true;
                }
            }
        }

        /// <summary>
        /// Called by <see cref="PollTrigger.OnPollStarted"/>
        /// </summary>
        private void OnTwitchPollStarted(ModifierPoll poll)
        {
            logger.Information($"Received OnTwitchPollStarted Event");
            if (poll == null)
            {
                logger.Error($"Unable to create a visual as the provided Twitch Poll is null!");
                return;
            }

            currentPoll = poll;

            // Make the description text
            string descriptionText = "Vote on the modifier to activate by typing in chat!";
            Vector2 descriptionPosition = JumpGame.GAME_RECT.Size.ToVector2();
            descriptionPosition.Y = initialYOffset;
            descriptionPosition.X -= InitialPositionXPadding;
            pollDescriptionEntity = new UITextEntity(modEntityManager, descriptionPosition, descriptionText,
                Color.White, UIEntityAnchor.TopRight, JKContentManager.Font.MenuFontSmall, zOrder: 2);
            bottomOfOptionsYValue = descriptionPosition.Y + pollDescriptionEntity.TextFont.MeasureString(descriptionText).Y;

            // Make the countdown text
            string countdownText = $"Time Remaining: {((int)currentPoll.TimeRemainingInSeconds).ToString().PadLeft(2, '0')}";
            Vector2 countdownPosition = JumpGame.GAME_RECT.Size.ToVector2();
            countdownPosition.Y = bottomOfOptionsYValue;
            countdownPosition.X -= InitialPositionXPadding;
            pollCountdownEntity = new UITextEntity(modEntityManager, countdownPosition, countdownText, 
                Color.White, UIEntityAnchor.TopRight, JKContentManager.Font.MenuFontSmall, zOrder: 2);
            bottomOfOptionsYValue += (CountdownYPadding + pollDescriptionEntity.TextFont.MeasureString(descriptionText).Y);

            // Make each of the choices
            var choicesList = currentPoll.Choices.Values.ToList();
            for (int i = 0; i < choicesList.Count; i++)
            {

                string pollOptionText = GetPollOptionText(choicesList[i]);
                Vector2 pollOptionPosition = JumpGame.GAME_RECT.Size.ToVector2();
                pollOptionPosition.Y = bottomOfOptionsYValue;
                pollOptionPosition.X -= InitialPositionXPadding;
                var pollOptionEntity = new UITextEntity(modEntityManager, pollOptionPosition, pollOptionText,
                    Color.White, UIEntityAnchor.TopRight, JKContentManager.Font.MenuFontSmall, zOrder: 2);
                pollOptionEntities.Add(new Tuple<ModifierPollOption, UITextEntity>(choicesList[i], pollOptionEntity));

                bottomOfOptionsYValue += (YPadding + pollOptionEntity.TextFont.MeasureString(pollOptionText).Y);
            }
        }

        /// <summary>
        /// Called by <see cref="PollTrigger.OnPollClosed"/>
        /// </summary>
        private void OnTwitchPollClosed(ModifierPoll poll)
        {
            // Get the winner
            ModifierPollOption winningOption = poll.FindWinningModifier();

            // Set the winning item to the right colour
            for (int i = 0; i < pollOptionEntities.Count; i++)
            {
                if (pollOptionEntities[i].Item1 == winningOption)
                {
                    pollOptionEntities[i].Item2.TextColor = new Color(100, 200, 100, 255);
                }
                else
                {
                    pollOptionEntities[i].Item2.TextColor = new Color(Color.Gray.R, Color.Gray.G, Color.Gray.B, (byte)(Color.Gray.A * 0.5f));
                }
            }
        }

        /// <summary>
        /// Called by <see cref="PollTrigger.OnPollEnded"/>
        /// </summary>
        private void OnTwitchPollEnded(ModifierPoll poll)
        {
            // Clean up the poll
            logger.Information($"Received OnTwitchPollEnded Event");
            currentPoll = null;
            CleanUpUIEntities();
        }

        /// <summary>
        /// Returns a string representing a poll option and it's current count
        /// </summary>
        private string GetPollOptionText(ModifierPollOption pollOption)
        {
            return $"{pollOption.ChoiceNumber}. {pollOption.Modifier.DisplayName} - {pollOption.Count}";
        }

        /// <summary>
        /// Disposes and cleans up all known UI entities for the visual
        /// </summary>
        private void CleanUpUIEntities()
        {
            pollDescriptionEntity?.Dispose();
            pollDescriptionEntity = null;
            pollCountdownEntity?.Dispose();
            pollCountdownEntity = null;
            for (int i = 0; i < pollOptionEntities.Count; i++)
            {
                pollOptionEntities[i].Item2?.Dispose();
            }
            pollOptionEntities.Clear();
            if (activeModifierEntitiesPool == null)
            {
                for (int i = 0; i < activeModifierEntitiesPool.Count; i++)
                {
                    activeModifierEntitiesPool[i]?.Dispose();
                }
                activeModifierEntitiesPool.Clear();
                activeModifierEntitiesPool = null;
            }
        }

        /// <inheritdoc/>
        public void Draw()
        {
            // Do nothing
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            if (activeModifierEntitiesPool == null)
            {
                activeModifierEntitiesPool = new List<UITextEntity>();
                for (int i = 0; i < MaxActiveModifiersOnDisplay; i++)
                {
                    activeModifierEntitiesPool.Add(new UITextEntity(modEntityManager, Vector2.Zero, string.Empty,
                        new Color(150, 150, 200, 127), UIEntityAnchor.TopRight, JKContentManager.Font.MenuFontSmall, zOrder: 2));
                }
            }

            if (currentPoll != null)
            {
                string countdownText = $"Time Remaining: {((int)currentPoll.TimeRemainingInSeconds).ToString()}s";
                pollCountdownEntity.TextValue = countdownText;

                // Update the current options
                for (int i = 0; i < pollOptionEntities.Count; i++)
                {
                    ModifierPollOption option = pollOptionEntities[i].Item1;
                    UITextEntity optionEntity = pollOptionEntities[i].Item2;
                    optionEntity.TextValue = GetPollOptionText(option);
                }
            }

            // Show all active modifiers
            if (trigger != null && activeModifierEntitiesPool != null)
            {
                float currentYValue = bottomOfOptionsYValue + ActiveModifierYPadding;
                IReadOnlyList<ActiveModifierCountdown> activeModifierCountdowns = trigger.GetActiveModifierCountdowns();
                for (int i = 0; i < activeModifierEntitiesPool.Count; i++)
                {
                    Vector2 position = JumpGame.GAME_RECT.Size.ToVector2();
                    position.Y = currentYValue;
                    position.X -= InitialPositionXPadding;

                    // Show as many active modifiers we have that fit within the pool
                    // Hide any left over
                    if (i < activeModifierCountdowns.Count)
                    {
                        activeModifierEntitiesPool[i].ScreenSpacePosition = position;
                        activeModifierEntitiesPool[i].TextValue = 
                            $"{activeModifierCountdowns[i].Modifier.DisplayName}: {((int)activeModifierCountdowns[i].DurationCounter).ToString()}s";
                    }
                    else
                    {
                        activeModifierEntitiesPool[i].TextValue = string.Empty;
                    }

                    currentYValue += activeModifierEntitiesPool[i].TextFont.MeasureString(activeModifierEntitiesPool[i].TextValue).Y;
                }
            }
        }
    }
}
