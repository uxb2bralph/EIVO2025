---
description: "Scaffold EF Core DbContext and entities from an existing SQL Server database into HNB.Api. Use when: scaffold dbcontext, scaffold entities, ef scaffold, reverse engineer database, 產生 DbContext, 從資料庫產生 entities, scaffold datacontext."
argument-hint: "Connection string (e.g. Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True)"
agent: "agent"
---

Scaffold an EF Core `HNBDbContext` and all entity classes into the `HNB.Api` project from the provided connection string.

## Connection String

$args

If no connection string was provided, ask the user for one before proceeding.

## Steps

### 1. Add Required NuGet Packages

Ensure `HNB.Api/HNB.Api.csproj` contains the following references (versions are managed centrally in `Directory.Packages.props`):

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Design">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

If any are missing, add them to the `<ItemGroup>` that already contains `Microsoft.AspNetCore.Authentication.JwtBearer`.

### 2. Run Scaffold Command

Run in terminal from `HNB.Api/`:

```
dotnet ef dbcontext scaffold "<CONNECTION_STRING>" Microsoft.EntityFrameworkCore.SqlServer \
  --output-dir Models/Entities \
  --context-dir Data \
  --context HNBDbContext \
  --namespace HNB.Api.Models.Entities \
  --context-namespace HNB.Api.Data \
  --data-annotations \
  --no-onconfiguring \
  --force
```

Replace `<CONNECTION_STRING>` with the provided connection string.

### 3. Post-Scaffold Verification

After the command completes:
- Confirm `Data/HNBDbContext.cs` exists with namespace `HNB.Api.Data`
- Confirm `Models/Entities/` contains the scaffolded entity `.cs` files
- Report how many entity files were generated

### 4. Register DbContext in Program.cs

Check `Program.cs`. If `HNBDbContext` is not yet registered, add:

```csharp
builder.Services.AddDbContext<HNBDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

Add `using HNB.Api.Data;` and `using Microsoft.EntityFrameworkCore;` if not already present via global usings.

### 5. Store Connection String

Check `appsettings.json`. If a `ConnectionStrings` section is missing or `DefaultConnection` is absent, add:

```json
"ConnectionStrings": {
  "DefaultConnection": "<CONNECTION_STRING>"
}
```

Also add the same key to `appsettings.Development.json` if it exists, pointing to the dev database.

## Conventions (from [csharp.instructions.md](../.github/instructions/csharp.instructions.md))

- Entity namespace: `HNB.Api.Models.Entities`
- DbContext namespace: `HNB.Api.Data`
- DbContext class name: `HNBDbContext`
- Entity output path: `HNB.Api/Models/Entities/`
- Context output path: `HNB.Api/Data/`
- Use `--data-annotations` to apply `[Table]`, `[Column]`, `[Key]` attributes
- Use `--no-onconfiguring` so the connection string is NOT hardcoded in `OnConfiguring`
- Composite primary keys and relationships are configured via Fluent API in `OnModelCreating`
