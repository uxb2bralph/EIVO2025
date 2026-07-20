/**
 * useCreateInvoice.ts — 線上開立發票 頁面邏輯
 *
 * 遷移自 WebHome InvoiceBusinessController.CreateInvoice + Views/Forms/SimpleInvoice（F0401 存證）
 * 與 B2BInvoice（A0101 交換）。提供：開立人 autocomplete、相對營業人（買受人）帶入、
 * 發票主檔與明細輸入、金額試算（沿用舊版 uiInvoice.sum 邏輯）、產品快速查詢、內容預覽、開立。
 * 資料範圍由後端依角色（CanAccessSeller）守門。列印 / 立即檢視證明聯（需 QRCode 金鑰）延後。
 */
import { computed, reactive, ref } from 'vue'
import {
  searchSellers,
  searchCounterparts,
  searchProducts,
  commitInvoice,
  previewInvoice,
  PROCESS_TYPE,
  type CreateInvoiceRequest,
  type CreateInvoiceLine,
  type CreateInvoiceResult,
  type InvoicePreview,
  type CreateInvoiceSellerOption,
  type CounterpartOption,
  type ProductOption,
} from '@/services/create-invoice-api'

/** 課稅別選項（對應舊版 TaxType select）。 */
export const taxTypeOptions = [
  { value: 1, label: '應稅' },
  { value: 2, label: '零稅率' },
  { value: 3, label: '免稅' },
  { value: 4, label: '特種稅率' },
]

/** 發票類別（對應舊版 InvoiceType）。 */
export const invoiceTypeOptions = [
  { value: 7, label: '一般稅額計算之電子發票' },
  { value: 8, label: '特種稅額計算之電子發票' },
]

/** 載具類型（對應舊版 SimpleInvoice CarrierType）。 */
export const carrierTypeOptions = [
  { value: '', label: '不使用' },
  { value: '3J0002', label: '手機條碼' },
  { value: 'CQ0001', label: '自然人憑證條碼' },
  { value: '3J0001', label: '網際優勢會員條碼' },
  { value: '5G0001', label: '境外電商' },
]

/** F0401：買受人簽署適用零稅率註記。 */
export const clearanceMarkF0401Options = [
  { value: '', label: '無' },
  { value: 1, label: '買受人為園區事業' },
  { value: 2, label: '買受人為遠洋漁業' },
  { value: 3, label: '買受人為保稅區(自由貿易港區)' },
]

/** A0101：通關方式註記（零稅率必填）。 */
export const clearanceMarkA0101Options = [
  { value: '', label: '' },
  { value: 1, label: '非經海關出口' },
  { value: 2, label: '經海關出口' },
]

/** A0101：買受人註記。 */
export const buyerRemarkOptions = [
  { value: '', label: '' },
  { value: '1', label: '得抵扣之進貨及費用' },
  { value: '2', label: '得抵扣之固定資產' },
  { value: '3', label: '不得抵扣之進貨及費用' },
  { value: '4', label: '不得抵扣之固定資產' },
]

interface LineRow {
  itemNo: string
  brief: string
  piece: number | null
  unitCost: number | null
  costAmount: number | null
  remark: string
}

function emptyLine(): LineRow {
  return { itemNo: '', brief: '', piece: 1, unitCost: 1, costAmount: null, remark: '' }
}

/** 產生 4 位數隨機碼（對應舊版由 Ticks 取模）。 */
function genRandomNo(): string {
  return String(Math.floor(Math.random() * 10000)).padStart(4, '0')
}

