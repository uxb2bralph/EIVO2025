// usePeriodicalExchangeRateIndex.ts – PeriodicalExchangeRateIndex.vue 的邏輯 composable。
// 遷移自 WebHome PeriodicalExchangeRateController.Index / Inquire、列管理動作（CommitItem / DeleteItem），
// 以及匯率資料範本下載（GetExchangeRateSample）/ Excel 匯入（UploadExchangeRate）。
// 查詢一律帶入發票年度；匯率以複合鍵（periodId + currencyId）識別，列表分頁呈現並可就地編輯幣別 / 匯率。
import { ref, computed } from 'vue'
import {
  queryExchangeRates,
  commitExchangeRate,
  deleteExchangeRate,
  downloadExchangeRateSample,
  uploadExchangeRate,
} from '@/services/periodical-exchange-rate-api'
import type { ExchangeRateDatatable } from '@/services/periodical-exchange-rate-api'

/** 觸發瀏覽器下載 Blob（建立暫時性物件 URL 後點擊隱藏連結） */
function triggerBlobDownload(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  a.remove()
  URL.revokeObjectURL(url)
}

// 期別選項（1~6 對應雙月；沿用舊版 TrackCodePeriod.cshtml 之下拉選項）。
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

/** 期別 → 雙月標籤（如 1 → "01-02月"，沿用舊版格式） */
export function periodLabel(periodNo: number): string {
  const from = String(periodNo * 2 - 1).padStart(2, '0')
  const to = String(periodNo * 2).padStart(2, '0')
  return `${from}-${to}月`
}

