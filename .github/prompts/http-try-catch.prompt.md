---
description: "為 Vue 3 composable 或 API 呼叫加上標準 HTTP try-catch 錯誤處理。Use when: adding try-catch to http.post/get/put/delete, handling API errors, wrapping axios calls, 加 catch, 加錯誤處理, 包 try catch, HTTP 例外處理."
argument-hint: "選填：指定要處理的 function 名稱或檔案路徑 (e.g. searchHandler in useQueryLcApp.ts)"
agent: "agent"
---

你正在為 `HNB.Web/src/` 的 Vue 3 composable 加入標準 HTTP try-catch 錯誤處理。

## 共用工具

專案使用 `handleApiError` 作為統一的 API 錯誤記錄工具，定義於 [http.ts](../../HNB.Web/src/api/http.ts)。

**絕對不要重複實作錯誤記錄邏輯**，一律使用此共用方法：

```ts
import http, { handleApiError } from '@/api/http'
```

## 目標

分析 `$argument` 中指定的 function（若未提供則分析當前選取的程式碼），將所有裸露的 `await http.*` 呼叫包進 try-catch-finally 結構。

## 處理規則

### 1. try-catch-finally 結構

```ts
async function exampleHandler() {
  loading.value = true
  try {
    const res = await http.post<ResponseType>('/endpoint', body)
    // 處理成功結果
  } catch (error) {
    handleApiError(error, 'composableName/functionName')
  } finally {
    loading.value = false
  }
}
```

- `context` 參數格式：`'composableName/functionName'`（小駝峰，斜線分隔）
- 若 function 內已有 `finally { loading.value = false }`，保留原有結構，只補 `catch`
- 若無 `loading` ref，省略 `finally` 區塊

### 2. 不得更動的部分

- 成功路徑邏輯（`items.value = ...`、`showResult.value = true` 等）
- 現有的 try 區塊內容
- function 簽名與 return 型別

### 3. import 更新

若檔案目前是 `import http from '@/api/http'`，改為：

```ts
import http, { handleApiError } from '@/api/http'
```

### 4. 多個 http 呼叫

若同一 function 有多個 `await http.*`，用單一 try-catch 包住全部，`context` 取 function 名稱。

## 輸出

1. 顯示修改後的完整 function 程式碼
2. 若需更新 import，一併顯示
3. 若有多個 function 需要修改，逐一列出並套用

## 範例

**修改前：**
```ts
async function fetchData() {
  loading.value = true
  try {
    const res = await http.get<Item[]>('/items')
    items.value = res.data
  } finally {
    loading.value = false
  }
}
```

**修改後：**
```ts
async function fetchData() {
  loading.value = true
  try {
    const res = await http.get<Item[]>('/items')
    items.value = res.data
  } catch (error) {
    handleApiError(error, 'useItems/fetchData')
  } finally {
    loading.value = false
  }
}
```
