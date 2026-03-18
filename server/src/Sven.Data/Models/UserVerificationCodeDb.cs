namespace Sven.Data.Models
{
    internal class UserVerificationCodeDb
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public int Status { get; set; }
        public DateTimeOffset Expiration { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
