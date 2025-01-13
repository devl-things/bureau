using Bureau.Core.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Data.Postgres.Mappers
{
    internal static class RecordMapper
    {
        public static Postgres.Models.Record ToRecord(this Core.Models.BaseEntry baseEntry)
        {
            return new Postgres.Models.Record()
            {
                ExternalId = baseEntry.ExternalId,
                ProviderName = baseEntry.ProviderName,
                CreatedAt = baseEntry.CreatedAt,
                UpdatedAt = baseEntry.UpdatedAt,
                CreatedBy = baseEntry.CreatedBy,
                UpdatedBy = baseEntry.UpdatedBy,
                Status = (int)baseEntry.Status
            };
        }

        public static Postgres.Models.Record ToRecord(this Core.Models.BaseRecord baseRecord, Guid guid)
        {
            return new Postgres.Models.Record()
            {
                Id = guid,
                ProviderName = BureauConstants.BureauProvider,
                CreatedAt = baseRecord.CreatedAt,
                UpdatedAt = baseRecord.UpdatedAt,
                CreatedBy = baseRecord.CreatedBy,
                UpdatedBy = baseRecord.UpdatedBy,
                Status = (int)baseRecord.Status
            };
        }
    }
}
