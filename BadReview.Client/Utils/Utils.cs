namespace BadReview.Client.Utils;

public static class Uris
{
    public static string trending = "api/games/trending";
}

public static class ImageHelper
{
    public static string GetCoverUrl(string? cover)
    {
        if (string.IsNullOrEmpty(cover))
            return ""; // Placeholder si no hay cover

        return $"https://images.igdb.com/igdb/image/upload/t_cover_big_2x/{cover}.png";
    }
}

public enum CardsEnum {BASICGAME, DETAILSGAME, BASICREVIEW, DETAILREVIEW};