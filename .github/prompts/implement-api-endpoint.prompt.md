---
description: "在 HNB.Api 實作後台 API endpoint，對應前端 Vue composable 的呼叫。Use when: 實現後台API, implement backend api, 新增 api endpoint, 新增 controller action, 實作 service, 後端實作, scaffold api, create endpoint, 建立後台, 建立 api."
argument-hint: "選填：指定前端 composable 或頁面檔案路徑 (e.g. useQueryAmendApp.ts 或 src/pages/query/query-amend-app.vue)"
agent: "agent"
---

你正在 `HNB.Api/` 為前端 Vue 3 composable 的 API 呼叫實作對應的後端 endpoint。

## 專案架構

```
HNB.Api/
  Controllers/QueryController.cs        ← [Authorize] REST controller
  Interfaces/IQuery<Entity>Service.cs   ← Service 介面
  Services/Query<Entity>Service.cs      ← EF Core 查詢實作
  Models/DTOs/Query<Entity>Dtos.cs      ← Request / Response DTO
  Program.cs                            ← DI 註冊
```

參考實作藍本：
- [QueryController.cs](../../HNB.Api/Controllers/QueryController.cs)
- [IQueryLcAppService.cs](../../HNB.Api/Interfaces/IQueryLcAppService.cs)
- [QueryLcAppService.cs](../../HNB.Api/Services/QueryLcAppService.cs)
- [QueryLcAppDtos.cs](../../HNB.Api/Models/DTOs/QueryLcAppDtos.cs)

## 工作目標

分析 `$argument` 指定的前端 composable / 頁面（未提供則分析當前選取或開啟的檔案），識別其 API 呼叫（URL、HTTP 方法、請求欄位、回應欄位），然後依序完成以下步驟：

---

## 步驟 1：分析前端需求

讀取指定的前端檔案，找出：
- API URL（如 `/query/amend-app`）
- HTTP 方法（GET / POST）
- 請求 body / query 欄位及型別
- 回應資料結構（interface）
- 是否需要分頁（`pageIndex` / `pageSize`）

---

## 步驟 2：建立 DTOs

在 `HNB.Api/Models/DTOs/Query<Entity>Dtos.cs` 建立：

**搜尋 + 分頁（POST）：**

```csharp
/// <summary>查詢 XXX POST Request</summary>
public record Query<Entity>Request
{
    public int PageIndex { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    // ... 依前端欄位對應（camelCase → PascalCase）
}

/// <summary>XXX 清單項目</summary>
public record <Entity>ItemDto
{
    // ... 對應前端 interface 欄位（camelCase → PascalCase）
}
```

**明細查詢（GET by key）——若前端有 `openDetail` 函式：**

```csharp
/// <summary>XXX 明細</summary>
public record <Entity>DetailDto
{
    // 對應前端 interface（camelCase → PascalCase）
    // 字串預設 ""，巢狀物件宣告為 new()，List 預設 []
    public string SomeField { get; init; } = "";
    public List<SubItemDto> SubItems { get; init; } = [];
    public <Entity>SummaryDto Summary { get; init; } = new();
}
```

規則：
- 使用 `record` 型別，屬性加 `{ get; init; }`
- 字串預設值為 `""`, 陣列預設值為 `[]`
- 加上 `/// <summary>` 中文說明

---

## 步驟 3：建立 Service 介面

在 `HNB.Api/Interfaces/IQuery<Entity>Service.cs`：

```csharp
using HNB.Api.Models.DTOs;

namespace HNB.Api.Interfaces;

public interface IQuery<Entity>Service
{
    Task<PagedResult<<Entity>ItemDto>> SearchAsync(Query<Entity>Request request, CancellationToken cancellationToken);

    // 若前端有 openDetail（GET by key）：
    Task<ApiResult<<Entity>DetailDto>> GetDetailAsync(string key, CancellationToken cancellationToken);
}
```

---

## 步驟 4：實作 Service

在 `HNB.Api/Services/Query<Entity>Service.cs`：

