using Bureau.Calendar.Models;
using Bureau.Core;
using Bureau.Core.Factories;
using Bureau.Core.Models;
using Bureau.Core.Models.Data;
using Bureau.Factories;

namespace Bureau.Calendar.Factories
{
    internal class CalendarDtoFactory : IDtoFactory<CalendarDto>
    {
        public Result<CalendarDto> Create(InsertAggregateModel aggregate)
        {
            if (aggregate.Edges.Count != 1)
            {
                return ResultErrorFactory.UnexpectedNumberOrEdges(1, aggregate.Edges.Count);
            }
            CalendarFactory factory = new CalendarFactory(aggregate);

            Result<Edge> edgeResult = factory.GetEdgeById(aggregate.MainReference.Id, nameof(CalendarDto));
            if (edgeResult.IsError)
            {
                return edgeResult.Error;
            }
            return factory.CreateCalendar(edgeResult.Value);
        }

        public PaginatedResult<List<CalendarDto>> CreatePaged(QueryAggregateModel aggregate)
        {
            List<CalendarDto> result = new List<CalendarDto>();
            CalendarFactory factory = new CalendarFactory(aggregate);
            foreach (Edge edge in aggregate.Edges)
            {
                Result<CalendarDto> calendarResult = factory.CreateCalendar(edge);
                if (calendarResult.IsError)
                {
                    return calendarResult.Error;
                }
                result.Add(calendarResult.Value);
            }
            return new PaginatedResult<List<CalendarDto>>(result, aggregate.Pagination);
        }
    }
}
