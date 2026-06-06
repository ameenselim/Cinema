using System.ComponentModel.DataAnnotations;

namespace Cinema_Project.ViewModel
{
    public class ForgetPasswordVM
    {
        [Required]
        public string UserNameOrEmail { get; set; } = string.Empty;
    }
}
