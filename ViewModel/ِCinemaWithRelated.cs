using CinemaProject.Models;

namespace Cinema_Project.ViewModel
{
    public class CinemaWithRelated
    {
        public string? Query { get; set; } 
        public int TotalPage { get; set; }
        public int CurrentPage { get; set; }
        public IEnumerable<Cinema> cinemas { get; set; } = [];

    }
}
