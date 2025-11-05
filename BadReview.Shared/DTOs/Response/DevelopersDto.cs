namespace BadReview.Shared.DTOs.Response;

// Logo, LogoHeight, LogoWidth could be replaced by a ImageDto
public record DetailDeveloperDto(
    int Id,
    string Name,
    int? Country,
    string? Description,
    long? StartDate,
    string? LogoId, int? LogoHeight, int? LogoWidth,
    List<BasicGameDto>? Games
);

public record BasicDeveloperDto(int Id, string Name, int? Country, string? LogoId, int? LogoHeight, int? LogoWidth);