using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sven.Data.SqlServer
{
    internal class SvenContextSqlServerFactory : IDesignTimeDbContextFactory<SvenContextSqlServer>
    {
        public SvenContextSqlServer CreateDbContext(string[] args)
        {
            if (args.Length == 0)
                throw new ArgumentException("No arguments provided for connection string.");

            string connectionString = args[0];

            Console.WriteLine($"Creating BureauContext {connectionString}");

            var optionsBuilder = new DbContextOptionsBuilder<SvenContextSqlServer>();

            optionsBuilder.UseSqlServer(connectionString);

            return new SvenContextSqlServer(optionsBuilder.Options);
        }
    }
}
