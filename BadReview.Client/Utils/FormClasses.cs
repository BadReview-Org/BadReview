namespace BadReview.Client.Utils;

public class LoginForm
{
    public string? Username { get; set; } = null!;
    public string? Password { get; set; } = null!;
}

public class RegisterForm
{
    public string? Username { get; set; } = null!;
    public string? Password { get; set; } = null!;
    public string? RepeatPassword { get; set; } = null!;
    public string? FullName { get; set; } = null!;
    public string? Email { get; set; } = null!;
    public DateTime? Birthday { get; set; } = null!;
    public int? Country { get; set; } = null!;
}