using Bureau.Core;
using Bureau.Core.Extensions;
using Bureau.Google.Abstractions;
using Bureau.Google.Calendar.Data;
using Bureau.Google.Calendar.Mappers;
using Bureau.Google.Calendar.Models;
using Bureau.Identity.Factories;
using Bureau.Identity.Models;
using System.Net.Http.Json;

namespace Bureau.Google.Calendar.Services
{
    internal class CalendarListService : ICalendarListService
    {
        private readonly HttpClient _client;

        public CalendarListService(IHttpClientFactory clientFactory)
        {
            _client = clientFactory.CreateClient(GoogleHttpClientNames.GoogleApiClientName);
        }

        public async Task<Result<CalendarListModel>> GetCalendarListModelAsync(BureauUserLoginModel loginInfo, CancellationToken cancellationToken = default)
        {
            HttpRequestMessage request = HttpRequestMessageFactory.CreateRequestWithBureauUserLogin(HttpMethod.Get, GoogleCalendarRelativeUris.CalendarListRelativeUri, loginInfo);

            HttpResponseMessage response = await _client.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return ErrorMessages.BureauUnsuccessfulHttpResponse.Format("Get calendar list", response.StatusCode, response.ReasonPhrase, await response.Content.ReadAsStringAsync());
            }

            CalendarList? result = await response.Content.ReadFromJsonAsync<CalendarList>(cancellationToken);

            if (result == null) 
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(CalendarListEntry));
            }

            CalendarListModel model = result.ToModel();

            //TODO see what to do and how with mappers
            model.LoginEmail = loginInfo.Email ?? "UNKNOWN";

            return model;
        }
    }
}
