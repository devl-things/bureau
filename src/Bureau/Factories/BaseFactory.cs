using Bureau.Core.Factories;
using Bureau.Core.Models.Data;
using Bureau.Core.Models;
using Bureau.Core;

namespace Bureau.Factories
{
    internal class BaseFactory
    {
        protected QueryAggregateModel? _aggregate;

        public BaseFactory(QueryAggregateModel aggregate)
        {
            _aggregate = aggregate;
        }
        protected void Init(QueryAggregateModel aggregate) 
        {
            _aggregate = aggregate;
        }
        protected bool TryGetFlexibleData<T>(string id, out T data)
        {
            Result<FlexRecord> flexResult = GetFlexById(id);
            if (flexResult.IsSuccess)
            {
                Result<FlexibleRecord<T>> flexibleRecordResult = FlexibleRecordFactory.CreateFlexibleRecord<T>(flexResult.Value);
                if (flexibleRecordResult.IsSuccess)
                {
                    data = flexibleRecordResult.Value.Data;
                    return true;
                }
            }
            data = default!;
            return false;
        }

        public Result<Edge> GetEdgeById(string id, string? representing)
        {
            if (_aggregate != null && _aggregate.Edges.TryGetValue(Edge.EmptyEdgeWithId(id), out Edge? edge))
            {
                return edge;
            }
            return ResultErrorFactory.EdgeNotFound(id, representing);
        }

        protected Result<TermEntry> GetTermById(string id, string? representing)
        {
            if (_aggregate != null && _aggregate.TermEntries.TryGetValue(new TermEntry(id), out TermEntry? term))
            {
                return term;
            }
            return ResultErrorFactory.TermNotFound(id, representing);

        }
        protected Result<FlexRecord> GetFlexById(string id)
        {
            if (_aggregate != null && _aggregate.FlexRecords.TryGetValue(new FlexRecord(id), out FlexRecord? flex))
            {
                return flex;
            }
            return ResultErrorFactory.EmptyResultError();
        }
    }
}
