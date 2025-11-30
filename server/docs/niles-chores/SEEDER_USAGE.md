# Database Seeder Usage Guide

The ChoresSeeder provides sample data for development and testing. It creates:
- **10 sample chores** (mix of Maintenance and Extra types)
- **3 housekeeping records** (today, last week, two weeks ago)
- **12 completed chore records** (linking chores to housekeeping)
- **2 critical chores** (for testing the critical feature)

## Usage Methods

### 1. Automatic Seeding (Development Mode)

The seeder runs **automatically** when you start the API in **Development** environment:

```bash
# Just run the API - seeding happens automatically
dotnet run --project src/Niles.Chores.Api/Niles.Chores.Api.csproj
```

**Behavior:**
- Only seeds if the database is **empty** (checks if any chores exist)
- Won't overwrite existing data
- Check console output for seeding messages

**Console Output:**
```
Created 10 chores.
Created 3 housekeeping records.
Created 12 completed chore records.
Created 2 critical chore records.
Database seeding completed successfully!
```

### 2. Manual Seeding via API Endpoint

You can manually trigger seeding via HTTP endpoints:

#### Seed (only if empty):
```bash
POST http://localhost:5032/api/seed
```

**Response:**
```json
{
  "message": "Database seeded successfully!"
}
```

**Note:** This will only seed if the database is empty. If data exists, it will skip.

#### Clear and Reseed:
```bash
POST http://localhost:5032/api/seed/clear
```

**Response:**
```json
{
  "message": "Database cleared and seeded successfully!"
}
```

**Warning:** This will **delete all existing data** and reseed!

### 3. Programmatic Usage

You can also use the seeder programmatically in your code:

```csharp
using Niles.Chores.Data;
using Microsoft.EntityFrameworkCore;

// Get your DbContext
var context = serviceProvider.GetRequiredService<ChoresContext>();

// Seed (only if empty)
await ChoresSeeder.SeedAsync(context);

// Or clear and reseed
await ChoresSeeder.ClearAndSeedAsync(context);
```

## Sample Data Created

### Chores (10 total):
1. **Vacuum Living Room** - Maintenance, Weekly
2. **Clean Bathroom** - Maintenance, Weekly
3. **Mop Kitchen Floor** - Maintenance, Weekly
4. **Dust Furniture** - Maintenance, Bi-weekly
5. **Clean Windows** - Maintenance, Monthly (marked as critical)
6. **Organize Closet** - Extra, Every 8 weeks (marked as critical)
7. **Deep Clean Refrigerator** - Extra, Every 12 weeks
8. **Wash Bedding** - Maintenance, Bi-weekly
9. **Clean Oven** - Extra, Every 16 weeks
10. **Take Out Trash** - Maintenance, Weekly

### Housekeeping Records:
- **Today**: 3 completed chores, 45 minutes
- **Last Week**: 5 completed chores, 60 minutes
- **Two Weeks Ago**: 4 completed chores, 50 minutes

### Critical Chores:
- **Clean Windows**: "Windows are getting dirty, need attention soon"
- **Organize Closet**: "Closet is getting cluttered, should organize this week"

## Notes

- Seeding is **safe** - it won't overwrite existing data unless you use `ClearAndSeedAsync`
- The seeder checks if data exists before seeding
- All timestamps use UTC
- Created/Updated by fields are set to "seeder"

## Troubleshooting

**Seeder doesn't run:**
- Check that you're in Development environment
- Check console for error messages
- Verify database connection string is correct

**"Database already contains data" message:**
- This is normal - the seeder skips if data exists
- Use `/api/seed/clear` endpoint to clear and reseed

**Errors during seeding:**
- Check database connection
- Ensure migrations are applied
- Check console/logs for detailed error messages

