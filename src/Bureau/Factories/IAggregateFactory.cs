using Bureau.Core;
using Bureau.Core.Models;
using Bureau.Core.Models.Data;

namespace Bureau.Factories
{
    internal interface IAggregateFactory
    {
        HashSet<string> TermLabels { get; }
        void UpdateTerms(Dictionary<string, TermEntry> value);
    }

    internal interface IAggregateFactory<T> : IAggregateFactory where T : QueryAggregateModel
    {
        Result<T> CreateAggregate();
    }
}
