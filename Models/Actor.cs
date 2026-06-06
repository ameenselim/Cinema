using Cinema_Project.Models;

namespace CinemaProject.Models
{
    public class Actor
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;        
        public String Image { get; set; } = null!;
        //Relationships   
            
        public ICollection<Movie> Movies { get; set; } = new List<Movie>();

    }
}
