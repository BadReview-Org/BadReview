namespace BadReview.Api.DTOs.External;

[AttributeUsage(AttributeTargets.Class)]
public class IgdbFieldsAttribute : Attribute
{
    public string Fields { get; }
    public IgdbFieldsAttribute(string fields) => Fields = fields;
}

public record CoverDto(string? Url);
public record VideoDto(string? Video_Id);

[IgdbFields("id, name, cover.url")]
public record BasicGameIgdbDto(int Id, string Name, CoverDto? Cover);

[IgdbFields("id, name, cover.url, video.video_id")]
public record DetailGameIgdbDto(int Id, string Name, CoverDto? Cover, VideoDto? Video);