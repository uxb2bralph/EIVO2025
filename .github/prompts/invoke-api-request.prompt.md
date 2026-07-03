---
description: "將 Vue 3 composable / store 的 API 呼叫從舊 @/api/http 遷移為 @/services/api-service 的 apiRequest。Use when: migrate http to apiRequest, 改用 apiRequest, 換成 api-service, replace http.post/get with apiRequest, 遷移 api 呼叫, 改用 api-service, migrate api call."
argument-hint: "選填：指定檔案路徑或 function 名稱 (e.g. useQueryLcApp.ts 或 searchHandler)"
agent: "agent"
---

你正在將 `HNB.Web/src/` 的 Vue 3 composable / Pinia store，把舊的 `http`（`@/api/http`）API 呼叫遷移為新的 `apiRequest`（`@/services/api-service`）。

## 背景

新的統一 HTTP 入口定義於 [api-service.ts](../../HNB.Web/src/services/api-service.ts)：
- `apiRequest<T>(url, options?)` — 永不拋出例外，回傳 `ApiResponse<T>`
- `apiDownloadBlob(url, options?)` — Blob 下載，**可能拋出例外**，呼叫方需自行處理

`ApiResponse<T>` 結構：
```ts
interface ApiResponse<T = unknown> {
  success: boolean
  data?: T
  message?: string
  errors?: string[]
}
```

## 目標

分析 `$argument` 指定的檔案或 function（未提供則分析當前選取的程式碼），完成以下遷移：

## 遷移規則

### 1. 替換 import

```ts
// ❌ 舊
import http, { handleApiError } from '@/api/http'

// ✅ 新
import { apiRequest } from '@/services/api-service'
```

若檔案同時需要 Blob 下載：
```ts
import { apiRequest, apiDownloadBlob } from '@/services/api-service'
```

### 2. 替換 POST / GET / PUT / DELETE

```ts
// ❌ 舊
const res = await http.post<{ data: Foo[]; total: number }>('/endpoint', body)
items.value = res.data.data

// ✅ 新
const res = await apiRequest<{ data: Foo[]; total: number }>('/endpoint', { method: 'POST', data: body })
if (res.success) {
  items.value = res.data!.data
} else {
  console.error('[composableName/functionName]', res.message, res.errors)
}
```

HTTP method 對應：
| 舊 | 新 `options.method` |
|----|---------------------|
| `http.get(url)` | `apiRequest(url)` 或 `apiRequest(url, { method: 'GET' })` |
| `http.post(url, body)` | `apiRequest(url, { method: 'POST', data: body })` |
| `http.put(url, body)` | `apiRequest(url, { method: 'PUT', data: body })` |
| `http.delete(url)` | `apiRequest(url, { method: 'DELETE' })` |

### 3. 移除 try-catch（apiRequest 永不拋出）

```ts
// ❌ 舊
try {
  const res = await http.post(...)
  // 成功邏輯
} catch (error) {
  handleApiError(error, 'composable/fn')
} finally {
  loading.value = false
}

// ✅ 新（保留 finally，移除 catch）
try {
  const res = await apiRequest(...)
  if (res.success) {
    // 成功邏輯
  } else {
    console.error('[composable/fn]', res.message, res.errors)
  }
} finally {
  loading.value = false
}
```

> 若原本沒有 `loading` 狀態，整個 try/finally 可拿掉，直接呼叫 `apiRequest` 並做 `res.success` 判斷。

### 4. Blob 下載（可能拋出，需保留 try-catch）

```ts
// ✅
try {
  const blob = await apiDownloadBlob('/reports/export', { method: 'POST', body: payload })
  // 處理 blob ...
} catch (error) {
  console.error('[composable/fn] download failed', error)
}
```

### 5. Pinia store (login 等直接拋出給呼叫方的情境)

若 store action 原本讓例外往上傳遞（讓 UI 層 catch），改用 `apiRequest` 後需調整：

```ts
// ✅ 建議：store action 回傳 ApiResponse，由 UI 自行判斷
async function login(request: LoginRequest) {
  const res = await apiRequest<LoginResponse>('/api/auth/login', { method: 'POST', data: request })
  if (res.success) {
    token.value = res.data!.token
    saveToken(res.data!.token, res.data!.expiresAt)
    localStorage.setItem('user_name', res.data!.userName)
  }
  return res
}
```

## 完成後檢查

- [ ] 所有 `import ... from '@/api/http'` 已移除或替換
- [ ] 所有 `http.get / post / put / delete` 已替換為 `apiRequest`
- [ ] 所有 `handleApiError` 已以 `console.error` 或 `res.message` 取代
- [ ] `res.data` 存取前已確認 `res.success` 為 `true`，並以 `!` 非空斷言
- [ ] Blob 下載改用 `apiDownloadBlob`，並保留 try-catch
