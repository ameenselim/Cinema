namespace Cinema_Project.Models
{
    public class ApplicationUserOTP
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } =string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public string OTP { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpireAt { get; set; } = DateTime.UtcNow.AddMinutes(10);
        public bool IsUsed { get; set; } = false;

    }
}
