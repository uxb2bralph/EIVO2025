# 🎯 HNB.Api 後台 API 設計架構文件規劃

本次工作僅產出文件規劃，不執行程式修改。目標是整理一份描述 `HNB.Api/` 現況的後台 API 設計架構 `.md` 文件，內容涵蓋使用者指定的三個主題：`程式設計指引`、`底層各項掛載元件模組`、`其他注意事項`。

文件應以目前程式碼為依據，避免寫成純理想規範；若需加入建議事項，應清楚標示為「建議」或「後續可優化項目」，與「現況」區分。

已確認的主要依據如下：

- `HNB.Api/Program.cs`
  - 啟動流程使用 `WebApplication.CreateBuilder`。
  - 掛載 `NLog` 作為 logging provider。
  - 掛載 `CORS` 政策 `VueFrontend`，來源由 `Cors:AllowedOrigins` 設定。
  - 掛載 `JWT Bearer Authentication`，支援 `Authorization: Bearer <token>` 與 HttpOnly cookie fallback。
  - 掛載 `Authorization`。
  - 掛載 `EF Core + SQL Server + LazyLoadingProxies`。
  - 註冊 `IJwtService`、`IUnitOfWork`、多個查詢服務與 `ICryptoService`。
  - 掛載 `Controllers`、`OpenAPI`、`SwaggerGen`。
  - Middleware pipeline 依序包含 `ExceptionMiddleware`、`HTTPS Redirection`、`CORS`、`Authentication`、`Authorization`、`MapControllers()`。
  - Swagger 僅在 `Development` 啟用。

- `HNB.Api/appsettings.json`
  - 主要設定區塊為 `Logging`、`ConnectionStrings`、`Jwt`、`Cors`。
  - 文件可說明後台 API 與前端 `HNB.Web` 的整合依賴，如 Audience 與 AllowedOrigins。

- `HNB.Api/Controllers/AuthController.cs`
  - 展示登入、登出、目前登入者資訊的 API 設計模式。
  - 使用 `ApiResult<T>` 與 `ProblemDetails` 回應。
  - 登入成功會簽發 JWT 並寫入 HttpOnly cookie。
  - 驗證資料來源為 `HNBDbContext`。

- `HNB.Api/Controllers/QueryController.cs`
  - 展示主要業務查詢 API 的實作風格：
    - `[ApiController]`
    - `[Route("api/[controller]")]`
    - `[Authorize]`
    - constructor injection
    - async action + `CancellationToken`
    - `ProblemDetails` / `NotFound` / `BadRequest` / `500` 回應模式
  - 可作為「Controller 設計指引」的主要樣板來源。

- `HNB.Api/Data/HNBDbContext.cs`
  - 顯示資料庫存取核心由 `HNBDbContext` 管理。
  - 含大量 `DbSet<T>` 與 Fluent API 設定，屬於核心資料模型層。

- `HNB.Api/Repositories/GenericRepository.cs`
  - 提供通用 CRUD / 查詢封裝。

- `HNB.Api/Repositories/UnitOfWork.cs`
- `HNB.Api/Interfaces/IUnitOfWork.cs`
  - 顯示目前採用 `Repository + Unit of Work` 的資料存取模式。
  - `UnitOfWork` 管理 repository 懶載入、`SaveChangesAsync()`、巢狀 transaction begin/commit/rollback。

- `HNB.Api/Services/QueryLcListService.cs`
  - 展示 service 層負責商業查詢邏輯與 EF 查詢投影。
  - 可在文件中說明 service 層不應將商業邏輯堆入 controller。
  - 同時可列入注意事項：部分狀態/類型映射仍有 `TODO`。

