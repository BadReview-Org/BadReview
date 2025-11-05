namespace BadReview.Shared.DTOs.Response;

public record DetailPlatformDto(
    int Id,
    string Name,
    string? Abbr,
    int? Generation,
    string? Summary,
    string? LogoId, int? LogoHeight, int? LogoWidth,
    List<BasicGameDto>? Games
);

public record BasicPlatformDto(
    int Id,
    string Name,
    string? Abbr,
    string? LogoId, int? LogoHeight, int? LogoWidth);