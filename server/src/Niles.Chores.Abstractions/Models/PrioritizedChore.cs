namespace Niles.Chores
{
    public sealed class PrioritizedChore : Chore
    {
        public int Priority { get; set; }

        /// <summary>
        /// If the chore has an open critical chore, this contains the note associated with it.
        /// </summary>
        public string? Note { get; set; }
    }
}
