/**
 * useMonthlyReportIndex.ts — 下載發票月報表 頁面邏輯
 *
 * 遷移自 WebHome InvoiceQueryController.MonthlyReport / InquireMonthlyReport
 * （選單「下載發票月報表」，/InvoiceQuery/MonthlyReport）。
 * 查詢條件僅開立人 / 代理業者 / 發票日期起迄；按「製作報表」後由後端逐月統計並回傳 Excel。
 *
 * 驗證沿用舊版：日期起迄必填、開立人與代理業者至少擇一。
 * 實際統計區間為「起日所屬月份 1 日」至「迄日所屬月份月底」（後端正規化，同舊版 CreateReport）。
 */
import { computed, ref } from 'vue'
import { useAuthStore } from '@/auth'
import {
  searchSellers,
  searchAgents,
  type InvoiceQuerySellerOption,
  type InvoiceQueryAgentOption,
} from '@/services/invoice-process-api'
import { exportMonthlyReport, type MonthlyReportQuery } from '@/services/monthly-report-api'

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

export function useMonthlyReportIndex() {
  const { state: authState } = useAuthStore()
  const isAdmin = computed(() => authState.user?.roleId === ROLE_SYS)

  // ── 查詢條件 ──────────────────────────────────────────────
  const dateFrom = ref('')
  const dateTo = ref('')

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
    selectedSellerKey.value = null
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

  // ── 代理業者 autocomplete（僅系統管理）────────────────────
  const agentKeyword = ref('')
  const agentOptions = ref<InvoiceQueryAgentOption[]>([])
  const selectedAgentKey = ref<string | null>(null)
  let agentTimer: ReturnType<typeof setTimeout> | null = null

  async function runAgentSearch() {
    const res = await searchAgents(agentKeyword.value.trim() || undefined)
    agentOptions.value = res.success && res.data ? res.data : []
  }
  function onAgentInput() {
    selectedAgentKey.value = null
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

  // ── 產製報表 ──────────────────────────────────────────────
  const exporting = ref(false)
  const error = ref('')
  const message = ref('')

  function buildQuery(): MonthlyReportQuery {
    return {
      sellerKey: selectedSellerKey.value ?? undefined,
      agentKey: isAdmin.value ? (selectedAgentKey.value ?? undefined) : undefined,
      dateFrom: dateFrom.value || undefined,
      dateTo: dateTo.value || undefined,
    }
  }

  /** 對應舊版 InquireMonthlyReport 的 ModelState 驗證。回傳錯誤訊息，通過則為空字串。 */
  function validate(): string {
    const messages: string[] = []
    if (!dateFrom.value) messages.push('請輸入查詢起日')
    if (!dateTo.value) messages.push('請輸入查詢迄日')
    if (!selectedSellerKey.value && !(isAdmin.value && selectedAgentKey.value)) {
      messages.push('請選擇代理人或開立人')
    }
    return messages.join('\n')
  }

  async function createReport() {
    const invalid = validate()
    if (invalid) {
      error.value = invalid
      message.value = ''
      return
    }

    exporting.value = true
    error.value = ''
    message.value = ''
    try {
      const blob = await exportMonthlyReport(buildQuery())
      triggerBlobDownload(blob, '月報表資料明細.xlsx')
      message.value = '月報表已下載!!'
    } catch (e) {
      error.value = e instanceof Error ? e.message : '月報表製作失敗'
    } finally {
      exporting.value = false
    }
  }

  return {
    // 角色
    isAdmin,
    // 條件
    dateFrom,
    dateTo,
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
    // 產製
    exporting,
    error,
    message,
    createReport,
  }
}
