using Bureau;
namespace Niles.Chores.Data
{
    public interface IChoresSeeder
    {
        public void Seed();
        public Task<Result> SeedAsync(CancellationToken cancellationToken = default);
        public Task<Result> ClearAndSeedAsync(CancellationToken cancellationToken = default);
    }
}
