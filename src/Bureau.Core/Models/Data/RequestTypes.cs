namespace Bureau.Core.Models.Data
{
    /// <summary>
    /// EdgeRequestType describes what properties can be searched or retrieved
    /// </summary>
    [Flags]
    public enum EdgeRequestType
    {
        Edge = 0,
        SourceNode = 1,
        TargetNode = 2,
        RootNode = 4,
    }

    /// <summary>
    /// RecordRequestType describes what objects can be retrieved
    /// </summary>
    [Flags]
    public enum RecordRequestType
    {
        Edges = 0,
        TermEntries = 1,
        FlexRecords = 2,
    }

    public enum TermRequestType
    {
        Label = 0,
        Title = 1,
    }
}
