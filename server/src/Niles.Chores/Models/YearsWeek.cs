using System.Globalization;

namespace Niles.Chores.Models
{
    internal struct YearsWeek
    {
        public int Year { get; set; }
        public int Week { get; set; }

        public YearsWeek(DateOnly dateOnly)
        {
            DateTime dateTime = dateOnly.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
            Year = ISOWeek.GetYear(dateTime);
            Week = ISOWeek.GetWeekOfYear(dateTime);
        }

        public YearsWeek(DateTimeOffset dateTimeOffset)
        {
            DateTime dateTime = dateTimeOffset.UtcDateTime;
            Year = ISOWeek.GetYear(dateTime);
            Week = ISOWeek.GetWeekOfYear(dateTime);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (obj is not YearsWeek) return false;
            YearsWeek yearsWeek = (YearsWeek)obj;
            return Year == yearsWeek.Year && Week == yearsWeek.Week;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Year, Week);
        }

        public static int operator -(YearsWeek a, YearsWeek b)
        {
            DateTime da = ISOWeek.ToDateTime(a.Year, a.Week, DayOfWeek.Monday);
            DateTime db = ISOWeek.ToDateTime(b.Year, b.Week, DayOfWeek.Monday);

            return (int)((da - db).TotalDays / 7);
        }

        public override string ToString()
        {
            return $"{nameof(Year)}={Year};{nameof(Week)}={Week}";
        }
    }
}
