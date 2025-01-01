using Bureau.Core;

namespace Bureau.UI.API.Models
{
    internal static class ApiResponseMapper
    {
        internal static ApiResponse ToApiResponse(this ResultError result)
        {
            return new ApiResponse()
            {
                Status = ApiResponse.StatusError,
                Message = $"{result.ErrorMessage} {result.Exception?.ToString()}"
            };
        }
    }
}
