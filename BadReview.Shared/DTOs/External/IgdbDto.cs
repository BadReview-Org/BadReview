using System.Collections;

namespace BadReview.Shared.DTOs.External;

[AttributeUsage(AttributeTargets.Class)]
public class IgdbFieldsAttribute : Attribute
{
    public string Fields { get; }
    public IgdbFieldsAttribute(string fields) => Fields = fields;
}

public record GetCredentialsDto(string access_token, int expires_in, string token_type);

public record ImageIgdbDto(string? Image_Id);
public record VideoIgdbDto(string? Video_Id);

public record GenreIgdbDto(int Id, string Name);
public record PlatformIgdbDto(int Id, string Name, string? Abbreviation, int? Generation, ImageIgdbDto? Platform_logo);
public record InvCompIgdbDto(int Id, CompanyIgdbDto Company, bool Developer);
public record CompanyIgdbDto(int Id, string Name, int? Country, ImageIgdbDto? Logo);

[IgdbFields("id, name, cover.image_id")]
public record BasicGameIgdbDto(int Id, string Name, ImageIgdbDto? Cover);

public record PopularIgdbDto(int Id, int game_id);

[IgdbFields(
    @"name, cover.image_id, first_release_date, summary, rating, videos.video_id,
    genres.name,
    platforms.abbreviation, platforms.name, platforms.generation, platforms.platform_logo.image_id,
    involved_companies.developer,
    involved_companies.company.name, involved_companies.company.country, involved_companies.company.logo.image_id")]
public record DetailGameIgdbDto(
    int Id, string Name, ImageIgdbDto? Cover, long? First_release_date, string? Summary, double? Rating, List<VideoIgdbDto>? Videos,
    HashSet<GenreIgdbDto>? Genres, HashSet<PlatformIgdbDto>? Platforms, HashSet<InvCompIgdbDto>? Involved_Companies);