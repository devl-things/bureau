using Bureau.Core.Constants;

namespace Bureau.Data.Postgres.Mappers
{
    internal static class TermEntryMapper
    {
        public static Core.Models.TermEntry ToModel(this Postgres.Models.TermEntry termEntry)
        {
            return new Core.Models.TermEntry(termEntry.Id.ToString(), termEntry.Title)
            {
                ExternalId = termEntry.Record.ExternalId,
                ProviderName = termEntry.Record.ProviderName,
                CreatedAt = termEntry.Record.CreatedAt,
                UpdatedAt = termEntry.Record.UpdatedAt,
                CreatedBy = termEntry.Record.CreatedBy,
                UpdatedBy = termEntry.Record.UpdatedBy,
                Status = (Core.Models.RecordStatus)termEntry.Record.Status
            };
        }

        public static Postgres.Models.TermEntry ToDBTermEntry(this Core.Models.TermEntry termEntry)
        {
            return termEntry.ToDBTermEntry(Guid.Parse(termEntry.Id));  
        }

        public static Postgres.Models.TermEntry ToDBTermEntry(this Core.Models.TermEntry termEntry, Guid guid)
        {
            return new Postgres.Models.TermEntry()
            {
                Id = guid,
                Title = termEntry.Title,
                Label = termEntry.Label,
                Record = termEntry.ToRecord(guid)
            };
        }
    }
}
