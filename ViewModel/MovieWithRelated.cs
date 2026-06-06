using CinemaProject.Models;

namespace Cinema_Project.ViewModel
{
    public class MovieWithRelated
    {
        public string? Query { get; set; } 
        public int TotalPage { get; set; }
        public int CurrentPage { get; set; }
        public IEnumerable<Movie> movies { get; set; } = [];
        //public IEnumerable<Category> categories { get; set; } = [];
        //public IEnumerable<Cinema> Cinemas { get; set; } = [];


    }
}
