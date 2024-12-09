using Bureau.Core;
using Bureau.Cache;
using Bureau.Google.Calendar.Models;
using Bureau.Identity.Models;

namespace Bureau.Google.Calendar.Services.Cache
{
    internal class CalendarListServiceCache : ICalendarListService
    {
        ICalendarListService _service;
        ICache _cache;
        public CalendarListServiceCache(ICalendarListService service, ICache cache)
        {
            _service = service;
            _cache = cache;
        }
        public async Task<Result<CalendarListModel>> GetCalendarListModelAsync(BureauUserLoginModel loginInfo, CancellationToken cancellationToken = default)
        {
            string cacheKey = _cache.CreateCacheKey<BureauUserLoginModel>(loginInfo);
            if (_cache.TryGetValue<CalendarListModel>(cacheKey, out CalendarListModel cachedValue)) 
            {
                return cachedValue;
            }
            
            Result<CalendarListModel> result = await _service.GetCalendarListModelAsync(loginInfo, cancellationToken);

            if (result.IsSuccess)
            {
                _cache.SetValue<CalendarListModel>(cacheKey, result.Value);
            }

            return result;
        }
    }
}
