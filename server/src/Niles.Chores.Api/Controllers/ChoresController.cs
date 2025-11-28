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
        private readonly IChoreService _choreService;
        private static readonly string[] AcceptedDateFormats = new[] { "yyyy-MM-dd", "yyyy/MM/dd" };

        public ChoresController(
            IPrioritizedChoreService prioritizedChoreService, 
            IHousekeepingService housekeepingService,
            IChoreService choreService)
        {
            _prioritizedChoreService = prioritizedChoreService;
            _housekeepingService = housekeepingService;
            _choreService = choreService;
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

        // GET: api/chores/all - Get all chores (for admin)
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<ChoreDto>>> GetAllChores(CancellationToken cancellationToken = default)
        {
            IEnumerable<Chore> chores = await _choreService.ListChoresAsync(cancellationToken);
            
            var dtos = chores.Select(chore => new ChoreDto
            {
                Id = chore.Id,
                Title = chore.Title,
                Description = chore.Description ?? string.Empty,
                Type = chore.Type.ToString(),
                Priority = 0, // Regular chores don't have priority
                IsCompleted = false,
                WeeklyInterval = chore.WeeklyInterval,
                IsImportant = chore.IsImportant,
                ImportantReminderDate = chore.ImportantReminderDate,
                ImportantNotes = chore.ImportantNotes
            });

            return Ok(dtos);
        }

        // GET: api/chores/{id} - Get single chore
        [HttpGet("{id}")]
        public async Task<ActionResult<ChoreDto>> GetChore(int id, CancellationToken cancellationToken = default)
        {
            Chore? chore = await _choreService.GetChoreAsync(id, cancellationToken);
            if (chore == null)
            {
                return NotFound();
            }

            var dto = new ChoreDto
            {
                Id = chore.Id,
                Title = chore.Title,
                Description = chore.Description ?? string.Empty,
                Type = chore.Type.ToString(),
                Priority = 0,
                IsCompleted = false,
                WeeklyInterval = chore.WeeklyInterval,
                IsImportant = chore.IsImportant,
                ImportantReminderDate = chore.ImportantReminderDate,
                ImportantNotes = chore.ImportantNotes
            };

            return Ok(dto);
        }

        // POST: api/chores - Create new chore
        [HttpPost]
        public async Task<ActionResult<ChoreDto>> CreateChore([FromBody] ChoreCreateDto dto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!Enum.TryParse<ChoreType>(dto.Type, ignoreCase: true, out ChoreType choreType))
            {
                choreType = ChoreType.Maintenance; // Default
            }

            var chore = new Chore
            {
                Title = dto.Title,
                Description = dto.Description,
                Type = choreType,
                WeeklyInterval = dto.WeeklyInterval ?? 1
            };

            try
            {
                Chore created = await _choreService.CreateChoreAsync(chore, cancellationToken);
                
                var responseDto = new ChoreDto
                {
                    Id = created.Id,
                    Title = created.Title,
                    Description = created.Description ?? string.Empty,
                    Type = created.Type.ToString(),
                    Priority = 0,
                    IsCompleted = false,
                    WeeklyInterval = created.WeeklyInterval,
                    IsImportant = created.IsImportant,
                    ImportantReminderDate = created.ImportantReminderDate,
                    ImportantNotes = created.ImportantNotes
                };

                return CreatedAtAction(nameof(GetChore), new { id = created.Id }, responseDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // PATCH: api/chores/{id} - Update chore
        [HttpPatch("{id}")]
        public async Task<ActionResult<ChoreDto>> UpdateChore(int id, [FromBody] ChoreUpdateDto dto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Chore? existing = await _choreService.GetChoreAsync(id, cancellationToken);
            if (existing == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(dto.Type) && Enum.TryParse<ChoreType>(dto.Type, ignoreCase: true, out ChoreType choreType))
            {
                existing.Type = choreType;
            }

            existing.Title = dto.Title;
            existing.Description = dto.Description;
            if (dto.WeeklyInterval.HasValue)
            {
                existing.WeeklyInterval = dto.WeeklyInterval.Value;
            }

            Chore? updated = await _choreService.UpdateChoreAsync(existing, cancellationToken);
            if (updated == null)
            {
                return NotFound();
            }

            var responseDto = new ChoreDto
            {
                Id = updated.Id,
                Title = updated.Title,
                Description = updated.Description ?? string.Empty,
                Type = updated.Type.ToString(),
                Priority = 0,
                IsCompleted = false,
                WeeklyInterval = updated.WeeklyInterval,
                IsImportant = updated.IsImportant,
                ImportantReminderDate = updated.ImportantReminderDate,
                ImportantNotes = updated.ImportantNotes
            };

            return Ok(responseDto);
        }

        // DELETE: api/chores/{id} - Delete chore
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChore(int id, CancellationToken cancellationToken = default)
        {
            bool deleted = await _choreService.DeleteChoreAsync(id, cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/chores/{id}/important - Mark chore as important
        [HttpPost("{id}/important")]
        public async Task<ActionResult<ChoreDto>> MarkImportant(int id, [FromBody] ChoreImportantDto dto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Chore? chore = await _choreService.GetChoreAsync(id, cancellationToken);
            if (chore == null)
            {
                return NotFound();
            }

            if (!DateTime.TryParse(dto.Date, out DateTime reminderDate))
            {
                return BadRequest(new { error = "Invalid date format." });
            }

            chore.IsImportant = true;
            chore.ImportantReminderDate = reminderDate;
            chore.ImportantNotes = dto.Description;

            Chore? updated = await _choreService.UpdateChoreAsync(chore, cancellationToken);
            if (updated == null)
            {
                return NotFound();
            }

            var responseDto = new ChoreDto
            {
                Id = updated.Id,
                Title = updated.Title,
                Description = updated.Description ?? string.Empty,
                Type = updated.Type.ToString(),
                Priority = 0,
                IsCompleted = false,
                WeeklyInterval = updated.WeeklyInterval,
                IsImportant = updated.IsImportant,
                ImportantReminderDate = updated.ImportantReminderDate,
                ImportantNotes = updated.ImportantNotes
            };

            return Ok(responseDto);
        }

        // DELETE: api/chores/{id}/important - Remove important flag from chore
        [HttpDelete("{id}/important")]
        public async Task<ActionResult<ChoreDto>> RemoveImportant(int id, CancellationToken cancellationToken = default)
        {
            Chore? chore = await _choreService.GetChoreAsync(id, cancellationToken);
            if (chore == null)
            {
                return NotFound();
            }

            chore.IsImportant = false;
            chore.ImportantReminderDate = null;
            chore.ImportantNotes = null;

            Chore? updated = await _choreService.UpdateChoreAsync(chore, cancellationToken);
            if (updated == null)
            {
                return NotFound();
            }

            var responseDto = new ChoreDto
            {
                Id = updated.Id,
                Title = updated.Title,
                Description = updated.Description ?? string.Empty,
                Type = updated.Type.ToString(),
                Priority = 0,
                IsCompleted = false,
                WeeklyInterval = updated.WeeklyInterval,
                IsImportant = updated.IsImportant,
                ImportantReminderDate = updated.ImportantReminderDate,
                ImportantNotes = updated.ImportantNotes
            };

            return Ok(responseDto);
        }
    }
}
