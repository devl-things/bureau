using Microsoft.EntityFrameworkCore;
using Sven.Data.Models;

namespace Sven.Data.Contexts
{
    public abstract class SvenContext : DbContext
    {
        protected SvenContext(DbContextOptions options) : base(options)
        {
        }

        internal DbSet<ClientDb> Clients { get; set; } = null!;
        internal DbSet<UserDb> Users { get; set; } = null!;
        internal DbSet<RefreshTokenDb> RefreshTokens { get; set; } = null!;
        internal DbSet<UserExternalTokenDb> UserExternalTokens { get; set; } = null!;
        internal DbSet<HouseholdDb> Households { get; set; } = null!;
        internal DbSet<HouseholdMemberDb> HouseholdMembers { get; set; } = null!;
        internal DbSet<SharedExternalTokenDb> SharedExternalTokens { get; set; } = null!;
        internal DbSet<AuthCodeDb> AuthCodes { get; set; } = null!;
        internal DbSet<OAuthRequestDb> OAuthRequests { get; set; } = null!;
        internal DbSet<UserVerificationCodeDb> VerificationCodes { get; set; } = null!;
        internal DbSet<TicketDb> Tickets { get; set; } = null!;
        internal DbSet<FailedExchangeAttemptDb> FailedExchangeAttempts { get; set; } = null!;
    }
}
