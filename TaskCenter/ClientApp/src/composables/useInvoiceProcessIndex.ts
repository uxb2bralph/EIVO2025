/**
 * useInvoiceProcessIndex.ts — 發票資料查詢／列印／匯出 頁面邏輯
 *
 * 遷移自 WebHome InvoiceProcessController.Index/Inquire（選單「資料查詢／列印／匯出」）。
 * 提供多條件查詢、分頁、排序、幣別統計、發票明細預覽，以及 Excel／買受人／ERP 匯出。
 * 查詢資料範圍由後端依登入者角色過濾；此處僅以角色（roleId）控制部分 UI 顯示。
 */
import { computed, reactive, ref } from 'vue'
import { useAuthStore } from '@/auth'
import {
  queryInvoices,
  getSummary,
  getInvoiceDetail,
  searchSellers,
  searchAgents,
  exportXlsx,
  exportBuyer,
  exportErp,
  type InvoiceProcessQuery,
  type InvoiceItemDatatable,
  type CurrencySummary,
  type InvoiceDetail,
  type InvoiceQuerySellerOption,
  type InvoiceQueryAgentOption,
} from '@/services/invoice-process-api'

/** Naming.RoleID.ROLE_SYS */
const ROLE_SYS = 1

/** 載具類型選項（對應舊版 ByCarrierType.cshtml）。 */
export const carrierTypeOptions = [
  { value: '3J0001', label: '網際優勢會員條碼' },
  { value: '3J0002', label: '手機條碼' },
  { value: '1K0001', label: '悠遊卡' },
  { value: 'BJ0001', label: '行動X卡' },
  { value: '2G0001', label: 'iCash' },
  { value: 'EK0002', label: '信用卡' },
  { value: '1K0002', label: '台灣智慧卡' },
  { value: 'BK0001', label: 'SmartPay金融卡' },
  { value: '1H0001', label: '一卡通' },
  { value: 'CQ0001', label: '自然人憑證條碼' },
]

/** 交易類型（Naming.InvoiceCenterBusinessType）。 */
export const businessTypeOptions = [
  { value: 1, label: '銷項' },
  { value: 2, label: '進項' },
]

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

