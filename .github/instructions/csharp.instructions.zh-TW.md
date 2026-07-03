---
applyTo: "**/*.cs"
description: "HNB.Api .NET 10 Web API 專案的 C# 編碼慣例。適用於撞寫 controllers、services、repositories、entities、DTOs 或任何 C# 後端程式碼。"
---

# C# 後端慣例

## 專案與命名空間

- 目標版本：**.NET 10**（或以上）
- 根命名空間：`HNB.Api`
- 分層命名空間：`HNB.Api.Controllers`、`HNB.Api.Models.Entities`、`HNB.Api.Models.DTOs`、`HNB.Api.Services`、`HNB.Api.Data`、`HNB.Api.Mappings`

## 實體類別（`Models/Entities/`）

- 類別名稱 = 資料表名稱（PascalCase）：`Organization`、`NegoLC`、`PurchaseOrder`
- 屬性名稱需與 DB 欄位名稱完全對應
- 中文欄位名稱使用 `[Column("管理費")]` attribute 對應英文屬性名：
  ```csharp
  [Column("管理費")]
  public decimal? ManagementFee { get; set; }
  ```
- 複合主鍵請在 `IEntityTypeConfiguration<T>` 中以 Fluent API 設定，不要使用 `[Key]`
- 如可推導 FK 關係，請加入 navigation properties（例如 `CompanyID` → `Organization`）
- 實體類別一律加上 `[Table("TableName")]` attribute

## DTOs（`Models/DTOs/`）

- 命名方式：`<Entity>Dto`（讀取）、`Create<Entity>Request`（建立）、`Update<Entity>Request`（更新）
- 不可包含敏感欄位：`Password`、`Password2`、`X509Certificate`、`PKCS12`、`Thumbprint`
- `IDENTITY` 欄位不得出現在 create request DTO 中
- 若可行，優先使用 `record` 型別建立不可變 DTO

## DbContext（`Data/HNBDbContext.cs`）

- 每個實體對應一個 `DbSet<T>`
- 在 `OnModelCreating` 中套用組件內所有 `IEntityTypeConfiguration<T>`：
  ```csharp
  modelBuilder.ApplyConfigurationsFromAssembly(typeof(HNBDbContext).Assembly);
  ```

## Controllers

- 繼承 `ControllerBase`，並加上 `[ApiController]` 與 `[Route("api/[controller]")]`
- 透過建構式注入服務（介面式 DI）
- 標準端點：
  - `GET /api/{entity}` — 分頁列表（query params: `pageIndex`、`pageSize`）
  - `GET /api/{entity}/{id}` — 單筆資料
  - `POST /api/{entity}` — 建立
  - `PUT /api/{entity}/{id}` — 更新
  - `DELETE /api/{entity}/{id}` — 刪除
- 回傳 `ActionResult<T>` 並使用正確 HTTP 狀態碼（200、201、204、400、404）
- 所有 async 方法都必須接受 `CancellationToken`

## Services

- 介面命名：`I<Entity>Service`，並提供 async 方法
- 實作命名：`<Entity>Service`
- 商業邏輯放在這一層，不要塞進 controller
- 注入 `HNBDbContext` 或 repositories

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

## 錯誤處理

- 以全域例外處理 middleware 處理未攔截例外（回傳 500）
- 4xx 錯誤使用 `ProblemDetails`
- 透過 **Data Annotations** 驗證輸入

## 安全性

- 在 `Program.cs` 設定 JWT Bearer 驗證
- 設定允許 Vue 3 前端來源的 CORS policy
- 不可使用原始 SQL 字串串接，必須使用 EF Core LINQ 或參數化查詢
- 在 DTO 映射層以 Mapster `.Ignore()` 過濾敏感欄位（詳見 `MappingRegistry`）

## 程式風格

- 型別明確時使用 `var`，需要提升可讀性時使用明確型別
- 一個檔案只放一個 class
- 所有非同步方法名稱都必須加上 `Async` 後綴
- 私有欄位使用 `_` 前綴
- 可使用 `using` 宣告時，優先不要使用區塊式 `using`
- 啟用 nullable reference types（`<Nullable>enable</Nullable>`）