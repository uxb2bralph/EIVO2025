# UnitOfWork 設計指引

## 1. 概述 (Overview)

Unit of Work（UoW）模式將一批資料庫操作包裝成單一工作單元，確保所有變更要麼全部成功、要麼全部回滾，從而維護資料一致性。

在本專案（`HNB.Api`）中，UoW 扮演以下角色：

- **Repository 聚合器**：透過單一注入點取得所有 Repository，避免在 Service 中直接注入多個 Repository。
- **交易控制器**：統一封裝 EF Core 的 `IDbContextTransaction`，支援巢狀呼叫。
- **DI 合約**：`IUnitOfWork` 是 DI 容器的注入合約；`UnitOfWork` 是具體實作。Service 層永遠依賴介面，不依賴具體類別。

```
Controller → Service → IUnitOfWork → Repository → HNBDbContext
```

---

## 2. 介面設計 (`IUnitOfWork`)

### 2.1 Repository 屬性

| 屬性名稱 | 對應實體 | 說明 |
|----------|----------|------|
| `BankUsers` | `IGenericRepository<BankUser>` | 行員帳號 |
| `BankUserRoles` | `IGenericRepository<BankUserRole>` | 行員角色對應 |
| `BankUserLogins` | `IGenericRepository<BankUserLogin>` | 行員登入紀錄 |
| `Organizations` | `IGenericRepository<Organization>` | 公司 / 機構 |
| `OrganizationExtensions` | `IGenericRepository<OrganizationExtension>` | 機構延伸資料 |
| `LettersOfCredit` | `IGenericRepository<LetterOfCredit>` | 信用狀主檔 |
| `LetterOfCreditExtensions` | `IGenericRepository<LetterOfCreditExtension>` | 信用狀延伸 |
| `LetterOfCreditVersions` | `IGenericRepository<LetterOfCreditVersion>` | 信用狀版本 |
| `LcItems` | `IGenericRepository<LcItem>` | 信用狀條款 |
| `NegoLcs` | `IGenericRepository<NegoLc>` | 押匯信用狀 |
| `NegoDrafts` | `IGenericRepository<NegoDraft>` | 押匯匯票 |
| `NegoHosts` | `IGenericRepository<NegoHost>` | 押匯主檔 |
| `AmendingLcApplications` | `IGenericRepository<AmendingLcApplication>` | 信用狀修改申請 |
| `RevisionLogs` | `IGenericRepository<RevisionLog>` | 稽核修改日誌 |
| `ReceivedDataQueues` | `IGenericRepository<ReceivedDataQueue>` | 進入資料佇列 |
| `ServiceDataQueues` | `IGenericRepository<ServiceDataQueue>` | 服務資料佇列 |
| `ResponseDataQueues` | `IGenericRepository<ResponseDataQueue>` | 回應資料佇列 |

### 2.2 交易方法

```csharp
Task BeginTransactionAsync(CancellationToken cancellationToken = default);
Task CommitTransactionAsync(CancellationToken cancellationToken = default);
Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
```

| 方法 | 語意 |
|------|------|
| `BeginTransactionAsync()` | 開始一個邏輯交易。`_transactionDepth == 0` 時才開啟真實 DB transaction，每次呼叫計數器 +1。 |
| `CommitTransactionAsync()` | 提交交易。計數器 -1；歸零時才執行真實 `COMMIT` 並釋放 transaction 物件。 |
| `RollbackTransactionAsync()` | 回滾交易。無論巢狀深度，立即執行 `ROLLBACK` 並將計數器重置為 0。 |

### 2.3 其他方法

```csharp
Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
void Dispose();
```

- **`SaveChangesAsync()`**：將 EF Core ChangeTracker 中的所有變更寫入資料庫。不包含 Commit，可在交易中多次呼叫。
- **`Dispose()`**：釋放 DbContext 及進行中的 transaction 物件（由 DI 框架的 Scoped 生命週期自動呼叫）。

