using Microsoft.AspNetCore.Components;
using Bureau.Google.Calendar.Managers;
using Bureau.Google.Calendar.Models;
using Bureau.Core;
using Bureau.Identity.Providers;
using Bureau.Identity.Models;

namespace Bureau.Google.Calendar.UI.Components.Settings
{
    public partial class UserCalendarSettings
    {
        private string _message = "frrr";
        private List<CalendarListModel> _calendars = new List<CalendarListModel>();

        [Inject]
        private ICalendarManager? _calendarManager { get; set; }

        [Inject]
        private IIdentityProvider _identityProvider { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
        }

        private async Task DoMagic()
        {
            try
            {
                Result<BureauUser> userResult = await _identityProvider.GetUserAsync();
                if (userResult.IsError) 
                {
                    //TODO [error handling]
                }
                _message = userResult.Value.Email;
                Result<List<CalendarListModel>> calendarListResult = await _calendarManager!.GetUserCalendarListAsync(userResult.Value);
                if (calendarListResult.IsError)
                {
                    _message = calendarListResult.Error.ToString();
                }
                else
                {
                    _calendars = calendarListResult.Value;
                }
            }
            catch (Exception ex)
            {
                _message = ex.ToString();
            }
        }
    }
}
