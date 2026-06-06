namespace CinemaProject.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } =null!;
        public String Description { get; set; } = null!;
        //Relationships
        public ICollection<Movie> Movies { get; set; } = new List<Movie>();


    }
}
