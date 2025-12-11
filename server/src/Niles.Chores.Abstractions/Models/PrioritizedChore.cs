namespace Niles.Chores
{
    public sealed class PrioritizedChore : Chore
    {
        public ChoreCriticality Criticality { get; set; }
        public int Priority { get; set; }
    }
}
