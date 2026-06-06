namespace CinemaProject.Models
{
    public class MovieSubImage
    {
        public int Id { get; set; }
        public string SubImg { get; set; } = null!;
        //Relationships
        public Movie Movie { get; set; } = null!;
        public int MovieId { get; set; }

    }
}
