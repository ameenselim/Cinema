using System.ComponentModel.DataAnnotations;

namespace Cinema_Project.ViewModel
{
    public class UpdateUserVM
    {
        public string Id { get; set; } = null!;

        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        public string Role { get; set; } = null!;
    }
}