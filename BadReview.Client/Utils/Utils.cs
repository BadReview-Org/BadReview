using System.Linq.Expressions;

using BadReview.Client.Components;

namespace BadReview.Client.Utils;

public static class Uris
{
    public const string Trending = "api/games/trending";
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

public interface ICardStrategy
{
    Type GetComponentType();
    Dictionary<string, object> GetParameters(object element);
}

public class CardContainer
{
    public ICardStrategy strat;
    public object dto;

    public CardContainer(ICardStrategy strat, object dto)
    {
        this.strat = strat;
        this.dto = dto;
    }
}

public class BasicGameCardStrat : ICardStrategy
{
    public bool ShowRatings { get; set; } = true;

    public BasicGameCardStrat(bool showRatings = true)
    {
        ShowRatings = showRatings;
    }

    public Type GetComponentType() => typeof(BasicGameCard);
    public Dictionary<string, object> GetParameters(object element) => new() 
    { 
        ["game"] = element,
        ["ShowRatings"] = ShowRatings
    };
}

public class DetailReviewCardStrat : ICardStrategy
{
    public Type GetComponentType() => typeof(DetailReviewCard);
    public Dictionary<string, object> GetParameters(object element) => new() { ["review"] = element };
}

public record IsoCountry(string Name, string Alpha_3, int Country_code);