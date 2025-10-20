namespace BadReview.Api.Models
{
    public class Developer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string Logo { get; set; } = null!;

        public ICollection<GameDeveloper> GameDevelopers { get; set; } = new List<GameDeveloper>();
    }
}
