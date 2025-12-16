using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sven.Data.Postgres
{
    internal class SvenContextPostgresFactory : IDesignTimeDbContextFactory<SvenContextPostgres>
    {
        public SvenContextPostgres CreateDbContext(string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("No arguments provided for connection string.");

            string connectionString = args[0];

            Console.WriteLine($"Creating BureauContext {connectionString}");

            var optionsBuilder = new DbContextOptionsBuilder<SvenContextPostgres>();

            optionsBuilder.UseNpgsql(connectionString);

            return new SvenContextPostgres(optionsBuilder.Options);
        }
    }
}
