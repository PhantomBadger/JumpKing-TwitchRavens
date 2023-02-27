using Logging.API;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.Twitch
{
    public abstract class TwitchHexColourParser
    {
        public static Color ParseColourFromHex(string colourHex, ILogger logger = null)
        {
            try
            {
                int rValue = int.Parse(colourHex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                int gValue = int.Parse(colourHex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                int bValue = int.Parse(colourHex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                Color color = new Color(rValue, gValue, bValue, 255);
                return color;
            }
            catch (Exception ex)
            {
                logger?.Error($"Failed to parse colour for colour hex '{colourHex}'");
            }
            return Color.White;
        }
    }
}
