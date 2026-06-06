using System.ComponentModel.DataAnnotations;

namespace Cinema_Project.ViewModel
{
    public class CreateRoleVM
    {
        [Required]
        [MaxLength(50)]
        [MinLength(3)]
        public string Name { get; set; } = null!;

    }
}
