---
name: vue-table-selection
description: 為 Vue 3 表格加入「每列勾選 + 表頭全選 + 半選 + 批次操作」功能。當使用者要求為某張表格 / result table 加上 checkbox 選取、全選、批次處理（批次停用/刪除/匯出等）時使用。產出響應式的 useTableSelection composable + v-model:selected 樣板，而非以 querySelectorAll 操作 DOM。
---

# Vue 表格列勾選功能

為指定的 Vue 3 表格元件加入列勾選 / 全選 / 半選 / 批次操作，採「響應式狀態」而非 DOM 查詢。

本專案（TaskCenter/ClientApp）已有可重用實作，優先重用，不要重寫：
- `src/composables/useTableSelection.ts`
- 參考元件 `src/components/OrganizationResultTable.vue`、父層 `src/components/OrganizationQueryIndex.vue`

## 步驟

1. **確認目標**：使用者要加勾選功能的表格元件檔、以及每列的唯一鍵欄位（key）。若 key 可能為 null（例如 `keyId: string | null`），沿用 composable 的 `keyOf 回傳 null = 不可選` 語意。

2. **共用 composable**：若 `useTableSelection.ts` 已存在就直接 import；不存在才建立。簽章：
   `useTableSelection<T, K>(items: Ref<T[]>, keyOf: (item: T) => K | null)`
   回傳 `{ selected, allChecked, indeterminate, toggleAll, toggle, isChecked, clear }`。

3. **改表格元件（呈現層）**：
   - `const props = defineProps<{ items: T[]; ... }>()`（需具名以便 `toRef(props, 'items')`）。
   - `const selected = defineModel<K[]>('selected', { default: () => [] })` 對外開放選取結果。
   - `const { selected: selectedKeys, allChecked, indeterminate, toggleAll, toggle, isChecked, clear } = useTableSelection(toRef(props, 'items'), it => it.<key>)`（改名避免與 defineModel 的 `selected` 衝突）。
   - `watch(selectedKeys, keys => { selected.value = [...keys] })` 回寫 model。
   - `watch(() => props.items, () => clear())` 換頁/重查清空。
   - 表頭 checkbox：`:checked="allChecked" :indeterminate="indeterminate" @change="onToggleAll"`。
   - 每列 checkbox：`:checked="isChecked(row)" @change="onToggleItem(row, $event)"`。

4. **父層接線 + 批次按鈕**：
   - `const selectedKeys = ref<K[]>([])`，表格加 `v-model:selected="selectedKeys"`。
   - `const hasSelection = computed(() => selectedKeys.value.length > 0)`。
   - 批次處理函式讀 `[...selectedKeys.value]`；工具列按鈕 `:disabled="!hasSelection"`。真正呼叫後端批次 API 前先向使用者確認 endpoint，不要捏造不存在的 service 函式（留 TODO 並示範讀取結果）。

5. **驗證**：於 `TaskCenter/ClientApp` 執行 `npm run typecheck`（vue-tsc），確認無 unused / 型別錯誤。

## 踩雷點

- `indeterminate` 只能綁 DOM property：`:indeterminate="..."` 有效，純 HTML attribute 無效。
- defineModel 的 `selected` 與 composable 回傳的 `selected` 名稱衝突，destructure 時務必改名。
- 換頁/重查若沿用同一元件實例，未 `clear()` 會殘留舊 key，造成錯誤的半選狀態。
