using Bureau;
using Bureau.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Niles.Chores.Contexts;
using Niles.Chores.Models;

namespace Niles.Chores.Data
{
    internal class ChoresSeeder : IChoresSeeder
    {
        private const string DatabaseAlreadyFullMessage = "Database already contains data. Skipping seed.";
        private const string SEEDER = "seeder";
        private readonly ILogger<ChoresSeeder> _logger;
        private readonly ChoresContext _context;
        private readonly TimeProvider _timeProvider;

        public ChoresSeeder(ILogger<ChoresSeeder> logger, ChoresContext context, TimeProvider timeProvider)
        {
            _logger = logger;
            _context = context;
            _timeProvider = timeProvider;
        }

        public async Task<Result> ClearAndSeedTestAsync(CancellationToken cancellationToken = default)
        {
            // this should be done more effieciently with TRUNCATE or similar
            // but since this is for test purposes, it's acceptable for now
            _context.CriticalChores.RemoveRange(await _context.CriticalChores.ToListAsync(cancellationToken));
            _context.CompletedChores.RemoveRange(await _context.CompletedChores.ToListAsync(cancellationToken));
            _context.Housekeeping.RemoveRange(await _context.Housekeeping.ToListAsync(cancellationToken));
            _context.Chores.RemoveRange(await _context.Chores.ToListAsync(cancellationToken));

            await _context.SaveChangesAsync(cancellationToken);
            _logger.Info("Existing data cleared.");

            return await SeedTestAsync(cancellationToken);
        }

        public Result SeedTest()
        {
            return SeedTestAsync().GetAwaiter().GetResult();
        }

        public async Task<Result> SeedChoresAsync(CancellationToken cancellationToken = default)
        {
            if (await _context.Chores.AnyAsync(cancellationToken))
            {
                _logger.LogWarning(DatabaseAlreadyFullMessage);
                return DatabaseAlreadyFullMessage;
            }
            await CreateChoresAsync(cancellationToken);
            return true;
        }

        public async Task<Result> SeedTestAsync(CancellationToken cancellationToken = default)
        {
            // Check if data already exists
            if (await _context.Chores.AnyAsync(cancellationToken))
            {
                _logger.LogWarning(DatabaseAlreadyFullMessage);
                return DatabaseAlreadyFullMessage;
            }

            DateTimeOffset now = _timeProvider.GetUtcNow();
            DateTime utcNow = now.UtcDateTime;

            List<ChoreDb> chores = await CreateChoresAsync(cancellationToken);
            // Create some housekeeping records with completed chores
            DateOnly today = DateOnly.FromDateTime(utcNow);
            DateOnly lastWeek = today.AddDays(-7);
            DateOnly twoWeeksAgo = today.AddDays(-14);

            List<HousekeepingDb> housekeepingRecords = new List<HousekeepingDb>();

            // Today's housekeeping
            HousekeepingDb todayHousekeeping = new HousekeepingDb
            {
                Timestamp = new DateTimeOffset(today.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(10))), TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(45),
                Note = "Quick morning cleanup",
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = SEEDER,
                UpdatedBy = SEEDER
            };
            housekeepingRecords.Add(todayHousekeeping);

            // Last week's housekeeping
            HousekeepingDb lastWeekHousekeeping = new HousekeepingDb
            {
                Timestamp = new DateTimeOffset(lastWeek.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(14))), TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(60),
                Note = "Weekly deep clean",
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = SEEDER,
                UpdatedBy = SEEDER
            };
            housekeepingRecords.Add(lastWeekHousekeeping);

