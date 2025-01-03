using Bureau.Core.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Models.Data
{
    public class BaseAggregateModel
    {
        public HashSet<TermEntry> TermEntries { get; set; }
        public HashSet<FlexRecord> FlexRecords { get; set; }
        public HashSet<Edge> Edges { get; set; }

        public PaginationMetadata? Pagination { get; set; }

    }

    public class AggregateModel : BaseAggregateModel
    {
        public required IReference MainReference { get; set; }

        public AggregateModel()
        {
            
        }
        public AggregateModel(BaseAggregateModel baseModel)
        {
            TermEntries = baseModel.TermEntries;
            FlexRecords = baseModel.FlexRecords;
            Edges = baseModel.Edges;
            Pagination = baseModel.Pagination;
        }
    }

    public class ExtendedAggregateModel : AggregateModel
    {
        public required HashSet<FlexRecord> FlexRecordsToDelete { get; set; }
        public required HashSet<Edge> EdgesToDelete { get; set; }
    }
}