```csharp
using HNB.Api.Interfaces;
using HNB.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HNB.Api.Services;

public class Query<Entity>Service(IUnitOfWork unitOfWork) : IQuery<Entity>Service
{
    public async Task<PagedResult<<Entity>ItemDto>> SearchAsync(
        Query<Entity>Request request,
        CancellationToken cancellationToken)
    {
        var query = unitOfWork.<DbSet>.DbSet.AsNoTracking();

        // 篩選條件
        if (!string.IsNullOrEmpty(request.Xxx))
            query = query.Where(e => e.Xxx == request.Xxx);

        // 日期篩選
        if (!string.IsNullOrEmpty(request.DateStart) &&
            DateOnly.TryParse(request.DateStart, out var dateStart))
            query = query.Where(e => e.Date >= dateStart.ToDateTime(TimeOnly.MinValue));

        // 計算總筆數
        var total = await query.CountAsync(cancellationToken);

        // 分頁投影
        var rows = await query
            .OrderByDescending(e => e.Date)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new { ... })
            .ToListAsync(cancellationToken);

        var data = rows
            .Select((r, idx) => new <Entity>ItemDto
            {
                No = (request.PageIndex - 1) * request.PageSize + idx + 1,
                // ... 對應欄位
            })
            .ToList();

        return new PagedResult<<Entity>ItemDto> { Data = data, Total = total };
    }

    // 若有明細查詢（GET by key）：
    public async Task<ApiResult<<Entity>DetailDto>> GetDetailAsync(string key, CancellationToken cancellationToken)
    {
        var row = await unitOfWork.<DbSet>.DbSet
            .AsNoTracking()
            .Where(e => e.Key == key)
            .Select(e => new { /* 投影所需欄位與關聯 */ })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return new ApiResult<<Entity>DetailDto> { Message = "查無資料" };

        var detail = new <Entity>DetailDto
        {
            // ... 對應欄位
        };

        return new ApiResult<<Entity>DetailDto> { Data = detail };
    }
}
```

規則：
- 使用 `IUnitOfWork` 存取 DbSet，不直接注入 `HNBDbContext`
- 投影使用匿名型別 `.Select(e => new { ... })` 再轉 DTO，避免 EF lazy loading
- 狀態對應前端 key（pending/approved/completed/rejected）→ `CurrentLevel` 數值，參考 `QueryLcAppService` 的 `StatusLevelMap` 模式

---

## 步驟 5：在 Controller 新增 Action

在 `HNB.Api/Controllers/QueryController.cs` 注入新 service 並新增 action：

```csharp
/// <summary>查詢 XXX（分頁）</summary>
[HttpPost("<route>")]
public async Task<IActionResult> Search<Entity>(
    [FromBody] Query<Entity>Request request,
    CancellationToken cancellationToken)
{
    try
    {
        var result = await query<Entity>Service.SearchAsync(request, cancellationToken);
        return Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        logger.LogWarning(ex, "Invalid operation in {Action}", nameof(Search<Entity>));
        return BadRequest(new ProblemDetails { Status = 400, Title = ex.Message });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error in {Action}", nameof(Search<Entity>));
        return StatusCode(500, new ProblemDetails { Status = 500, Title = "查詢失敗，請稍後再試。" });
    }
}

// 若有明細查詢（GET by key）：
/// <summary>查詢 XXX 明細</summary>
[HttpGet("<route>/{key}")]
public async Task<IActionResult> Get<Entity>Detail(
    [FromRoute] string key,
    CancellationToken cancellationToken)
{
    try
    {
        var result = await query<Entity>Service.GetDetailAsync(key, cancellationToken);
        if (result.Data is null)
            return NotFound(new ProblemDetails { Status = 404, Title = result.Message ?? "查無資料" });
        return Ok(result);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error in {Action}", nameof(Get<Entity>Detail));
        return StatusCode(500, new ProblemDetails { Status = 500, Title = "查詢失敗，請稍後再試。" });
    }
}
```
    }
    catch (InvalidOperationException ex)
    {
        logger.LogWarning(ex, "Invalid operation in {Action}", nameof(Search<Entity>));
        return BadRequest(new ProblemDetails { Status = 400, Title = ex.Message });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error in {Action}", nameof(Search<Entity>));
        return StatusCode(500, new ProblemDetails { Status = 500, Title = "查詢失敗，請稍後再試。" });
    }
}
```

規則：
- Controller 保持 `[Authorize]`，endpoint 不例外
- Route 對應前端 URL（去掉 `/api` 前綴，因 controller 已有 `[Route("api/[controller]")]`）

---

## 步驟 6：DI 註冊

在 `Program.cs` 的 Services 區塊加入：

```csharp
builder.Services.AddScoped<IQuery<Entity>Service, Query<Entity>Service>();
```

---

## 步驟 7：更新前端 composable（如有需要）

若前端 composable 使用 `http.get` 但後台實作為 POST，更新 composable 改用 `apiRequest`：

```ts
import { apiRequest } from '@/services/api-service'

// POST with body
const res = await apiRequest<XxxItem[]>('/query/xxx', { method: 'POST', data: body })
if (res.success) {
  items.value = res.data ?? []
  totalItems.value = res.total ?? 0
} else {
  console.error('[useQueryXxx/searchHandler]', res.message, res.errors)
}
```

---

## 完成後確認

- [ ] DTO 欄位與前端 interface 欄位一一對應（名稱、型別）
- [ ] Service 篩選邏輯涵蓋前端所有查詢條件
- [ ] 若有明細端點：GET route 對應前端 composable 的 `openDetail` URL 格式（如 `/{appNo}`）
- [ ] Controller action route 與前端 URL 吻合
- [ ] DI 已註冊
- [ ] 確認無 build error（若 IDE 有 error lens 請確認）
