namespace BadReview.Api.Models.Owned;

public class Image
{
    public string ImageId { get; set; } = null!;
    public int? ImageHeight { get; set; }
    public int? ImageWidth { get; set; }

    public Image() { }
    public Image(string Id, int? Height, int? Width)
    {
        ImageId = Id;
        ImageHeight = Height;
        ImageWidth = Width;
    }
}