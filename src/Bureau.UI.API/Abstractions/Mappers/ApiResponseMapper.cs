using Bureau.Core;

namespace Bureau.UI.API.Models
{
    internal static class ApiResponseMapper
    {
        internal static ApiResponse<List<TTarget>> ToPagedApiResponse<TSource, TTarget>(this PaginatedResult<List<TSource>> values, Func<TSource, TTarget> mappingAction) where TSource : class
        {
            ApiResponse<List<TTarget>> response = new ApiResponse<List<TTarget>>()
            {
                Status = ApiResponse.StatusSuccess,
                Data = new List<TTarget>(values.Value.Count),
                Pagination = new PaginationData(values.Pagination)
            };
            values.Value.ForEach(x => response.Data.Add(mappingAction(x)));
            return response;
        }

        internal static ApiResponse<TTarget> ToApiResponse<TSource, TTarget>(this Result<TSource> result, Func<TSource, TTarget> mappingAction)
        {
            return new ApiResponse<TTarget>()
            {
                Status = ApiResponse.StatusSuccess,
                Data = mappingAction(result.Value)
            };
        }

        internal static ApiResponse ToApiResponse(this ResultError result)
        {
            return new ApiResponse()
            {
                Status = ApiResponse.StatusError,
                Message = $"{result.ErrorMessage} {result.Exception?.ToString()}"
            };
        }
        internal static ApiResponse ToApiResponse(this Result result, string? successMessage = null)
        {
            return new ApiResponse()
            {
                Status = ApiResponse.StatusSuccess,
                Message = successMessage
            };
        }
    }
}
