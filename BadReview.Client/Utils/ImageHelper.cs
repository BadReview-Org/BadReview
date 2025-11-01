namespace BadReview.Client.Utils;

public static class ImageHelper
{
    public static string GetCoverUrl(string? cover)
    {
        if (string.IsNullOrEmpty(cover))
            return ""; // Placeholder si no hay cover
        
        return $"https://images.igdb.com/igdb/image/upload/t_cover_big_2x/{cover}.png";
    }
}
