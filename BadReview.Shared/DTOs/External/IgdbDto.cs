using System.Collections;
using System.Text.Json;
using BadReview.Shared.DTOs.Response;

namespace BadReview.Shared.DTOs.External;

[AttributeUsage(AttributeTargets.Class)]
public class IgdbFieldsAttribute : Attribute
{
    public string Fields { get; }
    public IgdbFieldsAttribute(string fields) => Fields = fields;
}

public record GetCredentialsDto(string access_token, int expires_in, string token_type);


public record ImageIgdbDto(string? Image_Id, int? Height, int? Width);
public record VideoIgdbDto(string? Video_Id);


[IgdbFields("id, name")]
public record GenreIgdbDto(int Id, string Name);


public record PlatformTypeIgdbDto(int Id, string Name);

[IgdbFields(
    @"id, name, abbreviation, generation, summary,
    platform_type.id, platform_type.name,
    platform_logo.image_id, platform_logo.height, platform_logo.width")]
public record PlatformIgdbDto(
    int Id, string Name, string? Abbreviation, int? Generation,
    PlatformTypeIgdbDto? Platform_type, ImageIgdbDto? Platform_logo, string? Summary
);


public record InvCompIgdbDto(int Id, DetailCompanyIgdbDto Company, bool Developer);

[IgdbFields("name, country, logo.image_id, logo.height, logo.width")]
public record BasicCompanyIgdbDto(int Id, string Name, int? Country, ImageIgdbDto? Logo);

[IgdbFields("name, country, logo.image_id, logo.height, logo.width, description, start_date")]
public record DetailCompanyIgdbDto(
    int Id, string Name, int? Country, ImageIgdbDto? Logo, string? Description, long? Start_date, List<BasicGameDto>? Games);


[IgdbFields("id, name, cover.image_id, cover.height, cover.width, rating")]
public record BasicGameIgdbDto(int Id, string Name, ImageIgdbDto? Cover, double? Rating);

[IgdbFields(
    @"name, cover.image_id, cover.height, cover.width, first_release_date, summary, rating, videos.video_id,
    genres.name,
    platforms.abbreviation, platforms.name, platforms.generation, platforms.summary,
    platforms.platform_type.id, platforms.platform_type.name,
    platforms.platform_logo.image_id, platforms.platform_logo.height, platforms.platform_logo.width,
    involved_companies.developer,
    involved_companies.company.name, involved_companies.company.country,
    involved_companies.company.description, involved_companies.company.start_date,
    involved_companies.company.logo.image_id, involved_companies.company.logo.height, involved_companies.company.logo.width")]
public record DetailGameIgdbDto(
    int Id, string Name, ImageIgdbDto? Cover, long? First_release_date, string? Summary, double? Rating, List<VideoIgdbDto>? Videos,
    HashSet<GenreIgdbDto>? Genres, HashSet<PlatformIgdbDto>? Platforms, HashSet<InvCompIgdbDto>? Involved_Companies);

[IgdbFields("game_id")]
public record PopularIgdbDto(int Game_id);

public record IgdbResponse(string Name, JsonElement? Result, int? Count);