namespace Sven.Models
{
    public class UserVerificationCode
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Email { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
        public VerificationStatus Status { get; set; }
        public DateTimeOffset Expiration { get; set; }

        public UserVerificationCode(string email, string code, VerificationStatus status, DateTimeOffset expiration)
        {
            Email = email;
            VerificationCode = code;
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
