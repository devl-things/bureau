namespace Bureau.Core.Factories
{
    public class ResultErrorFactory
    {
        public static ResultError InvalidLimit(int limit, int maxLimit) => new ResultError($"Limit ({limit}) is not in allowed boundaries ({maxLimit}).");
        public static ResultError InvalidRecord(string recordId) => new ResultError($"Flexible record with Id = {recordId} is invalid.");
        public static ResultError InvalidRecord(string recordId, Exception ex) => new ResultError($"Flexible record with Id = {recordId} is invalid.", ex);
        public static ResultError UnexpectedError() => new ResultError("Upsy-daisy! Something went unexpected. Try again, maybe");
        public static ResultError UnknownEdgeReference(string edgeId, string refId) => new ResultError($"Unknown reference ({refId}) in edge with Id = {edgeId}.");
        public static ResultError UnknownTerm(string term) => new ResultError($"Unknown term ({term}).");
        public static ResultError ParentNodeNotFound(string edgeId) => new ResultError($"Parent node not defined but expected. On edge with Id = {edgeId}.");
    }
}
