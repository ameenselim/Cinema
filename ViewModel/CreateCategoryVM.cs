using System.ComponentModel.DataAnnotations;

namespace Cinema_Project.ViewModel
{
    public class CreateCategoryVM
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = null!;
        [Required]
        [StringLength(500, MinimumLength = 10)]
        public String Description { get; set; } = null!;
    }
}
