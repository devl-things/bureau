namespace Sven.Models
{
    public interface IPasswordResetModel
    {
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }
}
