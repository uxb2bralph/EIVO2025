// useWinningNumberIndex.ts – WinningNumberIndex.vue 的邏輯 composable。
// 遷移自 WebHome WinningNumberController.Index / Inquire、列管理動作（CommitItem / DeleteItem），
// 發票對獎（MatchWinningInvoiceNo）/ 清除中獎發票（ClearWinningInvoiceNo），
// 以及雲端發票中獎清冊範本下載（GetSample）/ Excel 上傳（UploadWinningNo）。
// 查詢須同時帶入發票年度與期別；列表依獎別排序，頭獎會由後端自動衍生二~六獎。
import { ref, computed, onUnmounted } from 'vue'
import {
  queryWinningNumbers,
  commitWinningNumber,
  deleteWinningNumber,
  matchWinningInvoiceNo,
  clearWinningInvoiceNo,
  downloadWinningSample,
  uploadWinningNo,
  checkWinningProcess,
  downloadWinningResult,
} from '@/services/winning-number-api'
import type { WinningNumberDatatable } from '@/services/winning-number-api'

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

// 可維護的獎別選項（對應舊版 Naming.EditableWinningPrizeType）。
export const EDITABLE_RANK_OPTIONS = [
  { value: 1, label: '特別獎' },
  { value: 2, label: '特獎' },
  { value: 3, label: '頭獎' },
  { value: 9, label: '增開六獎' },
]

// 期別選項（1~6 對應雙月；沿用舊版 WinningNoQuery.cshtml 之下拉選項）。
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

/** 期別 → 雙月標籤（如 1 → "01-02月"，沿用舊版 DataItem.cshtml 格式） */
export function periodLabel(period: number): string {
  const from = String(period * 2 - 1).padStart(2, '0')
  const to = String(period * 2).padStart(2, '0')
  return `${from}-${to}月`
}

/** 中獎金額格式化（千分位；沿用舊版 "{0:##,###,###,##0.##}"） */
export function bonusLabel(bonus: number | null): string {
  return bonus == null ? '' : bonus.toLocaleString('en-US')
}

/** 依獎別回傳中獎號碼碼數（特別獎 / 特獎 / 頭獎為 8 碼，增開六獎為 3 碼） */
export function winningNoLength(rank: number): number {
  return rank === 9 ? 3 : 8
}

