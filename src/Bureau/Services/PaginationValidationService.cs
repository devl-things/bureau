using Bureau.Core;
using Bureau.Core.Configuration;
using Bureau.Core.Factories;
using Bureau.Core.Services;
using Microsoft.Extensions.Options;

namespace Bureau.Services
{
    internal class PaginationValidationService : IPaginationValidationService
    {
        private const int BottomValue = 1;
        private readonly BureauOptions _options;
        public PaginationValidationService(IOptions<BureauOptions> options)
        {
            _options = options.Value;
        }

        public int GetValidPage(int? page) 
        {
            return page.HasValue ? page.Value : BottomValue;
        }

        public int GetValidLimit(int? limit)
        {
            return limit.HasValue ? limit.Value : _options.DefaultLimit;
        }

        public Result Validate(int? page, int? limit)
        {
            if ((page.HasValue && page < BottomValue) || (limit.HasValue && limit < BottomValue))
            {
                return ResultErrorFactory.InvalidPageAndLimit(page, limit);
            }

            if (limit.HasValue && limit > _options.MaximumLimit)
            {
                return ResultErrorFactory.InvalidLimit(limit.Value, _options.MaximumLimit);
            }

            return true;
        }
    }
}
