namespace Niles.Chores.Models
{
    internal class CompletedChoreDb
    {
        public int Id { get; set; }
        public int ChoreId { get; set; }
        public int HousekeepingId { get; set; }
        public HousekeepingDb Housekeeping { get; set; } = null!;
        public ChoreDb Chore { get; set; } = null!;
    }
}
