using Bureau.Core;
using Bureau.Google.Calendar.Models;
using System.Security.Claims;

namespace Bureau.Google.Calendar.Managers
{
    public interface ICalendarManager
    {
        public Task<Result<List<CalendarListModel>>> GetUserCalendarListAsync(IUserId userId, CancellationToken cancellationToken = default);
    }
}
