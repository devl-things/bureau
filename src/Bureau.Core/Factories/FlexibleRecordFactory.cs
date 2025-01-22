using Bureau.Core.Models;
using System.Text.Json;

namespace Bureau.Core.Factories
{
    public class FlexibleRecordFactory
    {
        public static Result<FlexibleRecord<T>> CreateFlexibleRecord<T>(FlexRecord flex)
        {
            if (string.IsNullOrWhiteSpace(flex.DataType) || string.IsNullOrWhiteSpace(flex.Data))
            {
                return ResultErrorFactory.InvalidRecord(flex.Id);
            }
            Type? targetType = Type.GetType(flex.DataType);
            if (targetType == null)
            {
                return ResultErrorFactory.InvalidRecord(flex.Id);
            }
            if (!typeof(T).IsAssignableFrom(targetType))
            {
                return ResultErrorFactory.InvalidRecord(flex.Id);
            }
            try
            {
                T? deserializedObject = JsonSerializer.Deserialize<T>(flex.Data);
                FlexibleRecord<T> result = new FlexibleRecord<T>(flex.Id)
                {
                    CreatedAt = flex.CreatedAt,
                    UpdatedAt = flex.UpdatedAt,
                    CreatedBy = flex.CreatedBy,
                    UpdatedBy = flex.UpdatedBy,
                    Status = flex.Status,
                    Data = deserializedObject!
                };
                return result;
            }
            catch (Exception ex)
            {
                return ResultErrorFactory.InvalidRecord(flex.Id, ex);
            }
        }
    }
}
