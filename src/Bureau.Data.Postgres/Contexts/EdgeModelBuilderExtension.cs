using Bureau.Data.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace Bureau.Data.Postgres.Contexts
{
    internal static class EdgeModelBuilderExtension
    {
        public static void ConfigureEdge(this ModelBuilder modelBuilder) 
        {
            // Configure Edge
            modelBuilder.Entity<Edge>()
                .HasOne(x => x.Record)
                .WithOne()
                .HasForeignKey<Edge>(x => x.Id);

            modelBuilder.Entity<Edge>()
                .HasOne(x => x.SourceNode)
                .WithMany()
                .HasForeignKey(x => x.SourceNodeId);

            modelBuilder.Entity<Edge>()
                .HasOne(x => x.TargetNode)
                .WithMany()
                .HasForeignKey(x => x.TargetNodeId);
            modelBuilder.Entity<Edge>()
                .HasOne(x => x.ParentNode)
                .WithMany()
                .HasForeignKey(x => x.ParentNodeId);
            modelBuilder.Entity<Edge>()
                .HasOne(x => x.RootNode)
                .WithMany()
                .HasForeignKey(x => x.RootNodeId);
        }
    }
}
