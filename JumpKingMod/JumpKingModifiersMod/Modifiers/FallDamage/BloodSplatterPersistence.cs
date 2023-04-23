using JumpKing;
using JumpKingModifiersMod.Settings;
using Logging.API;
using Microsoft.Xna.Framework;
using PBJKModBase.Entities;
using Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Modifiers
{
    /// <summary>
    /// Manages the persistent nature of the Blood Splatters
    /// </summary>
    public class BloodSplatterPersistence : IDisposable
    {
        private readonly ModEntityManager modEntityManager;
        private readonly UserSettings userSettings;
        private readonly Random random;
        private readonly ILogger logger;

        public List<WorldspaceImageEntity> BloodSplatters;

        /// <summary>
        /// Ctor for creating a <see cref="BloodSplatterPersistence"/>
        /// </summary>
        public BloodSplatterPersistence(ModEntityManager modEntityManager, UserSettings userSettings, Random random, ILogger logger)
        {
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            BloodSplatters = new List<WorldspaceImageEntity>();
        }

        /// <summary>
        /// Implementation of <see cref="IDisposable.Dispose"/> which clears up the entities
        /// </summary>
        public void Dispose()
        {
            ClearAllBloodSplats();
        }

        /// <summary>
        /// Clears all the blood splats
        /// </summary>
        public void ClearAllBloodSplats()
        {
            for (int i = 0; i < BloodSplatters.Count; i++)
            {
                BloodSplatters[i]?.Dispose();
            }
            BloodSplatters.Clear();
        }

        /// <summary>
        /// Loads the blood splats from disk
        /// </summary>
        public bool LoadBloodSplatters()
        {
            try
            {
                // Parse all the positions
                List<Vector2> positions = new List<Vector2>();
                if (File.Exists(JumpKingModifiersModSettingsContext.FallDamageBloodSplatterFilePath))
                {
                    string[] fileContents = File.ReadAllLines(JumpKingModifiersModSettingsContext.FallDamageBloodSplatterFilePath);
                    for (int i = 0; i < fileContents.Length; i++)
                    {
                        string line = fileContents[i].Trim();
                        if (line.Length <= 0 || line[0] == JumpKingModifiersModSettingsContext.CommentCharacter)
                        {
                            continue;
                        }

                        string[] numbers = line.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries); 
                        if (numbers.Length != 2)
                        {
                            // Skip its not valid
                            continue;
                        }

                        if (float.TryParse(numbers[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float parsedX) &&
                            float.TryParse(numbers[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float parsedY))
                        {
                            Vector2 position = new Vector2(parsedX, parsedY);
                            positions.Add(position);
                        }
                    }
                    logger.Information($"Successfully loaded 'Blood Splatter' File with '{positions.Count}' entries");
                }
                else
                {
                    logger.Warning($"Unable to find 'Blood Splatter' File at '{JumpKingModifiersModSettingsContext.FallDamageSubtextsFilePath}' - creating a new one!");
                    return false;
                }

                // Make all the blood splatters
                for (int i = 0; i < positions.Count; i++)
                {
                    if (ModifiersModContentManager.BloodSplatterSprites == null || ModifiersModContentManager.BloodSplatterSprites.Length == 0)
                    {
                        return false;
                    }

                    Sprite splatSprite = ModifiersModContentManager.BloodSplatterSprites[random.Next(ModifiersModContentManager.BloodSplatterSprites.Length)];

                    var bloodSplat = new WorldspaceImageEntity(modEntityManager, positions[i], splatSprite, zOrder: 1);
                    BloodSplatters.Add(bloodSplat);
                }
                logger.Information($"Successfully loaded '{BloodSplatters.Count}' Blood Splatters from disk");
                return true;
            }
            catch (Exception e)
            {
                logger.Error($"Encountered exception when loading 'Blood Splatter' File: {e.ToString()}");
                return false;
            }
        }

        /// <summary>
        /// Saves the blood splats to disk
        /// </summary>
        public bool SaveBloodSplatters()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < BloodSplatters.Count; i++)
            {
                stringBuilder.AppendLine($"{BloodSplatters[i].WorldSpacePosition.X},{BloodSplatters[i].WorldSpacePosition.Y}");
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(JumpKingModifiersModSettingsContext.FallDamageBloodSplatterFilePath));
                File.WriteAllText(JumpKingModifiersModSettingsContext.FallDamageBloodSplatterFilePath, stringBuilder.ToString());
                return true;
            }
            catch (Exception e)
            {
                logger.Error($"Failed to save out 'Blood Splatters' file to '{JumpKingModifiersModSettingsContext.FallDamageBloodSplatterFilePath}': {e.ToString()}");
                return false;
            }
        }
    }
}
