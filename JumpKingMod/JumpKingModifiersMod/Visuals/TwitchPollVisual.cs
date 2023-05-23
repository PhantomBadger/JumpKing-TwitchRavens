using JumpKing;
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
    /// <see cref="TwitchPollTrigger"/>, creating and managing UI texts to display the poll state to the chat
    /// </summary>
    public class TwitchPollVisual : IModEntity, IDisposable
    {
        private readonly ModEntityManager modEntityManager;
        private readonly TwitchPollTrigger trigger;
        private readonly ILogger logger;

        private ModifierTwitchPoll currentPoll;
        private UITextEntity pollDescriptionEntity;
        private List<Tuple<ModifierTwitchPollOption, UITextEntity>> pollOptionEntities;

        private const float YPadding = 1;
        private const float InitialPositionXPadding = 8f;

        /// <summary>
        /// Ctor for creating a <see cref="TwitchPollVisual"/>
        /// </summary>
        /// <param name="modEntityManager">The <see cref="ModEntityManager"/> to register to</param>
        /// <param name="trigger">The <see cref="TwitchPollTrigger"/> to act as a visual for</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to use for logging</param>
        public TwitchPollVisual(ModEntityManager modEntityManager, TwitchPollTrigger trigger, ILogger logger)
        {
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.trigger = trigger ?? throw new ArgumentNullException(nameof(trigger));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            pollOptionEntities = new List<Tuple<ModifierTwitchPollOption, UITextEntity>>();

            this.trigger.OnTwitchPollStarted += OnTwitchPollStarted;
            this.trigger.OnTwitchPollEnded += OnTwitchPollEnded;
            this.modEntityManager.AddEntity(this, zOrder: 0);
        }

        /// <summary>
        /// An implementation of <see cref="IDisposable.Dispose"/> to clean up events
        /// </summary>
        public void Dispose()
        {
            trigger.OnTwitchPollStarted -= OnTwitchPollStarted;
            trigger.OnTwitchPollEnded -= OnTwitchPollEnded;
            modEntityManager.RemoveEntity(this);
            CleanUpUIEntities();
        }

        /// <summary>
        /// Called by <see cref="TwitchPollTrigger.OnTwitchPollStarted"/>
        /// </summary>
        private void OnTwitchPollStarted(ModifierTwitchPoll poll)
        {
            logger.Information($"Received OnTwitchPollStarted Event");
            if (poll == null)
            {
                logger.Error($"Unable to create a visual as the provided Twitch Poll is null!");
                return;
            }

            currentPoll = poll;

            // Make the description text
            StringBuilder descriptionText = new StringBuilder();
            descriptionText.Append("Vote on the modifier to activate by typing ");
            var choicesNumberList = currentPoll.Choices.Keys.ToList();
            for (int i = 0; i < choicesNumberList.Count; i++)
            {
                if (i == choicesNumberList.Count - 1)
                {
                    descriptionText.Append($"or {choicesNumberList[i]} ");
                }
                else
                {
                    descriptionText.Append($"{choicesNumberList[i]}, ");
                }
            }
            descriptionText.Append("in chat!");
            Vector2 descriptionPosition = JumpGame.GAME_RECT.Size.ToVector2();
            descriptionPosition.Y = 0;
            descriptionPosition.X -= InitialPositionXPadding;
            pollDescriptionEntity = new UITextEntity(modEntityManager, descriptionPosition, descriptionText.ToString(),
                Color.White, UIEntityAnchor.TopRight, JKContentManager.Font.MenuFontSmall, zOrder: 2);

            // Make each of the choices
            var choicesList = currentPoll.Choices.Values.ToList();
            float currentY = descriptionPosition.Y + pollDescriptionEntity.TextFont.MeasureString(descriptionText.ToString()).Y;
            for (int i = 0; i < choicesList.Count; i++)
            {

                string pollOptionText = GetPollOptionText(choicesList[i]);
                Vector2 pollOptionPosition = JumpGame.GAME_RECT.Size.ToVector2();
                pollOptionPosition.Y = currentY;
                pollOptionPosition.X -= InitialPositionXPadding;
                var pollOptionEntity = new UITextEntity(modEntityManager, pollOptionPosition, pollOptionText,
                    Color.White, UIEntityAnchor.TopRight, zOrder: 2);
                pollOptionEntities.Add(new Tuple<ModifierTwitchPollOption, UITextEntity>(choicesList[i], pollOptionEntity));
                
                currentY += (YPadding + pollOptionEntity.TextFont.MeasureString(pollOptionText).Y);
            }
        }

        /// <summary>
        /// Called by <see cref="TwitchPollTrigger.OnTwitchPollEnded"/>
        /// </summary>
        private void OnTwitchPollEnded(ModifierTwitchPoll poll)
        {
            // TODO - show winner somehow
            logger.Information($"Received OnTwitchPollEnded Event");
            currentPoll = null;
            CleanUpUIEntities();
        }

        /// <summary>
        /// Returns a string representing a poll option and it's current count
        /// </summary>
        private string GetPollOptionText(ModifierTwitchPollOption pollOption)
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
            for (int i = 0; i < pollOptionEntities.Count; i++)
            {
                pollOptionEntities[i].Item2?.Dispose();
            }
            pollOptionEntities.Clear();
        }

        /// <inheritdoc/>
        public void Draw()
        {
            // Do nothing
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            if (currentPoll != null)
            {
                // Update the current options
                for (int i = 0; i < pollOptionEntities.Count; i++)
                {
                    ModifierTwitchPollOption option = pollOptionEntities[i].Item1;
                    UITextEntity optionEntity = pollOptionEntities[i].Item2;
                    optionEntity.TextValue = GetPollOptionText(option);
                }
            }
        }
    }
}
