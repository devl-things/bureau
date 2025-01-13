namespace Bureau.Core.Models
{
    public class FlexRecord : BaseRecord
    {
        public string DataType { get; set; } = default!;
        public string Data { get; set; } = null!;
        public FlexRecord(string id)
        {
            Id = id;
        }

        public FlexRecord Clone(string id)
        {
            return new FlexRecord(id)
            {
                DataType = DataType,
                Data = Data,
                Status = Status,
                CreatedAt = CreatedAt,
                CreatedBy = CreatedBy,
                UpdatedAt = UpdatedAt,
                UpdatedBy = UpdatedBy
            };
        }
    }
}
