---
name: "Generate Service Design Guide"
description: "根據現有 Service 實作，產生 Service 設計指引文件 (.md)，聚焦於 UnitOfWork DB 操作規範。Use when: service design guide, service 設計文件, service 設計規範, 產生 service 指引, unitofwork service pattern"
agent: "agent"
tools: [read_file, create_file, grep_search, file_search]
argument-hint: "輸出檔案路徑 (選填，預設: docs/Service-design-guide.md)"
---

## Task

閱讀以下參考資料，產生一份繁體中文的 **Service 層設計指引文件**，聚焦於透過 `IUnitOfWork` 操作資料庫的正確模式，並存成 Markdown 檔案。

**參考檔案（請先完整讀取再撰寫）**
- [IUnitOfWork.cs](../../HNB.Api/Interfaces/IUnitOfWork.cs) — UnitOfWork 合約
- [UnitOfWork.cs](../../HNB.Api/Repositories/UnitOfWork.cs) — UnitOfWork 實作（含交易計數器邏輯）
- [CalendarEventAttendeeAssessmentService.cs](../../HNB.Api/Services/CalendarEventAttendeeAssessmentService.cs) — 跨 repository 交易範例
- [RoleService.cs](../../HNB.Api/Services/RoleService.cs) — 標準 CRUD 範例

**輸出路徑**：使用者提供的引數；若未提供，預設為 `docs/Service-design-guide.md`（相對於 workspace 根目錄）。

---

## 文件結構要求

產生的 Markdown 必須依序包含以下章節：

### 1. 概述 (Overview)

- Service 層在架構中的職責：Controller → Service → Repository（透過 UnitOfWork）
- Service 是唯一允許驅動交易與 DTO 映射的層
- Controller **不得**直接呼叫 `_unitOfWork`、DbContext 或 AutoMapper/Mapperly

### 2. 建構子注入規範

說明 Service 類別的標準建構子模式，必須注入：
- `IUnitOfWork _unitOfWork`
- `WisMapper _mapper`（需要 DTO 映射時）
- `ILogger<TService> _logger`

提供標準範例：

```csharp
public class FooService : IFooService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly WisMapper _mapper;
    private readonly ILogger<FooService> _logger;

    public FooService(IUnitOfWork unitOfWork, WisMapper mapper, ILogger<FooService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }
}
```

### 3. 資料讀取 (Query)

#### 3.1 單筆查詢

```csharp
public async Task<FooDto?> GetByIdAsync(int id)
{
    var entity = await _unitOfWork.Foos.GetByIdAsync(id);
    return entity == null ? null : _mapper.MapToFooDto(entity);
}
```

#### 3.2 分頁查詢

```csharp
public async Task<PagedResult<FooDatatableDto>> GetPagedAsync(FooQueryDto queryDto)
{
    var (items, totalCount) = await _unitOfWork.Foos.GetFilteredPagedAsync(queryDto);
    return new PagedResult<FooDatatableDto>
    {
        Items = items.Select(_mapper.MapToFooDatatableDto),
        TotalCount = totalCount,
        PageNumber = queryDto.Page,
        PageSize = queryDto.PageSize
    };
}
```

- 說明 `PagedResult<T>` 的必填欄位
- 強調查詢方法應該盡量在 Repository 層完成過濾、分頁等邏輯，Service 層僅負責呼叫並映射 DTO

### 4. 資料寫入 — 無交易 (Single Repository Write)

適用：只操作單一 repository、不需原子性保證時。

```csharp
public async Task<FooDto> CreateAsync(CreateFooDto dto, int operatorId)
{
    var entity = _mapper.MapToFoo(dto);
    await _unitOfWork.Foos.AddAsync(entity);
    await _unitOfWork.SaveChangesAsync();
    return _mapper.MapToFooDto(entity);
}
```

**規則**：
- 呼叫 `AddAsync` / `Update` / `Remove` 後，必須呼叫 `SaveChangesAsync()` 才會寫入 DB
- **不需要** `BeginTransactionAsync`；EF Core 單次 `SaveChanges` 本身即為原子操作
- 不可省略 `SaveChangesAsync()`

### 5. 資料寫入 — 需交易 (Multi-Repository Atomic Write)

適用：跨多個 repository、或需要分段 flush（取得 Id 後繼續寫入）時。

#### 5.1 標準交易模板

```csharp
await _unitOfWork.BeginTransactionAsync();
try
{
    // 1. 第一個 repository 操作
    await _unitOfWork.Foos.AddAsync(foo);
    await _unitOfWork.SaveChangesAsync(); // 若需取得 foo.Id 才繼續

    // 2. 第二個 repository 操作（依賴 foo.Id）
    var bar = new Bar { FooId = foo.Id, ... };
    await _unitOfWork.Bars.AddAsync(bar);

    // 3. 最終 save + commit
    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();
}
catch (Exception ex)
{
    await _unitOfWork.RollbackTransactionAsync();
    _logger.LogError(ex, "操作說明 FooId={FooId}", foo.Id);
    throw; // 重新拋出，讓 Controller 回傳 500
}
```

**規則**：
- `BeginTransactionAsync` **必須** 在 `try` 之前呼叫（確保 catch 能 rollback）
- `catch` 塊中**必須**呼叫 `RollbackTransactionAsync()`，否則 `_transactionDepth` 不歸零，後續請求的交易將永久失效
- `throw` 不得吞掉例外；讓上層決定回傳何種 HTTP 狀態碼
- 中途發現業務邏輯錯誤（如資料不存在）應立即 `RollbackTransactionAsync()` 再 `return`

