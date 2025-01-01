namespace Bureau.Data.Postgres.Models
{
    internal class Edge
    {
        /// <summary>
        /// Primary Key, unique for each Edge.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// References SourceNode.Id, identifying the source node in the graph.
        /// </summary>
        public Guid SourceNodeId { get; set; }

        /// <summary>
        /// References TargetNode.Id, identifying the target node in the graph.
        /// </summary>
        public Guid TargetNodeId { get; set; }

        /// <summary>
        /// Represents the type of connection between nodes.
        /// </summary>
        public int EdgeType { get; set; }

        /// <summary>
        /// Indicates whether the Edge is active.
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Represents the parent node of the Edge.
        /// </summary>
        public Guid? ParentNodeId { get; set; }
        /// <summary>
        /// Represents the root node of the Edge.
        /// </summary>
        public Guid RootNodeId { get; set; }

        /// <summary>
        /// Represents an order in a list if the Edge is part of a list.
        /// </summary>
        public int? Order { get; set; }

        public Record Record { get; set; } = null!; // Navigation Property
        public Record SourceNode { get; set; } = null!; // Navigation Property for the source node.
        public Record TargetNode { get; set; } = null!; // Navigation Property for the target node. 
        public Record? RootNode { get; set; } = null!; // Navigation Property for the source node.
        public Record? ParentNode { get; set; } = null!; // Navigation Property for the target node. 

    }
}
