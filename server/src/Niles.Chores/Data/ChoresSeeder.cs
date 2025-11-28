using Microsoft.EntityFrameworkCore;
using Niles.Chores.Contexts;
using Niles.Chores.Models;

namespace Niles.Chores.Data
{
    public static class ChoresSeeder
    {
        public static async Task SeedAsync(DbContext context)
        {
            // Check if data already exists
            var choresContext = (ChoresContext)context;
            if (await choresContext.Chores.AnyAsync())
            {
                Console.WriteLine("Database already contains data. Skipping seed.");
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var utcNow = now.UtcDateTime;

            // Create sample chores
            var chores = new List<ChoreDb>
            {
                new ChoreDb
                {
                    Title = "Vacuum Living Room",
                    Description = "Vacuum all carpets and rugs in the living room",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Clean Bathroom",
                    Description = "Clean toilet, sink, mirror, and shower",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Mop Kitchen Floor",
                    Description = "Sweep and mop the kitchen floor",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Dust Furniture",
                    Description = "Dust all furniture surfaces and shelves",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 2,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Clean Windows",
                    Description = "Clean interior windows and window sills",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Organize Closet",
                    Description = "Sort and organize bedroom closet",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Deep Clean Refrigerator",
                    Description = "Remove all items, clean shelves and drawers",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 12,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Wash Bedding",
                    Description = "Wash all bed sheets, pillowcases, and duvet covers",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 2,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Clean Oven",
                    Description = "Deep clean the oven interior and racks",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 16,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Take Out Trash",
                    Description = "Empty all trash bins and take to curb",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Take Out Trash Biological",
                    Description = "Empty all trash bins and take to curb",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Take Out Trash Plastic",
                    Description = "Empty all trash bins and take to curb",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Take Out Trash Papir",
                    Description = "Empty all trash bins and take to curb",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Obrisati prasinu",
                    Description = "Empty all trash bins and take to curb",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                }
            };

            choresContext.Chores.AddRange(chores);
            await choresContext.SaveChangesAsync();

            Console.WriteLine($"Created {chores.Count} chores.");

            // Create some housekeeping records with completed chores
            var today = DateOnly.FromDateTime(utcNow);
            var lastWeek = today.AddDays(-7);
            var twoWeeksAgo = today.AddDays(-14);
            var threeWeeksAgo = today.AddDays(-21);

            var housekeepingRecords = new List<HousekeepingDb>();

            // Today's housekeeping
            var todayHousekeeping = new HousekeepingDb
            {
                Timestamp = new DateTimeOffset(today.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(10))), TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(45),
                Note = "Quick morning cleanup",
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "seeder",
                UpdatedBy = "seeder"
            };
            housekeepingRecords.Add(todayHousekeeping);

            // Last week's housekeeping
            var lastWeekHousekeeping = new HousekeepingDb
            {
                Timestamp = new DateTimeOffset(lastWeek.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(14))), TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(60),
                Note = "Weekly deep clean",
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "seeder",
                UpdatedBy = "seeder"
            };
            housekeepingRecords.Add(lastWeekHousekeeping);

            // Two weeks ago
            var twoWeeksAgoHousekeeping = new HousekeepingDb
            {
                Timestamp = new DateTimeOffset(twoWeeksAgo.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(11))), TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(50),
                Note = "Regular maintenance",
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = "seeder",
                UpdatedBy = "seeder"
            };
            housekeepingRecords.Add(twoWeeksAgoHousekeeping);

            choresContext.Housekeeping.AddRange(housekeepingRecords);
            await choresContext.SaveChangesAsync();

            Console.WriteLine($"Created {housekeepingRecords.Count} housekeeping records.");

            // Create completed chores
            var completedChores = new List<CompletedChoreDb>
            {
                // Today's completed chores
                new CompletedChoreDb { ChoreId = chores[0].Id, HousekeepingId = todayHousekeeping.Id },
                new CompletedChoreDb { ChoreId = chores[1].Id, HousekeepingId = todayHousekeeping.Id },
                new CompletedChoreDb { ChoreId = chores[2].Id, HousekeepingId = todayHousekeeping.Id },

                // Last week's completed chores
                new CompletedChoreDb { ChoreId = chores[0].Id, HousekeepingId = lastWeekHousekeeping.Id },
                new CompletedChoreDb { ChoreId = chores[1].Id, HousekeepingId = lastWeekHousekeeping.Id },
                new CompletedChoreDb { ChoreId = chores[2].Id, HousekeepingId = lastWeekHousekeeping.Id },
                new CompletedChoreDb { ChoreId = chores[3].Id, HousekeepingId = lastWeekHousekeeping.Id },
                new CompletedChoreDb { ChoreId = chores[7].Id, HousekeepingId = lastWeekHousekeeping.Id },

                // Two weeks ago completed chores
                new CompletedChoreDb { ChoreId = chores[0].Id, HousekeepingId = twoWeeksAgoHousekeeping.Id },
                new CompletedChoreDb { ChoreId = chores[1].Id, HousekeepingId = twoWeeksAgoHousekeeping.Id },
                new CompletedChoreDb { ChoreId = chores[2].Id, HousekeepingId = twoWeeksAgoHousekeeping.Id },
                new CompletedChoreDb { ChoreId = chores[9].Id, HousekeepingId = twoWeeksAgoHousekeeping.Id }
            };

            choresContext.CompletedChores.AddRange(completedChores);
            await choresContext.SaveChangesAsync();

            Console.WriteLine($"Created {completedChores.Count} completed chore records.");

            // Create some critical chores (for chores that haven't been completed recently)
            var criticalChores = new List<CriticalChoreDb>
            {
                new CriticalChoreDb
                {
                    ChoreId = chores[4].Id, // Clean Windows - hasn't been done
                    Note = "Windows are getting dirty, need attention soon",
                    CompletedChoreId = null,
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now.AddDays(-3),
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new CriticalChoreDb
                {
                    ChoreId = chores[5].Id, // Organize Closet - hasn't been done
                    Note = "Closet is getting cluttered, should organize this week",
                    CompletedChoreId = null,
                    CreatedAt = now.AddDays(-5),
                    UpdatedAt = now.AddDays(-5),
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                }
            };

            choresContext.CriticalChores.AddRange(criticalChores);
            await choresContext.SaveChangesAsync();

            Console.WriteLine($"Created {criticalChores.Count} critical chore records.");
            Console.WriteLine("Database seeding completed successfully!");
        }

        public static async Task ClearAndSeedAsync(DbContext context)
        {
            Console.WriteLine("Clearing existing data...");
            
            var choresContext = (ChoresContext)context;
            choresContext.CriticalChores.RemoveRange(await choresContext.CriticalChores.ToListAsync());
            choresContext.CompletedChores.RemoveRange(await choresContext.CompletedChores.ToListAsync());
            choresContext.Housekeeping.RemoveRange(await choresContext.Housekeeping.ToListAsync());
            choresContext.Chores.RemoveRange(await choresContext.Chores.ToListAsync());
            
            await choresContext.SaveChangesAsync();
            
            Console.WriteLine("Existing data cleared.");
            
            await SeedAsync(context);
        }
    }
}

