using MudBlazor;

namespace BadReview.Client.Utils;

public static class CustomPalettes
{
    public static readonly MudTheme CustomTheme;

    static CustomPalettes()
    {
        CustomTheme = SetTheme();
    }

    private static MudTheme SetTheme() =>
        new MudTheme
        {
            PaletteDark = new PaletteDark()
            {
                PrimaryDarken = "#27272F",
                Primary = "rgba(255,255,255, 0.2)",
                InfoLighten = Colors.Green.Default,
                InfoDarken = Colors.Red.Default,

                Black = "#27272f",
                Info = "#3299ff",
                Success = "#0bba83",
                Warning = "#ffa800",
                Error = "#f64e62",
                Dark = "#27272f",
                TextPrimary = "rgba(255,255,255, 0.70)",
                TextSecondary = "rgba(255,255,255, 0.50)",
                TextDisabled = "rgba(255,255,255, 0.2)",
                ActionDefault = "#adadb1",
                ActionDisabled = "rgba(255,255,255, 0.26)",
                ActionDisabledBackground = "rgba(255,255,255, 0.12)",
                Background = "#32333d",
                BackgroundGray = "#27272f",
                Surface = "#373740",
                DrawerBackground = "#27272f",
                DrawerText = "rgba(255,255,255, 0.50)",
                DrawerIcon = "rgba(255,255,255, 0.50)",
                AppbarBackground = "#27272f",
                AppbarText = "rgba(255,255,255, 0.70)",
                LinesDefault = "rgba(255,255,255, 0.12)",
                LinesInputs = "rgba(255,255,255, 0.3)",
                TableLines = "rgba(255,255,255, 0.12)",
                TableStriped = "rgba(255,255,255, 0.2)",
                Divider = "rgba(255,255,255, 0.12)",
                DividerLight = "rgba(255,255,255, 0.06)",
                Skeleton = "rgba(255,255,255, 0.11)"
            },
            PaletteLight = new PaletteLight()
            {
                PrimaryDarken = "#27272F",
                Primary = "rgba(255,255,255, 0.2)",
                InfoLighten = Colors.Green.Default,
                InfoDarken = Colors.Red.Default,

                Black = "#27272f",
                Info = "#3299ff",
                Success = "#0bba83",
                Warning = "#ffa800",
                Error = "#f64e62",
                Dark = "#27272f",
                TextPrimary = "rgba(255,255,255, 0.70)",
                TextSecondary = "rgba(255,255,255, 0.50)",
                TextDisabled = "rgba(255,255,255, 0.2)",
                ActionDefault = "#adadb1",
                ActionDisabled = "rgba(255,255,255, 0.26)",
                ActionDisabledBackground = "rgba(255,255,255, 0.12)",
                Background = "#32333d",
                BackgroundGray = "#27272f",
                Surface = "#373740",
                DrawerBackground = "#27272f",
                DrawerText = "rgba(255,255,255, 0.50)",
                DrawerIcon = "rgba(255,255,255, 0.50)",
                AppbarBackground = "#27272f",
                AppbarText = "rgba(255,255,255, 0.70)",
                LinesDefault = "rgba(255,255,255, 0.12)",
                LinesInputs = "rgba(255,255,255, 0.3)",
                TableLines = "rgba(255,255,255, 0.12)",
                TableStriped = "rgba(255,255,255, 0.2)",
                Divider = "rgba(255,255,255, 0.12)",
                DividerLight = "rgba(255,255,255, 0.06)",
                Skeleton = "rgba(255,255,255, 0.11)"
            }
        };
}