- `.github/instructions/csharp.instructions.md`
  - 應納入文件中的一致性規範：
    - Controller 繼承 `ControllerBase`，使用 `[ApiController]` 與 `[Route("api/[controller]")]`
    - 服務介面 `I<Entity>Service`、實作 `<Entity>Service`
    - DTO 命名與敏感欄位排除
    - 全域例外處理、`ProblemDetails`
    - JWT、CORS、安全性基本要求
    - async 命名、私有欄位命名、nullable 啟用等

建議文件內容方向：

1. 文件抬頭與目的
   - 說明此文件用於整理 `HNB.Api` 現行後台 API 架構，供新功能開發、維護與交接參考。

2. 架構概觀
   - 簡述整體請求流：`HNB.Web / Client` → `Controller` → `Service` → `Repository / UnitOfWork` → `HNBDbContext` → `SQL Server`。
   - 說明認證驗證與 middleware 在請求流程中的位置。

3. 程式設計指引
   - Controller 設計規範
   - Service 設計規範
   - Repository / UnitOfWork 使用原則
   - DTO / 回應模型規範
   - 例外處理與 HTTP 狀態碼原則
   - 安全性設計要點
   - 命名、非同步、nullable 等 C# coding style 摘要

4. 底層各項掛載元件模組
   - Logging：`NLog`
   - Configuration：`appsettings.json` + `AppSettings`
   - CORS：`VueFrontend`
   - Authentication：`JwtBearer`
   - Authorization
   - Database：`EF Core`、`SqlServer`、`LazyLoadingProxies`
   - Middleware：`ExceptionMiddleware`
   - API 文件：`OpenAPI` / `Swagger`
   - DI 模組：各 `Scoped` service、`UnitOfWork`、`CryptoService`

5. 其他注意事項
   - `AppSettings` 內含 fallback 設定，部署時須由正式環境設定覆蓋，避免使用預設敏感值。
   - JWT 支援 header 與 cookie 兩種來源，前後端串接需一致。
   - Swagger 僅開發環境可用。
   - `HNBDbContext` 規模大，資料表與關聯異動需審慎評估。
   - Lazy Loading 便利但需注意查詢效能與 N+1 問題。
   - 查詢服務中尚有對照表 `TODO`，文件可註記為待補完項目。
   - CORS `AllowedOrigins` 必須與 `HNB.Web` 實際部署網址同步。
   - `ExceptionMiddleware` 目前會回傳 `exception.Message`，文件可標示此為現況並提醒正式環境需注意錯誤訊息揭露。

6. 文件建議位置
   - 優先建議：`HNB.Api/Docs/BackendApiArchitecture.md`
   - 若專案希望集中管理文件，也可放在 repository root 的 `docs/` 目錄。

文件建議採用 markdown 標題層次清楚列點，並適度加入表格：
- 元件模組表
- 分層責任表
- 注意事項清單

**Progress**: 0% [░░░░░░░░░░]

**Last Updated**: 2026-05-25 02:32:20

## 📝 Plan Steps
-  **建立文件位置 — 以 `HNB.Api/Docs/BackendApiArchitecture.md` 為優先，若既有文件慣例不同則改放於 repo 級 `docs/`。**
-  **撰寫文件摘要與架構概觀 — 說明 `HNB.Api` 的角色、適用範圍與整體請求流向。**
-  **整理程式設計指引 — 依 `Controller`、`Service`、`Repository/UnitOfWork`、`DTO`、錯誤處理、安全性與 C# 規範分節說明。**
-  **整理掛載元件模組 — 依 `Program.cs` 與設定檔列出 logging、CORS、JWT、Authorization、EF Core、Swagger、Middleware、DI 等組成。**
-  **補充其他注意事項 — 納入部署設定、敏感資訊、Lazy Loading、Swagger 環境限制、TODO 對照表與錯誤訊息揭露等提醒。**
-  **加入檔案參考資訊 — 在文件末段附上主要參考檔案清單，方便後續維護人員追溯依據。**
-  **確認文件語氣與內容 — 保持為現況導向的技術文件，明確區分已實作架構與後續建議。**

