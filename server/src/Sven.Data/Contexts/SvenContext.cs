using Microsoft.EntityFrameworkCore;
using Sven.Data.Models;

namespace Sven.Data.Contexts
{
    public abstract class SvenContext : DbContext
    {
        protected SvenContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ClientDb> Clients { get; set; } = null!;
        public DbSet<UserDb> Users { get; set; } = null!;

    }
}
