namespace Sven.Data.Models
{
    internal class HouseholdMemberDb
    {
        public int Id { get; set; }
        public string HouseholdIdentifier { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        /// <summary>"owner" or "member"</summary>
        public string Role { get; set; } = string.Empty;
        public DateTimeOffset JoinedAt { get; set; }
    }
}
