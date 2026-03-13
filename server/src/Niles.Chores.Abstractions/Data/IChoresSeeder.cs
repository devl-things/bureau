using Bureau;
namespace Niles.Chores.Data
{
    public interface IChoresSeeder
    {
        public Result<SeedOutcome> SeedTest();
        public Task<Result<SeedOutcome>> SeedChoresAsync(CancellationToken cancellationToken = default);
        public Task<Result<SeedOutcome>> SeedTestAsync(CancellationToken cancellationToken = default);
        public Task<Result> ClearAndSeedTestAsync(CancellationToken cancellationToken = default);
    }

    public enum SeedOutcome
    {
        AlreadySeeded,
        Seeded,
    }
}
