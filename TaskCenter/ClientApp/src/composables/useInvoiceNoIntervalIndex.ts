// useInvoiceNoIntervalIndex.ts — InvoiceNoIntervalIndex.vue 的邏輯 composable。
// 遷移自 WebHome InvoiceNoController.MaintainInvoiceNoInterval 之核心動作
// （InquireInterval / CommitItem / DeleteNoInterval / LockInterval）。
// 開立人以加密 sellerKey 傳遞；查詢一律指定開立人 + 年度，列表以雙月期別呈現。
import { ref, computed } from 'vue'
import {
  queryIntervals,
  searchSellers,
  getTrackCodeOptions,
  commitInterval,
  deleteInterval,
  lockInterval,
  splitInterval,
  allotInterval,
  applyHeadquarter,
  commitBranch,
  downloadE0401,
  getSafetyStock,
  setSafetyStock,
} from '@/services/invoice-no-interval-api'
import type {
  InvoiceNoIntervalDatatable,
  InvoiceNoSellerOption,
  InvoiceTrackCodeOption,
} from '@/services/invoice-no-interval-api'
import { toRocYear, periodLabel, invoiceTypeLabel, PERIOD_OPTIONS } from '@/composables/useTrackCodeIndex'

export { toRocYear, periodLabel, invoiceTypeLabel }

/** 8 位數字補零（發票號碼顯示） */
export function padInvoiceNo(no: number): string {
  return String(no).padStart(8, '0')
}

