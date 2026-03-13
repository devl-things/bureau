namespace Bureau.Core
{
    public class ProgressInfo
    {
        public int? Found { get; init; }
        public int? Inserted { get; init; }
        public int? Updated { get; init; }

        public override string ToString()
        {
            return $"Found {Found}, inserted {Inserted}, updated {Updated}";
        }
    }
}
