using System.ComponentModel.DataAnnotations;

namespace Cinema_Project.ViewModel
{
    public class ResendEmailConfirmationVM
    {
        public int Id { get; set; }

        [Required]
        public string UserNameOrEmail { get; set; } = string.Empty;
    }
}