export function useInvoiceProcessIndex() {
  const { state: authState } = useAuthStore()
  const isAdmin = computed(() => authState.user?.roleId === ROLE_SYS)

  // ── 查詢條件 ──────────────────────────────────────────────
  const filters = reactive({
    businessType: '' as number | '',
    buyerReceiptNo: '',
    buyerName: '',
    customerId: '',
    dateFrom: '',
    dateTo: '',
    invoiceNo: '',
    endNo: '',
    dataNo: '',
    attachment: '' as number | '',
    winning: '' as number | '',
    cancelled: '' as string, // '' 全部 / 'false' 未作廢 / 'true' 已作廢
    printMark: '',
    printed: '' as string, // '' 全部 / 'false' 未列印 / 'true' 已列印
    hasAddr: false,
    carrierType: '',
    carrierNo: '',
    isNoticed: false,
    currencySummary: false,
  })

  // ── 開立人 autocomplete ───────────────────────────────────
  const sellerKeyword = ref('')
  const sellerOptions = ref<InvoiceQuerySellerOption[]>([])
  const sellerLoading = ref(false)
  const selectedSellerKey = ref<string | null>(null)
  const selectedSellerLabel = ref('')
  let sellerTimer: ReturnType<typeof setTimeout> | null = null

  async function runSellerSearch() {
    sellerLoading.value = true
    try {
      const res = await searchSellers(sellerKeyword.value.trim() || undefined)
      sellerOptions.value = res.success && res.data ? res.data : []
    } finally {
      sellerLoading.value = false
    }
  }
  function onSellerInput() {
    if (sellerTimer) clearTimeout(sellerTimer)
    sellerTimer = setTimeout(runSellerSearch, 300)
  }
  function pickSeller(opt: InvoiceQuerySellerOption) {
    selectedSellerKey.value = opt.sellerKey
    selectedSellerLabel.value = `${opt.receiptNo ?? ''} ${opt.companyName ?? ''}`.trim()
    sellerOptions.value = []
    sellerKeyword.value = selectedSellerLabel.value
  }
  function clearSeller() {
    selectedSellerKey.value = null
    selectedSellerLabel.value = ''
    sellerKeyword.value = ''
    sellerOptions.value = []
  }

  // ── 代理 autocomplete（僅系統管理）───────────────────────
  const agentKeyword = ref('')
  const agentOptions = ref<InvoiceQueryAgentOption[]>([])
  const agentLoading = ref(false)
  const selectedAgentKey = ref<string | null>(null)
  const selectedAgentLabel = ref('')
  let agentTimer: ReturnType<typeof setTimeout> | null = null

  async function runAgentSearch() {
    agentLoading.value = true
    try {
      const res = await searchAgents(agentKeyword.value.trim() || undefined)
      agentOptions.value = res.success && res.data ? res.data : []
    } finally {
      agentLoading.value = false
    }
  }
  function onAgentInput() {
    if (agentTimer) clearTimeout(agentTimer)
    agentTimer = setTimeout(runAgentSearch, 300)
  }
  function pickAgent(opt: InvoiceQueryAgentOption) {
    selectedAgentKey.value = opt.agentKey
    selectedAgentLabel.value = `${opt.receiptNo ?? ''} ${opt.companyName ?? ''}`.trim()
    agentOptions.value = []
    agentKeyword.value = selectedAgentLabel.value
  }
  function clearAgent() {
    selectedAgentKey.value = null
    selectedAgentLabel.value = ''
    agentKeyword.value = ''
    agentOptions.value = []
  }

  // ── 清單狀態 ──────────────────────────────────────────────
  const items = ref<InvoiceItemDatatable[]>([])
  const totalCount = ref(0)
  const page = ref(1)
  const pageSize = ref(10)
  const loading = ref(false)
  const error = ref('')
  const searched = ref(false)
  const totalPages = computed(() =>
    totalCount.value > 0 ? Math.ceil(totalCount.value / pageSize.value) : 0,
  )

  // ── 排序 ──────────────────────────────────────────────────
  const sortName = ref('')
  const sortType = ref(0) // 0 未排序 / 1 遞增 / 2 遞減

  // ── 幣別統計 ──────────────────────────────────────────────
  const summary = ref<CurrencySummary[]>([])

  /** 組合送出查詢的條件物件。 */
  function buildQuery(): InvoiceProcessQuery {
    return {
      sellerKey: selectedSellerKey.value ?? undefined,
      agentKey: isAdmin.value ? (selectedAgentKey.value ?? undefined) : undefined,
      buyerReceiptNo: filters.buyerReceiptNo || undefined,
      buyerName: filters.buyerName || undefined,
      customerId: filters.customerId || undefined,
      dateFrom: filters.dateFrom || undefined,
      dateTo: filters.dateTo || undefined,
      invoiceNo: filters.invoiceNo || undefined,
      endNo: filters.endNo || undefined,
      dataNo: filters.dataNo || undefined,
      businessType: filters.businessType === '' ? undefined : Number(filters.businessType),
      attachment: filters.attachment === '' ? undefined : Number(filters.attachment),
      winning: filters.winning === '' ? undefined : Number(filters.winning),
      cancelled: filters.cancelled === '' ? undefined : filters.cancelled === 'true',
      printMark: filters.printMark || undefined,
      printed: filters.printed === '' ? undefined : filters.printed === 'true',
      hasAddr: filters.hasAddr || undefined,
      carrierType: filters.carrierType || undefined,
      carrierNo: filters.carrierNo || undefined,
      isNoticed: filters.isNoticed || undefined,
      sortName: sortType.value ? sortName.value : undefined,
      sortType: sortType.value || undefined,
      page: page.value,
      pageSize: pageSize.value,
    }
  }

  async function load() {
    loading.value = true
    error.value = ''
    try {
      const res = await queryInvoices(buildQuery())
      if (res.success && res.data) {
        items.value = res.data.items
        totalCount.value = res.data.totalCount
        page.value = res.data.pageNumber
      } else {
        items.value = []
        totalCount.value = 0
        error.value = res.errors?.length ? res.errors.join('\n') : res.message || '查詢失敗'
      }

      // 幣別統計（僅在開關開啟時額外查詢完整過濾集）。
      if (filters.currencySummary && !error.value) {
        const sres = await getSummary(buildQuery())
        summary.value = sres.success && sres.data ? sres.data : []
      } else {
        summary.value = []
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

  // ── 發票明細 Modal ───────────────────────────────────────
  const detailOpen = ref(false)
  const detail = ref<InvoiceDetail | null>(null)
  const detailLoading = ref(false)
  const detailError = ref('')

  async function openDetail(keyId: string | null) {
    if (!keyId) return
    detailOpen.value = true
    detailLoading.value = true
    detailError.value = ''
    detail.value = null
    try {
      const res = await getInvoiceDetail(keyId)
      if (res.success && res.data) {
        detail.value = res.data
      } else {
        detailError.value = res.message || '查無發票明細'
      }
    } finally {
      detailLoading.value = false
    }
  }
  function closeDetail() {
    detailOpen.value = false
    detail.value = null
    detailError.value = ''
  }

  // ── 匯出 ──────────────────────────────────────────────────
  const exporting = ref(false)

  async function runExport(fn: (q: InvoiceProcessQuery) => Promise<Blob>, filename: string) {
    exporting.value = true
    error.value = ''
    try {
      const query = buildQuery()
      query.page = 1
      query.pageSize = 0 // 匯出全量（後端不分頁）
      const blob = await fn(query)
      triggerBlobDownload(blob, filename)
    } catch (e) {
      error.value = e instanceof Error ? e.message : '匯出失敗'
    } finally {
      exporting.value = false
    }
  }

  const doExportXlsx = () => runExport(exportXlsx, '發票資料明細.xlsx')
  const doExportBuyer = () => runExport(exportBuyer, '發票買受人資料.xlsx')
  const doExportErp = () => runExport(exportErp, 'POSINV.dat')

  // ── 顯示格式 ──────────────────────────────────────────────
  function formatDate(value: string | null): string {
    if (!value) return ''
    const d = new Date(value)
    if (Number.isNaN(d.getTime())) return value
    const y = d.getFullYear()
    const m = String(d.getMonth() + 1).padStart(2, '0')
    const day = String(d.getDate()).padStart(2, '0')
    return `${y}/${m}/${day}`
  }
  function formatAmount(value: number | null): string {
    if (value === null || value === undefined) return ''
    return value.toLocaleString('en-US')
  }

  return {
    // 角色
    isAdmin,
    // 選項
    carrierTypeOptions,
    businessTypeOptions,
    // 條件
    filters,
    // 開立人
    sellerKeyword,
    sellerOptions,
    sellerLoading,
    selectedSellerKey,
    selectedSellerLabel,
    onSellerInput,
    runSellerSearch,
    pickSeller,
    clearSeller,
    // 代理
    agentKeyword,
    agentOptions,
    agentLoading,
    selectedAgentKey,
    selectedAgentLabel,
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
    // 統計
    summary,
    // 明細
    detailOpen,
    detail,
    detailLoading,
    detailError,
    openDetail,
    closeDetail,
    // 匯出
    exporting,
    doExportXlsx,
    doExportBuyer,
    doExportErp,
    // 格式
    formatDate,
    formatAmount,
  }
}
