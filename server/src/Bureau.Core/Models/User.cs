namespace Bureau.Core
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string SubjectId { get; set; } = Guid.NewGuid().ToString("N");
    }
}
