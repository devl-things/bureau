
using Bureau.Core.Factories;

namespace Bureau.Core.Models
{
    public class Edge : BaseRecord
    {
        public required IReference SourceNode { get; set; }
        public required IReference TargetNode { get; set; }
        public required IReference RootNode { get; set; }
        public IReference? ParentNode { get; set; }
        public int EdgeType { get; set; }
        public bool Active { get; set; }

        public int? Order { get; set; }

        public Edge(string id)
        {
            Id = id;
            Status = RecordStatus.Active;
            Active = true;
        }
        public string SourceTypeTargetKey()
        {
            return $"{SourceNode.Id}_{EdgeType}_{TargetNode.Id}";
        }

        public static Edge EmptyEdgeWithId(string id)
        {
            return new Edge(id)
            {
                SourceNode = BureauReferenceFactory.EmptyReference,
                TargetNode = BureauReferenceFactory.EmptyReference,
                RootNode = BureauReferenceFactory.EmptyReference,
            };
        }
    }
}