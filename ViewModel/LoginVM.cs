using System.ComponentModel.DataAnnotations;

namespace Cinema_Project.ViewModel
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Email or Username is required.")]
        public string EmailOrUserName { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
