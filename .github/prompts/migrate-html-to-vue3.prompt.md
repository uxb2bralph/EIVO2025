---
description: "Migrate a legacy static HTML page to Vue 3. Use when: converting .html files, replacing iframe navigation, migrating Bootstrap/jQuery, moving assets to Vue src/. Converts HTML structure → Vuetify 3 components, CSS → src/assets/css/, images → src/assets/images/, JS logic → composables/stores."
argument-hint: "Path to the .html file to migrate (e.g. public/pages/TodoList.html)"
agent: "agent"
---

You are migrating a legacy static HTML page to the Vue 3 project at `HNB.Web/src/`.
Follow the project conventions in [vue.instructions.md](../../.github/instructions/vue.instructions.md).

## Input

The HTML file to migrate: `$argument`

If no argument is provided, ask which HTML file the user wants to migrate.

## Migration Rules

### 1. Component Structure

Create a `.vue` file following Composition API order:

```vue
<script setup lang="ts">
// 1. imports (auto-imports: ref, computed, watch, useRouter, useRoute, defineStore, storeToRefs)
// 2. props / emits
// 3. reactive state
// 4. composables
// 5. lifecycle hooks
// 6. methods
</script>

<template><!-- Vuetify 3 markup --></template>

<style scoped>/* scoped styles */</style>
```

### 2. File Placement

| HTML artefact | Vue destination |
|---|---|
| `public/pages/<Section>/<Page>.html` | `src/pages/<section>/<page>.vue` (kebab-case) |
| `public/pages/<Page>.html` (top-level) | `src/pages/<page>.vue` |
| `<link rel="stylesheet" href="...custom.css">` | Already in `src/assets/css/hnb-custom.css` — **remove the link** |
| `public/assets/images/*` | Keep at `public/assets/images/`; reference as `/assets/images/<file>` in CSS or `import` in `<script>` |
| `public/assets/js/<script>.js` | Extract logic into `src/composables/use<Name>.ts` or `src/stores/<domain>Store.ts` |

### 3. HTML → Vuetify 3 Mapping

| Bootstrap / HTML | Vuetify 3 equivalent |
|---|---|
| `<table class="table">` | `<v-data-table>` or `<v-table>` |
| `<input class="form-control">` | `<v-text-field>` |
| `<select class="form-select">` | `<v-select>` |
| `<button class="btn btn-*">` | `<v-btn>` with `:color` and `:variant` |
| `<div class="modal fade">` | `<v-dialog>` |
| `<div class="container-fluid">` | `<v-container fluid>` |
| `<div class="row"> / <div class="col-*">` | `<v-row> / <v-col cols="*">` |
| `<div class="card">` | `<v-card>` |
| `<div class="alert alert-*">` | `<v-alert :type="*">` |
| `<nav class="breadcrumb">` | `<v-breadcrumbs>` |
| `<span class="badge">` | `<v-chip>` |
| `$.ajax / fetch` | `async` function using `src/api/http.ts` |
| `alert() / confirm()` | `useSnackbar()` composable + `<v-dialog>` for confirm |
| `<iframe>` | `<RouterView />` (see layout) |

### 4. Logic Migration Checklist

- **jQuery DOM manipulation** → `ref()` reactive state + `v-model` / `v-if` / `v-for`
- **`$.ajax` / `fetch` calls** → `async` API function in `src/api/<domain>Api.ts` using `http.ts`
- **`document.cookie` auth** → `useAuthStore()` from `src/stores/authStore.ts`
- **`window.location.href` navigation** → `useRouter().push()`
- **`localStorage` state** → Pinia store
- **Inline `<script>` globals** → composable or store
- **Bootstrap accordion/tab JS** → Vuetify `<v-expansion-panels>` / `<v-tabs>`
- **`setTimeout`/polling** → lifecycle hook `onMounted` / `onUnmounted` with `clearInterval`

### 5. Routing

- The file-based router (`unplugin-vue-router`) uses `src/pages/` as the routes folder.
- Filename → route path: `src/pages/app/lc-app.vue` → `/app/lc-app`
- Use **kebab-case** for route paths and filenames.
- `useRouter()` and `useRoute()` are auto-imported.

### 6. API Layer

If the page makes HTTP calls, create or extend `src/api/<domain>Api.ts`:

```typescript
import http from '@/api/http'
import type { PagedResult, PageQuery } from '@/types/...'

export async function getItems(query: PageQuery): Promise<PagedResult<ItemDto>> {
  const { data } = await http.get('/api/...', { params: query })
  return data
}
```

### 7. User-visible Text

All labels, placeholders, button text, error messages, and titles must be **繁體中文**.

### 8. Error Handling

Wrap API calls in `try/catch`. Use `useSnackbar()` for success/error feedback.
Use `<v-dialog>` for delete confirmation.

## Output

1. Create the `.vue` page file(s) in `src/pages/`.
2. If new API functions are needed, create or update `src/api/<domain>Api.ts`.
3. If new TypeScript types are needed, create or update `src/types/<domain>.ts`.
4. If shared logic is extracted, create `src/composables/use<Name>.ts`.
5. Report what was migrated and any items that need manual follow-up (e.g. backend API endpoints that don't exist yet).
