using Logging.API;
using Microsoft.Xna.Framework;
using PBJKModBase.Twitch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.YouTube
{
    public abstract class YouTubeHexColourGenerator
    {
        public static Color GenerateColourFromName(string colourName, ILogger logger = null)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(colourName));
                    string hexCode = $"#{BitConverter.ToString(data).Replace("-", string.Empty).Substring(0, 6)}";

                    return TwitchHexColourParser.ParseColourFromHex(hexCode, logger);
                }
            }
            catch (Exception ex)
            {
                logger?.Error($"Failed to generate colour for string {colourName}");
            }
            return Color.White;
        }
    }
}