---

## 3. 實作細節 (`UnitOfWork`)

### 3.1 懶載入 (Lazy Initialization)

每個 Repository 以私有欄位儲存，屬性存取時才透過 `??=` 初始化：

```csharp
private IGenericRepository<LetterOfCredit>? _lettersOfCredit;

public IGenericRepository<LetterOfCredit> LettersOfCredit
    => _lettersOfCredit ??= new GenericRepository<LetterOfCredit>(_context);
```

**優點**：
- 不需要在建構子中一次初始化所有 Repository，降低記憶體消耗。
- 同一請求內多次存取同一屬性，會取得相同的 Repository 實例，共享同一個 DbContext（確保 ChangeTracker 一致）。

### 3.2 巢狀交易計數器 (`_transactionDepth`)

`UnitOfWork` 內部維護一個整數計數器 `_transactionDepth`，管理巢狀呼叫：

#### `BeginTransactionAsync`

```csharp
public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
{
    if (_transactionDepth == 0)
        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);

    _transactionDepth++;
}
```

- `depth == 0`：開啟真實 DB transaction，計數器設為 1。
- `depth > 0`：僅將計數器 +1，**不**開啟新的 DB transaction（SQL Server 不支援真正的巢狀 transaction）。

#### `CommitTransactionAsync`

```csharp
public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
{
    _transactionDepth--;

    if (_transactionDepth == 0 && _currentTransaction is not null)
    {
        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }
}
```

- 每次呼叫計數器 -1。
- 僅在計數器歸零時才真正 Commit，確保所有呼叫端都 Commit 後交易才生效。

#### `RollbackTransactionAsync`

```csharp
public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
{
    if (_currentTransaction is not null)
    {
        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }
    _transactionDepth = 0;
}
```

- **任何深度**呼叫都立即 Rollback，計數器重置為 0。
- 體現「任一層失敗，整筆交易取消」的語意。

### 3.3 重要警告

> ⚠️ 若 exception 在 Service 中逃逸且未執行 `RollbackTransactionAsync`，`_transactionDepth` 不會歸零。同一 Scoped 實例的後續操作（若有）將無法正確開啟新 transaction。  
> **請務必在 `catch` 中呼叫 `RollbackTransactionAsync`，並 `throw` 重新拋出例外。**

---

## 4. 使用規範 (Usage Rules)

### 4.1 標準 CRUD（無交易）

不跨 Repository 的單純新增 / 更新操作，直接操作 Repository 後呼叫 `SaveChangesAsync`：

```csharp
public async Task CreateOrganizationAsync(CreateOrganizationRequest request)
{
    var entity = new Organization { /* 屬性設定 */ };
    await _unitOfWork.Organizations.AddAsync(entity);
    await _unitOfWork.SaveChangesAsync();
}
```

### 4.2 跨 Repository 交易

需要保證原子性時，使用 try/catch 包裹交易：

```csharp
public async Task IssueLetterOfCreditAsync(IssueLetterOfCreditRequest request)
{
    await _unitOfWork.BeginTransactionAsync();
    try
    {
        var lc = new LetterOfCredit { /* 屬性設定 */ };
        await _unitOfWork.LettersOfCredit.AddAsync(lc);

        var log = new RevisionLog { /* 稽核日誌 */ };
        await _unitOfWork.RevisionLogs.AddAsync(log);

        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

### 4.3 巢狀交易（Service 呼叫 Service）

內層 Service 同樣呼叫 `BeginTransactionAsync` / `CommitTransactionAsync`，但實際 DB transaction 只由**最外層**管控：

```csharp
// OuterService
public async Task ProcessAmendmentAsync(AmendmentRequest request)
{
    await _unitOfWork.BeginTransactionAsync(); // depth: 0→1，開啟真實 transaction
    try
    {
        await _innerService.UpdateLcAsync(request.LcId, ...); // 內部 depth: 1→2→1
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync(); // depth: 1→0，真正 Commit
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync(); // 立即 Rollback，depth=0
        throw;
    }
}

