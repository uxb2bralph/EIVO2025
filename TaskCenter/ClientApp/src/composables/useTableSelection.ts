import { computed, ref, type Ref } from 'vue'

/**
 * 表格列勾選共用邏輯：管理「已選 key 清單」，並推導全選 / 半選狀態。
 *
 * 以響應式狀態取代直接操作 DOM 的 checked，父層可透過回傳的 `selected` 取得選取結果。
 *
 * @param items 表格資料來源（響應式）
 * @param keyOf 取得單列唯一鍵的函式；回傳 null 的列視為不可選，全選時略過
 */
export function useTableSelection<T, K>(items: Ref<T[]>, keyOf: (item: T) => K | null) {
  const selected = ref<K[]>([]) as Ref<K[]>

  // 可被選取的列（排除無 key 者），全選 / 半選皆以此為基準
  const selectableKeys = computed(() =>
    items.value.map(keyOf).filter((k): k is K => k !== null),
  )

  const allChecked = computed(
    () => selectableKeys.value.length > 0 && selected.value.length === selectableKeys.value.length,
  )

  // 半選：部分勾選（表頭 checkbox 的 indeterminate 狀態）
  const indeterminate = computed(
    () => selected.value.length > 0 && selected.value.length < selectableKeys.value.length,
  )

  function toggleAll(checked: boolean) {
    selected.value = checked ? [...selectableKeys.value] : []
  }

  function toggle(item: T, checked: boolean) {
    const key = keyOf(item)
    if (key === null) return
    selected.value = checked
      ? [...selected.value, key]
      : selected.value.filter((k) => k !== key)
  }

  const isChecked = (item: T) => {
    const key = keyOf(item)
    return key !== null && selected.value.includes(key)
  }

  /** 換頁 / 重新查詢後清空選取，避免殘留舊 key */
  function clear() {
    selected.value = []
  }

  return { selected, allChecked, indeterminate, toggleAll, toggle, isChecked, clear }
}
