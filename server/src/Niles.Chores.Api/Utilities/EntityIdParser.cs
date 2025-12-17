using Bureau;
using Bureau.Primitives.Errors;

namespace Niles.Chores.Api.Utilities
{
    public static class EntityIdParser
    {
        public static Result<int> ParseEntityId(string id)
        {
            if (!IdObfuscator.TryDecode(id, out int intId))
            {
                return ResultError.FromLogMessage(ProblemCodes.Request.InvalidId, string.Format("Id = {0} cannot be decoded", id));
            }
            return intId;
        }
    }
}