export function useWinningNumberIndex() {
  const now = new Date()

  // 發票年度下拉：2011 ~ 今年（沿用舊版 WinningNoQuery.cshtml），預設今年（西元）。
  const yearOptions = (() => {
    const result: number[] = []
    for (let y = now.getFullYear(); y >= 2011; y--) result.push(y)
    return result
  })()

  // 查詢條件（Year / PeriodNo 皆必填）。期別預設當期（沿用舊版 (Month+1)/2）。
  const year = ref<number>(now.getFullYear())
  const periodNo = ref<number>(Math.floor((now.getMonth() + 2) / 2))

  // 列表狀態
  const items = ref<WinningNumberDatatable[]>([])
  const loading = ref(false)
  const error = ref('')
  const message = ref('')
  const searched = ref(false)
  // 目前已查詢的年度 / 期別（供對獎 / 清除 / 新增列沿用；避免使用者改動查詢條件但尚未查詢造成不一致）。
  const queriedYear = ref<number | null>(null)
  const queriedPeriod = ref<number | null>(null)

  const hasResult = computed(() => items.value.length > 0)

  async function load() {
    loading.value = true
    error.value = ''
    message.value = ''
    cancelEdit()
    try {
      const res = await queryWinningNumbers({ year: year.value, periodNo: periodNo.value })
      if (res.success && res.data) {
        items.value = res.data
        queriedYear.value = year.value
        queriedPeriod.value = periodNo.value
      } else {
        items.value = []
        queriedYear.value = null
        queriedPeriod.value = null
        error.value = res.errors?.length ? res.errors.join('\n') : res.message || '查詢失敗'
      }
    } finally {
      searched.value = true
      loading.value = false
    }
  }

  function onSearch() {
    load()
  }

  // --- 列內編輯（遷移自舊版 editItem → EditItem / commitItem → CommitItem）---
  // 僅可維護獎別（特別獎 / 特獎 / 頭獎 / 增開六獎）可編輯；獎別與號碼皆可修改。
  const editingId = ref<number | null>(null)
  const editRank = ref<number>(3)
  const editWinningNo = ref('')
  const saving = ref(false)

  function startEdit(row: WinningNumberDatatable) {
    editingId.value = row.winningId
    editRank.value = row.rank
    editWinningNo.value = row.winningNo ?? ''
    error.value = ''
    message.value = ''
  }

  function cancelEdit() {
    editingId.value = null
    editRank.value = 3
    editWinningNo.value = ''
  }

  // 儲存列編輯（對應後端 CommitItem）。成功後重新查詢以反映頭獎衍生列。
  async function saveEdit(row: WinningNumberDatatable) {
    saving.value = true
    error.value = ''
    try {
      const res = await commitWinningNumber({
        winningId: row.winningId,
        year: row.year,
        period: row.period,
        rank: editRank.value,
        winningNo: editWinningNo.value.trim(),
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

  // 刪除中獎號碼（對應後端 DeleteItem）。破壞性動作先行確認；成功後重新載入。
  async function removeItem(row: WinningNumberDatatable) {
    if (!window.confirm('確定刪除此筆資料?')) {
      return
    }
    error.value = ''
    const res = await deleteWinningNumber(row.winningId)
    if (res.success) {
      await load()
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '刪除失敗'
    }
  }

  // --- 新增中獎號碼（遷移自舊版 AddItem → commitItem → CommitItem）---
  // 年度 / 期別沿用目前查詢條件；獎別預設頭獎。
  const addRank = ref<number>(3)
  const addWinningNo = ref('')
  const adding = ref(false)

  async function addItem() {
    if (queriedYear.value == null || queriedPeriod.value == null) {
      error.value = '請先查詢後再新增!!'
      return false
    }
    adding.value = true
    error.value = ''
    try {
      const res = await commitWinningNumber({
        winningId: null,
        year: queriedYear.value,
        period: queriedPeriod.value,
        rank: addRank.value,
        winningNo: addWinningNo.value.trim(),
      })
      if (res.success) {
        addWinningNo.value = ''
        await load()
        return true
      }
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '新增失敗'
      return false
    } finally {
      adding.value = false
    }
  }

  // --- 發票對獎 / 清除中獎發票（遷移自 MatchWinningInvoiceNo / ClearWinningInvoiceNo）---
  const matching = ref(false)
  const clearing = ref(false)

  // 執行發票對獎（對應後端 MatchWinningInvoiceNo）。以目前查詢的年度 / 期別為對象。
  async function doMatch() {
    if (queriedYear.value == null || queriedPeriod.value == null) {
      error.value = '請先查詢後再執行對獎!!'
      return
    }
    if (!window.confirm('確定執行發票對獎？此作業將建立中獎發票對應並發送中獎通知。')) {
      return
    }
    matching.value = true
    error.value = ''
    message.value = ''
    try {
      const res = await matchWinningInvoiceNo({
        year: queriedYear.value,
        periodNo: queriedPeriod.value,
      })
      if (res.success) {
        message.value = res.message || '對獎作業完成!!'
      } else {
        error.value = res.errors?.length ? res.errors.join('\n') : res.message || '對獎作業失敗'
      }
    } finally {
      matching.value = false
    }
  }

  // 清除中獎發票（對應後端 ClearWinningInvoiceNo）。破壞性動作先行確認。
  async function doClear() {
    if (queriedYear.value == null || queriedPeriod.value == null) {
      error.value = '請先查詢後再執行清除!!'
      return
    }
    if (!window.confirm('確定清除此年度 / 期別的中獎發票？此動作無法復原。')) {
      return
    }
    clearing.value = true
    error.value = ''
    message.value = ''
    try {
      const res = await clearWinningInvoiceNo({
        year: queriedYear.value,
        periodNo: queriedPeriod.value,
      })
      if (res.success) {
        message.value = res.message || '中獎發票已清除完成!!'
      } else {
        error.value = res.errors?.length ? res.errors.join('\n') : res.message || '清除失敗'
      }
    } finally {
      clearing.value = false
    }
  }

  // --- 雲端發票中獎清冊：範本下載 / Excel 上傳（遷移自舊版 WinningNoQuery.cshtml 資料維護區塊）---
  const downloadingSample = ref(false)
  const uploading = ref(false)
  const uploadError = ref('')
  // 處理中（已上傳，背景比對尚未完成，輪詢中）。
  const processing = ref(false)
  // 例外 / 提示訊息（對應舊版 ExceptionLog；即使有例外仍可下載含處理狀態之結果檔）。
  const processMessage = ref('')
  // 結果檔是否已就緒可供下載。
  const resultReady = ref(false)
  const resultTaskId = ref<number | null>(null)
  const resultFileName = ref('中獎發票回應.xlsx')
  const downloadingResult = ref(false)

  // 下載中獎清冊範本（對應後端 DownloadSample）。
  async function downloadSample() {
    downloadingSample.value = true
    uploadError.value = ''
    try {
      const blob = await downloadWinningSample()
      triggerBlobDownload(blob, 'WinningSample.xlsx')
    } catch {
      uploadError.value = '範本下載失敗'
    } finally {
      downloadingSample.value = false
    }
  }

  // 輪詢計時器（單一，重新排程前先清除；卸載時一併清除）。
  let pollTimer: ReturnType<typeof setTimeout> | null = null
  function clearPoll() {
    if (pollTimer) {
      clearTimeout(pollTimer)
      pollTimer = null
    }
  }
  function schedulePoll() {
    clearPoll()
    pollTimer = setTimeout(poll, 3000)
  }

  // 查詢處理狀態（對應後端 CheckProcess）。完成則開放下載；未完成則續行輪詢。
  async function poll() {
    if (resultTaskId.value == null) {
      return
    }
    const res = await checkWinningProcess(resultTaskId.value)
    if (res.success && res.data) {
      if (res.data.message) {
        processMessage.value = res.data.message
      }
      if (res.data.completed) {
        processing.value = false
        resultReady.value = true
        return
      }
      schedulePoll()
    } else {
      processing.value = false
      uploadError.value = res.errors?.length ? res.errors.join('\n') : res.message || '處理狀態查詢失敗'
    }
  }

  // 上傳中獎清冊 Excel（對應後端 UploadWinningNo）。成功後開始輪詢處理狀態。
  async function uploadWinningNoFile(file: File) {
    uploading.value = true
    uploadError.value = ''
    processMessage.value = ''
    resultReady.value = false
    resultTaskId.value = null
    clearPoll()
    try {
      const res = await uploadWinningNo(file)
      if (res.success && res.data) {
        resultTaskId.value = res.data.taskId
        resultFileName.value = res.data.fileDownloadName || '中獎發票回應.xlsx'
        processing.value = true
        schedulePoll()
      } else {
        uploadError.value = res.errors?.length ? res.errors.join('\n') : res.message || '上傳失敗'
      }
    } finally {
      uploading.value = false
    }
  }

  // 下載處理結果檔（對應後端 DownloadResult）。
  async function downloadResult() {
    if (resultTaskId.value == null) {
      return
    }
    downloadingResult.value = true
    uploadError.value = ''
    try {
      const blob = await downloadWinningResult(resultTaskId.value)
      triggerBlobDownload(blob, resultFileName.value)
    } catch {
      uploadError.value = '結果檔下載失敗'
    } finally {
      downloadingResult.value = false
    }
  }

  onUnmounted(clearPoll)

  return {
    // 選項
    yearOptions,
    periodOptions: PERIOD_OPTIONS,
    rankOptions: EDITABLE_RANK_OPTIONS,
    // 查詢條件
    year,
    periodNo,
    // 列表狀態
    items,
    loading,
    error,
    message,
    searched,
    hasResult,
    // 列內編輯
    editingId,
    editRank,
    editWinningNo,
    saving,
    startEdit,
    cancelEdit,
    saveEdit,
    removeItem,
    // 新增
    addRank,
    addWinningNo,
    adding,
    addItem,
    // 對獎 / 清除
    matching,
    clearing,
    doMatch,
    doClear,
    // 雲端發票中獎清冊：範本下載 / Excel 上傳
    downloadingSample,
    uploading,
    uploadError,
    processing,
    processMessage,
    resultReady,
    resultFileName,
    downloadingResult,
    downloadSample,
    uploadWinningNoFile,
    downloadResult,
    // 事件 / 工具
    load,
    onSearch,
    toRocYear,
    periodLabel,
    bonusLabel,
    winningNoLength,
  }
}
