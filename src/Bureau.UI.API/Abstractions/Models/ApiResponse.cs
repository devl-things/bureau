using Microsoft.AspNetCore.Mvc;

namespace Bureau.UI.API.Models
{
    public class ApiResponse
    {
        public const string StatusSuccess = "success";
        public const string StatusError = "error";
        public string Status { get; set; } = StatusSuccess;
        public string? Message { get; set; } // Optional description
        public ProblemDetails? Error { get; set; } // Error details    
    }
    public class ApiResponse<T> : ApiResponse
    {
        public required T Data { get; set; }         // Actual payload
    }
}
