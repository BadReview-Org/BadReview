namespace BadReview.Shared.DTOs.Response;

public record PlatformDto(
    int Id,
    string Name,
    string? Abbr,
    int? Generation,
    string? Summary,
    int? PlatformType, string? PlatformTypeName,
    string? LogoId, int? LogoHeight, int? LogoWidth,
    List<BasicGameDto>? Games
);

/*public record BasicPlatformDto(
    int Id,
    string Name,
    string? Abbr,
    string? LogoId, int? LogoHeight, int? LogoWidth);*/