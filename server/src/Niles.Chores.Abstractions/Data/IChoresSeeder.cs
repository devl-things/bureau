namespace Niles.Chores.Data
{
    public interface IChoresSeeder
    {
        public void Seed();
        public Task SeedAsync(CancellationToken cancellationToken = default);
        public Task ClearAndSeedAsync(CancellationToken cancellationToken = default);
    }
}
