using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Data.Postgres.Mappers
{
    internal static class FlexRecordMapper
    {
        public static Postgres.Models.FlexibleRecord ToDBFlexibleRecord(this Core.Models.FlexRecord flexRecord, Guid guid)
        {
            return new Postgres.Models.FlexibleRecord()
            {
                Id = guid,
                DataType = flexRecord.DataType,
                Data = flexRecord.Data,
                Record = flexRecord.ToRecord(guid)
            };
        }

        public static Core.Models.FlexRecord ToModel(this Postgres.Models.FlexibleRecord flexRecord)
        {
            return new Core.Models.FlexRecord(flexRecord.Id.ToString())
            {
                DataType = flexRecord.DataType,
                Data = flexRecord.Data,
                CreatedAt = flexRecord.Record.CreatedAt,
                UpdatedAt = flexRecord.Record.UpdatedAt,
                CreatedBy = flexRecord.Record.CreatedBy,
                UpdatedBy = flexRecord.Record.UpdatedBy,
                Status = (Core.Models.RecordStatus)flexRecord.Record.Status                
            };
        }
    }
}
