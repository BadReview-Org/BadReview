namespace BadReview.Api.Models
{
    public class Review
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GameId { get; set; }
        public int Rating { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReviewText { get; set; } = null!;
        public string StateEnum { get; set; } = null!;
        public bool IsFavorite { get; set; }

        public User User { get; set; } = null!;
        public Game Game { get; set; } = null!;
    }
}
