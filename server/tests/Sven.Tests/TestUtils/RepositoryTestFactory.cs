using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Models;
using Sven.Data.Repositories;
using Sven.Data.TypeConfigurations;

namespace Sven.Tests.TestUtils
{
    /// <summary>
    /// Creates SQLite in-memory EF-backed repositories for unit tests.
    /// Each call to CreateContext() returns a fresh isolated SQLite database with the
    /// entity type configurations applied — exactly the same pattern as SvenTestContext in the
    /// integration fixture, but usable without the full WebApplicationFactory.
    /// SQLite is required (instead of EF InMemory) because repositories use
    /// ExecuteDeleteAsync / ExecuteUpdateAsync which require a relational provider.
    /// </summary>
    internal static class RepositoryTestFactory
    {
        internal sealed class UnitTestSvenContext : SvenContext
        {
            public UnitTestSvenContext(DbContextOptions<UnitTestSvenContext> options) : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
                modelBuilder.Ignore<SerializedData>();
                new ClientBaseTypeConfiguration<ClientDb>().Configure(modelBuilder.Entity<ClientDb>());
                new ClientFeatureBaseTypeConfiguration().Configure(modelBuilder.Entity<ClientFeatureDb>());
                new ClientFeatureExternalRequirementBaseTypeConfiguration().Configure(modelBuilder.Entity<ClientFeatureExternalRequirementDb>());
                new AuthCodeBaseTypeConfiguration().Configure(modelBuilder.Entity<AuthCodeDb>());
                new OAuthRequestBaseTypeConfiguration().Configure(modelBuilder.Entity<OAuthRequestDb>());
                new UserVerificationCodeBaseTypeConfiguration().Configure(modelBuilder.Entity<UserVerificationCodeDb>());
                new TicketBaseTypeConfiguration().Configure(modelBuilder.Entity<TicketDb>());
                new FailedExchangeAttemptBaseTypeConfiguration().Configure(modelBuilder.Entity<FailedExchangeAttemptDb>());
            }
        }

        /// <summary>
        /// Creates a fresh SQLite in-memory context. The caller is responsible for opening
        /// the returned connection and keeping it alive for the lifetime of the test; the
        /// context shares that connection so all repository calls see the same in-memory DB.
        /// Call context.Database.EnsureCreated() before first use.
        /// </summary>
        internal static (UnitTestSvenContext Context, SqliteConnection Connection) CreateContextWithConnection()
        {
            SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            DbContextOptions<UnitTestSvenContext> options = new DbContextOptionsBuilder<UnitTestSvenContext>()
                .UseSqlite(connection)
                .Options;
            UnitTestSvenContext context = new UnitTestSvenContext(options);
            context.Database.EnsureCreated();
            return (context, connection);
        }

        /// <summary>
        /// Convenience overload that creates a context and opens the connection, relying on
        /// the caller to keep the connection open for the lifetime of the context.
        /// The connection is stored in the returned <see cref="OwnedContext"/> which implements
        /// IDisposable and closes the connection on dispose.
        /// </summary>
        internal static OwnedContext CreateContext()
        {
            (UnitTestSvenContext context, SqliteConnection connection) = CreateContextWithConnection();
            return new OwnedContext(context, connection);
        }

        internal static RefreshTokenRepository CreateRefreshTokenRepository(OwnedContext owned)
        {
            return new RefreshTokenRepository(owned.Context, TimeProvider.System);
        }

        internal static AuthCodeRepository CreateAuthCodeRepository(OwnedContext owned)
        {
            return new AuthCodeRepository(owned.Context, TimeProvider.System, Microsoft.Extensions.Logging.Abstractions.NullLogger<AuthCodeRepository>.Instance);
        }

        internal static PkceRequestRepository CreatePkceRequestRepository(OwnedContext owned)
        {
            return new PkceRequestRepository(owned.Context, TimeProvider.System);
        }

        internal static VerificationCodeRepository CreateVerificationCodeRepository(OwnedContext owned)
        {
            return new VerificationCodeRepository(owned.Context, TimeProvider.System);
        }

        internal static TicketRepository CreateTicketRepository(OwnedContext owned)
        {
            return new TicketRepository(owned.Context, TimeProvider.System);
        }

        /// <summary>
        /// Wraps a UnitTestSvenContext together with its backing SQLite connection.
        /// Disposing this object closes the connection and disposes the context.
        /// </summary>
        internal sealed class OwnedContext : IDisposable
        {
            public UnitTestSvenContext Context { get; }
            private readonly SqliteConnection _connection;
            private bool _disposed;

            public OwnedContext(UnitTestSvenContext context, SqliteConnection connection)
            {
                Context = context;
                _connection = connection;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _disposed = true;
                    Context.Dispose();
                    _connection.Dispose();
                }
            }
        }
    }
}
