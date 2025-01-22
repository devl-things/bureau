using Bureau.Core.Models;
using System.Diagnostics.CodeAnalysis;

namespace Bureau.Core.Comparers
{
    public class EdgeSourceTypeTargetComparer : IEqualityComparer<Edge>
    {
        public bool Equals(Edge? x, Edge? y)
        {
            // Check for nulls to avoid NullReferenceException
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;
            // Compare based on the Id property
            return string.Equals(x.SourceTypeTargetKey(), y.SourceTypeTargetKey(), StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode([DisallowNull] Edge obj)
        {
            return obj.SourceTypeTargetKey().GetHashCode();
        }
    }
}
