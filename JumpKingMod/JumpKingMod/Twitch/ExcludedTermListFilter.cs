using JumpKingMod.API;
using Logging.API;
using Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Twitch
{
    /// <summary>
    /// An implementation of <see cref="IExcludedTermFilter"/> which checks against a loaded list of excluded phrases
    /// </summary>
    public class ExcludedTermListFilter : IExcludedTermFilter
    {
        private readonly List<string> excludedPhrases;

        /// <summary>
        /// Ctor for creating an <see cref="ExcludedTermListFilter"/>
        /// </summary>
        public ExcludedTermListFilter(ILogger logger)
        {
            excludedPhrases = new List<string>();

            // Parsing Excluded Term List
            if (File.Exists(JumpKingModSettingsContext.ExcludedTermFilePath))
            {
                string[] fileContents = File.ReadAllLines(JumpKingModSettingsContext.ExcludedTermFilePath);
                for (int i = 0; i < fileContents.Length; i++)
                {
                    string line = fileContents[i].Trim();
                    if (line.Length <= 0 || line[0] == JumpKingModSettingsContext.CommentCharacter)
                    {
                        continue;
                    }

                    excludedPhrases.Add(line);
                }
                logger.Information($"Successfully loaded Excluded Term File with '{excludedPhrases.Count}' entries");
            }
            else
            {
                logger.Error($"Unable to find Excluded Term File List at '{JumpKingModSettingsContext.ExcludedTermFilePath}'");
            }
        }

        /// <summary>
        /// Checks whether the provided string contains any of the excluded phrases
        /// </summary>
        public bool ContainsExcludedTerm(string textToCheck)
        {
            return excludedPhrases.Any((string excludedPhrase) => textToCheck.ToLower().Contains(excludedPhrase.ToLower()));
        }
    }
}
