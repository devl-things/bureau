namespace Sven.Models
{
    public class UserVerificationCode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Email { get; set; }
        public string? UserId { get; set; }
        public string VerificationCode { get; set; }
        public VerificationStatus Status { get; set; }
        public DateTimeOffset Expiration { get; set; }

        public bool IsUserKnown => !string.IsNullOrWhiteSpace(UserId);

        public UserVerificationCode(string email, string code, string? userId, VerificationStatus status, DateTimeOffset expiration)
        {
            Email = email;
            VerificationCode = code;
            UserId = userId;
            Status = status;
            Expiration = expiration;
        }
    }

    [Flags]
    public enum VerificationStatus
    {
        None = 0,
        Verified = 1,
        Invalid = 2,
        EmailSent = 4,
    }
}
