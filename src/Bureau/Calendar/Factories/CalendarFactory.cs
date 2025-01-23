using Bureau.Calendar.Models;
using Bureau.Core;
using Bureau.Core.Models;
using Bureau.Core.Models.Data;
using Bureau.Factories;
using Bureau.Models;

namespace Bureau.Calendar.Factories
{
    internal class CalendarFactory : BaseFactory 
    {
        public CalendarFactory(QueryAggregateModel aggregate) : base(aggregate)
        {
        }

        public Result<CalendarDto> CreateCalendar(Edge edge)
        {
            Result<TermEntry> termResult = GetTermById(edge.TargetNode.Id, $"{nameof(CalendarDto)}.{nameof(CalendarDto.Name)}");
            if (termResult.IsError)
            {
                return termResult.Error;
            }
            CalendarDto calendar = CalendarDto.EmptyCalendar();
            calendar.Id = edge.Id;
            calendar.CreatedAt = edge.CreatedAt;
            calendar.UpdatedAt = edge.UpdatedAt;
            calendar.Name = termResult.Value.Title;

            if (TryGetFlexibleData(edge.Id, out NoteDetails data))
            {
                calendar.Description = data.Note;
            }

            return calendar;
        }
    }
}
