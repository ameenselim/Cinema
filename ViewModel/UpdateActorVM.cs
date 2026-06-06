using System.ComponentModel.DataAnnotations;

namespace Cinema_Project.ViewModel
{
    public class UpdateActorVM
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [MinLength(2)]
        public string Name { get; set; } = null!;
        public IFormFile? actorImg { get; set; }
        public string? Image { get; set; }
    }
}
