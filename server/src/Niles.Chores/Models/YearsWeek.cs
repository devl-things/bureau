using System.Globalization;

namespace Niles.Chores.Models
{
    internal class YearsWeek
    {
        public int Year { get; set; }
        public int Week { get; set; }

        public YearsWeek(DateOnly dateOnly)
        {
            Year = dateOnly.Year;
            Week = ISOWeek.GetWeekOfYear(dateOnly.ToDateTime(TimeOnly.MinValue));
        }

        public YearsWeek(DateTimeOffset dateTimeOffset)
        {
            Year = dateTimeOffset.Year;
            Week = ISOWeek.GetWeekOfYear(dateTimeOffset.DateTime);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (obj.GetType() != typeof(YearsWeek)) return false;
            YearsWeek yearsWeek = (YearsWeek)obj;
            return Year == yearsWeek.Year && Week == yearsWeek.Week;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Year, Week);
        }

        public static int operator -(YearsWeek a, YearsWeek b)
        {
            var da = ISOWeek.ToDateTime(a.Year, a.Week, DayOfWeek.Monday);
            var db = ISOWeek.ToDateTime(b.Year, b.Week, DayOfWeek.Monday);

            return (int)((da - db).TotalDays / 7);
        }
    }
}
