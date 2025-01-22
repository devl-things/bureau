namespace Bureau.Core.Models.Data
{
    public struct IdSearchRequest
    {
        /// <summary>
        /// The reference id to search for
        /// </summary>
        public IReference FilterReferenceId { get; set; }
        /// <summary>
        /// The <see cref="FilterReferenceId"/> is what type of node
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
    }
}