            // Two weeks ago
            HousekeepingDb twoWeeksAgoHousekeeping = new HousekeepingDb
            {
                Timestamp = new DateTimeOffset(twoWeeksAgo.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(11))), TimeSpan.Zero),
                Duration = TimeSpan.FromMinutes(50),
                Note = "Regular maintenance",
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = SEEDER,
                UpdatedBy = SEEDER
            };
            housekeepingRecords.Add(twoWeeksAgoHousekeeping);

            _context.Housekeeping.AddRange(housekeepingRecords);
            await _context.SaveChangesAsync(cancellationToken);

            // Create completed chores
            List<CompletedChoreDb> completedChores = new List<CompletedChoreDb>
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

            _context.CompletedChores.AddRange(completedChores);
            await _context.SaveChangesAsync(cancellationToken);

            // Create some critical chores (for chores that haven't been completed recently)
            List<CriticalChoreDb> criticalChores = new List<CriticalChoreDb>
                {
                    new CriticalChoreDb
                    {
                        ChoreId = chores[6].Id, // Apartment — Clean Glass on Doors and Cabinets
                        Note = "Glass on doors and cabinets is cloudy, needs cleaning",
                        CompletedChoreId = null,
                        CreatedAt = now.AddDays(-3),
                        UpdatedAt = now.AddDays(-3),
                        CreatedBy = SEEDER,
                        UpdatedBy = SEEDER
                    },
                    new CriticalChoreDb
                    {
                        ChoreId = chores[38].Id, // Hallway — Wipe Closet Interior
                        Note = "Hallway closet is dusty, needs interior cleaning",
                        CompletedChoreId = null,
                        CreatedAt = now.AddDays(-5),
                        UpdatedAt = now.AddDays(-5),
                        CreatedBy = SEEDER,
                        UpdatedBy = SEEDER
                    }
                };

            _context.CriticalChores.AddRange(criticalChores);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.Info("Created: {0} chores; {1} housekeeping records; {2} completed chore records; {3} critical chore records.",
                chores.Count.ToString(), housekeepingRecords.Count.ToString(), completedChores.Count.ToString(), criticalChores.Count.ToString());
            return true;
        }

        private async Task<List<ChoreDb>> CreateChoresAsync(CancellationToken cancellationToken = default)
        {
            DateTimeOffset now = _timeProvider.GetUtcNow();

            // Create sample chores
            List<ChoreDb> chores = new List<ChoreDb>
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
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Apartment — Dust Surfaces",
                    Description = "Clean dust from tables, shelves, cabinets, TV unit, and other flat surfaces.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Apartment — Mop Floors",
                    Description = "Clean floors with wet wiping using appropriate cleaner for the floor type.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Apartment — Ventilation",
                    Description = "Open windows and air out the space for 10–15 minutes.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Apartment — Plant Maintenance",
                    Description = "Water plants, remove dry leaves, check plant condition.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Apartment — Clean Glass on Doors and Cabinets",
                    Description = "Clean glass surfaces and mirrors with glass cleaner or microfiber cloth.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Apartment — Clean Windows (Interior)",
                    Description = "Clean interior glass in all rooms.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "[Q] Apartment — Clean Light Fixtures",
                    Description = "Remove shades/lamps and remove dust from inside and outside.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 12,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                // 🌿 BALCONY
                new ChoreDb
                {
                    Title = "Balcony — Clean Floor",
                    Description = "Sweep or wash the balcony floor, remove debris and leaves.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Balcony — Wipe Railings and Furniture",
                    Description = "Wipe the railing, chairs, table, and other outdoor surfaces.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Balcony — Plant extra work",
                    Description = "Replenish soil.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 30,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
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
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Kitchen — Clean Sink and Faucets",
                    Description = "Remove limescale and soap residue from sink and faucet.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Kitchen — Wash/Replace Kitchen Towels and Sponges",
                    Description = "Wash/replace towels and sponges due to bacteria.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Kitchen — Wipe Microwave Interior",
                    Description = "Wipe microwave walls and remove stains.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Kitchen — Oven: Quick Clean",
                    Description = "Wipe bottom and racks after use to prevent grease buildup.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 2,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Kitchen — Oven: Deep Clean",
                    Description = "Clean oven interior with cleaner, remove racks.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Kitchen — Clean Refrigerator",
                    Description = "Wipe shelves and check for expired or spoiled food.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Kitchen — Freezer Organization",
                    Description = "Sort food items, check dates and arrangement.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 12,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Kitchen — Defrost Freezer",
                    Description = "Turn off appliance and remove ice (if not no-frost).",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 14,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Kitchen — Coffee Machine: Basic Clean",
                    Description = "Empty water reservoir and wipe exterior parts.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Kitchen — Coffee Machine: Cleaning Cycle",
                    Description = "Run descaling program or automatic cleaning.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Kitchen — Coffee Grinder: Clean",
                    Description = "Disassemble grinder and remove oil and coffee residue.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Kitchen — Kettle: Remove Limescale",
                    Description = "Clean kettle with citric acid.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Kitchen — Range Hood Filter: Clean",
                    Description = "Wash filter by hand or in dishwasher.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 12,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "[Q] Kitchen — Refrigerator Rear Grilles: Clean",
                    Description = "Wipe grilles for better cooling and energy savings.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 12,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
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
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Living Room — Robot Vacuum: Empty Container",
                    Description = "Remove and empty the robot vacuum container, clean filter and check the brushes.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Living Room — Robot Vacuum: Clean Sensors",
                    Description = "Clean sensors, wheels, and side brushes.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Living Room — Leather Furniture: Regular Care",
                    Description = "Wipe leather surfaces with special cloth and cleaner.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 2,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Living Room — Leather Furniture: Deep Care",
                    Description = "Apply leather balm to prevent drying out.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 14,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Living Room — Air Purifier: Clean Filter",
                    Description = "Remove filter and vacuum or wash if washable.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Living Room — Air Purifier: Replace Filter",
                    Description = "Install new filter according to manufacturer's recommendation.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 16,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
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
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Dining Room — Clean Chairs",
                    Description = "Wipe seat, backrest, and frame.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Dining Room — Deep Clean Wood Surfaces",
                    Description = "Use wood care product for long-term wood protection.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
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
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Hallway — Organize Shoes",
                    Description = "Sort shoes, remove seasonal or unnecessary pairs.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 14,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Hallway — Wipe Closet Interior",
                    Description = "Wipe/vacuum shelves, drawers, and interior surfaces of dust.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
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
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Bathroom — Clean Sink",
                    Description = "Remove soap residue and limescale from sink and faucet.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Bathroom — Clean Bathtub or Shower",
                    Description = "Clean glass/walls of limescale and soap buildup.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Bathroom — Clean Mirror",
                    Description = "Wipe mirror without streaks.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 2,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Bathroom — Mop Floors",
                    Description = "Clean floors and edges along grout and joints.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Bathroom — Put anti-limescale tabs in toilet",
                    Description = "Use anti-limescale tabs",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 14,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "[Q] Bathroom — Wash Shower Curtain",
                    Description = "Remove curtain and wash in washing machine or by hand.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 15,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Bathroom — Deep Clean Grout",
                    Description = "Remove mold or buildup with steam cleaner or cleaner.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Bathroom — Clean Drain",
                    Description = "Use drain cleaner or baking soda + vinegar.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                // 🧺 WASHING MACHINE / DRYER
                new ChoreDb
                {
                    Title = "Bathroom — Washing Machine: Cleaning Cycle",
                    Description = "Run 90°C program with drum cleaner. Add tabs for limescale",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Bathroom — Dryer: Clean Filter",
                    Description = "Clean lint filter after each drying cycle.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Bathroom — Dryer: Clean Lower Filter",
                    Description = "Vacuum and brush lower filter.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "[Q] Bathroom — Dryer: Clean Condenser",
                    Description = "Rinse condenser under water (if model requires it).",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                // 🛏️ BEDROOM
                new ChoreDb
                {
                    Title = "Bedroom — Change Bedding",
                    Description = "Replace sheet, pillowcases, and duvet cover.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 2,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Bedroom — Dust Nightstands",
                    Description = "Clean lamp, surfaces, and items.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Bedroom — Vacuum Room",
                    Description = "Focus on area around bed and under furniture.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 1,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Bedroom — Clean Mattress",
                    Description = "Vacuum mattress and spray with anti-mite spray.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 14,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Bedroom — Air Pillows and Duvet",
                    Description = "Expose to air on balcony or window.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "[Q] Bedroom — Deep Clean Mattress",
                    Description = "Steam cleaner or professional cleaning.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 16,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
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
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Office — Disinfect Keyboard and Mouse",
                    Description = "Wipe with alcohol wipes or electronics cleaner.",
                    Type = ChoreType.Maintenance,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Office — Clean Screen",
                    Description = "Use microfiber cloth and alcohol-free monitor cleaner.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 4,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Office — Organize Cables",
                    Description = "Arrange cables, remove excess, and add cable holders.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 8,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                },
                new ChoreDb
                {
                    Title = "Office — Printer Maintenance",
                    Description = "Check toner/ink, clean trays and dust.",
                    Type = ChoreType.Extra,
                    WeeklyInterval = 14,
                    CreatedAt = now,
                    UpdatedAt = now,
                    CreatedBy = SEEDER,
                    UpdatedBy = SEEDER
                }
            };

            _context.Chores.AddRange(chores);
            await _context.SaveChangesAsync(cancellationToken);
            return chores;
        }

    }
}

