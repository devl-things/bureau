namespace Bureau.Core.Services
{
    public interface IPaginationValidationService
    {
        public int GetValidPage(int? page);

        public int GetValidLimit(int? limit);

        public Result Validate(int? page, int? limit);
    }
}
