using Bureau.Core.Factories;
using System.Dynamic;

namespace Bureau.Core.Models.Data
{
    public class QueryAggregateModel
    {
        public HashSet<TermEntry> TermEntries { get; set; } = default!;
        public HashSet<FlexRecord> FlexRecords { get; set; } = default!;
        public HashSet<Edge> Edges { get; set; } = default!;

        public PaginationMetadata Pagination { get; set; }

        public QueryAggregateModel(PaginationMetadata pagination)
        {
            Pagination = pagination;
        }
    }

    public class InsertAggregateModel : QueryAggregateModel
    {
        public required IReference MainReference { get; set; }

        public InsertAggregateModel() : base(new PaginationMetadata())
        {

        }
        public InsertAggregateModel(QueryAggregateModel baseModel) : base(baseModel.Pagination)
        {
            TermEntries = baseModel.TermEntries;
            FlexRecords = baseModel.FlexRecords;
            Edges = baseModel.Edges;
            Pagination = baseModel.Pagination;
        }
    }

    public class UpdateAggregateModel : InsertAggregateModel, IRemoveAggregateModel
    {
        public required HashSet<FlexRecord> FlexRecordsToDelete { get; set; }
        public required HashSet<Edge> EdgesToDelete { get; set; }
    }

    public interface IRemoveAggregateModel
    {
        public HashSet<FlexRecord> FlexRecordsToDelete { get; set; }
        public HashSet<Edge> EdgesToDelete { get; set; }
    }

    public class RemoveAggregateModel : IRemoveAggregateModel
    {
        public required HashSet<FlexRecord> FlexRecordsToDelete { get; set; }
        public required HashSet<Edge> EdgesToDelete { get; set; }
    }

}
