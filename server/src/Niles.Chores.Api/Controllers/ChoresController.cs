using Microsoft.AspNetCore.Mvc;
using Niles.Chores.Api.Dtos;

namespace Niles.Chores.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChoresController : ControllerBase
    {
        private static DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        private static readonly Dictionary<int, Chore> _chores = new Dictionary<int, Chore>()
        {
            { 1, new Chore { Id = 1, Title = "Cleaning of small bathroom", Description = "Clean the water closet, sink, vacuum the floor,Clean the water closet, sink, vacuum the floor,Clean the water closet, sink, vacuum the floor,Clean the water closet, sink, vacuum the floor,Clean the water closet, sink, vacuum the floor,Clean the water closet, sink, vacuum the floor", IsCompleted = false, Date = today, Priority = 2, Type = "Maintenance" } },
            { 2, new Chore { Id = 2, Title = "Cleaning of big bathroom", Description = "Clean the big bathroom", IsCompleted = false, Date = today, Priority = 1, Type = "Maintenance" } },
            { 3, new Chore { Id = 3, Title = "Dusting of the whole flat", Description = "Dust the entire flat", IsCompleted = false, Date = today, Priority = 3, Type = "Maintenance" } },
            { 4, new Chore { Id = 4, Title = "Vacuuming the whole flat", Description = "Vacuum the entire flat", IsCompleted = false, Date = today, Priority = 2, Type = "Maintenance" } },
            { 5, new Chore { Id = 5, Title = "Mopping the whole flat", Description = "Mop the entire flat", IsCompleted = false, Date = today, Priority = 3, Type = "Maintenance" } },
            { 6, new Chore { Id = 6, Title = "Cleaning of the kitchen", Description = "Includes cleaning pans, stove, counters, and tidying up", IsCompleted = false, Date = today, Priority = 4, Type = "Maintenance" } },
            { 7, new Chore { Id = 7, Title = "Cleaning the microwave", Description = "Clean the microwave", IsCompleted = false, Date = today, Priority = 3, Type = "Extra" } },
            { 8, new Chore { Id = 8, Title = "Cleaning the coffee machine", Description = "Maintain the coffee machine", IsCompleted = false, Date = today, Priority = 5, Type = "Maintenance" } },
            { 9, new Chore { Id = 9, Title = "Cleaning the robot vacuum", Description = "Clean the robotic vacuum", IsCompleted = false, Date = today, Priority = 3, Type = "Maintenance" } },
            { 10, new Chore { Id = 10, Title = "Changing the bed sheets", Description = "Change the bed sheets", IsCompleted = false, Date = today, Priority = 3, Type = "Maintenance" } }
        };

        // GET /api/chores?date=2025-11-27
        [HttpGet]
        public ActionResult<IEnumerable<Chore>> GetChores([FromQuery] DateOnly? date)
        {
            IEnumerable<Chore> result = _chores.Values;

            if (date.HasValue)
            {
                result = result.Where(c => c.Date == date.Value);
            }

            return Ok(result);
        }

        // PATCH /api/chores/{id}
        [HttpPatch("{id:int}")]
        public ActionResult<Chore> PatchChore(int id, [FromBody] ChorePatchDto patch)
        {
            if (_chores.TryGetValue(id, out Chore? chore))
            {
                chore.Date = patch.Date;
                chore.IsCompleted = patch.IsCompleted;

                return Ok(chore);
            }
            return NotFound();
        }
    }
}
