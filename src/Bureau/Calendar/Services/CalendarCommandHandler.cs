using Bureau.Calendar.Models;
using Bureau.Core;
using Bureau.Core.Models;
using Bureau.Handlers;

namespace Bureau.Calendar.Services
{
    internal class CalendarCommandHandler : ICommandHandler<CalendarDto>
    {
        public Task<Result> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IReference>> InsertAsync(CalendarDto calendarDto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<Result<IReference>> UpdateAsync(CalendarDto calendarDto, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
