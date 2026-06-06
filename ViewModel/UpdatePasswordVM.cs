using System.ComponentModel.DataAnnotations;

namespace Cinema_Project.ViewModel
{
    public class UpdatePasswordVM
    {
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = null!;

        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = null!;

        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
