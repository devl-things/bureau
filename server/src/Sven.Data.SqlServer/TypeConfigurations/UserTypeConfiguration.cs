using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;
using Sven.Data.TypeConfigurations;

namespace Sven.Data.SqlServer.TypeConfigurations
{
    internal class UserTypeConfiguration<TEntity> : UserBaseTypeConfiguration<TEntity> where TEntity : UserDb
    {
        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            base.Configure(builder);

            builder.ConfigureAuditFields();
        }
    }
}
