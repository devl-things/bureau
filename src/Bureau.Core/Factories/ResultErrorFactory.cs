namespace Bureau.Core.Factories
{
    public static class ResultErrorFactory
    {
        public static ResultError RecordIdBadFormat(string recordType, string id) => new ResultError($"Record {recordType} with Id = {id} has a bad format for the requested operation.");
        public static ResultError InvalidPageAndLimit(int? page, int? limit) => new ResultError($"If defined, page ({page}) and limit ({limit}) must be greater than zero.");
        public static ResultError InvalidLimit(int limit, int maxLimit) => new ResultError($"Limit ({limit}) is not in allowed boundaries ({maxLimit}).");
        public static ResultError InvalidRecord(string recordId) => new ResultError($"Flexible record with Id = {recordId} is invalid.");
        public static ResultError InvalidRecord(string recordId, Exception ex) => new ResultError($"Flexible record with Id = {recordId} is invalid.", ex);
        public static ResultError UnexpectedError() => new ResultError("Upsy-daisy! Something went unexpected. Try again, maybe");
        public static ResultError UnknownEdgeType(string edgeId, int edgeType, string? representing) => new ResultError($"Edge (for {representing}) with Id = {edgeId} has unknown edge type ({edgeType}).");
        public static ResultError UnknownEdgeReference(string edgeId, string refId) => new ResultError($"Unknown reference ({refId}) in edge with Id = {edgeId}.");
        public static ResultError UnexpectedNumberOrEdges(int expectedCount, int edgeCount) => new ResultError($"Actually number of edges ({edgeCount}) not the same as expected number of edges  ({expectedCount}).");
        public static ResultError UnknownTerm(string term) => new ResultError($"Unknown term ({term}).");
        public static ResultError TermNotFound(string termId, string? representing) => new ResultError($"Term (as {representing}) with Id = {termId} not found.");
        public static ResultError EdgeNotFound(string edgeId, string? representing) => new ResultError($"Edge (for {representing}) with Id = {edgeId} not found.");
        public static ResultError ParentNodeNotFound(string edgeId) => new ResultError($"Parent node not defined but expected. On edge with Id = {edgeId}.");
        public static ResultError EmptyResultError() => new ResultError();
    }
}
