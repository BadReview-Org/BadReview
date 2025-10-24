namespace BadReview.Api.DTOs
{
    public class GameWithReviewsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Cover { get; set; } = null!;
        public DateTime Date { get; set; }
        public string Summary { get; set; } = null!;
        public double RatingIGDB { get; set; }
        public double RatingBadReview { get; set; }
        public string Video { get; set; } = null!;
        public List<ReviewDto> Reviews { get; set; } = new();
        public List<GenreDto> Genres { get; set; } = new();
        public List<DeveloperDto> Developers { get; set; } = new();
        public List<PlatformDto> Platforms { get; set; } = new();
    }

    public class ReviewDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReviewText { get; set; } = null!;
        public string StateEnum { get; set; } = null!;
        public bool IsFavorite { get; set; }
        public UserDto User { get; set; } = null!;
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
    }

    public class GenreDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class DeveloperDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }

    public class PlatformDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
