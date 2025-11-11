using BadReview.Api.Models.Owned;

namespace BadReview.Api.Models;

public class Platform
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Abbreviation { get; set; }
    public int? Generation { get; set; }
    public string? Summary { get; set; }
    public int? PlatformType { get; set; }
    public string? PlatformTypeName { get; set; }
    public Image? Logo { get; set; }

    public ICollection<GamePlatform> GamePlatforms { get; set; } = new List<GamePlatform>();
}