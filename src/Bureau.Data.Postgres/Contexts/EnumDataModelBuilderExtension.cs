using Bureau.Data.Postgres.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bureau.Data.Postgres.Contexts
{
    internal static class EnumDataModelBuilderExtension
    {
        public static void ConfigureEnumData(this ModelBuilder modelBuilder) 
        {
            modelBuilder.Entity<EnumData>()
                .HasKey(x => x.Id); // Primary Key, manually managed

            modelBuilder.Entity<EnumData>()
                .Property(x => x.Id)
                .ValueGeneratedNever(); // Prevent auto-increment to allow manual assignment

            modelBuilder.Entity<EnumData>()
                .Property(x => x.EnumType)
                .HasMaxLength(20) // Set a maximum length for the description
                .IsRequired(); // Ensure the type is not null

            modelBuilder.Entity<EnumData>()
                .Property(x => x.Description)
                .HasMaxLength(100); // Set a maximum length for the description

            modelBuilder.AddEnumData();
        }

        private static void AddEnumData(this ModelBuilder modelBuilder) 
        {
            // Seed data for EnumData
            modelBuilder.Entity<EnumData>().HasData(
                // RecordStatus enum values
                new EnumData { Id = 1, EnumType = "RecordStatus", Description = "Draft" },
                new EnumData { Id = 2, EnumType = "RecordStatus", Description = "New" },
                new EnumData { Id = 3, EnumType = "RecordStatus", Description = "Active" },
                new EnumData { Id = 4, EnumType = "RecordStatus", Description = "Archived" },
                new EnumData { Id = 5, EnumType = "RecordStatus", Description = "Deleted" },

                // EdgeTypeEnum values
                new EnumData { Id = 10, EnumType = "EdgeTypeEnum", Description = "Details" },
                new EnumData { Id = 11, EnumType = "EdgeTypeEnum", Description = "Items" },
                new EnumData { Id = 12, EnumType = "EdgeTypeEnum", Description = "Group" },

                // Recurrence frequency
                new EnumData { Id = 90, EnumType = "RecurrenceFrequency", Description = "Daily" },
                new EnumData { Id = 91, EnumType = "RecurrenceFrequency", Description = "Weekly" },
                new EnumData { Id = 92, EnumType = "RecurrenceFrequency", Description = "Monthly" },
                new EnumData { Id = 93, EnumType = "RecurrenceFrequency", Description = "Yearly" },
                new EnumData { Id = 94, EnumType = "RecurrenceFrequency", Description = "Minutely" },
                new EnumData { Id = 95, EnumType = "RecurrenceFrequency", Description = "Secondly" },

                // Recipe values
                new EnumData { Id = 100, EnumType = "EdgeTypeEnum", Description = "Recipe" },

                // Calendar values
                new EnumData { Id = 110, EnumType = "EdgeTypeEnum", Description = "Calendar" }

            );
        }
    }
}
