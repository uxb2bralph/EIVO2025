---
applyTo: "**/*.cs"
description: "C# coding conventions for HNB.Api .NET 10 Web API project. Use when: writing controllers, services, repositories, entities, DTOs, or any C# backend code."
---

# C# Backend Conventions

## Project & Namespace

- Target: **.NET 10** (or later)
- Root namespace: `HNB.Api`
- Layer namespaces: `HNB.Api.Controllers`, `HNB.Api.Models.Entities`, `HNB.Api.Models.DTOs`, `HNB.Api.Services`, `HNB.Api.Data`, `HNB.Api.Mappings`

## Entity Classes (`Models/Entities/`)

- Class name = table name (PascalCase): `Organization`, `NegoLC`, `PurchaseOrder`
- Properties match DB column names exactly
- Chinese column names use `[Column("管理費")]` attribute with an English property name:
  ```csharp
  [Column("管理費")]
  public decimal? ManagementFee { get; set; }
  ```
- Composite primary keys: configure via Fluent API in `IEntityTypeConfiguration<T>`, not `[Key]`
- Navigation properties: add where FK relationships are implied (e.g., `CompanyID` → `Organization`)
- Always include `[Table("TableName")]` attribute on entity classes

## DTOs (`Models/DTOs/`)

- Naming: `<Entity>Dto` (read), `Create<Entity>Request` (create), `Update<Entity>Request` (update)
- Never include sensitive fields: `Password`, `Password2`, `X509Certificate`, `PKCS12`, `Thumbprint`
- IDENTITY columns excluded from create request DTOs
- Use `record` types for immutable DTOs when possible

## DbContext (`Data/HNBDbContext.cs`)

- One `DbSet<T>` per entity
- Use `OnModelCreating` to apply all `IEntityTypeConfiguration<T>` from the assembly:
  ```csharp
  modelBuilder.ApplyConfigurationsFromAssembly(typeof(HNBDbContext).Assembly);
  ```

## Controllers

- Inherit `ControllerBase` with `[ApiController]` and `[Route("api/[controller]")]`
- Inject services via constructor (interface-based DI)
- Standard endpoints:
  - `GET /api/{entity}` — paginated list (`pageIndex`, `pageSize` query params)
  - `GET /api/{entity}/{id}` — single item
  - `POST /api/{entity}` — create
  - `PUT /api/{entity}/{id}` — update
  - `DELETE /api/{entity}/{id}` — delete
- Return `ActionResult<T>` with proper HTTP codes (200, 201, 204, 400, 404)
- Use `CancellationToken` on all async methods

## Services

- Interface: `I<Entity>Service` with async methods
- Implementation: `<Entity>Service`
- Handle business logic here, not in controllers
- Inject `HNBDbContext` or repositories

## Response Envelope

```csharp
// List response
public class PagedResult<T>
{
    public IEnumerable<T> Data { get; set; }
    public int Total { get; set; }
    public string? Message { get; set; }
}

// Single item response
public class ApiResult<T>
{
    public T? Data { get; set; }
    public string? Message { get; set; }
}
```

## Error Handling

- Use a global exception middleware for unhandled exceptions (return 500)
- Return `ProblemDetails` for 4xx errors
- Validate inputs with **Data Annotations**

## Security

- JWT Bearer authentication configured in `Program.cs`
- CORS policy allowing the Vue 3 frontend origin
- Never use raw SQL string concatenation — EF Core LINQ or parameterized queries only
- Sensitive fields filtered at DTO mapping level (Mapster `.Ignore()` in `MappingRegistry`)

## Code Style

- `var` for obvious types, explicit types when clarity is needed
- One class per file
- Async methods always suffixed with `Async`
- Private fields prefixed with `_`
- Use `using` declarations (not blocks) where applicable
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
