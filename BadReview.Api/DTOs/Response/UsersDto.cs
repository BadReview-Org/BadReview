namespace BadReview.Api.DTOs.User;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string FullName { get; set; } = null!;
}