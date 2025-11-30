namespace Niles.Chores.Abstractions.Models
{
    //TODO this should be enhanced with implicit creation from T to Result<T> and from string to Result
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

