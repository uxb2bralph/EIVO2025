# HNB 16 升級案 — Workspace Instructions

## Project Overview

This is a bank (華南銀行) internal LC system migration: the legacy ASP.NET Core MVC app (`Web/`) is being replaced by a new .NET 10 Web API (`HNB.Api/`) + Vue 3 SPA (`HNB.Web/`).

## Repository Layout

| Folder | Role |
|--------|------|
| `HNB.Api/` | New .NET 10 Web API (active development) |
| `HNB.Web/` | New Vue 3 + Vuetify 3 SPA (active development) |
| `Web/` | **Legacy** ASP.NET Core MVC — read only for reference |
| `Application/` | Business logic services (old layered arch) |
| `ApplicationAbstractions/` | Service interfaces + DTOs (old arch) |
| `Domain/` | EF Core entities + repositories (old arch) |
| `Infrastructure/` | DbContext, external connections, tools (old arch) |
| `E2E/` | Playwright TypeScript E2E tests |
| `UnitTest/` | xUnit unit tests |

## Build & Run

```bash
# New API backend
dotnet watch run --project HNB.Api/HNB.Api.csproj

# New Vue frontend  (in HNB.Web/)
pnpm dev          # dev server at http://localhost:5173
pnpm build        # type-check + Vite build
pnpm type-check   # vue-tsc --noEmit

# Legacy Web (for E2E tests or reference)
dotnet watch run --project Web/Web.csproj --launch-profile httpsFunction

# E2E tests (in E2E/, requires Web running on https://localhost:7192)
npx playwright test
```

## Coding Conventions

- **C# (HNB.Api)**: See [.github/instructions/csharp.instructions.md](.github/instructions/csharp.instructions.md)
- **Vue 3 (HNB.Web)**: See [.github/instructions/vue.instructions.md](.github/instructions/vue.instructions.md)

Key points not in those files:
- Auto-imports are configured — never manually import `ref`, `computed`, `watch`, `useRouter`, `useRoute`, `defineStore` in `.vue` files
- Frontend dev server proxies `/api` → `https://localhost:7000` (the new API)
- JWT is passed as `Authorization: Bearer <token>`; all API routes under `HNB.Api/Controllers/` require `[Authorize]` unless explicitly public

## Architecture — Old Layered Backend

```
ApplicationAbstractions  ←  Application  ←  Web
        ↑                       ↑
      Domain  ←  Infrastructure
```

- Register services with `[ScopedService]` attribute; the DI scanner picks them up automatically
- Schedule jobs with `[Job("Name", "cron", enabled: true)]` + `[ScopedService]` implementing `IJob`
- Use `IClaimsAuthService.GetCurrentUser()` to read the authenticated user in controllers
- Role-based access: `[RequireRoles(Roles.officer, Roles.manager)]` on controller or action

## Database

- SQL Server; EF Core (code-first from database via **EF Core Power Tools** on `Domain/`)
- Run `DbPatch` console app to apply SQL scripts: configure `dbpatch.json` connection string + `Domain/DbScripts` path before publishing

## E2E Test Conventions

- Tests mirror `Web\Areas\{Area}\Controllers\{Controller}.cs` → `E2E/tests/ui/{Area}/{Controller}/`
- See [E2E/README.md](E2E/README.md) for setup details

## Existing Agents & Prompts

| File | Purpose |
|------|---------|
| `.github/agents/migrate-html-batch.agent.md` | Batch-migrate legacy HTML pages → Vue 3 components |
| `.github/agents/verify-vue-migration.agent.md` | Verify Vue migration completeness against original HTML |
| `.github/prompts/migrate-html-to-vue3.prompt.md` | Single-page HTML → Vue 3 migration |
| `.github/prompts/extract-script-setup-to-composable.md` | Refactor inline logic into composables |
