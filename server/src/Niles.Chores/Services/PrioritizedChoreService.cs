namespace Niles.Chores.Services
{
    internal class PrioritizedChoreService : IPrioritizedChoreService
    {
        //private static DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        //private static readonly Dictionary<int, ChoreDto> _chores = new Dictionary<int, ChoreDto>()
        //{
        //    { 1, new ChoreDto { Id = 1, Title = "Cleaning of small bathroom", Description = "Clean the water closet, sink, vacuum the floor,Clean the water closet, sink, vacuum the floor,Clean the water closet, sink, vacuum the floor,Clean the water closet, sink, vacuum the floor,Clean the water closet, sink, vacuum the floor,Clean the water closet, sink, vacuum the floor", IsCompleted = false, Date = today, Priority = 2, Type = "Maintenance" } },
        //    { 2, new ChoreDto { Id = 2, Title = "Cleaning of big bathroom", Description = "Clean the big bathroom", IsCompleted = false, Date = today, Priority = 1, Type = "Maintenance" } },
        //    { 3, new ChoreDto { Id = 3, Title = "Dusting of the whole flat", Description = "Dust the entire flat", IsCompleted = false, Date = today, Priority = 3, Type = "Maintenance" } },
        //    { 4, new ChoreDto { Id = 4, Title = "Vacuuming the whole flat", Description = "Vacuum the entire flat", IsCompleted = false, Date = today, Priority = 2, Type = "Maintenance" } },
        //    { 5, new ChoreDto { Id = 5, Title = "Mopping the whole flat", Description = "Mop the entire flat", IsCompleted = false, Date = today, Priority = 3, Type = "Maintenance" } },
        //    { 6, new ChoreDto { Id = 6, Title = "Cleaning of the kitchen", Description = "Includes cleaning pans, stove, counters, and tidying up", IsCompleted = false, Date = today, Priority = 4, Type = "Maintenance" } },
        //    { 7, new ChoreDto { Id = 7, Title = "Cleaning the microwave", Description = "Clean the microwave", IsCompleted = false, Date = today, Priority = 3, Type = "Extra" } },
        //    { 8, new ChoreDto { Id = 8, Title = "Cleaning the coffee machine", Description = "Maintain the coffee machine", IsCompleted = false, Date = today, Priority = 5, Type = "Maintenance" } },
        //    { 9, new ChoreDto { Id = 9, Title = "Cleaning the robot vacuum", Description = "Clean the robotic vacuum", IsCompleted = false, Date = today, Priority = 3, Type = "Maintenance" } },
        //    { 10, new ChoreDto { Id = 10, Title = "Changing the bed sheets", Description = "Change the bed sheets", IsCompleted = false, Date = today, Priority = 3, Type = "Maintenance" } }
        //};

        public Task<List<PrioritizedChore>> GetPrioritizedChoresAsync(DateOnly date, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
