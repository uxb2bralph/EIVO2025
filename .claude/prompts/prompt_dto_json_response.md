---
name: dto-json-response
description: 將 Controller 中 Json() 回傳的匿名物件改為具名 DTO 類別的標準作法
metadata: 
  node_type: memory
  type: project
  originSessionId: 787a2e55-7e1f-4b49-91bd-860169c528dd
---

# Controller JSON 回應改用 DTO

## 問題情境

Controller 的 `Json()` 直接以匿名物件回傳資料，缺乏型別定義，不易維護與重用：

**修改前（匿名物件）：**
```csharp
return Json(new
{
    result = items.Count() > 0,
    data = items.Select(q =>
            new
            {
                q.TaskID,
                q.ProcessTypeNavigation!.ChannelName,
                q.ProcessTypeNavigation!.ChannelResponse,
                ResponseName = Path.GetFileName(q.ResponsePath),
                TxnPath = q.ViewModel != null ? JsonConvert.DeserializeObject<InvoiceRequestViewModel>(q.ViewModel).StoragePath : null
            }).ToArray()
});
```

**修改後（具名 DTO）：**
```csharp
var list = items.ToList();
return Json(new NotifyRequestCompletionResultDto
{
    result = list.Count > 0,
    data = [.. list.Select(q => new ProcessRequestNotificationItemDto
    {
        TaskID = q.TaskID,
        ChannelName = q.ProcessTypeNavigation!.ChannelName,
        ChannelResponse = q.ProcessTypeNavigation!.ChannelResponse,
        ResponseName = Path.GetFileName(q.ResponsePath),
        TxnPath = q.ViewModel != null ? JsonConvert.DeserializeObject<InvoiceRequestViewModel>(q.ViewModel)?.StoragePath : null
    })]
});
```

## 修改步驟

### 1. 建立 DTO 檔案

放在 `TaskCenter/Core/DTOs/` 目錄下，一個功能對應一個 `.cs` 檔案：

```csharp
namespace TaskCenter.Core.DTOs
{
    public class ProcessRequestNotificationItemDto   // 資料列表項目
    {
        public int TaskID { get; set; }
        public string? ChannelName { get; set; }
        public string? ChannelResponse { get; set; }
        public string? ResponseName { get; set; }
        public string? TxnPath { get; set; }
    }

    public class NotifyRequestCompletionResultDto    // 外層包裝
    {
        public bool result { get; set; }
        public ProcessRequestNotificationItemDto[]? data { get; set; }
    }
}
```

> **命名慣例：**
> - 外層包裝 DTO：`{ActionName}ResultDto`
> - 列表項目 DTO：`{Entity}ItemDto` 或 `{ActionName}ItemDto`
> - 外層 `result`/`data` 屬性保持小寫，與原 API 契約一致

### 2. Controller 加入 using

```csharp
using TaskCenter.Core.DTOs;
```

移除因改用 DTO 後不再需要的 using（IDE hint 會提示）。

### 3. 先 `.ToList()` 再 `.Select()`

當 `.Select()` 投影內含無法被 EF Core 翻譯的方法（`Path.GetFileName`、`JsonConvert.DeserializeObject`、null 條件運算子等）時，
必須先 `.ToList()` 將資料載入記憶體，再於記憶體中投影至 DTO。

### 4. 使用 C# 12 Collection Expression

陣列屬性改用 `[.. enumerable]` 語法取代 `.ToArray()`：

```csharp
data = [.. list.Select(q => new SomeDto { ... })]
```

### 5. null 安全

反序列化結果加上 null 條件運算子 `?.`：

```csharp
// 修改前
JsonConvert.DeserializeObject<T>(q.ViewModel).StoragePath

// 修改後
JsonConvert.DeserializeObject<T>(q.ViewModel)?.StoragePath
```

## 適用規則

- 所有對外 API 的 `Json()` 回傳值，若超過 2 個欄位就應定義 DTO。
- 外層 `{ result, data }` 包裝也應有對應的 ResultDto，方便統一型別。
- DTO 一律放在 `TaskCenter/Core/DTOs/` 目錄，保持與 [[ef-core-query-fix]] 的 `.ToList()` 模式一致。

## 本次修改檔案

- [TaskCenter/Core/DTOs/ProcessRequestNotificationDto.cs](../../../../../Project/Github/EIVO2025/TaskCenter/Core/DTOs/ProcessRequestNotificationDto.cs)
  - 新增 `ProcessRequestNotificationItemDto`、`NotifyRequestCompletionResultDto`
- [TaskCenter/Controllers/InvoiceDataController.cs](../../../../../Project/Github/EIVO2025/TaskCenter/Controllers/InvoiceDataController.cs)
  - `NotifyRequestCompletion()` 改用 DTO 回傳
