---
applyTo: "**/*.vue"
description: "Vue 3 coding conventions for HNB.Web frontend project. Use when: writing Vue components, views, composables, or any .vue file."
---

# Vue 3 Frontend Conventions

## Project Setup

- **Vue 3** with Composition API (`<script setup lang="ts">`)
- **TypeScript** throughout — no `any` types unless absolutely unavoidable
- **Vite** as build tool
- **Vuetify 3** as UI framework (components prefixed `<v-*>`)
- **Pinia** for state management
- **Axios** for HTTP calls
- **`unplugin-vue-router`** — file-based routing; pages live in `src/pages/`
- **`unplugin-auto-import`** — `ref`, `computed`, `watch`, `useRouter`, `useRoute`, `defineStore`, `storeToRefs` are all **auto-imported** (no explicit imports needed)
- **`unplugin-vue-components`** — Vuetify components auto-imported

## Single File Component Structure

Always use this order within `.vue` files:

```vue
<script setup lang="ts">
// 1. imports (only non-auto-imported items)
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

## Component Naming

- File name: **PascalCase** (`OrganizationList.vue`, `PurchaseOrderForm.vue`)
- In template: use PascalCase (`<OrganizationList />`)
- Page files go in `src/pages/` — filename becomes the route path (e.g., `src/pages/organizationInfo.vue` → `/organizationInfo`)
- Shared components go in `src/components/`
- Layouts go in `src/layouts/`

## TypeScript Types (`src/types/`)

- One file per domain: `organization.ts`, `negoLC.ts`, `purchaseOrder.ts`
- Interface names match backend DTO names: `OrganizationDto`, `CreateOrganizationRequest`
- Pagination types shared:
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

## API Layer (`src/api/`)

- One file per domain: `organizationApi.ts`, `negoLCApi.ts`
- Use a shared Axios instance from `src/api/http.ts` configured with:
  - `baseURL` from environment variable
  - JWT token interceptor (attach `Authorization: Bearer <token>` header)
  - Response error interceptor (handle 401 redirect, show error messages)
- All API functions are `async` and return typed responses:
  ```typescript
  export async function getOrganizations(query: PageQuery): Promise<PagedResult<OrganizationDto>> {
    const { data } = await http.get('/api/Organization', { params: query })
    return data
  }
  ```

## Pinia Stores (`src/stores/`)

- File name: `<domain>Store.ts` (camelCase)
- Use `defineStore` with setup syntax (auto-imported — no import statement needed):
  ```typescript
  export const useOrganizationStore = defineStore('organization', () => {
    // state as refs
    // actions as functions
    // getters as computed
    return { ... }
  })
  ```

## Composables (`src/composables/`)

- File name: `use<Name>.ts`
- Encapsulate reusable logic (e.g., `usePagedList.ts`, `useFormValidation.ts`)

## UI Patterns (Vuetify 3)

### List Views

- Use `<v-data-table>` or `<v-data-table-server>` for server-side pagination
- Include search/filter bar above the table using `<v-text-field>` and `<v-btn>`
- Loading state via `:loading` prop on the data table
- Action column with edit/delete icon buttons (`<v-btn icon>`)

### Form Views

- Use `<v-form ref="formRef">` with `<v-text-field>`, `<v-select>`, etc. and `:rules` prop for validation
- Client-side validation must match backend constraints
- Use `<v-dialog>` for create/edit modals or navigate to a separate form page
- Submit button shows loading state: `<v-btn :loading="saving">`

### Notifications

- Use `useSnackbar` composable or Vuetify `<v-snackbar>` for success/error feedback
- Use `<v-dialog>` with confirmation text before delete operations

### User-Facing Text

- All labels, placeholders, button text, error messages, and headings: **繁體中文**
- Examples: `新增`, `編輯`, `刪除`, `查詢`, `儲存`, `取消`, `確定要刪除嗎？`

## Router

- Routes are **file-based** via `unplugin-vue-router` — add a file to `src/pages/` to create a route
- Route paths: **kebab-case** (`/purchase-orders`, `/nego-lc`)
- `useRouter()` and `useRoute()` are auto-imported — no explicit import needed

## Error Handling

- Wrap API calls in `try/catch`
- Show snackbar/toast for failed operations
- Show snackbar/toast for successful create/update/delete
- Confirm with `<v-dialog>` before delete operations

## Code Style

- Use `const` by default, `let` only when reassignment is needed
- Destructure props and API responses
- Prefer `computed` over methods for derived state
- No `this` — Composition API only
- Use template refs with `ref<InstanceType<typeof Component>>()` pattern
