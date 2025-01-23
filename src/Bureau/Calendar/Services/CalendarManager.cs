using Bureau.Managers;
using Bureau.Calendar.Models;
using Bureau.Core;
using Bureau.Core.Models;
using Bureau.Handlers;
using Bureau.Providers;

namespace Bureau.Calendar.Services
{
    internal class CalendarManager : IDtoManager<CalendarDto>
    {
        private readonly TimeProvider _timeProvider;
        private readonly IDtoProvider<CalendarDto> _queryManager;
        private readonly ICommandHandler<CalendarDto> _commandHandler;
        public CalendarManager(TimeProvider timeProvider,
            IDtoProvider<CalendarDto> queryManager,
            ICommandHandler<CalendarDto> commandHandler)
        {
            _timeProvider = timeProvider;
            _queryManager = queryManager;
            _commandHandler = commandHandler;
        }

        public Task<Result> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            return _commandHandler.DeleteAsync(id, cancellationToken);
        }

        public async Task<Result<CalendarDto>> InsertAsync(CalendarDto calendarDto, CancellationToken cancellationToken)
        {
            calendarDto.CreatedAt = _timeProvider.GetUtcNow().UtcDateTime;
            calendarDto.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;

            Result<IReference> result = await _commandHandler.InsertAsync(calendarDto, cancellationToken);
            if (result.IsError)
            {
                return result.Error;
            }

            Result<CalendarDto> finalResult = await _queryManager.GetByIdAsync(result.Value.Id, cancellationToken);
            if (finalResult.IsError)
            {
                return finalResult.Error;
            }
            return finalResult.Value;
        }

        public async Task<Result<CalendarDto>> UpdateAsync(CalendarDto calendarDto, CancellationToken cancellationToken)
        {
            calendarDto.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;

            Result<IReference> result = await _commandHandler.UpdateAsync(calendarDto, cancellationToken);
            if (result.IsError)
            {
                return result.Error;
            }

            Result<CalendarDto> finalResult = await _queryManager.GetByIdAsync(result.Value.Id, cancellationToken);
            if (finalResult.IsError)
            {
                return finalResult.Error;
            }
            return finalResult.Value;
        }
    }
}