export function useCreateInvoice() {
  // ── 電子發票型式 ──────────────────────────────────────────
  const processType = ref<number>(PROCESS_TYPE.F0401)
  const isA0101 = computed(() => processType.value === PROCESS_TYPE.A0101)

  // ── 表單狀態 ──────────────────────────────────────────────
  const form = reactive({
    // 買受人
    buyerReceiptNo: '',
    customerId: '',
    buyerName: '',
    address: '',
    phone: '',
    email: '',
    // 共用
    taxCalc: 'TaxIncluded' as 'TaxIncluded' | 'NonTaxed',
    invoiceType: 7,
    taxType: 1 as number,
    customsClearanceMark: '' as number | '',
    taxRate: '0.05',
    salesAmount: '' as number | '',
    taxAmount: '' as number | '',
    totalAmount: '' as number | '',
    randomNo: genRandomNo(),
    remark: '',
    // F0401 專屬
    carrierType: '',
    carrierId1: '',
    npoban: '',
    // A0101 專屬
    checkNo: '',
    buyerRemark: '',
    relateNumber: '',
    category: '',
    counterpart: false,
  })

  const lines = ref<LineRow[]>([emptyLine()])

  function addLine() {
    lines.value.push(emptyLine())
  }
  function removeLine(idx: number) {
    if (lines.value.length <= 1) return
    lines.value.splice(idx, 1)
  }

  /** 切換發票型式：套用各型式合理的稅額計算預設（SimpleInvoice 含稅 / B2BInvoice 未稅）。 */
  function setProcessType(pt: number) {
    processType.value = pt
    form.taxCalc = pt === PROCESS_TYPE.A0101 ? 'NonTaxed' : 'TaxIncluded'
    form.customsClearanceMark = ''
  }

  /** 課稅別變更 → 帶出對應稅率（對應舊版 onTaxTypeChange）。 */
  function onTaxTypeChange() {
    const map: Record<number, string> = { 1: '0.05', 2: '0', 3: '', 4: '' }
    form.taxRate = map[form.taxType] ?? ''
  }

  // ── 金額試算（沿用舊版 uiInvoice.sum）──────────────────────
  function sum(): boolean {
    let total = 0
    for (const row of lines.value) {
      const piece = Number(row.piece) || 0
      const unit = Number(row.unitCost) || 0
      const cost = piece * unit
      row.costAmount = cost
      total += cost
    }

    const rate = Number(form.taxRate) || 0
    let tax = 0
    let sales = 0
    if (form.taxCalc === 'TaxIncluded') {
      tax = Math.round((total / (1 + rate)) * rate)
      sales = total - tax
    } else {
      tax = Math.round(total * rate)
      sales = total
      total = sales + tax
    }

    form.totalAmount = total
    form.taxAmount = tax
    form.salesAmount = sales
    // 沿用舊版：備註 = 各明細備註串接。
    form.remark = lines.value.map((l) => l.remark || '').join('')
    return true
  }

  // ── 開立人 autocomplete ───────────────────────────────────
  const sellerKeyword = ref('')
  const sellerOptions = ref<CreateInvoiceSellerOption[]>([])
  const sellerLoading = ref(false)
  const selectedSellerKey = ref<string | null>(null)
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
    selectedSellerKey.value = null
    if (sellerTimer) clearTimeout(sellerTimer)
    sellerTimer = setTimeout(runSellerSearch, 300)
  }
  function pickSeller(opt: CreateInvoiceSellerOption) {
    selectedSellerKey.value = opt.sellerKey
    sellerKeyword.value = `${opt.receiptNo ?? ''} ${opt.companyName ?? ''}`.trim()
    sellerOptions.value = []
  }
  function clearSeller() {
    selectedSellerKey.value = null
    sellerKeyword.value = ''
    sellerOptions.value = []
  }

  // ── 相對營業人（買受人）帶入 ──────────────────────────────
  const buyerKeyword = ref('')
  const buyerOptions = ref<CounterpartOption[]>([])
  const buyerLoading = ref(false)
  let buyerTimer: ReturnType<typeof setTimeout> | null = null

  async function runBuyerSearch() {
    if (!selectedSellerKey.value) {
      buyerOptions.value = []
      return
    }
    const term = buyerKeyword.value.trim()
    if (!term) {
      buyerOptions.value = []
      return
    }
    buyerLoading.value = true
    try {
      const res = await searchCounterparts(selectedSellerKey.value, term)
      buyerOptions.value = res.success && res.data ? res.data : []
    } finally {
      buyerLoading.value = false
    }
  }
  function onBuyerInput() {
    if (buyerTimer) clearTimeout(buyerTimer)
    buyerTimer = setTimeout(runBuyerSearch, 300)
  }
  function applyCounterpart(opt: CounterpartOption) {
    form.buyerReceiptNo = opt.receiptNo ?? ''
    form.buyerName = opt.companyName ?? ''
    form.address = opt.address ?? ''
    form.phone = opt.phone ?? ''
    form.email = opt.email ?? ''
    if (opt.customerId) form.customerId = opt.customerId
    buyerKeyword.value = `${opt.receiptNo ?? ''} ${opt.companyName ?? ''}`.trim()
    buyerOptions.value = []
  }

  // ── 產品快速查詢（逐列）────────────────────────────────────
  const productSearchOpen = ref(false)
  const productSearchRow = ref<number>(-1)
  const productKeyword = ref('')
  const productOptions = ref<ProductOption[]>([])
  const productLoading = ref(false)
  let productTimer: ReturnType<typeof setTimeout> | null = null

  function openProductSearch(rowIdx: number) {
    productSearchRow.value = rowIdx
    productKeyword.value = lines.value[rowIdx]?.brief ?? ''
    productOptions.value = []
    productSearchOpen.value = true
    runProductSearch()
  }
  function closeProductSearch() {
    productSearchOpen.value = false
    productSearchRow.value = -1
    productOptions.value = []
  }
  async function runProductSearch() {
    productLoading.value = true
    try {
      const res = await searchProducts(
        selectedSellerKey.value ?? undefined,
        productKeyword.value.trim() || '*',
      )
      productOptions.value = res.success && res.data ? res.data : []
    } finally {
      productLoading.value = false
    }
  }
  function onProductInput() {
    if (productTimer) clearTimeout(productTimer)
    productTimer = setTimeout(runProductSearch, 300)
  }
  function pickProduct(opt: ProductOption) {
    const row = lines.value[productSearchRow.value]
    if (row) {
      row.brief = opt.productName ?? ''
      row.unitCost = opt.salePrice
      row.remark = opt.remark ?? ''
    }
    closeProductSearch()
  }

  // ── 組合請求 ──────────────────────────────────────────────
  function buildRequest(): CreateInvoiceRequest {
    const payloadLines: CreateInvoiceLine[] = lines.value.map((l) => ({
      itemNo: l.itemNo || undefined,
      brief: l.brief || undefined,
      piece: l.piece ?? null,
      unitCost: l.unitCost ?? null,
      costAmount: l.costAmount ?? null,
      remark: l.remark || undefined,
    }))

    const req: CreateInvoiceRequest = {
      sellerKey: selectedSellerKey.value ?? undefined,
      processType: processType.value,
      buyerReceiptNo: form.buyerReceiptNo || undefined,
      buyerName: form.buyerName || undefined,
      address: form.address || undefined,
      phone: form.phone || undefined,
      email: form.email || undefined,
      customerID: form.customerId || undefined,
      randomNo: form.randomNo || undefined,
      invoiceType: form.invoiceType,
      taxType: form.taxType,
      customsClearanceMark: form.customsClearanceMark === '' ? null : Number(form.customsClearanceMark),
      taxRate: form.taxRate === '' ? null : Number(form.taxRate),
      salesAmount: form.salesAmount === '' ? null : Number(form.salesAmount),
      taxAmount: form.taxAmount === '' ? null : Number(form.taxAmount),
      totalAmount: form.totalAmount === '' ? null : Number(form.totalAmount),
      remark: form.remark || undefined,
      lines: payloadLines,
    }

    if (isA0101.value) {
      req.checkNo = form.checkNo || undefined
      req.buyerRemark = form.buyerRemark || undefined
      req.relateNumber = form.relateNumber || undefined
      req.category = form.category || undefined
      req.counterpart = form.counterpart
    } else {
      req.carrierType = form.carrierType || undefined
      req.carrierId1 = form.carrierId1 || undefined
      req.npoban = form.npoban || undefined
    }

    return req
  }

  function validateBeforeSubmit(): string | null {
    if (!selectedSellerKey.value) return '請選擇發票開立人!!'
    if (!lines.value.some((l) => (l.brief || '').trim())) return '請至少輸入一筆發票明細!!'
    return null
  }

  // ── 內容預覽 ──────────────────────────────────────────────
  const previewOpen = ref(false)
  const preview = ref<InvoicePreview | null>(null)

  // ── 開立結果 ──────────────────────────────────────────────
  const resultOpen = ref(false)
  const result = ref<CreateInvoiceResult | null>(null)

  const submitting = ref(false)
  const error = ref('')

  async function doPreview() {
    error.value = ''
    const invalid = validateBeforeSubmit()
    if (invalid) {
      error.value = invalid
      return
    }
    sum()
    submitting.value = true
    try {
      const res = await previewInvoice(buildRequest())
      if (res.success && res.data) {
        preview.value = res.data
        previewOpen.value = true
      } else {
        error.value = res.message || '預覽失敗'
      }
    } finally {
      submitting.value = false
    }
  }
  function closePreview() {
    previewOpen.value = false
    preview.value = null
  }

  async function doCommit() {
    error.value = ''
    const invalid = validateBeforeSubmit()
    if (invalid) {
      error.value = invalid
      return
    }
    sum()
    submitting.value = true
    try {
      const res = await commitInvoice(buildRequest())
      if (res.success && res.data) {
        result.value = res.data
        resultOpen.value = true
      } else {
        error.value = res.errors?.length ? res.errors.join('\n') : res.message || '開立失敗'
      }
    } finally {
      submitting.value = false
    }
  }

  /** 開立成功後重置表單，供連續開立（對應舊版 resetForm）。 */
  function resetForm() {
    form.buyerReceiptNo = ''
    form.customerId = ''
    form.buyerName = ''
    form.address = ''
    form.phone = ''
    form.email = ''
    form.customsClearanceMark = ''
    form.salesAmount = ''
    form.taxAmount = ''
    form.totalAmount = ''
    form.randomNo = genRandomNo()
    form.remark = ''
    form.carrierType = ''
    form.carrierId1 = ''
    form.npoban = ''
    form.checkNo = ''
    form.buyerRemark = ''
    form.relateNumber = ''
    form.category = ''
    form.counterpart = false
    lines.value = [emptyLine()]
    buyerKeyword.value = ''
    buyerOptions.value = []
  }
  function closeResult() {
    resultOpen.value = false
    result.value = null
    resetForm()
  }

  return {
    // 型式
    processType,
    isA0101,
    setProcessType,
    // 選項
    taxTypeOptions,
    invoiceTypeOptions,
    carrierTypeOptions,
    clearanceMarkF0401Options,
    clearanceMarkA0101Options,
    buyerRemarkOptions,
    // 表單
    form,
    lines,
    addLine,
    removeLine,
    onTaxTypeChange,
    sum,
    // 開立人
    sellerKeyword,
    sellerOptions,
    sellerLoading,
    selectedSellerKey,
    onSellerInput,
    runSellerSearch,
    pickSeller,
    clearSeller,
    // 買受人
    buyerKeyword,
    buyerOptions,
    buyerLoading,
    onBuyerInput,
    runBuyerSearch,
    applyCounterpart,
    // 產品查詢
    productSearchOpen,
    productKeyword,
    productOptions,
    productLoading,
    openProductSearch,
    closeProductSearch,
    onProductInput,
    runProductSearch,
    pickProduct,
    // 預覽
    previewOpen,
    preview,
    doPreview,
    closePreview,
    // 結果
    resultOpen,
    result,
    doCommit,
    closeResult,
    // 狀態
    submitting,
    error,
  }
}
