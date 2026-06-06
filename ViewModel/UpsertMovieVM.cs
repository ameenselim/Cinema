using CinemaProject.Models;

namespace Ecomerce.ViewModels
{
    public class UpsertMovieVM
    {        
        public IEnumerable<Category> Categories { get; set; } = [];
        public IEnumerable<Cinema> Cinemas { get; set; } = [];
        public IEnumerable<MovieSubImage>? MovieSubImgs { get; set; }
        public Movie Movie { get; set; }

    }
}
