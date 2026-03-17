using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.TypeConfigurations
{
    public class ClientFeatureBaseTypeConfiguration : IEntityTypeConfiguration<ClientFeatureDb>
    {
        public void Configure(EntityTypeBuilder<ClientFeatureDb> builder)
        {
            builder.ToTable("ClientFeatures");
            builder.HasKey(x => new { x.ClientId, x.FeatureKey });
            builder.Property(x => x.FeatureKey).IsRequired();
            builder.HasOne(x => x.Client).WithMany(x => x.ClientFeatures).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
