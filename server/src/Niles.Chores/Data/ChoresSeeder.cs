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
                // 🧹 GENERAL
                new ChoreDb
                {
                    Title = "Apartment — Vacuum Floors",
                    Description = "Vacuum all floor surfaces in the apartment, including carpets, edges, and under furniture.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Living Room — Robot Vacuum: Empty Container",
                    Description = "Remove and empty the robot vacuum container and check the brushes.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Apartment — Dust Surfaces",
                    Description = "Clean dust from tables, shelves, cabinets, TV unit, and other flat surfaces.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Apartment — Mop Floors",
                    Description = "Clean floors with wet wiping using appropriate cleaner for the floor type.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Apartment — Ventilation",
                    Description = "Open windows and air out the space for 10–15 minutes.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Apartment — Clean Glass on Doors and Cabinets",
                    Description = "Clean glass surfaces and mirrors with glass cleaner or microfiber cloth.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Apartment — Clean Windows (Interior)",
                    Description = "Clean interior glass in all rooms.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Apartment — Clean Light Fixtures",
                    Description = "Remove shades/lamps and remove dust from inside and outside.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                // 🌿 BALCONY
                new ChoreDb
                {
                    Title = "Balcony — Clean Floor",
                    Description = "Sweep or wash the balcony floor, remove debris and leaves.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Balcony — Wipe Railings and Furniture",
                    Description = "Wipe the railing, chairs, table, and other outdoor surfaces.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 2,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Balcony — Plant Maintenance",
                    Description = "Water plants, remove dry leaves, check plant condition.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                // 🍳 KITCHEN
                new ChoreDb
                {
                    Title = "Kitchen — Wipe Countertops",
                    Description = "Clean countertops after food preparation and remove residue.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Kitchen — Clean Sink and Faucets",
                    Description = "Remove limescale and soap residue from sink and faucet.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Kitchen — Wash Kitchen Towels and Sponges",
                    Description = "Wash towels and sponges or replace them due to bacteria.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Kitchen — Wipe Microwave Interior",
                    Description = "Wipe microwave walls and remove stains.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Kitchen — Oven: Quick Clean",
                    Description = "Wipe bottom and racks after use to prevent grease buildup.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 2,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Kitchen — Oven: Deep Clean",
                    Description = "Clean oven interior with cleaner, remove racks.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Kitchen — Clean Refrigerator",
                    Description = "Wipe shelves and check for expired or spoiled food.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Kitchen — Freezer Organization",
                    Description = "Sort food items, check dates and arrangement.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 6,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Kitchen — Defrost Freezer",
                    Description = "Turn off appliance and remove ice (if not no-frost).",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 14,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Kitchen — Coffee Machine: Basic Clean",
                    Description = "Empty water reservoir and wipe exterior parts.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Kitchen — Coffee Machine: Cleaning Cycle",
                    Description = "Run descaling program or automatic cleaning.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 5,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Kitchen — Coffee Grinder: Clean",
                    Description = "Disassemble grinder and remove oil and coffee residue.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Kitchen — Kettle: Remove Limescale",
                    Description = "Clean kettle with cleaner or citric acid.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Kitchen — Range Hood Filter: Clean",
                    Description = "Wash filter by hand or in dishwasher.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Kitchen — Refrigerator Rear Grilles: Clean",
                    Description = "Wipe grilles for better cooling and energy savings.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 12,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                // 🛋️ LIVING ROOM
                new ChoreDb
                {
                    Title = "Living Room — Dust TV and Devices",
                    Description = "Clean screen and electronics with dry microfiber cloth.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Living Room — Robot Vacuum: Empty",
                    Description = "Remove container, clean filter and check brushes.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Living Room — Robot Vacuum: Clean Sensors",
                    Description = "Clean sensors, wheels, and side brushes.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Living Room — Leather Furniture: Regular Care",
                    Description = "Wipe leather surfaces with special cloth and cleaner.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 2,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Living Room — Leather Furniture: Deep Care",
                    Description = "Apply leather balm to prevent drying out.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 14,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Living Room — Air Purifier: Clean Filter",
                    Description = "Remove filter and vacuum or wash if washable.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Living Room — Air Purifier: Replace Filter",
                    Description = "Install new filter according to manufacturer's recommendation.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 16,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                // 🍽️ DINING ROOM
                new ChoreDb
                {
                    Title = "Dining Room — Wipe Dining Table",
                    Description = "Clean surface after meals or when marks are visible.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Dining Room — Clean Chairs",
                    Description = "Wipe seat, backrest, and frame.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 2,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Dining Room — Deep Clean Wood Surfaces",
                    Description = "Use wood care product for long-term wood protection.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                // 🚪 HALLWAY
                new ChoreDb
                {
                    Title = "Hallway — Clean Mirror",
                    Description = "Wipe mirror with glass cleaner or dry microfiber cloth.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 2,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Hallway — Organize Shoes",
                    Description = "Sort shoes, remove seasonal or unnecessary pairs.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 2,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Hallway — Wipe Closet Interior",
                    Description = "Wipe shelves, drawers, and interior surfaces of dust.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                // 🛁 BATHROOM
                new ChoreDb
                {
                    Title = "Bathroom — Clean Toilet Bowl",
                    Description = "Clean interior, seat, and lid with disinfectant cleaner.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Bathroom — Clean Sink",
                    Description = "Remove soap residue and limescale from sink and faucet.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Bathroom — Clean Bathtub or Shower",
                    Description = "Clean glass/walls of limescale and soap buildup.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Bathroom — Clean Mirror",
                    Description = "Wipe mirror without streaks.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 2,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Bathroom — Mop Floors",
                    Description = "Clean floors and edges along grout and joints.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Bathroom — Wash Shower Curtain",
                    Description = "Remove curtain and wash in washing machine or by hand.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 5,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Bathroom — Deep Clean Grout",
                    Description = "Remove mold or buildup with steam cleaner or cleaner.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Bathroom — Clean Drain",
                    Description = "Use drain cleaner or baking soda + vinegar.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                // 🧺 WASHING MACHINE / DRYER
                new ChoreDb
                {
                    Title = "Bathroom — Washing Machine: Cleaning Cycle",
                    Description = "Run 90°C program with drum cleaner.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Bathroom — Dryer: Clean Filter",
                    Description = "Clean lint filter after each drying cycle.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Bathroom — Dryer: Clean Condenser",
                    Description = "Rinse condenser under water (if model requires it).",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                // 🛏️ BEDROOM
                new ChoreDb
                {
                    Title = "Bedroom — Change Bedding",
                    Description = "Replace sheet, pillowcases, and duvet cover.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Bedroom — Dust Nightstands",
                    Description = "Clean lamp, surfaces, and items.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Bedroom — Vacuum Room",
                    Description = "Focus on area around bed and under furniture.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Bedroom — Clean Mattress",
                    Description = "Vacuum mattress and spray with anti-mite spray.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Bedroom — Air Pillows and Duvet",
                    Description = "Expose to air on balcony or window.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Bedroom — Deep Clean Mattress",
                    Description = "Steam cleaner or professional cleaning.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 16,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                // 🖥️ OFFICE
                new ChoreDb
                {
                    Title = "Office — Wipe Desk",
                    Description = "Clean surfaces of dust, crumbs, and fingerprints.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Office — Disinfect Keyboard and Mouse",
                    Description = "Wipe with alcohol wipes or electronics cleaner.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 2,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Office — Clean Screen",
                    Description = "Use microfiber cloth and alcohol-free monitor cleaner.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Office — Organize Cables",
                    Description = "Arrange cables, remove excess, and add cable holders.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new ChoreDb
                {
                    Title = "Office — Printer Maintenance",
                    Description = "Check toner/ink, clean trays and dust.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 10,
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
                    ChoreId = chores[6].Id, // Apartment — Clean Glass on Doors and Cabinets
                    Note = "Glass on doors and cabinets is cloudy, needs cleaning",
                    CompletedChoreId = null,
                    CreatedAt = now.AddDays(-3),
                    UpdatedAt = now.AddDays(-3),
                    CreatedBy = "seeder",
                    UpdatedBy = "seeder"
                },
                new CriticalChoreDb
                {
                    ChoreId = chores[38].Id, // Hallway — Wipe Closet Interior
                    Note = "Hallway closet is dusty, needs interior cleaning",
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

