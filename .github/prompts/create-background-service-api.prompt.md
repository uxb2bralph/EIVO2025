---
description: >
  在 HNB.Api 新增「背景服務 + 佇列 + API 端點」組合，用於接受本機路徑、掃描檔案、批次處理後
  搬移至備份區。Use when: 創建 background service, 背景掃描處理, 佇列 API, batch file processing,
  Channel queue, BackgroundService, 背景檔案處理, file scan api, process files background.
name: 創建背景服務 API (Background Service API)
argument-hint: "描述要處理的檔案類型與業務邏輯，例如：掃描 XML 發票並匯入 NegoInvoice"
agent: agent
---

# 創建背景服務 API

## 輸入參數

請根據需求提供以下資訊（未提供者使用預設值）：

| 參數 | 說明 | 範例 |
|------|------|------|
| **業務名稱** | 功能模組名稱（CamelCase） | `NegoDraft` |
| **API 端點路由** | controller 相對路由 | `process-invoice` |
| **輸入參數名稱** | 路徑參數名稱 | `pathInfo` |
| **檔案副檔名** | 要掃描的檔案類型 | `*.xml` |
| **處理邏輯** | 每個檔案如何轉換並存入 DB | 反序列化 XML → DTO → Service.UploadAsync |
| **備份子資料夾** | 成功後搬移的目的地子資料夾名稱 | `_processed` |
| **IService 介面** | 現有的 Scoped Service 介面名稱（處理單筆資料） | `INegoDraftService` |
| **Service 方法** | Scoped Service 上的非同步方法 | `UploadInvoiceAsync(dto, ct)` |

---

## 輸出規格

依序產生以下 4 個產出物。

### 1. `HNB.Api/Interfaces/I{業務名稱}ProcessingQueue.cs`

- 定義兩個方法：
  - `void Enqueue(string pathInfo)` — 加入佇列
  - `IAsyncEnumerable<string> DequeueAllAsync(CancellationToken)` — 非同步取出

### 2. `HNB.Api/Services/{業務名稱}ProcessingService.cs`

包含兩個類別：

#### `{業務名稱}ProcessingQueue : I{業務名稱}ProcessingQueue`
- 以 `Channel<string>.CreateUnbounded<string>(new UnboundedChannelOptions { SingleReader = true })` 實作
- `Enqueue` 呼叫 `_channel.Writer.TryWrite`
- `DequeueAllAsync` 以 `await foreach` + `[EnumeratorCancellation]` yield 回傳

#### `{業務名稱}ProcessingService : BackgroundService`
- 建構子注入：`I{業務名稱}ProcessingQueue`, `IServiceScopeFactory`, `ILogger<…>`
- `ExecuteAsync`：`await foreach` 消費佇列，每個 `pathInfo` 呼叫 `ProcessPathAsync`
- `ProcessPathAsync`：
  1. 若路徑不存在 → `LogWarning` 後 return
  2. `Directory.EnumerateFiles(pathInfo, "*.副檔名", SearchOption.AllDirectories)` 逐一處理
  3. 每個檔案建立 `IServiceScope`，取得 `IScoped Service`
  4. 反序列化 / 轉換成 DTO，呼叫 Scoped Service 方法存入 DB
  5. 成功 → 呼叫 `MoveToBackup` 搬移；`LogInformation` 記錄成功
  6. 失敗 → `LogError(ex, ...)` 記錄錯誤，繼續處理下一個檔案（不 rethrow）
- `MoveToBackup`：
  - 目的地 = `{rootPath}/{備份子資料夾}/{yyyyMMdd}/`
  - 目的地已存在同名檔 → 加上 `{HHmmss_fff}` 時間戳後再搬移

### 3. `HNB.Api/Controllers/{業務名稱}Controller.cs`（新增 Action 至現有 Controller）

新增 `POST api/{controller}/{端點路由}?{輸入參數名稱}=...`：
- `[FromQuery] string {輸入參數名稱}` 參數
- 驗證空白 → `400 BadRequest`
- 驗證路徑存在 → `400 BadRequest`（`Directory.Exists`）
- 呼叫 `_processingQueue.Enqueue(pathInfo)`
- 回傳 `202 Accepted`（含說明訊息）

> 若 Controller 尚未存在，一併產生完整 Controller 檔案（含 `[ApiController]`、`[Route]`、`[Authorize]`）。

### 4. `Program.cs` 新增註冊片段

```csharp
builder.Services.AddSingleton<I{業務名稱}ProcessingQueue, {業務名稱}ProcessingQueue>();
builder.Services.AddHostedService<{業務名稱}ProcessingService>();
```

---

## 預設行為

- 佇列以 **Singleton** 註冊；BackgroundService 以 **AddHostedService** 註冊
- Scoped Service（`INegoDraftService` 等）在背景服務中透過 `IServiceScopeFactory.CreateAsyncScope()` 取得，避免 DI lifetime 問題
- 處理失敗時 **不中止整批次**，僅記錄錯誤後繼續下一個檔案
- 備份目錄不存在時自動 `Directory.CreateDirectory`
- 檔案反序列化使用 `static readonly XmlSerializer`（可依需求改為 JSON/CSV）

## 程式碼慣例

- 遵循 [csharp.instructions.md](../../.github/instructions/csharp.instructions.md)
- `CancellationToken` 傳遞至所有 async 方法
- 類別宣告 `sealed`；靜態成員（如 `XmlSerializer`）宣告 `static readonly`
- 使用 `await using var scope = ...` 確保 scope 正確釋放
