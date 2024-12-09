using Bureau.Core;
using Bureau.Google.Calendar.Services;
using Bureau.Google.Calendar.Models;
using Bureau.Identity.Managers;
using Bureau.Identity.Models;
using System.Security.Claims;

namespace Bureau.Google.Calendar.Managers
{
    internal class CalendarManager : ICalendarManager
    {
        private readonly IUserManager _userManager;
        private readonly ICalendarListService _calendarListService;

        public CalendarManager(IUserManager userManager, ICalendarListService calendarListService)
        {
            _userManager = userManager;
            _calendarListService = calendarListService;
        }
        public async Task<Result<List<CalendarListModel>>> GetUserCalendarListAsync(IUserId userId, CancellationToken cancellationToken = default)
        {
            if (userId == null)
            {
                return new ArgumentNullException(paramName: nameof(userId));
            }

            Result<List<BureauUserLoginModel>> userLoginsResult = await _userManager.GetUserLoginsAsync(userId, cancellationToken);
            if (userLoginsResult.IsError)
            {
                return userLoginsResult.Error;
            }

            List<BureauUserLoginModel> userLogins = userLoginsResult.Value ?? new List<BureauUserLoginModel>();

            List<Task<Result<CalendarListModel>>> tasks = new List<Task<Result<CalendarListModel>>>(userLogins.Count);
            foreach (BureauUserLoginModel loginInfo in userLogins)
            {
                tasks.Add(_calendarListService.GetCalendarListModelAsync(loginInfo));
            }

            AggregateResultFactory<CalendarListModel> resultFactory = new AggregateResultFactory<CalendarListModel>();
            while (tasks.Count > 0)
            {
                var completedTask = await Task.WhenAny(tasks);
                tasks.Remove(completedTask);
                resultFactory.Add(await completedTask);
            }

            return resultFactory.Result;
        }
    }
}