export function usePeriodicalExchangeRateIndex() {
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
  const currency = ref('')

  // 列表狀態
  const items = ref<ExchangeRateDatatable[]>([])
  const totalCount = ref(0)
  const page = ref(1)
  const pageSize = ref(10)
  const loading = ref(false)
  const error = ref('')
  const searched = ref(false)
  // 目前已查詢的年度（供新增列沿用；避免使用者改動查詢條件但尚未查詢造成不一致）。
  const queriedYear = ref<number | null>(null)

  const totalPages = computed(() =>
    totalCount.value > 0 ? Math.ceil(totalCount.value / pageSize.value) : 0,
  )

  async function load() {
    loading.value = true
    error.value = ''
    cancelEdit() // 重新查詢時結束任何進行中的列編輯
    try {
      const res = await queryExchangeRates({
        year: year.value,
        periodNo: periodNo.value === '' ? undefined : periodNo.value,
        currency: currency.value.trim() || undefined,
        page: page.value,
        pageSize: pageSize.value,
      })
      if (res.success && res.data) {
        items.value = res.data.items
        totalCount.value = res.data.totalCount
        page.value = res.data.pageNumber
        queriedYear.value = year.value
      } else {
        items.value = []
        totalCount.value = 0
        queriedYear.value = null
        error.value = res.errors?.length ? res.errors.join('\n') : res.message || '查詢失敗'
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

  // --- 列內編輯（遷移自舊版 editItem → EditItem / commitItem → CommitItem）---
  // 修改幣別與匯率；年度 / 期別維持唯讀。變更幣別時後端會將資料列搬移至新複合鍵。
  const editingKey = ref<string | null>(null)
  const editCurrency = ref('')
  const editExchangeRate = ref<number | null>(null)
  const saving = ref(false)

  /** 複合鍵字串（periodId + currencyId），供列識別 */
  function rowKey(row: ExchangeRateDatatable): string {
    return `${row.periodId}-${row.currencyId}`
  }

  function startEdit(row: ExchangeRateDatatable) {
    editingKey.value = rowKey(row)
    editCurrency.value = row.currency ?? ''
    editExchangeRate.value = row.exchangeRate
    error.value = ''
  }

  function cancelEdit() {
    editingKey.value = null
    editCurrency.value = ''
    editExchangeRate.value = null
  }

  // 儲存列編輯（對應後端 CommitItem）。成功後重新查詢以反映可能的搬移 / 排序變動。
  async function saveEdit(row: ExchangeRateDatatable) {
    saving.value = true
    error.value = ''
    try {
      const res = await commitExchangeRate({
        origPeriodId: row.periodId,
        origCurrencyId: row.currencyId,
        year: row.year,
        periodNo: row.periodNo,
        currency: editCurrency.value.trim(),
        exchangeRate: editExchangeRate.value,
      })
      if (res.success) {
        cancelEdit()
        await load()
        return true
      }
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '儲存失敗'
      return false
    } finally {
      saving.value = false
    }
  }

  // 刪除匯率（對應後端 DeleteItem）。破壞性動作先行確認；成功後重新載入。
  async function removeItem(row: ExchangeRateDatatable) {
    if (!window.confirm('確定刪除此筆匯率資料?')) {
      return
    }
    error.value = ''
    const res = await deleteExchangeRate(row.periodId, row.currencyId)
    if (res.success) {
      await load()
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '刪除失敗'
    }
  }

  // --- 新增匯率（遷移自舊版 AddItem / Create → CommitItem）---
  // 年度沿用目前查詢年度；期別預設當期（沿用舊版 (Month+1)/2）。
  const addPeriodNo = ref<number>(Math.floor((now.getMonth() + 2) / 2))
  const addCurrency = ref('')
  const addExchangeRate = ref<number | null>(null)
  const adding = ref(false)

  async function addItem() {
    if (queriedYear.value == null) {
      error.value = '請先查詢後再新增!!'
      return false
    }
    adding.value = true
    error.value = ''
    try {
      const res = await commitExchangeRate({
        origPeriodId: null,
        origCurrencyId: null,
        year: queriedYear.value,
        periodNo: addPeriodNo.value,
        currency: addCurrency.value.trim(),
        exchangeRate: addExchangeRate.value,
      })
      if (res.success) {
        addCurrency.value = ''
        addExchangeRate.value = null
        await load()
        return true
      }
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '新增失敗'
      return false
    } finally {
      adding.value = false
    }
  }

  // --- 匯率資料：範本下載 / Excel 匯入（遷移自舊版資料維護區塊）---
  const downloadingSample = ref(false)
  const uploading = ref(false)
  const uploadError = ref('')
  const uploadMessage = ref('')

  // 下載匯率資料範本（對應後端 GetExchangeRateSample）。
  async function downloadSample() {
    downloadingSample.value = true
    uploadError.value = ''
    uploadMessage.value = ''
    try {
      const blob = await downloadExchangeRateSample()
      triggerBlobDownload(blob, 'ExchangeRateSample.xlsx')
    } catch {
      uploadError.value = '範本下載失敗'
    } finally {
      downloadingSample.value = false
    }
  }

  // 匯入匯率 Excel（對應後端 UploadExchangeRate）。同步處理後下載含「處理狀態」欄的結果檔，並重新查詢列表。
  async function uploadExchangeRateFile(file: File) {
    uploading.value = true
    uploadError.value = ''
    uploadMessage.value = ''
    try {
      const result = await uploadExchangeRate(file)
      if (result.blob) {
        triggerBlobDownload(result.blob, '匯率資料(回應).xlsx')
        uploadMessage.value = '匯入完成，已下載處理結果檔，請確認各列處理狀態。'
        if (searched.value) {
          await load()
        }
      } else {
        uploadError.value = result.error || '匯入失敗'
      }
    } finally {
      uploading.value = false
    }
  }

  return {
    // 選項
    yearOptions,
    periodOptions: PERIOD_OPTIONS,
    // 查詢條件
    year,
    periodNo,
    currency,
    // 列表狀態
    items,
    totalCount,
    page,
    loading,
    error,
    searched,
    totalPages,
    // 列內編輯
    editingKey,
    editCurrency,
    editExchangeRate,
    saving,
    rowKey,
    startEdit,
    cancelEdit,
    saveEdit,
    removeItem,
    // 新增
    addPeriodNo,
    addCurrency,
    addExchangeRate,
    adding,
    addItem,
    // 匯率資料：範本下載 / Excel 匯入
    downloadingSample,
    uploading,
    uploadError,
    uploadMessage,
    downloadSample,
    uploadExchangeRateFile,
    // 事件 / 工具
    load,
    onSearch,
    goToPage,
    toRocYear,
    periodLabel,
  }
}
