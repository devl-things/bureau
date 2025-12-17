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
                ResultError error = new ResultError(ProblemCodes.Request.InvalidId, ErrorMessages.InvalidIdFormat, null!, string.Format("Id = {0} cannot be decoded", id));
                return error;
            }
            return intId;
        }
    }
}