// InnerService（不感知外層 transaction）
public async Task UpdateLcAsync(int lcId, ...)
{
    await _unitOfWork.BeginTransactionAsync(); // depth: 1→2，不開新 transaction
    try
    {
        // ... 操作 ...
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync(); // depth: 2→1，不真正 Commit
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync(); // 立即 Rollback
        throw;
    }
}
```

---

## 5. 反模式 (Anti-patterns)

| ❌ 反模式 | 說明 |
|-----------|------|
| 在 Controller 直接呼叫 `_unitOfWork` | 業務邏輯應封裝在 Service，Controller 只負責 HTTP 請求/回應的轉換 |
| `BeginTransactionAsync` 後沒有 catch/rollback | 發生例外時 `_transactionDepth` 不歸零，後續請求的交易行為將錯誤 |
| 重複呼叫 `BeginTransactionAsync` 而未對等 `Commit` | 計數器累積，最後一次 Commit 無法真正提交 |
| 在 Service 之外直接注入 `HNBDbContext` | 繞過 UoW 封裝，造成 ChangeTracker 不一致、交易無法統一管控 |
| 使用原始字串拼接 SQL | 潛在 SQL Injection 風險，應透過 EF Core LINQ 或參數化查詢 |

---

## 6. 新增 Repository 步驟 (How to Add a New Repository)

以新增 `DocumentOwner` 的 Repository 為例：

1. **（選填）建立專屬介面**：若有超出 `IGenericRepository<T>` 的方法，在 `HNB.Api/Interfaces/` 建立 `IDocumentOwnerRepository : IGenericRepository<DocumentOwner>`。
2. **（選填）建立具體實作**：在 `HNB.Api/Repositories/` 建立 `DocumentOwnerRepository : GenericRepository<DocumentOwner>, IDocumentOwnerRepository`。
3. **在 `IUnitOfWork` 加入屬性定義**：
   ```csharp
   IGenericRepository<DocumentOwner> DocumentOwners { get; }
   ```
4. **在 `UnitOfWork` 加入私有欄位 + 屬性實作**：
   ```csharp
   private IGenericRepository<DocumentOwner>? _documentOwners;

   public IGenericRepository<DocumentOwner> DocumentOwners
       => _documentOwners ??= new GenericRepository<DocumentOwner>(_context);
   ```
5. **在 `Program.cs` 確認 DI 已註冊**（若只用 `GenericRepository` 泛型版本，不需額外登記；若有自訂實作則需要）：
   ```csharp
   builder.Services.AddScoped<IDocumentOwnerRepository, DocumentOwnerRepository>();
   ```

---

## 7. 相關檔案參考 (Related Files)

| 檔案 | 說明 |
|------|------|
| [HNB.Api/Interfaces/IUnitOfWork.cs](../HNB.Api/Interfaces/IUnitOfWork.cs) | Unit of Work DI 合約 |
| [HNB.Api/Repositories/UnitOfWork.cs](../HNB.Api/Repositories/UnitOfWork.cs) | Unit of Work 具體實作（含巢狀交易計數器） |
| [HNB.Api/Interfaces/IGenericRepository.cs](../HNB.Api/Interfaces/IGenericRepository.cs) | 泛型 Repository 介面 |
| [HNB.Api/Repositories/GenericRepository.cs](../HNB.Api/Repositories/GenericRepository.cs) | 泛型 Repository 基底類別 |
| [HNB.Api/Data/HNBDbContext.cs](../HNB.Api/Data/HNBDbContext.cs) | EF Core DbContext |
| [HNB.Api/Program.cs](../HNB.Api/Program.cs) | DI 容器服務註冊 |

---

> 此文件由 GitHub Copilot 依 IUnitOfWork.cs / UnitOfWork.cs 自動產生於 2026-04-30