/** 觸發瀏覽器下載 Blob（沿用 useWinningNumberIndex 的做法） */
function triggerBlobDownload(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

export function useInvoiceNoIntervalIndex() {
  const now = new Date()

  const yearOptions = (() => {
    const result: number[] = []
    for (let y = 2012; y <= now.getFullYear() + 1; y++) result.push(y)
    return result
  })()

  // ── 查詢條件 ──────────────────────────────────────────────
  const year = ref<number>(now.getFullYear())
  const periodNo = ref<number | ''>('')
  const branchRelation = ref(false)

  // 開立人選擇（autocomplete）
  const sellerKeyword = ref('')
  const sellerOptions = ref<InvoiceNoSellerOption[]>([])
  const sellerLoading = ref(false)
  const selectedSellerKey = ref('')
  const selectedSellerLabel = ref('')

  async function searchSellerOptions() {
    sellerLoading.value = true
    try {
      const res = await searchSellers(sellerKeyword.value.trim() || undefined)
      sellerOptions.value = res.success && res.data ? res.data : []
    } finally {
      sellerLoading.value = false
    }
  }

  function pickSeller(opt: InvoiceNoSellerOption) {
    selectedSellerKey.value = opt.sellerKey
    selectedSellerLabel.value = `${opt.receiptNo ?? ''} ${opt.companyName ?? ''}`.trim()
    sellerOptions.value = []
    sellerKeyword.value = selectedSellerLabel.value
  }

  function clearSeller() {
    selectedSellerKey.value = ''
    selectedSellerLabel.value = ''
    sellerKeyword.value = ''
    sellerOptions.value = []
  }

  // ── 列表狀態 ──────────────────────────────────────────────
  const items = ref<InvoiceNoIntervalDatatable[]>([])
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
    if (!selectedSellerKey.value) {
      error.value = '請選擇開立人!!'
      return
    }
    loading.value = true
    error.value = ''
    cancelEdit()
    try {
      const res = await queryIntervals({
        sellerKey: selectedSellerKey.value,
        year: year.value,
        periodNo: periodNo.value === '' ? undefined : periodNo.value,
        branchRelation: branchRelation.value || undefined,
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
        error.value = res.errors?.length ? res.errors.join('\n') : res.message || '查詢失敗'
      }
    } finally {
      searched.value = true
      loading.value = false
    }
    // 一併載入該開立人之可用配號存量警戒值（不阻塞列表顯示）。
    void loadSafetyStock()
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

  // ── 字軌選擇器（新增用）────────────────────────────────────
  const trackOptions = ref<InvoiceTrackCodeOption[]>([])
  async function loadTrackOptions() {
    const res = await getTrackCodeOptions(year.value, periodNo.value === '' ? undefined : periodNo.value)
    trackOptions.value = res.success && res.data ? res.data : []
  }

  // ── 列內編輯（修改僅變更起迄與 POS 機號）─────────────────────
  const editingId = ref<number | null>(null)
  const editStartNo = ref<number | null>(null)
  const editEndNo = ref<number | null>(null)
  const editDeviceName = ref('')
  const saving = ref(false)

  function startEdit(row: InvoiceNoIntervalDatatable) {
    editingId.value = row.intervalId
    editStartNo.value = row.startNo
    editEndNo.value = row.endNo
    editDeviceName.value = row.deviceName ?? ''
    error.value = ''
  }

  function cancelEdit() {
    editingId.value = null
    editStartNo.value = null
    editEndNo.value = null
    editDeviceName.value = ''
  }

  async function saveEdit(row: InvoiceNoIntervalDatatable) {
    saving.value = true
    error.value = ''
    try {
      const res = await commitInterval({
        intervalId: row.intervalId,
        trackId: null,
        sellerKey: null,
        startNo: editStartNo.value,
        endNo: editEndNo.value,
        deviceName: editDeviceName.value.trim() || null,
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

  async function removeItem(row: InvoiceNoIntervalDatatable) {
    if (!window.confirm('確定刪除此筆資料?')) return
    error.value = ''
    const res = await deleteInterval(row.intervalId)
    if (res.success) {
      await load()
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '刪除失敗'
    }
  }

  async function toggleLock(row: InvoiceNoIntervalDatatable) {
    const next = !row.locked
    if (!window.confirm(next ? '確定鎖定此筆資料?' : '確定解除鎖定此筆資料?')) return
    error.value = ''
    const res = await lockInterval(row.intervalId, next)
    if (res.success) {
      await load()
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '操作失敗'
    }
  }

  // ── 分割 / 本組數均分 ─────────────────────────────────────
  async function doSplit(row: InvoiceNoIntervalDatatable) {
    if (!window.confirm('確定分割此配號區間?')) return
    error.value = ''
    const res = await splitInterval(row.intervalId)
    if (res.success) {
      await load()
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '分割失敗'
    }
  }

  async function doAllot(row: InvoiceNoIntervalDatatable) {
    const input = window.prompt('請輸入均分本數（每本 50 個號碼）:', '')
    if (input == null) return
    const parts = Number(input)
    if (!Number.isInteger(parts) || parts <= 0) {
      error.value = '請輸入均分本數!!'
      return
    }
    error.value = ''
    const res = await allotInterval(row.intervalId, parts)
    if (res.success) {
      await load()
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '均分失敗'
    }
  }

  // ── 主機構配號 / 指派分支機構 ─────────────────────────────
  async function doApplyHeadquarter(row: InvoiceNoIntervalDatatable) {
    if (!window.confirm('確定指定此筆資料為主機構配號?')) return
    error.value = ''
    const res = await applyHeadquarter(row.intervalId)
    if (res.success) {
      await load()
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '操作失敗'
    }
  }

  // 分支機構選擇對話框（沿用開立人搜尋 API）
  const branchDialogOpen = ref(false)
  const branchIntervalId = ref<number | null>(null)
  const branchKeyword = ref('')
  const branchOptions = ref<InvoiceNoSellerOption[]>([])
  const branchLoading = ref(false)
  const branchSaving = ref(false)

  function openBranchDialog(row: InvoiceNoIntervalDatatable) {
    branchIntervalId.value = row.intervalId
    branchKeyword.value = ''
    branchOptions.value = []
    branchDialogOpen.value = true
    error.value = ''
  }

  function closeBranchDialog() {
    branchDialogOpen.value = false
    branchIntervalId.value = null
    branchOptions.value = []
  }

  async function searchBranchOptions() {
    branchLoading.value = true
    try {
      const res = await searchSellers(branchKeyword.value.trim() || undefined)
      branchOptions.value = res.success && res.data ? res.data : []
    } finally {
      branchLoading.value = false
    }
  }

  async function confirmBranch(opt: InvoiceNoSellerOption) {
    if (branchIntervalId.value == null) return
    branchSaving.value = true
    error.value = ''
    try {
      const res = await commitBranch(branchIntervalId.value, opt.sellerKey)
      if (res.success) {
        closeBranchDialog()
        await load()
      } else {
        error.value = res.errors?.length ? res.errors.join('\n') : res.message || '指派失敗'
      }
    } finally {
      branchSaving.value = false
    }
  }

  // ── 可用配號存量警戒值 ────────────────────────────────────
  const safetyStock = ref<number | null>(null)
  const safetyStockSaving = ref(false)

  async function loadSafetyStock() {
    if (!selectedSellerKey.value) {
      safetyStock.value = null
      return
    }
    const res = await getSafetyStock(selectedSellerKey.value)
    safetyStock.value = res.success && res.data ? res.data.safetyStock : null
  }

  async function saveSafetyStock() {
    if (!selectedSellerKey.value) return
    safetyStockSaving.value = true
    error.value = ''
    try {
      const res = await setSafetyStock(selectedSellerKey.value, safetyStock.value)
      if (!res.success) {
        error.value = res.errors?.length ? res.errors.join('\n') : res.message || '存量警戒值儲存失敗'
      }
    } finally {
      safetyStockSaving.value = false
    }
  }

  // ── 下載 E0401 ────────────────────────────────────────────
  const exporting = ref(false)
  async function exportE0401() {
    if (!selectedSellerKey.value) {
      error.value = '請選擇開立人!!'
      return
    }
    if (periodNo.value === '') {
      error.value = '請選擇期別（E0401 匯出需指定期別）!!'
      return
    }
    exporting.value = true
    error.value = ''
    try {
      const blob = await downloadE0401(selectedSellerKey.value, year.value, periodNo.value)
      const receipt = selectedSellerLabel.value.split(' ')[0] || 'export'
      triggerBlobDownload(blob, `E0402-${receipt}.zip`)
    } catch {
      error.value = 'E0401 匯出失敗（可能查無資料或伺服器錯誤）'
    } finally {
      exporting.value = false
    }
  }

  // ── 新增配號區間 ──────────────────────────────────────────
  const addTrackId = ref<number | ''>('')
  const addStartNo = ref<number | null>(null)
  const addEndNo = ref<number | null>(null)
  const addDeviceName = ref('')
  const adding = ref(false)

  async function addItem() {
    if (!selectedSellerKey.value) {
      error.value = '請選擇開立人!!'
      return false
    }
    if (addTrackId.value === '') {
      error.value = '字軌未設定!!'
      return false
    }
    adding.value = true
    error.value = ''
    try {
      const res = await commitInterval({
        intervalId: null,
        trackId: addTrackId.value,
        sellerKey: selectedSellerKey.value,
        startNo: addStartNo.value,
        endNo: addEndNo.value,
        deviceName: addDeviceName.value.trim() || null,
      })
      if (res.success) {
        addStartNo.value = null
        addEndNo.value = null
        addDeviceName.value = ''
        await load()
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
    // 查詢條件
    year,
    periodNo,
    branchRelation,
    // 開立人選擇
    sellerKeyword,
    sellerOptions,
    sellerLoading,
    selectedSellerKey,
    selectedSellerLabel,
    searchSellerOptions,
    pickSeller,
    clearSeller,
    // 列表
    items,
    totalCount,
    page,
    loading,
    error,
    searched,
    totalPages,
    load,
    onSearch,
    goToPage,
    // 字軌選擇器
    trackOptions,
    loadTrackOptions,
    // 列內編輯
    editingId,
    editStartNo,
    editEndNo,
    editDeviceName,
    saving,
    startEdit,
    cancelEdit,
    saveEdit,
    removeItem,
    toggleLock,
    doSplit,
    doAllot,
    doApplyHeadquarter,
    // 分支機構對話框
    branchDialogOpen,
    branchKeyword,
    branchOptions,
    branchLoading,
    branchSaving,
    openBranchDialog,
    closeBranchDialog,
    searchBranchOptions,
    confirmBranch,
    // 下載 E0401
    exporting,
    exportE0401,
    // 存量警戒值
    safetyStock,
    safetyStockSaving,
    saveSafetyStock,
    // 新增
    addTrackId,
    addStartNo,
    addEndNo,
    addDeviceName,
    adding,
    addItem,
    // 工具
    toRocYear,
    periodLabel,
    invoiceTypeLabel,
    padInvoiceNo,
  }
}
