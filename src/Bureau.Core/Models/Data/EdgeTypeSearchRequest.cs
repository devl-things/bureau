namespace Bureau.Core.Models.Data
{
    public struct EdgeTypeSearchRequest
    {
        public EdgeTypeSearchRequest()
        {
        }

        /// <summary>
        /// The type of edge to search for
        /// If edge type doesn't exist it returns empty
        /// </summary>
        public int EdgeType { get; set; }
        /// <summary>
        /// If true, only active edges are returned (default)
        /// If null, all edges are returned no matter the active state
        /// </summary>
        public bool? Active { get; set; } = true;

        /// <summary>
        /// Based on filter of <see cref="EdgeType"/> and <see cref="Active"/> filter other edges by which edge property
        /// </summary>
        public EdgeRequestType FilterRequestType { get; set; }

        /// <summary>
        /// Based on what <see cref="Edge" /> properties should records be selected 
        /// </summary>
        public EdgeRequestType SelectReferences { get; set; }

        /// <summary>
        /// What records types should be selected
        /// </summary>
        public RecordRequestType SelectRecordTypes { get; set; }

        public PaginationMetadata Pagination { get; set; }
    }
}
