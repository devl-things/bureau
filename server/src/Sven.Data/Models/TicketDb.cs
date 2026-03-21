namespace Sven.Data.Models
{
    internal class TicketDb
    {
        public int Id { get; set; }
        public string Ticket { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
