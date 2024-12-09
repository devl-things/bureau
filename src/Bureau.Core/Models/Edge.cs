namespace Bureau.Core.Models
{
    public class Edge
    {
        public IReference SourceNode { get; set; }
        public IReference TargetNode { get; set; }
        public string EdgeType { get; set; }
        public bool Active { get; set; }
    }
}