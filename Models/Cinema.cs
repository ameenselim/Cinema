using Cinema_Project.Models;

namespace CinemaProject.Models
{
    public class Cinema
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public String Description { get; set; } = null!;
        public String Address { get; set; } = null!;


        public String Logo { get; set; } = null!;

        public int RowsCount { get; set; }      // عدد الصفوف
        public int SeatsPerRow { get; set; }    // عدد الكراسي في كل صف
        
        //Relationships
        public ICollection<Movie> Movies { get; set; } = new List<Movie>();
        public ICollection<ShowTime> ShowTimes { get; set; } = new List<ShowTime>();
    }
}
