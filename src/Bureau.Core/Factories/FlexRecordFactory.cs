using Bureau.Core.Models;
using System.Text.Json;

namespace Bureau.Core.Factories
{
    public static class FlexRecordFactory
    {
        public static Result<FlexRecord> CreateFlexRecord<T>(FlexibleRecord<T> flex)
        {
            FlexRecord record = new FlexRecord(flex.Id)
            {
                CreatedAt = flex.CreatedAt,
                UpdatedAt = flex.UpdatedAt,
                CreatedBy = flex.CreatedBy,
                UpdatedBy = flex.UpdatedBy,
                Status = flex.Status,
                Data = JsonSerializer.Serialize(flex.Data),
                DataType = typeof(T).AssemblyQualifiedName!
            };
            return record;
        }
    }
}
