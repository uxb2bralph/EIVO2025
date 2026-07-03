---
name: ef-core-query-fix
description: EF Core 查詢修正 prompt — 修正 IQueryable 中使用無法翻譯的 extension method，並加上 AsNoTracking
metadata:
  type: project
---

# EF Core 查詢修正

## 問題情境

在 `IQueryable<T>` 的 `.Select()` 內呼叫複雜的 extension method（例如接受 `models` 參數的 `CreateAllowanceMIG`），EF Core 會嘗試將其翻譯成 SQL，因無法翻譯而在執行期拋出例外。

**修正前（錯誤寫法）：**
```csharp
var dataItems = items.Select(c => c.CreateAllowanceMIG(models, true)).ToList();
```

**修正後（正確寫法）：**
```csharp
var dataItems = items.AsNoTracking().ToList().Select(c => c.CreateAllowanceMIG(models, true)).ToList();
```

## 修正說明

1. **先 `.ToList()` 再 `.Select()`**  
   在 `.Select()` 前加 `.ToList()`，讓 EF Core 先執行 SQL 查詢、將資料載入記憶體，再用 LINQ-to-Objects 於記憶體中執行投影，避免 EF Core 嘗試翻譯無法翻譯的 C# 方法。

2. **加上 `.AsNoTracking()`**  
   針對純唯讀查詢（資料只序列化回傳，不會修改後 SaveChanges），加上 `AsNoTracking()` 跳過 EF Core 的變更追蹤（change tracking），減少記憶體與 CPU 消耗。

3. **補充 using 指示詞**  
   `AsNoTracking()` 的 extension method 位於 `Microsoft.EntityFrameworkCore`，若缺少須在檔案頂部加入：
   ```csharp
   using Microsoft.EntityFrameworkCore;
   ```

## 適用規則

- 所有 `IQueryable` 接著 `.Select()` 內含無法被 EF Core 翻譯的自訂方法，均應先 `.ToList()` 再做投影。
- 只要是唯讀查詢（結果不會再 SaveChanges），一律加 `AsNoTracking()`。

## 本次修改檔案

- [TaskCenter/Controllers/InvoiceQueryController.cs](../../../../../Project/Github/EIVO2025/TaskCenter/Controllers/InvoiceQueryController.cs)
  - 第 189 行：`items.Select(...)` → `items.AsNoTracking().ToList().Select(...)`
  - 第 17 行附近：加入 `using Microsoft.EntityFrameworkCore;`
