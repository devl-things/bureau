using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Niles.Chores;
using Niles.Chores.Abstractions.Services;
using Niles.Chores.Api.Dtos;

namespace Niles.Chores.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChoresController : ControllerBase
    {
        private readonly IPrioritizedChoreService _prioritizedChoreService;
        private readonly IHousekeepingService _housekeepingService;
        private static readonly string[] AcceptedDateFormats = new[] { "yyyy-MM-dd", "yyyy/MM/dd" };

        public ChoresController(IPrioritizedChoreService prioritizedChoreService, IHousekeepingService housekeepingService)
        {
            _prioritizedChoreService = prioritizedChoreService;
            _housekeepingService = housekeepingService;
        }

        // GET: api/chores?date=2025-11-28
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChoreDto>>> Get([FromQuery] DateOnly? date, CancellationToken cancellationToken = default)
        {
            DateOnly targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
            List<PrioritizedChore> chores = await _prioritizedChoreService.GetPrioritizedChoresAsync(targetDate, cancellationToken);

            IEnumerable<ChoreDto> response = chores.Select(chore => new ChoreDto
            {
                Id = chore.Id,
                Title = chore.Title,
                Description = chore.Description ?? string.Empty,
                Priority = chore.Priority,
                Type = chore.Type.ToString(),
                IsCompleted = false,
                Date = targetDate
            });

            return Ok(response);
        }

        // POST: api/chores/submit
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitAsync([FromBody] ChoreSubmissionDto dto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!DateOnly.TryParseExact(dto.Date, AcceptedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dateOnly))
            {
                return BadRequest(new { error = "Invalid date format." });
            }

            List<int> completedIds = dto.CompletedChoreIds?
                .Select(id => int.TryParse(id, out int parsed) ? parsed : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList() ?? new List<int>();

            if (completedIds.Count == 0)
            {
                return BadRequest(new { error = "At least one valid chore id is required." });
            }

            TimeSpan duration = dto.DurationMinutes.HasValue && dto.DurationMinutes > 0
                ? TimeSpan.FromMinutes(dto.DurationMinutes.Value)
                : TimeSpan.Zero;

            DateTime completedDate = DateTime.SpecifyKind(dateOnly.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

            var model = new Housekeeping
            {
                DateTime = new DateTimeOffset(completedDate),
                Duration = duration,
                Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim(),
                CompletedChoreIds = completedIds
            };

            bool stored = await _housekeepingService.CreateHousekeepingAsync(model, cancellationToken);
            if (!stored)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to persist housekeeping entry." });
            }

            return Ok(new { status = "stored" });
        }
    }
}
