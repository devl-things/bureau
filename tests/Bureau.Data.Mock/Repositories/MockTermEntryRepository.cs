using Bureau.Core;
using Bureau.Core.Models;
using Bureau.Core.Repositories;

namespace Bureau.Data.Mock.Repositories
{
    public class MockTermEntryRepository : ITermEntryRepository
    {
        public Task<Result<TermEntry>> GetAsync(IReference id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Result<TermEntry>>(new TermEntry()
            {
                Id = id.Id,
                Title = "Fritaja"
            });
        }
    }
}
