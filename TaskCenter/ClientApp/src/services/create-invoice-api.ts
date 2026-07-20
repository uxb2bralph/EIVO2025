/**
 * create-invoice-api.ts — 線上開立發票 API
 *
 * 對應後端 CreateInvoiceController（/api/CreateInvoice），
 * 遷移自舊版 InvoiceBusinessController.CreateInvoice / CommitInvoice（F0401 存證）/
 * CommitA0101（B2B 交換）（選單「線上開立發票」）。
 * 開立對象（開立人）以加密後的 sellerKey 傳遞；金額由前端「金額」按鈕計算後帶入。
 */
import { apiRequest } from '@/services/api-service'
import type { ApiResponse } from '@/interfaces/api-response'

/** 電子發票型式（Naming.InvoiceProcessType）。 */
export const PROCESS_TYPE = {
  /** 存證 */
  F0401: 41,
  /** B2B 交換 */
  A0101: 21,
} as const

/** 發票明細品項列（對應後端 CreateInvoiceLineDto）。 */
export interface CreateInvoiceLine {
  itemNo?: string
  brief?: string
  piece?: number | null
  unitCost?: number | null
  costAmount?: number | null
  remark?: string
}

/** 開立發票請求（對應後端 CreateInvoiceRequestDto）。 */
export interface CreateInvoiceRequest {
  sellerKey?: string
  processType: number
  forPreview?: boolean
  // 買受人
  buyerReceiptNo?: string
  buyerName?: string
  address?: string
  phone?: string
  email?: string
  customerID?: string
  buyerMark?: number | null
  counterpart?: boolean
  b2bRelation?: boolean
  // 發票主檔
  randomNo?: string
  invoiceType?: number
  taxType?: number | null
  customsClearanceMark?: number | null
  taxRate?: number | null
  salesAmount?: number | null
  taxAmount?: number | null
  totalAmount?: number | null
  discountAmount?: number | null
  carrierType?: string
  carrierId1?: string
  carrierId2?: string
  npoban?: string
  remark?: string
  dataNumber?: string
  invoiceDate?: string
  // A0101 交換專屬
  checkNo?: string
  buyerRemark?: string
  relateNumber?: string
  category?: string
  // 明細
  lines: CreateInvoiceLine[]
}

/** 開立成功結果（對應後端 CreateInvoiceResultDto）。 */
export interface CreateInvoiceResult {
  keyId: string | null
  invoiceNo: string | null
  trackCode: string | null
  no: string | null
  printMark: string | null
  hasCarrier: boolean
  processType: number
}

/** 內容預覽品項列（對應後端 InvoicePreviewLineDto）。 */
export interface InvoicePreviewLine {
  seq: number
  itemNo: string | null
  description: string | null
  quantity: number | null
  unitPrice: number | null
  amount: number | null
  remark: string | null
}

/** 發票內容預覽（對應後端 InvoicePreviewDto）。 */
export interface InvoicePreview {
  invoiceNo: string | null
  invoiceDate: string | null
  randomNo: string | null
  sellerName: string | null
  sellerReceiptNo: string | null
  buyerName: string | null
  buyerReceiptNo: string | null
  buyerAddress: string | null
  buyerEmail: string | null
  isB2C: boolean
  taxTypeLabel: string | null
  salesAmount: number | null
  taxAmount: number | null
  totalAmount: number | null
  carrierType: string | null
  carrierNo: string | null
  agencyCode: string | null
  remark: string | null
  lines: InvoicePreviewLine[]
}

/** 開立人候選項目（對應後端 CreateInvoiceSellerOptionDto）。 */
export interface CreateInvoiceSellerOption {
  sellerKey: string | null
  receiptNo: string | null
  companyName: string | null
}

/** 相對營業人（買受人）候選 / 帶入資料（對應後端 CounterpartOptionDto）。 */
export interface CounterpartOption {
  receiptNo: string | null
  companyName: string | null
  address: string | null
  phone: string | null
  email: string | null
  customerId: string | null
}

/** 產品快速查詢候選項目（對應後端 ProductOptionDto）。 */
export interface ProductOption {
  productId: number
  productName: string | null
  salePrice: number
  remark: string | null
  barcode: string | null
  spec: string | null
  pieceUnit: string | null
}

/** 搜尋開立人候選清單。GET /api/CreateInvoice/Sellers。 */
export function searchSellers(keyword?: string): Promise<ApiResponse<CreateInvoiceSellerOption[]>> {
  return apiRequest<CreateInvoiceSellerOption[]>('/CreateInvoice/Sellers', {
    method: 'GET',
    params: keyword ? { keyword } : {},
  })
}

/** 搜尋相對營業人（買受人）。GET /api/CreateInvoice/Counterparts。 */
export function searchCounterparts(
  sellerKey: string,
  term: string,
): Promise<ApiResponse<CounterpartOption[]>> {
  return apiRequest<CounterpartOption[]>('/CreateInvoice/Counterparts', {
    method: 'GET',
    params: { sellerKey, term },
  })
}

/** 產品快速查詢。GET /api/CreateInvoice/Products。 */
export function searchProducts(
  sellerKey: string | undefined,
  productName: string,
): Promise<ApiResponse<ProductOption[]>> {
  return apiRequest<ProductOption[]>('/CreateInvoice/Products', {
    method: 'GET',
    params: { sellerKey: sellerKey ?? '', productName },
  })
}

/** 開立發票。POST /api/CreateInvoice/Commit。 */
export function commitInvoice(req: CreateInvoiceRequest): Promise<ApiResponse<CreateInvoiceResult>> {
  return apiRequest<CreateInvoiceResult>('/CreateInvoice/Commit', {
    method: 'POST',
    data: { ...req, forPreview: false },
  })
}

/** 內容預覽（同 Commit 端點，forPreview=true，不寫入）。POST /api/CreateInvoice/Commit。 */
export function previewInvoice(req: CreateInvoiceRequest): Promise<ApiResponse<InvoicePreview>> {
  return apiRequest<InvoicePreview>('/CreateInvoice/Commit', {
    method: 'POST',
    data: { ...req, forPreview: true },
  })
}