#### 5.2 中途提前 Rollback（業務條件不符）

```csharp
await _unitOfWork.BeginTransactionAsync();
try
{
    var user = await _unitOfWork.UserProfiles.GetByIdAsync(userId);
    if (user == null)
    {
        await _unitOfWork.RollbackTransactionAsync();
        return false; // 提前離開，不拋例外
    }
    // ... 繼續操作 ...
    await _unitOfWork.CommitTransactionAsync();
    return true;
}
catch (Exception ex)
{
    await _unitOfWork.RollbackTransactionAsync();
    _logger.LogError(ex, "...");
    throw;
}
```

### 6. 巢狀交易 (Nested Transaction — Service Calls Service)

當 Service A 呼叫 Service B，且兩者都呼叫 `BeginTransactionAsync`：

- 內層 `Begin` 只增加計數器（不開新 DB transaction）
- 內層 `Commit` 只減少計數器（不實際 commit）
- 外層 `Commit` 才真正 commit
- 任意層 `Rollback` 立即回滾並重置計數器為 0

**結論**：可以放心在 Service 中呼叫另一個有交易的 Service，不會導致交易衝突。

### 7. DTO 映射規範

- 所有 Entity ↔ DTO 轉換在 **Service 層**完成，使用 `WisMapper`（Mapperly）
- **禁止**在 Controller 或 Repository 進行映射
- 業務條件邏輯（如欄位計算、狀態判斷）放在 Service，**不放在** `WisMapper` 的映射方法中
- AutoMapper（`MappingProfile.cs`）僅用於舊有程式碼，新功能一律使用 Mapperly

### 8. 錯誤處理規範

| 情境 | 做法 |
|------|------|
| 資源不存在 | 回傳 `null`（讓 Controller 呼叫 `CreateNotFoundResponse()`） |
| 業務邏輯錯誤 | 拋出具語意的 Exception（如 `InvalidOperationException`）或回傳 `false`/結果物件 |
| 系統錯誤（DB、IO） | `_logger.LogError(ex, "...", 參數)` 後 `throw` |
| 交易中的任何例外 | 先 `RollbackTransactionAsync()` 再 `throw` |

- Logger 訊息必須包含相關 ID（如 `UserId`, `ContractId`）以便追蹤
- 禁止在 Service 直接回傳 HTTP 狀態碼或 `IActionResult`

### 9. 多租戶注意事項 (Multi-tenancy)

- **不需要**手動在 LINQ 或 Repository 呼叫中加入 `EnterpriseId` 過濾
- EF Core Global Query Filter 已自動套用，依目前登入使用者的 `EnterpriseId` 隔離資料
- 若要建立新 Entity，才需要明確設定 `EnterpriseId`（通常從 JWT claims 取得並由 Controller 傳入）

### 10. 反模式 (Anti-patterns)

- ❌ `_unitOfWork.Context.Set<T>()` 直接操作（應透過 repository；唯有特殊情境如需立即 flush 取得 Id 才允許例外）
- ❌ `BeginTransactionAsync` 後無對應 `catch + RollbackTransactionAsync`
- ❌ 在 Controller 注入 `IUnitOfWork` 直接操作
- ❌ 在同一方法中混用 `_unitOfWork.SaveChangesAsync()` 與手動 SQL（`ExecuteSqlRawAsync`）而不包在交易中
- ❌ 捕捉例外後不 log、不 throw，直接回傳預設值（靜默吞掉錯誤）
- ❌ DTO 映射邏輯含業務判斷（應拆到 Service 方法中）

### 11. 新增 Service 步驟 (How to Add a New Service)

1. 建立 `I{Name}Service` 介面（放在 `Core/Interfaces/`）
2. 建立 `{Name}Service` 實作（放在 `Core/Services/`）
3. 在 `program.cs` 以 `builder.Services.AddScoped<I{Name}Service, {Name}Service>()` 註冊
4. Controller 以建構子注入 `I{Name}Service`，**不注入** `IUnitOfWork`

### 12. 相關檔案參考 (Related Files)

- [HNB.Api/Interfaces/IUnitOfWork.cs](../HNB.Api/Interfaces/IUnitOfWork.cs)
- [HNB.Api/Repositories/UnitOfWork.cs](../HNB.Api/Repositories/UnitOfWork.cs)
- [HNB.Api/Mappings/WisMapper.*.cs](../HNB.Api/Mappings/) — Mapperly 映射定義
- [HNB.Api/Controllers/LocalizedControllerBase.cs](../HNB.Api/Controllers/LocalizedControllerBase.cs) — Controller 回應方法
- [WebHome/program.cs](../WebHome/program.cs) — DI 容器設定

---

## Output Rules

- 語言：**繁體中文**（程式碼、類別名稱、介面名稱、檔案路徑保持英文）
- 程式碼區塊使用 `csharp` 語法高亮
- 所有章節標題使用 `##` / `###`
- 表格對齊使用標準 Markdown table 語法
- 最後一行加上：`> 此文件由 GitHub Copilot 依專案實際 Service 實作自動產生於 {今日日期}`
- 建立完檔案後，回報完整輸出路徑
