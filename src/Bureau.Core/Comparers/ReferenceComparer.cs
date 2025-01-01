using Bureau.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Comparers
{
    public class ReferenceComparer : IEqualityComparer<IReference>
    {
        public bool Equals(IReference? x, IReference? y)
        {
            // Check for nulls to avoid NullReferenceException
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            // Compare based on the Id property
            return string.Equals(x.Id, y.Id, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode([DisallowNull] IReference obj)
        {
            // Use Id's hash code if it exists; otherwise, return 0
            return obj.Id == null ? 0 : obj.Id.ToLower().GetHashCode();
        }
    }
}
