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
        public static Postgres.Models.Record ToRecord(this Core.Models.BaseEntry termEntry)
        {
            return new Postgres.Models.Record()
            {
                ExternalId = termEntry.ExternalId,
                ProviderName = termEntry.ProviderName,
                CreatedAt = termEntry.CreatedAt,
                UpdatedAt = termEntry.UpdatedAt,
                CreatedBy = termEntry.CreatedBy,
                UpdatedBy = termEntry.UpdatedBy,
                Status = (int)termEntry.Status
            };
        }

        public static Postgres.Models.Record ToRecord(this Core.Models.BaseRecord termEntry, Guid guid)
        {
            return new Postgres.Models.Record()
            {
                Id = guid,
                ProviderName = BureauConstants.BureauProvider,
                CreatedAt = termEntry.CreatedAt,
                UpdatedAt = termEntry.UpdatedAt,
                CreatedBy = termEntry.CreatedBy,
                UpdatedBy = termEntry.UpdatedBy,
                Status = (int)termEntry.Status
            };
        }
    }
}
