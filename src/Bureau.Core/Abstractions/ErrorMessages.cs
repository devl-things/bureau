namespace Bureau.Core
{
    public static class ErrorMessages
    {
        public const string BureauUnsuccessfulHttpResponse = "{0} request return status {1} with reason '{2}' with message '{3}'.";

        public const string BureauVariableUndefined = "'{0}' is not defined.";

        public const string BureauVariableIsNull = "'{0}' is null.";

        public const string BureauActionFailed = "Action '{0}' failed.";

        public const string InstanceCreation = "Can't create an instance of '{instance}'. Ensure that '{instance}' is not an abstract class and has a parameterless constructor.";
    }
}
