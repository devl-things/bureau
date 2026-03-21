namespace Sven.Models
{
    public class PasswordResetNotification
    {
        public string Email { get; set; } = string.Empty;
        public string ResetLink { get; set; }
    }
}
