using static MudBlazor.Icons;
using static MudBlazor.Icons.Material.Filled;

namespace BadReview.Client.Utils;

public static class IconsMap
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
        TURNBASEDSTRATEGY = 16, REALTIMESTRATEGY = 11
    };

    public enum PlatformTypes
    {
        CONSOLE = 1, ARCADE = 2, PLATFORM = 3,
        OPERATINGSYSTEM = 4, PORTABLECONSOLE = 5, COMPUTER = 6,
    }

    public static readonly Dictionary<Genres, string> GenreIcon = new()
    {
        [Genres.ADVENTURE] = Material.Filled.Explore,
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
    };

    public static readonly Dictionary<PlatformTypes, string> PlatformTypeIcon = new()
    {
        [PlatformTypes.CONSOLE] = VideogameAsset,
        [PlatformTypes.ARCADE] = Gamepad,
        [PlatformTypes.PLATFORM] = QuestionMark,
        [PlatformTypes.OPERATINGSYSTEM] = Custom.Brands.Linux,
        [PlatformTypes.PORTABLECONSOLE] = TabletMac,
        [PlatformTypes.COMPUTER] = Computer
    };

}