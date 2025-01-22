```bash
dotnet ef migrations add InitialCreate -- "{connectionString}"

dotnet ef migrations remove -- "{connectionString}"

dotnet ef database update -- "{connectionString}"

```

# Objects not yet implemented in the db

| Object Name | Description |
|-------------|-------------|
| AccessGrant |  |
| CheckedRecord |  |
| ContactEntry |  |
| ExpenseRecord |  |
| ExternalRecord |  |
| LocationRecord |  |
| MaterialRecord |  |
| NoteRecord |  |
| PermissionLevel |  |

# Objects defined in the db

| Model | Db Table | Description |
|-------|-------------|--------|
| BaseEntry | Record |
| BaseRecord | Record |
| Edge  | Edge   |
| FlexibleRecord | FlexibleRecord |
| EdgeType |  |
| TermEntry | TermEntry |
