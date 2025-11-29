namespace Niles.Chores.Api.Dtos
{
    public class ChoresQueryParams
    {
        public string? Date { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}

