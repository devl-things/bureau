using Bureau;
using Microsoft.Extensions.Options;
using Sven;
using Sven.Configurations;
using Sven.Data.Repositories;
using Sven.Models;
using Sven.Services;
using Sven.Tests.TestUtils;
using NSubstitute;

namespace Sven.Tests.Security
{
    [Trait("Category", "Phase1")]
    public class VerificationCodeTests
    {
        private static UserService BuildUserService(
            TimeProvider timeProvider,
            RepositoryTestFactory.OwnedContext? owned = null)
        {
            RepositoryTestFactory.OwnedContext ctx = owned ?? RepositoryTestFactory.CreateContext();
            TicketRepository ticketRepo = new TicketRepository(ctx.Context, timeProvider);
            VerificationCodeRepository codeRepo = new VerificationCodeRepository(ctx.Context, timeProvider);
            IUserRepository userRepo = Substitute.For<IUserRepository>();
            userRepo.GetByUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new Result<SvenUser>(ResultError.From("not_found"))));
            return new UserService(ticketRepo, codeRepo, userRepo, timeProvider);
        }

        [Fact]
        public async Task VerificationCode_ExpiredAfterFiveMinutes_IsRejected()
        {
            ManualTimeProvider timeProvider = new ManualTimeProvider(DateTimeOffset.UtcNow);
            RepositoryTestFactory.OwnedContext owned = RepositoryTestFactory.CreateContext();
            VerificationCodeRepository codeRepo = new VerificationCodeRepository(owned.Context, timeProvider);

            // Seed a code directly — bypass the email lookup
            DateTimeOffset createdAt = timeProvider.GetUtcNow();
            UserVerificationCode code = new UserVerificationCode(
                "test@example.com",
                "123456",
                null,
                VerificationStatus.None,
                createdAt.AddMinutes(5));
            await codeRepo.StoreAsync(code.Id, code, CancellationToken.None);

            // Build user service using same context and time provider
            IUserRepository userRepo = Substitute.For<IUserRepository>();
            TicketRepository ticketRepo = new TicketRepository(owned.Context, timeProvider);
            UserService service = new UserService(ticketRepo, codeRepo, userRepo, timeProvider);

            // Advance time 6 minutes past code creation → past the 5-minute TTL
            timeProvider.Advance(TimeSpan.FromMinutes(6));

            Result<UserVerificationCode> result = await service.GetVerificationCodeAsync(code.Id, CancellationToken.None);

            Assert.True(result.IsError, "Verification code should be rejected after 5-minute TTL expires");
        }

        [Fact]
        public async Task VerificationCode_NotExpiredBeforeFiveMinutes_IsAccepted()
        {
            ManualTimeProvider timeProvider = new ManualTimeProvider(DateTimeOffset.UtcNow);
            RepositoryTestFactory.OwnedContext owned = RepositoryTestFactory.CreateContext();
            VerificationCodeRepository codeRepo = new VerificationCodeRepository(owned.Context, timeProvider);

            DateTimeOffset createdAt = timeProvider.GetUtcNow();
            UserVerificationCode code = new UserVerificationCode(
                "test@example.com",
                "123456",
                null,
                VerificationStatus.None,
                createdAt.AddMinutes(5));
            await codeRepo.StoreAsync(code.Id, code, CancellationToken.None);

            IUserRepository userRepo = Substitute.For<IUserRepository>();
            TicketRepository ticketRepo = new TicketRepository(owned.Context, timeProvider);
            UserService service = new UserService(ticketRepo, codeRepo, userRepo, timeProvider);

            // Advance time only 4 minutes → still within the 5-minute TTL window
            timeProvider.Advance(TimeSpan.FromMinutes(4));

            Result<UserVerificationCode> result = await service.GetVerificationCodeAsync(code.Id, CancellationToken.None);

            Assert.False(result.IsError, "Verification code should be accepted before 5-minute TTL expires");
        }
    }

    /// <summary>
    /// A controllable TimeProvider for testing time-sensitive logic.
    /// </summary>
    internal sealed class ManualTimeProvider : TimeProvider
    {
        private DateTimeOffset _utcNow;

        public ManualTimeProvider(DateTimeOffset startTime)
        {
            _utcNow = startTime;
        }

        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }

        public void Advance(TimeSpan duration)
        {
            _utcNow = _utcNow.Add(duration);
        }
    }
}
