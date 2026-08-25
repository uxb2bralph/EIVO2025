/**
 * useWinningInvoiceReportIndex.ts — 中獎統計表 頁面邏輯
 *
 * 遷移自 WebHome WinningInvoiceController.ReportIndex / InquireReport / ReportGridPage / CreateXlsx
 * （選單「中獎統計表」）。查詢表單沿用舊版 Module/InquireWinning.cshtml：
 * 開立人、發票日期起迄、代理業者（僅系統管理）。
 * 結果僅計中獎發票，依開立人彙總「中獎張數 / 捐贈張數」；另提供中獎統計表 Excel 匯出。
 * 發票日期起迄為必填（遷移時新增，避免無條件全表彙總）；資料範圍由後端依登入者角色過濾。
 */
import { computed, reactive, ref } from 'vue'
import { useAuthStore } from '@/auth'
import {
  searchSellers,
  searchAgents,
  type InvoiceQuerySellerOption,
  type InvoiceQueryAgentOption,
} from '@/services/invoice-process-api'
import {
  queryWinningInvoiceReport,
  exportWinningInvoiceReport,
  type WinningInvoiceReportQuery,
  type WinningInvoiceReportRow,
} from '@/services/winning-invoice-report-api'

/** Naming.RoleID.ROLE_SYS */
const ROLE_SYS = 1

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

