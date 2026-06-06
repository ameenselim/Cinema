using System.ComponentModel.DataAnnotations;

namespace Cinema_Project.ViewModel
{
    public class NewPasswordVM
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
