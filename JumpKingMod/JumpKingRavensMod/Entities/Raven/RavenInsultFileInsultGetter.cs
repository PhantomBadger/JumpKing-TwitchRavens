using JumpKingRavensMod.API;
using JumpKingRavensMod.Settings;
using Logging.API;
using Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.Entities.Raven
{
    /// <summary>
    /// An implementation of <see cref="IInsultGetter"/> which gets an insult at random
    /// from a pre-made file
    /// </summary>
    public class RavenInsultFileInsultGetter : IInsultGetter
    {
        private readonly List<string> insults;
        private readonly Random random;

        /// <summary>
        /// Ctor for creating an <see cref="RavenInsultFileInsultGetter"/>
        /// </summary>
        public RavenInsultFileInsultGetter(ILogger logger)
        {
            insults = new List<string>();
            random = new Random(DateTime.Now.Millisecond);

            // Parsing Raven Insult List
            try
            {
                if (File.Exists(JumpKingRavensModSettingsContext.RavenInsultsFilePath))
                {
                    string[] fileContents = File.ReadAllLines(JumpKingRavensModSettingsContext.RavenInsultsFilePath);
                    for (int i = 0; i < fileContents.Length; i++)
                    {
                        string line = fileContents[i].Trim();
                        if (line.Length <= 0 || line[0] == JumpKingRavensModSettingsContext.CommentCharacter)
                        {
                            continue;
                        }

                        insults.Add(line);
                    }
                    logger.Information($"Successfully loaded Raven Insult File with '{insults.Count}' entries");
                }
                else
                {
                    logger.Error($"Unable to find Raven Insult List at '{JumpKingRavensModSettingsContext.RavenInsultsFilePath}'");
                }
            }
            catch (Exception e)
            {
                logger.Error($"Encountered exception when loading Raven Insults: {e.ToString()}");
            }
        }

        /// <summary>
        /// Gets a random entry from the insult list
        /// </summary>
        public string GetInsult()
        {
            if (insults == null || insults.Count <= 0)
            {
                return "NO INSULTS FOUND!";
            }

            int index = random.Next(0, insults.Count);

            return insults[index];
        }
    }
}
