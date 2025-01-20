using Bureau.Calendar.Models;
using Bureau.Core;
using Bureau.Core.Models.Data;
using Bureau.Factories;

namespace Bureau.Calendar.Factories
{
    internal class CalendarDtoFactory : IDtoFactory<CalendarDto>
    {
        public Result<CalendarDto> Create(InsertAggregateModel aggregate)
        {
            throw new NotImplementedException();
        }

        public PaginatedResult<List<CalendarDto>> CreatePaged(QueryAggregateModel value)
        {
            throw new NotImplementedException();
        }
    }
}
