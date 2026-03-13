# EF Core Migrations Without `IDesignTimeDbContextFactory`

This document explains **how `dotnet ef` works without a design-time factory**, and why this setup is intentional and preferred in the Watson/Bureau architecture.

---

### 1. Concrete DbContext lives in SQL Server project

```csharp
internal sealed class SqlServerNodesContext : NodesContext
{
    public SqlServerNodesContext(DbContextOptions<SqlServerNodesContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SqlServerNodesContext).Assembly);
    }
}
```

---

### 2. DbContext is registered in DI (runtime + design-time)

```csharp
services.AddDbContext<SqlServerNodesContext>(options =>
{
    options.UseSqlServer(
        configuration.GetConnectionString("WatsonNodes"),
        sql =>
        {
            sql.MigrationsAssembly(typeof(SqlServerNodesContext).Assembly.FullName);
        });
});
```


## Why Migrations Still End Up in the SQL Server Project

This line is the key:

```csharp
sql.MigrationsAssembly(typeof(SqlServerNodesContext).Assembly.FullName);
```

## Running `dotnet ef`

### Add migration

```bash
dotnet ef migrations add InitialCreate \
  --project Watson.Nodes.Data.SqlServer \
  --startup-project Watson.Nodes.Api \
  --context Watson.Nodes.Data.SqlServer.Contexts.SqlServerNodesContext
```

### Apply migration

```bash
dotnet ef database update \
  --project Watson.Nodes.Data.SqlServer \
  --startup-project Watson.Nodes.Api \
  --context Watson.Nodes.Data.SqlServer.Contexts.SqlServerNodesContext
```

---

## Runtime Migrations

```csharp
app.Services.MigrateWatsonNodes();
```

Internally calls:

```csharp
db.Database.Migrate();
```

---

## When a Factory Is Required

You need `IDesignTimeDbContextFactory` only if:

- No startup project exists
- DbContext is not registered in DI
- DbContext constructor cannot be satisfied
- You want different design-time configuration

---

## Summary

- Factories are optional
- Prefer real app startup
- Provider owns migrations
- Runtime and design-time stay aligned
