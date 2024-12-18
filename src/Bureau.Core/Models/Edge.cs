namespace Bureau.Core.Models
{
    public class Edge: IReference
    {
        public string Id { get; set; }
        public IReference SourceNode { get; set; }
        public IReference TargetNode { get; set; }
        public int EdgeType { get; set; }
        public bool Active { get; set; }

    }
}