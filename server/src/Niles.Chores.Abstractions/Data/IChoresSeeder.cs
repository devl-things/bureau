using Bureau;
namespace Niles.Chores.Data
{
    public interface IChoresSeeder
    {
        public Result SeedTest();
        public Task<Result> SeedChoresAsync(CancellationToken cancellationToken = default);
        public Task<Result> SeedTestAsync(CancellationToken cancellationToken = default);
        public Task<Result> ClearAndSeedTestAsync(CancellationToken cancellationToken = default);
    }
}
