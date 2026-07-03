# api-service.ts — 設計文件與應用說明

> 檔案位置：`ClientApp/src/services/api-service.ts`

---

## 概述

`api-service.ts` 是 ClientApp 前端所有 HTTP 請求的統一入口，基於 [ofetch](https://github.com/unjs/ofetch) 封裝而成。它負責：

- 自動附加 JWT Bearer Token
- Token 過期前主動刷新（proactive refresh）
- 收到 401 時自動重試（reactive refresh）
- 統一錯誤格式回傳（`ApiResponse<T>`）
- 檔案 Blob 下載

---

## 架構圖

```
呼叫方 (Vue 元件 / Store)
        │
        ▼
  apiRequest<T>()          ← 一般 JSON 請求
  apiDownloadBlob()        ← 檔案下載請求
        │
        ▼
    $api (ofetch instance)
        │
   ┌────┴────────────────────────────────┐
   │  onRequest hook                     │
   │  1. 讀取 accessToken                │
   │  2. 若已過期 → refreshAccessToken() │
   │  3. 若即將過期 → 提前刷新            │
   │  4. 寫入 Authorization header       │
   └────┬────────────────────────────────┘
        │  HTTP 請求
        ▼
    ASP.NET Core API
        │
   ┌────┴────────────────────────────────┐
   │  onResponse hook                    │
   │  記錄 API 層業務錯誤 (success=false) │
   └────┬────────────────────────────────┘
        │
   ┌────┴────────────────────────────────┐
   │  onResponseError hook               │
   │  401 → refreshAccessToken()         │
   │      → 使用新 token 重試原請求       │
   │      → 重試失敗 → redirectToLogin() │
   │  其他錯誤 → console.error           │
   └─────────────────────────────────────┘
```

---

## 匯出項目

### `$api`

ofetch 實例，已設定好所有 interceptor hook，通常不直接呼叫，而是透過 `apiRequest` / `apiDownloadBlob` 使用。

```ts
import { $api } from '@/services/api-service'
```

### `apiRequest<T>(url, options?)`

發送 JSON 請求，回傳 `ApiResponse<T>`，**永不拋出例外**。

| 參數 | 型別 | 說明 |
|------|------|------|
| `url` | `string` | API 路徑（相對於 `VITE_API_BASE_URL`，預設 `/api`） |
| `options` | `any`（ofetch options） | method、body、query 等 |

**回傳值：**

```ts
interface ApiResponse<T = unknown> {
  success: boolean
  message: string
  data?: T
  errors?: string[]
}
```

成功時 `success = true`，失敗（包含 4xx/5xx）時 `success = false`，呼叫方只需檢查 `success` 即可，無需 try/catch。

### `apiDownloadBlob(url, options?)`

發送請求並以 `Blob` 回傳，適用於 PDF / Excel 等檔案下載。認證 token 由 `$api` onRequest 自動處理。

| 參數 | 型別 | 說明 |
|------|------|------|
| `url` | `string` | API 路徑 |
| `options.method` | `string` | HTTP method，預設 `GET` |
| `options.body` | `unknown` | 請求 body（用於 POST 下載） |

---

## Token 生命週期管理

```
Token 狀態         處理方式
─────────────────  ──────────────────────────────────
正常（有效）        直接附加 Authorization header
即將過期（< 2分鐘） 提前呼叫 refreshAccessToken()，用新 token 發請求
已過期             先 refreshAccessToken()，成功才繼續；失敗則放棄請求
收到 401 回應      refreshAccessToken() → 用新 token 重試一次
重試仍 401         redirectToLogin()（避免無限迴圈，WeakSet 追蹤重試中的請求）
```

> **注意：** `isExpiringSoon` 閾值為 **2 分鐘**（`onRequest` 中），`isExpired` 判斷來自 `token-utils.ts`，`refreshAccessToken` / `redirectToLogin` 來自 `auth-utils.ts`。

---

## 使用範例

### 基本 GET 請求

```ts
import { apiRequest } from '@/services/api-service'
import type { Product } from '@/interfaces/product'

const res = await apiRequest<Product[]>('/products')
if (res.success) {
  console.log(res.data) // Product[]
} else {
  console.error(res.message)
}
```

### POST 請求（含 body）

```ts
const res = await apiRequest<Product>('/products', {
  method: 'POST',
  body: { name: 'Widget', price: 100 },
})
```

### 帶 Query String 的請求

```ts
const res = await apiRequest<PagedResult<Product>>('/products', {
  query: { page: 1, pageSize: 20 },
})
```

### 檔案下載（GET）

```ts
import { apiDownloadBlob } from '@/services/api-service'

const blob = await apiDownloadBlob('/reports/export')
const url = URL.createObjectURL(blob)
// 觸發下載 ...
URL.revokeObjectURL(url)
```

### 檔案下載（POST，帶篩選條件）

```ts
const blob = await apiDownloadBlob('/reports/export', {
  method: 'POST',
  body: { startDate: '2025-01-01', endDate: '2025-12-31' },
})
```

---

## 環境設定

| 環境變數 | 說明 | 預設值 |
|----------|------|--------|
| `VITE_API_BASE_URL` | API 基礎 URL | `/api` |

開發時 Vite `vite.config.ts` 中設定 proxy 將 `/api` 轉發至 `https://localhost:5443`。

---

## 依賴關係

```
api-service.ts
├── ofetch                          (HTTP client)
├── @/interfaces/api-response       (ApiResponse interface)
├── @/utils/auth-utils
│   ├── isExpiringSoon()
│   ├── refreshAccessToken()
│   └── redirectToLogin()
└── @/utils/token-utils
    └── isExpired()
```

---

## 注意事項 / 常見誤用

1. **不要直接呼叫 `$api` 處理 JSON**：請用 `apiRequest`，它已包裝錯誤處理，不會拋出例外。
2. **不需在呼叫方 try/catch**：`apiRequest` 保證永遠回傳 `ApiResponse`，只需檢查 `res.success`。
3. **`apiDownloadBlob` 可能拋出**：Blob 請求沒有包裝 try/catch，呼叫方需自行處理例外。
4. **401 自動重試只執行一次**：`retryingRequests` WeakSet 確保不會無限循環；若二次仍失敗會直接導向登入頁。
5. **不要手動設定 `Authorization` header**：`onRequest` 已自動處理，重複設定會導致 header 出現兩次。
