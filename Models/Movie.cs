using Cinema_Project.Models;

namespace CinemaProject.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public String Description { get; set; } = null!;
        public decimal Price { get; set; }
        public String MainImg { get; set; } = null!;
        public DateTime Date { get; set; }
        public String Status { get; set; } = null!;

        //Relationships
        public Category Category { get; set; } =null!;
        public int CategoryId { get; set; }
        public Cinema Cinema { get; set; } = null!;
        public int CinemaId { get; set; }

        public ICollection<Actor> Actors { get; set; } = new List<Actor>();
        public ICollection<MovieSubImage> MovieSubImages { get; set; } = new List<MovieSubImage>();
        public ICollection<ShowTime> ShowTimes { get; set; } = new List<ShowTime>();
    }
}
