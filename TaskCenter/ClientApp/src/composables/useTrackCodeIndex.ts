// useTrackCodeIndex.ts – TrackCodeIndex.vue 的邏輯 composable。
// 遷移自 WebHome TrackCodeController.Index / Inquire 及列管理動作（CommitItem / DeleteItem）。
// 字軌以原始 trackId 傳遞；查詢一律帶入發票年度，列表以雙月期別呈現。
import { ref, computed } from 'vue'
import { queryTrackCodes, commitTrackCode, deleteTrackCode } from '@/services/track-code-api'
import type { TrackCodeDatatable } from '@/services/track-code-api'

// 發票類別選項（對應舊版 Naming.InvoiceTypeDefinition：7 一般 / 8 特種）
export const INVOICE_TYPE_OPTIONS = [
  { value: 7, label: '一般稅額計算之電子發票' },
  { value: 8, label: '特種稅額計算之電子發票' },
]

// 期別選項（1~6 對應雙月；沿用舊版 AddItem.cshtml 之下拉選項）
export const PERIOD_OPTIONS = [
  { value: 1, label: '01-02月' },
  { value: 2, label: '03-04月' },
  { value: 3, label: '05-06月' },
  { value: 4, label: '07-08月' },
  { value: 5, label: '09-10月' },
  { value: 6, label: '11-12月' },
]

/** 民國年（= 西元年 - 1911） */
export function toRocYear(year: number): number {
  return year - 1911
}

/** 期別 → 雙月標籤（如 1 → "01~02月"，沿用舊版 DataItem.cshtml 格式） */
export function periodLabel(periodNo: number): string {
  const from = String(periodNo * 2 - 1).padStart(2, '0')
  const to = String(periodNo * 2).padStart(2, '0')
  return `${from}~${to}月`
}

/** 發票類別 → 顯示名稱 */
export function invoiceTypeLabel(invoiceType: number | null): string {
  const opt = INVOICE_TYPE_OPTIONS.find((o) => o.value === invoiceType)
  return opt ? opt.label : ''
}

export function useTrackCodeIndex() {
  const now = new Date()

  // 發票年度下拉：2012 ~ 今年 + 1（沿用舊版 TrackCodePeriod.cshtml），預設今年（西元）。
  const yearOptions = (() => {
    const result: number[] = []
    for (let y = 2012; y <= now.getFullYear() + 1; y++) result.push(y)
    return result
  })()

  // 查詢條件
  const year = ref<number>(now.getFullYear())
  const periodNo = ref<number | ''>('')

  // 列表狀態
  const items = ref<TrackCodeDatatable[]>([])
  const totalCount = ref(0)
  const page = ref(1)
  const pageSize = ref(10)
  const loading = ref(false)
  const error = ref('')
  const searched = ref(false)

  const totalPages = computed(() =>
    totalCount.value > 0 ? Math.ceil(totalCount.value / pageSize.value) : 0,
  )

  async function load() {
    loading.value = true
    error.value = ''
    cancelEdit() // 重新查詢時結束任何進行中的列編輯
    try {
      const res = await queryTrackCodes({
        year: year.value,
        periodNo: periodNo.value === '' ? undefined : periodNo.value,
        page: page.value,
        pageSize: pageSize.value,
      })
      if (res.success && res.data) {
        items.value = res.data.items
        totalCount.value = res.data.totalCount
        page.value = res.data.pageNumber
      } else {
        items.value = []
        totalCount.value = 0
        error.value = res.message || '查詢失敗'
      }
    } finally {
      searched.value = true
      loading.value = false
    }
  }

  function onSearch() {
    page.value = 1
    load()
  }

  function goToPage(target: number) {
    if (target >= 1 && target <= totalPages.value && target !== page.value) {
      page.value = target
      load()
    }
  }

  // --- 列內編輯（遷移自舊版 edit → EditItem / commitItem → CommitItem）---
  // 修改僅變更字軌與類別；年度 / 期別沿用舊版不可修改（唯讀顯示）。
  const editingId = ref<number | null>(null)
  const editTrackCode = ref('')
  const editInvoiceType = ref<number>(7)
  const saving = ref(false)

  function startEdit(row: TrackCodeDatatable) {
    editingId.value = row.trackId
    editTrackCode.value = row.trackCode ?? ''
    editInvoiceType.value = row.invoiceType ?? 7
    error.value = ''
  }

  function cancelEdit() {
    editingId.value = null
    editTrackCode.value = ''
    editInvoiceType.value = 7
  }

  // 儲存列編輯（對應後端 CommitItem）。成功回傳 true。
  async function saveEdit(row: TrackCodeDatatable) {
    saving.value = true
    error.value = ''
    try {
      const res = await commitTrackCode({
        trackId: row.trackId,
        year: row.year,
        periodNo: row.periodNo,
        trackCode: editTrackCode.value.trim(),
        invoiceType: editInvoiceType.value,
      })
      if (res.success && res.data) {
        // 以後端回傳結果就地更新該列。
        const idx = items.value.findIndex((t) => t.trackId === res.data!.trackId)
        if (idx >= 0) items.value[idx] = res.data
        cancelEdit()
        return true
      }
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '儲存失敗'
      return false
    } finally {
      saving.value = false
    }
  }

  // 刪除字軌（對應後端 DeleteItem）。破壞性動作先行確認；成功後重新載入列表。
  async function removeItem(row: TrackCodeDatatable) {
    if (!window.confirm(`確定要刪除字軌「${row.trackCode}」嗎？`)) {
      return
    }
    error.value = ''
    const res = await deleteTrackCode(row.trackId)
    if (res.success) {
      await load()
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '刪除失敗'
    }
  }

  // --- 新增字軌（遷移自舊版 AddItem → commitItem → CommitItem）---
  // 年度沿用目前查詢年度；期別預設當期（沿用舊版 (Month+1)/2）。
  const addPeriodNo = ref<number>(Math.floor((now.getMonth() + 2) / 2))
  const addTrackCode = ref('')
  const addInvoiceType = ref<number>(7)
  const adding = ref(false)

  // 新增字軌（對應後端 CommitItem，trackId 為 null）。成功回傳 true。
  async function addItem() {
    adding.value = true
    error.value = ''
    try {
      const res = await commitTrackCode({
        trackId: null,
        year: year.value,
        periodNo: addPeriodNo.value,
        trackCode: addTrackCode.value.trim(),
        invoiceType: addInvoiceType.value,
      })
      if (res.success) {
        addTrackCode.value = ''
        await load() // 重新載入列表以反映新增結果
        return true
      }
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '新增失敗'
      return false
    } finally {
      adding.value = false
    }
  }

  return {
    // 選項
    yearOptions,
    periodOptions: PERIOD_OPTIONS,
    invoiceTypeOptions: INVOICE_TYPE_OPTIONS,
    // 查詢條件
    year,
    periodNo,
    // 列表狀態
    items,
    totalCount,
    page,
    loading,
    error,
    searched,
    totalPages,
    // 列內編輯
    editingId,
    editTrackCode,
    editInvoiceType,
    saving,
    startEdit,
    cancelEdit,
    saveEdit,
    removeItem,
    // 新增
    addPeriodNo,
    addTrackCode,
    addInvoiceType,
    adding,
    addItem,
    // 事件 / 工具
    load,
    onSearch,
    goToPage,
    toRocYear,
    periodLabel,
    invoiceTypeLabel,
  }
}
