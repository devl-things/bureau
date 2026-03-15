using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace Bureau.EntityFrameworkCore.Conversions
{
    public static class JsonPropertyBuilderExtensions
    {
        public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> propertyBuilder)
        {
            propertyBuilder.HasConversion(
                x => JsonSerializer.Serialize(x, (JsonSerializerOptions?)null),
                x => JsonSerializer.Deserialize<T>(x, (JsonSerializerOptions?)null)!);

            return propertyBuilder;
        }

        public static ValueComparer<HashSet<T>> CreateHashSetComparer<T>()
        {
            return new ValueComparer<HashSet<T>>(
                (a, b) => a != null && b != null && a.SetEquals(b),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v!.GetHashCode())),
                c => new HashSet<T>(c));
        }
    }
}
