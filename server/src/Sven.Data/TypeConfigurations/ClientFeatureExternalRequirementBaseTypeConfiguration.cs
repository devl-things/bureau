using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.TypeConfigurations
{
    public class ClientFeatureExternalRequirementBaseTypeConfiguration : IEntityTypeConfiguration<ClientFeatureExternalRequirementDb>
    {
        public void Configure(EntityTypeBuilder<ClientFeatureExternalRequirementDb> builder)
        {
            builder.ToTable("ClientFeatureExternalRequirements");
            builder.HasKey(x => new { x.ClientId, x.FeatureKey, x.ExternalScopeKey });
            builder.Property(x => x.FeatureKey).IsRequired();
            builder.Property(x => x.ExternalScopeKey).IsRequired();
            builder.HasOne(x => x.ClientFeature).WithMany(x => x.ExternalRequirements).HasForeignKey(x => new { x.ClientId, x.FeatureKey }).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
