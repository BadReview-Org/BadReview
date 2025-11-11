using BadReview.Api.Models.Owned;

namespace BadReview.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!; // a-z 0-9 algunos caracteres especiales
    public string Email { get; set; } = null!; // validacion email generica
    public string Password { get; set; } = null!;
    
    public string? FullName { get; set; } = null!; // solo alfabetico y espacios, 100 caracteres max
    public DateTime? Birthday { get; set; } // edad minima de 12 años
    public int? Country { get; set; } // validar iso code
    public CUDate Date { get; set; } = null!;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}