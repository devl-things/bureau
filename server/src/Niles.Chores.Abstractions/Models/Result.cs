namespace Niles.Chores.Abstractions.Models
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class Result<T>
    {
        public T? Value { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

