using Cinema_Project.Models;
using CinemaProject.Models;

namespace Cinema_Project.ViewModel
{
    public class MovieDetailsVM
    {
        public Movie Movie { get; set; } = null!;
        public List<ShowTime> ShowTimes { get; set; } = [];

    }
}
