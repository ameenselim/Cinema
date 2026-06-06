using System.ComponentModel.DataAnnotations;

namespace Cinema_Project.ViewModel
{
    public class ValidOTPVM
    {
        [Required]
        public string OTP { get; set; } = string.Empty;

    }
}
