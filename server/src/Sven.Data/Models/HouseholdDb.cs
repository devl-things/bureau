namespace Sven.Data.Models
{
    internal class HouseholdDb
    {
        public int Id { get; set; }
        public string Identifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
