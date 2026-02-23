using Microsoft.Xna.Framework;
using StardewValley;

namespace StarDo.Services
{
    public static class SeasonHelper
    {
        public static readonly Season[] OrderedSeasons =
        {
            Season.Spring,
            Season.Summer,
            Season.Fall,
            Season.Winter
        };

        public static Season GetCurrentSeason()
        {
            return TryParseSeason(Game1.currentSeason, out var season)
                ? season
                : Season.Spring;
        }

        public static bool TryParseSeason(string seasonName, out Season season)
        {
            switch (seasonName?.ToLowerInvariant())
            {
                case "spring":
                    season = Season.Spring;
                    return true;
                case "summer":
                    season = Season.Summer;
                    return true;
                case "fall":
                    season = Season.Fall;
                    return true;
                case "winter":
                    season = Season.Winter;
                    return true;
                default:
                    season = Season.Spring;
                    return false;
            }
        }

        public static string GetDisplayName(Season season)
        {
            return season switch
            {
                Season.Spring => "Spring",
                Season.Summer => "Summer",
                Season.Fall => "Fall",
                Season.Winter => "Winter",
                _ => "Spring"
            };
        }

        public static string GetAbbreviation(Season season)
        {
            return season switch
            {
                Season.Spring => "Spr",
                Season.Summer => "Sum",
                Season.Fall => "Fall",
                Season.Winter => "Win",
                _ => "?"
            };
        }

        public static Color GetColor(Season season)
        {
            return season switch
            {
                Season.Spring => new Color(60, 180, 75),
                Season.Summer => new Color(220, 180, 30),
                Season.Fall => new Color(210, 120, 40),
                Season.Winter => new Color(80, 140, 210),
                _ => Color.Gray
            };
        }
    }
}
