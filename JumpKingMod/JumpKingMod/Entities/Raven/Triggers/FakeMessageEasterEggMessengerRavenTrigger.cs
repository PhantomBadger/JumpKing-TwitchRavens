using JumpKingMod.API;
using JumpKingMod.Entities.Raven.Triggers.EasterEgg;
using Logging.API;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JumpKingMod.Entities.Raven.Triggers
{
    /// <summary>
    /// An implementation of <see cref="IMessengerRavenTrigger"/> which spawns fake messages on a timer based on the
    /// a specific twitch/youtube user as an easter egg
    /// </summary>
    public class FakeMessageEasterEggMessengerRavenTrigger : IMessengerRavenTrigger
    {
        public event MessengerRavenTriggerArgs OnMessengerRavenTrigger;

        private readonly Dictionary<string, List<EasterEggMessageInfo>> easterEggMessages =
            new Dictionary<string, List<EasterEggMessageInfo>>(StringComparer.OrdinalIgnoreCase)
        {
            {   // Lo-Fi Girl YouTube Channel - Used for Testing
                "UCSJ4gkVC6NrvII8umztf0Ow", new List<EasterEggMessageInfo>()
                {
                    new EasterEggMessageInfo()
                    {
                        RavenMessage = "Test!",
                        RavenName = "PhantomBadger",
                        RavenNameColor = Color.Blue,
                        MessageOdds = 1,
                    },
                }
            },
            {
                "PhantomBadger", new List<EasterEggMessageInfo>()
                {
                    new EasterEggMessageInfo()
                    {
                        RavenMessage = "Hello, me!",
                        RavenName = "PhantomBadger",
                        RavenNameColor = Color.LightBlue,
                        MessageOdds = 1,
                    },
                }
            },
            {
                "cinna", new List<EasterEggMessageInfo>()
                {
                    new EasterEggMessageInfo()
                    {
                        RavenMessage = "Squirty Cream lol",
                        RavenName = "PhantomBadger",
                        RavenNameColor = Color.LightBlue,
                        MessageOdds = 1,
                    },
                    new EasterEggMessageInfo()
                    {
                        RavenMessage = "i'm short",
                        RavenName = "Cinna",
                        RavenNameColor = Color.LightBlue,
                        MessageOdds = 1,
                    }
                }
            },
            {
                "cinnabrit", new List<EasterEggMessageInfo>()
                {
                    new EasterEggMessageInfo()
                    {
                        RavenMessage = "Squirty Cream lol",
                        RavenName = "PhantomBadger",
                        RavenNameColor = Color.LightBlue,
                        MessageOdds = 1,
                    },
                    new EasterEggMessageInfo()
                    {
                        RavenMessage = "i'm short",
                        RavenName = "Cinna",
                        RavenNameColor = Color.LightBlue,
                        MessageOdds = 1,
                    }
                }
            },
            {
                "ludwig", new List<EasterEggMessageInfo>()
                {
                    new EasterEggMessageInfo()
                    {
                        RavenMessage = "Don't fall down :)",
                        RavenName = "PhantomBadger",
                        RavenNameColor = Color.LightBlue,
                        MessageOdds = 50,
                    },
                    new EasterEggMessageInfo()
                    {
                        RavenMessage = "Hey Ludwig!",
                        RavenName = "Twitch",
                        RavenNameColor = Color.Purple,
                        MessageOdds = 1,
                    }
                }
            },
            {
                "sennyk4", new List<EasterEggMessageInfo>()
                {
                    new EasterEggMessageInfo()
                    {
                        RavenMessage = "stop leeching",
                        RavenName = "Mizkif",
                        RavenNameColor = Color.Green,
                        MessageOdds = 1,
                    }
                }
            },
            {
                "rhymestyle", new List<EasterEggMessageInfo>()
                {
                    new EasterEggMessageInfo()
                    {
                        RavenMessage = "WUTISGOIN",
                        RavenName = "Rhymestyle",
                        RavenNameColor = Color.Red,
                        MessageOdds = 1,
                    },

                }
            },
            {
                "TheBrutalic", new List<EasterEggMessageInfo>()
                {
                    new EasterEggMessageInfo()
                    {
                        RavenMessage = "You're 30, right?",
                        RavenName = "TheBrutalic",
                        RavenNameColor = Color.Aquamarine,
                        MessageOdds = 1,
                    }
                }
            },
            {
                "Ottomated", new List<EasterEggMessageInfo>()
                {
                    new EasterEggMessageInfo()
                    {
                        RavenMessage = ":) Thanks for trying my mod!",
                        RavenName = "PhantomBadger",
                        RavenNameColor = Color.LightBlue,
                        MessageOdds = 1,
                    },
                }
            },
            {
                "cdawgva", new List<EasterEggMessageInfo>()
                {
                    new EasterEggMessageInfo()
                    {
                        RavenMessage = "JoJo Part 4 is the best part",
                        RavenName = "Hirohiko Araki",
                        RavenNameColor = Color.Purple,
                        MessageOdds = 1,
                    }
                }
            },
            {
                "duccW", new List<EasterEggMessageInfo>()
                {
                    new EasterEggMessageInfo()
                    {
                        RavenMessage = "Hey! can I have your opinion about a game I made?",
                        RavenName = "duccW",
                        RavenNameColor = Color.Orange,
                        MessageOdds = 1,
                    }
                }
            }
        };
        private readonly Random random;
        private readonly ILogger logger;

        private Thread triggerThread;
        private CancellationTokenSource cancellationTokenSource;

        private const int MinEasterEggIntervalInMilliseconds = 600000; // 10 minutes
        private const int MaxEasterEggInternalInMilliseconds = 6000000; // 100 minutes

        /// <summary>
        /// Ctor for creating a <see cref="FakeMessageEasterEggMessengerRavenTrigger"/>
        /// </summary>
        public FakeMessageEasterEggMessengerRavenTrigger(ILogger logger)
        {
            random = new Random(DateTime.Now.Millisecond);
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Checks whether we should activate easter eggs for this user or not
        /// </summary>
        public bool ShouldActivateEasterEggs(string channelId)
        {
            return easterEggMessages.ContainsKey(channelId.Trim());
        }

        /// <summary>
        /// Starts the easter egg processing thread for the specified user
        /// </summary>
        public void StartEasterEggTrigger(string channelId)
        {
            // Create the cancellation token
            cancellationTokenSource = new CancellationTokenSource();

            // Get the easter egg messages to use and calculate the percentage limits
            List<EasterEggMessageInfo> easterEggMessageInfos = easterEggMessages[channelId.Trim()];
            int maxOddsValue = 0;
            for (int i = 0; i < easterEggMessageInfos.Count; i++)
            {
                maxOddsValue += (int)easterEggMessageInfos[i].MessageOdds;
            }

            triggerThread = new Thread(() => { PerformTriggerLoop(channelId, easterEggMessageInfos, maxOddsValue, cancellationTokenSource.Token); });
            triggerThread.Start();
        }

        /// <summary>
        /// Requests that the easter egg trigger is ended
        /// </summary>
        public void StopEasterEggTrigger()
        {
            cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Performs the trigger spawning loop
        /// </summary>
        private void PerformTriggerLoop(string channelId, List<EasterEggMessageInfo> easterEggMessageInfos, int maxOddsValue, CancellationToken token)
        {
            logger.Information($"Starting Easter Egg Trigger for '{channelId}'");

            while (!token.IsCancellationRequested)
            {
                // Decide the random interval before sending the next message
                int interval = random.Next(MinEasterEggIntervalInMilliseconds, MaxEasterEggInternalInMilliseconds);
                logger.Information($"Waiting {interval} milliseconds for next Easter Egg Trigger");
                Task.Delay(interval).Wait(token);

                // Identify which easter egg message to use
                int oddsValue = random.Next(0, maxOddsValue + 1);
                int oddsCounter = 0;
                EasterEggMessageInfo easterEggMessageInfo = null;
                for (int i = 0; i < easterEggMessageInfos.Count; i++)
                {
                    // Increment up until we hit our odds value
                    if ((oddsCounter += (int)easterEggMessageInfos[i].MessageOdds) > oddsValue)
                    {
                        easterEggMessageInfo = easterEggMessageInfos[i];
                        break;
                    }
                }

                if (easterEggMessageInfo == null)
                {
                    logger.Warning($"Unable to identify Easter Egg from collection");
                    continue;
                }

                // Check for cancel
                if (token.IsCancellationRequested)
                {
                    break;
                }

                // Spawn the easter egg message
                logger.Information($"Spawning Easter Egg '{easterEggMessageInfo.RavenMessage}' from '{easterEggMessageInfo.RavenName}'");
                OnMessengerRavenTrigger?.Invoke(easterEggMessageInfo.RavenName, easterEggMessageInfo.RavenNameColor, easterEggMessageInfo.RavenMessage, isFromSubscriber: true);
            }
        }
    }
}
