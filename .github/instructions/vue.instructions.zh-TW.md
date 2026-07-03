---
applyTo: "**/*.vue"
description: "Vue 3 前端開發規範，用於 HNB.Web 專案。適用於撰寫 Vue 元件、views、composables 或任何 .vue 檔案。"
---

# Vue 3 前端規範

## 專案設定

- 使用 **Vue 3** 搭配 Composition API（`<script setup lang="ts">`）
- 全面使用 **TypeScript**，除非真的無法避免，否則不要使用 `any`
- 建置工具使用 **Vite**
- UI 框架使用 **Vuetify 3**（元件皆以 `<v-*>` 為前綴）
- 狀態管理使用 **Pinia**
- HTTP 呼叫使用 **Axios**
- 路由使用 **`unplugin-vue-router`** 的檔案式路由，頁面放在 `src/pages/`
- **`unplugin-auto-import`** 自動匯入 `ref`、`computed`、`watch`、`useRouter`、`useRoute`、`defineStore`、`storeToRefs`，**無需手動 import**
- **`unplugin-vue-components`** 自動匯入 Vuetify 元件

## 單一檔案元件結構

`.vue` 檔案內一律使用以下順序：

```vue
<script setup lang="ts">
// 1. imports（只需匯入非自動匯入的項目）
// 2. props / emits
// 3. reactive state (ref, reactive, computed)
// 4. composables
// 5. lifecycle hooks
// 6. methods
</script>

<template>
  <!-- Template content -->
</template>

<style scoped>
/* Scoped styles */
</style>
```

## 元件命名

- 檔名使用 **PascalCase**（`OrganizationList.vue`、`PurchaseOrderForm.vue`）
- 在 template 中也使用 PascalCase（`<OrganizationList />`）
- 頁面檔案放在 `src/pages/`，檔名即為路由路徑（例如 `src/pages/organizationInfo.vue` → `/organizationInfo`）
- 共用元件放在 `src/components/`
- Layout 放在 `src/layouts/`

## TypeScript 型別（`src/types/`）

- 每個領域一個檔案：`organization.ts`、`negoLC.ts`、`purchaseOrder.ts`
- 介面名稱需對應後端 DTO 名稱：`OrganizationDto`、`CreateOrganizationRequest`
- 分頁型別應共用：
  ```typescript
  export interface PagedResult<T> {
    data: T[]
    total: number
    message?: string
  }

  export interface ApiResult<T> {
    data: T | null
    message?: string
  }

  export interface PageQuery {
    pageIndex: number
    pageSize: number
  }
  ```

## API 層（`src/api/`）

- 每個領域一個檔案：`organizationApi.ts`、`negoLCApi.ts`
- 使用來自 `src/api/http.ts` 的共用 Axios instance，並設定：
  - 從環境變數讀取 `baseURL`
  - JWT token interceptor（附加 `Authorization: Bearer <token>` header）
  - Response error interceptor（處理 401 重新導向、顯示錯誤訊息）
- 所有 API 函式都必須是 `async` 並回傳明確型別：
  ```typescript
  export async function getOrganizations(query: PageQuery): Promise<PagedResult<OrganizationDto>> {
    const { data } = await http.get('/api/Organization', { params: query })
    return data
  }
  ```

## Pinia Stores（`src/stores/`）

- 檔名使用 `<domain>Store.ts`（camelCase）
- 使用 `defineStore` 的 setup 語法（自動匯入，無需 import）：
  ```typescript
  export const useOrganizationStore = defineStore('organization', () => {
    // state as refs
    // actions as functions
    // getters as computed
    return { ... }
  })
  ```

## Composables（`src/composables/`）

- 檔名使用 `use<Name>.ts`
- 封裝可重用邏輯（例如 `usePagedList.ts`、`useFormValidation.ts`）

## UI 模式（Vuetify 3）

### 列表頁

- 使用 `<v-data-table>` 或 `<v-data-table-server>` 實作伺服器端分頁
- 表格上方需有搜尋／篩選區（使用 `<v-text-field>` 與 `<v-btn>`）
- 透過資料表格的 `:loading` prop 顯示載入狀態
- 操作欄使用圖示按鈕（`<v-btn icon>`）提供編輯／刪除功能

### 表單頁

- 使用 `<v-form ref="formRef">` 搭配 `<v-text-field>`、`<v-select>` 等元件，並透過 `:rules` prop 進行驗證
- 前端驗證必須與後端限制一致
- 建立／編輯可使用 `<v-dialog>`，或導向獨立表單頁
- 送出按鈕在 API 呼叫期間需顯示 loading 狀態：`<v-btn :loading="saving">`

### 通知訊息

- 使用 `useSnackbar` composable 或 Vuetify `<v-snackbar>` 顯示成功／錯誤回饋
- 刪除前使用 `<v-dialog>` 顯示確認對話框

### 使用者可見文字

- 所有標籤、placeholder、按鈕文字、錯誤訊息與標題皆使用 **繁體中文**
- 範例：`新增`、`編輯`、`刪除`、`查詢`、`儲存`、`取消`、`確定要刪除嗎？`

## Router

- 路由為 **檔案式**，透過 `unplugin-vue-router` — 在 `src/pages/` 新增檔案即可建立路由
- 路由路徑使用 **kebab-case**（`/purchase-orders`、`/nego-lc`）
- `useRouter()` 與 `useRoute()` 皆自動匯入，無需手動 import

## 錯誤處理

- API 呼叫使用 `try/catch`
- 操作失敗時顯示 snackbar／toast 通知
- 建立／更新／刪除成功時顯示 snackbar／toast 通知
- 刪除前使用 `<v-dialog>` 二次確認

## 程式風格

- 預設使用 `const`，只有在需要重新指派時才使用 `let`
- 解構 props 與 API responses
- 衍生狀態優先使用 `computed`，不要用 methods 取代
- 不使用 `this`，一律採 Composition API
- template refs 使用 `ref<InstanceType<typeof Component>>()` 模式