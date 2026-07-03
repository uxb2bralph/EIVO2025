---
name: "Generate UnitOfWork Design Guide"
description: "根據 IUnitOfWork.cs 與 UnitOfWork.cs 的實作，產生設計指引文件 (.md)，供新專案或新成員引用。Use when: generate UnitOfWork design guide, UnitOfWork 設計文件, 產生設計指引"
agent: "agent"
tools: [read_file, create_file]
argument-hint: "輸出檔案路徑 (選填，預設: docs/UnitOfWork-design-guide.md)"
---

## Task

讀取下列兩個檔案的完整內容，然後產生一份中文設計指引文件，並存成 Markdown 檔案。

**來源檔案**
- [IUnitOfWork.cs](../../HNB.Api/Interfaces/IUnitOfWork.cs)
- [UnitOfWork.cs](../../HNB.Api/Repositories/UnitOfWork.cs)

**輸出路徑**：使用者提供的引數；若未提供，預設為 `docs/UnitOfWork-design-guide.md`（相對於 workspace 根目錄）。

---

## 文件結構要求

產生的 Markdown 必須依序包含以下章節：

### 1. 概述 (Overview)
- 說明 Unit of Work 模式的目的與在本專案中的角色
- 指出 `IUnitOfWork` 是 DI 注入的合約，`UnitOfWork` 是具體實作

### 2. 介面設計 (`IUnitOfWork`)
- 列出所有 Repository 屬性（名稱 + 對應的 `I{Name}Repository`）
- 說明三個交易方法的簽章與語意：
  - `BeginTransactionAsync()`
  - `CommitTransactionAsync()`
  - `RollbackTransactionAsync()`
- 說明 `SaveChangesAsync()` 與 `Dispose()`

### 3. 實作細節 (`UnitOfWork`)
- **懶載入 (Lazy Initialization)**：解釋 `??=` 模式，說明 Repository 只有在首次存取時才建立實例
- **巢狀交易計數器 (`_transactionDepth`)**：詳細說明計數器邏輯：
  - `BeginTransactionAsync`：depth == 0 才開啟真實 DB transaction，每次 +1
  - `CommitTransactionAsync`：每次 -1，depth 降至 0 才 Commit 並釋放
  - `RollbackTransactionAsync`：任何深度呼叫都立即 Rollback，depth 重置為 0
- **重要警告**：若 exception 逃逸而未呼叫 Rollback，`_transactionDepth` 不歸零，後續請求的交易將失效

### 4. 使用規範 (Usage Rules)

#### 4.1 標準 CRUD（無交易）
提供程式碼範例：
```csharp
// Service 層直接操作 repository，最後 SaveChangesAsync
await _unitOfWork.Users.AddAsync(entity);
await _unitOfWork.SaveChangesAsync();
```

#### 4.2 跨 Repository 交易
提供正確的 try/catch 範例，強調 `RollbackTransactionAsync` 必須在 `catch` 中：
```csharp
await _unitOfWork.BeginTransactionAsync();
try
{
    // ... 多個 repository 操作 ...
    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

#### 4.3 巢狀交易（Service 呼叫 Service）
說明內層 Service 同樣呼叫 Begin/Commit，但實際 DB transaction 只由最外層管控。

### 5. 反模式 (Anti-patterns)
- ❌ 在 Controller 直接呼叫 `_unitOfWork`（應只在 Service 使用）
- ❌ `BeginTransactionAsync` 後忘記 catch/rollback
- ❌ 在同一請求中重複呼叫 `BeginTransactionAsync` 而未搭配對等的 `CommitTransactionAsync`
- ❌ 直接存取 `Context` 執行 raw SQL（應透過 Repository 封裝）

### 6. 新增 Repository 步驟 (How to Add a New Repository)
以條列說明新增一個 Repository 所需的步驟：
1. 建立 `I{Name}Repository` 介面（繼承 `IGenericRepository<T>`）
2. 建立 `{Name}Repository` 實作
3. 在 `IUnitOfWork` 加入屬性定義
4. 在 `UnitOfWork` 加入私有欄位 + 建構子參數 + 屬性實作（`??=` lazy pattern）
5. 在 DI 容器（`program.cs`）註冊

### 7. 相關檔案參考 (Related Files)
以 Markdown 連結列出：
- `HNB.Api/Interfaces/IUnitOfWork.cs`
- `HNB.Api/Repositories/UnitOfWork.cs`
- `HNB.Api/DataAccess/GenericRepository.cs`（若存在）
- `HNB.Api/Program.cs`（DI 註冊位置）

---

## Output Rules

- 語言：**繁體中文**（程式碼、介面名稱、檔案路徑保持英文）
- 程式碼區塊使用 `csharp` 語法高亮
- 所有章節標題使用 `##` / `###`
- 最後一行加上產生日期：`> 此文件由 GitHub Copilot 依 IUnitOfWork.cs / UnitOfWork.cs 自動產生於 {今日日期}`
- 建立完檔案後，回報完整輸出路徑
