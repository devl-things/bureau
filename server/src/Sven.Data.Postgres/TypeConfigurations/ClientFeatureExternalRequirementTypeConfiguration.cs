using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.Postgres.TypeConfigurations
{
    internal class ClientFeatureExternalRequirementTypeConfiguration : IEntityTypeConfiguration<ClientFeatureExternalRequirementDb>
    {
        public void Configure(EntityTypeBuilder<ClientFeatureExternalRequirementDb> builder)
        {
            builder.ToTable("ClientFeatureExternalRequirements");
            builder.HasKey(x => new { x.ClientId, x.FeatureKey, x.ExternalScopeKey });
            builder.Property(x => x.FeatureKey).HasColumnType("text").IsRequired();
            builder.Property(x => x.ExternalScopeKey).HasColumnType("text").IsRequired();
            builder.HasOne(x => x.ClientFeature).WithMany(x => x.ExternalRequirements).HasForeignKey(x => new { x.ClientId, x.FeatureKey }).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
