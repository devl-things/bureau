using Bureau.Core;
using Bureau.Google.Calendar.Models;
using Bureau.Identity.Models;

namespace Bureau.Google.Calendar.Services
{
    public interface ICalendarListService
    {
        public Task<Result<CalendarListModel>> GetCalendarListModelAsync(BureauUserLoginModel loginInfo, CancellationToken cancellationToken = default);
    }
}
