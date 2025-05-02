using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;

namespace Sven.Data.SqlServer
{
    internal class SvenContextSqlServer : SvenContext
    {
        public SvenContextSqlServer(DbContextOptions<SvenContextSqlServer> options) : base(options)
        {
        }
    }
}