export function useWinningInvoiceReportIndex() {
  const { state: authState } = useAuthStore()
  const isAdmin = computed(() => authState.user?.roleId === ROLE_SYS)

  // ── 查詢條件（對應舊版 Module/InquireWinning.cshtml）──
  const filters = reactive({
    dateFrom: '',
    dateTo: '',
  })

  // ── 開立人 autocomplete ───────────────────────────────────
  const sellerKeyword = ref('')
  const sellerOptions = ref<InvoiceQuerySellerOption[]>([])
  const selectedSellerKey = ref<string | null>(null)
  let sellerTimer: ReturnType<typeof setTimeout> | null = null

  async function runSellerSearch() {
    const res = await searchSellers(sellerKeyword.value.trim() || undefined)
    sellerOptions.value = res.success && res.data ? res.data : []
  }
  function onSellerInput() {
    if (sellerTimer) clearTimeout(sellerTimer)
    sellerTimer = setTimeout(runSellerSearch, 300)
  }
  function pickSeller(opt: InvoiceQuerySellerOption) {
    selectedSellerKey.value = opt.sellerKey
    sellerKeyword.value = `${opt.receiptNo ?? ''} ${opt.companyName ?? ''}`.trim()
    sellerOptions.value = []
  }
  function clearSeller() {
    selectedSellerKey.value = null
    sellerKeyword.value = ''
    sellerOptions.value = []
  }

  // ── 代理 autocomplete（僅系統管理）───────────────────────
  const agentKeyword = ref('')
  const agentOptions = ref<InvoiceQueryAgentOption[]>([])
  const selectedAgentKey = ref<string | null>(null)
  let agentTimer: ReturnType<typeof setTimeout> | null = null

  async function runAgentSearch() {
    const res = await searchAgents(agentKeyword.value.trim() || undefined)
    agentOptions.value = res.success && res.data ? res.data : []
  }
  function onAgentInput() {
    if (agentTimer) clearTimeout(agentTimer)
    agentTimer = setTimeout(runAgentSearch, 300)
  }
  function pickAgent(opt: InvoiceQueryAgentOption) {
    selectedAgentKey.value = opt.agentKey
    agentKeyword.value = `${opt.receiptNo ?? ''} ${opt.companyName ?? ''}`.trim()
    agentOptions.value = []
  }
  function clearAgent() {
    selectedAgentKey.value = null
    agentKeyword.value = ''
    agentOptions.value = []
  }

  // ── 清單狀態 ──────────────────────────────────────────────
  const items = ref<WinningInvoiceReportRow[]>([])
  const totalCount = ref(0)
  const page = ref(1)
  const pageSize = ref(10)
  const loading = ref(false)
  const error = ref('')
  const searched = ref(false)
  const totalPages = computed(() =>
    totalCount.value > 0 ? Math.ceil(totalCount.value / pageSize.value) : 0,
  )

  // ── 排序（統編 / 名稱 / 中獎張數 / 捐贈張數；未指定時後端依開立人 ID 遞增）──
  const sortName = ref('')
  const sortType = ref(0) // 0 未排序 / 1 遞增 / 2 遞減

  /** 組合送出查詢的條件物件。 */
  function buildQuery(): WinningInvoiceReportQuery {
    return {
      sellerKey: selectedSellerKey.value ?? undefined,
      agentKey: isAdmin.value ? (selectedAgentKey.value ?? undefined) : undefined,
      dateFrom: filters.dateFrom || undefined,
      dateTo: filters.dateTo || undefined,
      sortName: sortType.value ? sortName.value : undefined,
      sortType: sortType.value || undefined,
      page: page.value,
      pageSize: pageSize.value,
    }
  }

  /** 發票日期起迄必填。回傳錯誤訊息，通過則為空字串。 */
  function validate(): string {
    const messages: string[] = []
    if (!filters.dateFrom) messages.push('請輸入查詢起日')
    if (!filters.dateTo) messages.push('請輸入查詢迄日')
    return messages.join('\n')
  }

  async function load() {
    const invalid = validate()
    if (invalid) {
      error.value = invalid
      items.value = []
      totalCount.value = 0
      searched.value = true
      return
    }

    loading.value = true
    error.value = ''
    try {
      const res = await queryWinningInvoiceReport(buildQuery())
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

  function changePageSize(size: number) {
    pageSize.value = size
    page.value = 1
    load()
  }

  /** 點擊欄位標題切換排序（遞增 → 遞減 → 取消）。 */
  function toggleSort(key: string) {
    if (sortName.value !== key) {
      sortName.value = key
      sortType.value = 1
    } else {
      sortType.value = sortType.value === 1 ? 2 : sortType.value === 2 ? 0 : 1
      if (sortType.value === 0) sortName.value = ''
    }
    page.value = 1
    load()
  }

  // ── 匯出中獎統計表 ────────────────────────────────────────
  const exporting = ref(false)

  async function doExport() {
    const invalid = validate()
    if (invalid) {
      error.value = invalid
      return
    }

    exporting.value = true
    error.value = ''
    try {
      const query = buildQuery()
      query.page = 1
      query.pageSize = 0 // 匯出為全量統計（後端不分頁）
      const blob = await exportWinningInvoiceReport(query)
      triggerBlobDownload(blob, `中獎統計表(${todayStamp()}).xlsx`)
    } catch (e) {
      error.value = e instanceof Error ? e.message : '匯出失敗'
    } finally {
      exporting.value = false
    }
  }

  // ── 顯示格式 ──────────────────────────────────────────────
  function todayStamp(): string {
    const d = new Date()
    const m = String(d.getMonth() + 1).padStart(2, '0')
    const day = String(d.getDate()).padStart(2, '0')
    return `${d.getFullYear()}-${m}-${day}`
  }
  function formatAmount(value: number | null): string {
    if (value === null || value === undefined) return ''
    return value.toLocaleString('en-US')
  }

  return {
    // 角色
    isAdmin,
    // 條件
    filters,
    // 開立人
    sellerKeyword,
    sellerOptions,
    selectedSellerKey,
    onSellerInput,
    runSellerSearch,
    pickSeller,
    clearSeller,
    // 代理
    agentKeyword,
    agentOptions,
    selectedAgentKey,
    onAgentInput,
    runAgentSearch,
    pickAgent,
    clearAgent,
    // 清單
    items,
    totalCount,
    page,
    pageSize,
    loading,
    error,
    searched,
    totalPages,
    load,
    onSearch,
    goToPage,
    changePageSize,
    // 排序
    sortName,
    sortType,
    toggleSort,
    // 匯出
    exporting,
    doExport,
    // 格式
    formatAmount,
  }
}
