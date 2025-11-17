using static MudBlazor.Icons.Custom.Brands;
using static MudBlazor.Icons.Material.Filled;

namespace BadReview.Client.Services;

public static class IconsMethods
{
    public static IconsMap.Genres ToGenres(this int id)
    {
        if (Enum.IsDefined(typeof(IconsMap.Genres), id))
            return (IconsMap.Genres)id;
        else
            return IconsMap.Genres.DEFAULT;
    }

    public static IconsMap.PlatformTypes ToPlatformTypes(this int id)
    {
        if (Enum.IsDefined(typeof(IconsMap.PlatformTypes), id))
            return (IconsMap.PlatformTypes)id;
        else
            return IconsMap.PlatformTypes.DEFAULT;
    }
}

public class IconsMap
{
    public enum Genres
    {
        ADVENTURE = 31, RPG = 12, STRATEGY = 15,
        SHOOTER = 5, PUZZLE = 9, RACING = 10,
        SPORT = 14, FIGHTING = 4, SIMULATOR = 13,
        PLATFORM = 8, ARCADE = 33, CARDBOARDGAME = 35,
        BEATEMUP = 25, INDIE = 32, MOBA = 36,
        MUSIC = 7, PINBALL = 30, POINTANDCLICK = 2,
        QUIZTRIVIA = 26, TACTICAL = 24, VISUALNOVEL = 34,
        TURNBASEDSTRATEGY = 16, REALTIMESTRATEGY = 11,
        DEFAULT = -1
    };

    public enum PlatformTypes
    {
        CONSOLE = 1, ARCADE = 2, PLATFORM = 3,
        OPERATINGSYSTEM = 4, PORTABLECONSOLE = 5, COMPUTER = 6,
        DEFAULT = -1
    }

    public static readonly Dictionary<Genres, string> GenreIcon = new()
    {
        [Genres.ADVENTURE] = Explore,
        [Genres.RPG] = Castle,
        [Genres.STRATEGY] = Psychology,
        [Genres.SHOOTER] = GpsFixed,
        [Genres.PUZZLE] = Extension,
        [Genres.RACING] = SportsScore,
        [Genres.SPORT] = SportsSoccer,
        [Genres.FIGHTING] = SportsMma,
        [Genres.SIMULATOR] = Engineering,
        [Genres.PLATFORM] = Stairs,
        [Genres.ARCADE] = SportsEsports,
        [Genres.CARDBOARDGAME] = Style,
        [Genres.BEATEMUP] = SportsKabaddi,
        [Genres.INDIE] = Coffee,
        [Genres.MOBA] = Groups,
        [Genres.MUSIC] = MusicNote,
        [Genres.PINBALL] = Album,
        [Genres.POINTANDCLICK] = TouchApp,
        [Genres.QUIZTRIVIA] = Quiz,
        [Genres.TACTICAL] = HealthAndSafety,
        [Genres.VISUALNOVEL] = MenuBook,
        [Genres.TURNBASEDSTRATEGY] = HourglassBottom,
        [Genres.REALTIMESTRATEGY] = Festival,

        [Genres.DEFAULT] = QuestionMark
    };

    public static readonly Dictionary<PlatformTypes, string> PlatformTypeIcon = new()
    {
        [PlatformTypes.CONSOLE] = VideogameAsset,
        [PlatformTypes.ARCADE] = Gamepad,
        [PlatformTypes.PLATFORM] = QuestionMark,
        [PlatformTypes.OPERATINGSYSTEM] = Linux,
        [PlatformTypes.PORTABLECONSOLE] = TabletMac,
        [PlatformTypes.COMPUTER] = Computer,

        [PlatformTypes.DEFAULT] = QuestionMark
    };

}