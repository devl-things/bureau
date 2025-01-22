using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Data.Postgres.Contexts
{
    internal class BureauContextFactory : IDesignTimeDbContextFactory<BureauContext>
    {
        public BureauContext CreateDbContext(string[] args)
        {
            if(args.Length == 0)
                throw new ArgumentException("No arguments provided for connection string.");

            string connectionString = args[0];

            Console.WriteLine($"Creating BureauContext {connectionString}");

            var optionsBuilder = new DbContextOptionsBuilder<BureauContext>();

            optionsBuilder.UseNpgsql(connectionString);

            return new BureauContext(optionsBuilder.Options);
        }
    }
}
