using System.ComponentModel.DataAnnotations;

namespace Cinema_Project.ViewModel
{
    public class CreateActorVM
    {

        [Required]
        [MaxLength(50)]
        [MinLength(2)]
        public string Name { get; set; } = null!;
        [Required]
        public IFormFile actorImg { get; set; } = null!;
        public string? Image { get; set; }
    }
}
