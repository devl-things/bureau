using Bureau.Core.Factories;
using Bureau.Core.Models;

namespace Bureau.Factories
{
    internal abstract class BaseAggregateFactory : IAggregateFactory
    {
        private int _tempId = 0;
        protected Dictionary<string, TermEntry> TermEntriesByLabel;
        public HashSet<string> TermLabels { get; internal set; }
        protected BaseAggregateFactory(int termCapacity)
        {
            TermLabels = new HashSet<string>(termCapacity);
            TermEntriesByLabel = new Dictionary<string, TermEntry>(termCapacity);
        }
        protected string GetNewTempId()
        {
            _tempId++;
            return BureauReferenceFactory.CreateTempId(_tempId);
        }

        protected bool TryGetTermEntry(string title, out TermEntry termEntry)
        {
            return TermEntriesByLabel.TryGetValue(TermEntry.GetLabel(title), out termEntry!);
        }

        public abstract void UpdateTerms(Dictionary<string, TermEntry> value);

    }
}
