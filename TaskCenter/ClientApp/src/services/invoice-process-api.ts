/**
 * invoice-process-api.ts — 發票資料查詢／列印／匯出 API
 *
 * 對應後端 InvoiceProcessQueryController（/api/InvoiceProcessQuery），
 * 遷移自舊版 InvoiceProcessController.Index/Inquire（選單「資料查詢／列印／匯出」）。
 * 查詢依登入者角色範圍過濾；開立人 / 代理以加密後的 key 傳遞。
 */
import { apiRequest, apiDownloadBlob } from '@/services/api-service'
import type { ApiResponse, PagedResult } from '@/interfaces/api-response'

/** 查詢條件（對應後端 InvoiceProcessQueryDto） */
export interface InvoiceProcessQuery {
  sellerKey?: string
  agentKey?: string
  buyerReceiptNo?: string
  buyerName?: string
  customerId?: string
  dateFrom?: string
  dateTo?: string
  invoiceNo?: string
  endNo?: string
  dataNo?: string
  businessType?: number
  attachment?: number
  winning?: number
  cancelled?: boolean
  printMark?: string
  printed?: boolean
  hasAddr?: boolean
  carrierType?: string
  carrierNo?: string
  isNoticed?: boolean
  page?: number
  pageSize?: number
  sortName?: string
  sortType?: number
}

/** 發票查詢結果列（對應後端 InvoiceItemDatatableDto） */
export interface InvoiceItemDatatable {
  invoiceId: number
  keyId: string | null
  sellerName: string | null
  sellerReceiptNo: string | null
  invoiceNo: string | null
  transTypeLabel: string | null
  invoiceDate: string | null
  buyerName: string | null
  buyerReceiptNo: string | null
  orderNo: string | null
  statusLabel: string | null
  migStatus: string | null
  currency: string | null
  salesAmount: number | null
  taxTypeLabel: string | null
  taxAmount: number | null
  totalAmount: number | null
  remark: string | null
  printMark: string | null
  winningLabel: string | null
  carrierNo: string | null
  agencyCode: string | null
  customerId: string | null
  email: string | null
  issuingNoticeDate: string | null
  buyerAddress: string | null
  buyerContact: string | null
  isCancelled: boolean
  isWinning: boolean
}

/** 幣別統計（對應後端 CurrencySummaryDto） */
export interface CurrencySummary {
  currency: string | null
  count: number
  salesAmount: number
  taxAmount: number
  totalAmount: number
}

/** 發票明細品項列（對應後端 InvoiceDetailLineDto） */
export interface InvoiceDetailLine {
  seq: number
  description: string | null
  quantity: number | null
  unit: string | null
  unitPrice: number | null
  amount: number | null
  remark: string | null
}

/** 發票明細（對應後端 InvoiceDetailDto） */
export interface InvoiceDetail {
  invoiceNo: string | null
  invoiceDate: string | null
  randomNo: string | null
  sellerName: string | null
  sellerReceiptNo: string | null
  buyerName: string | null
  buyerReceiptNo: string | null
  buyerAddress: string | null
  buyerEmail: string | null
  currency: string | null
  taxTypeLabel: string | null
  salesAmount: number | null
  taxAmount: number | null
  totalAmount: number | null
  carrierType: string | null
  carrierNo: string | null
  agencyCode: string | null
  statusLabel: string | null
  remark: string | null
  lines: InvoiceDetailLine[]
}

/** 開立人候選項目 */
export interface InvoiceQuerySellerOption {
  sellerKey: string | null
  receiptNo: string | null
  companyName: string | null
}

/** 代理業者候選項目 */
export interface InvoiceQueryAgentOption {
  agentKey: string | null
  receiptNo: string | null
  companyName: string | null
}

/** 移除空值，避免以空字串覆蓋後端可選條件 */
function toParams(query: InvoiceProcessQuery): Record<string, unknown> {
  const params: Record<string, unknown> = {}
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null && value !== '') params[key] = value
  }
  return params
}

/** 查詢發票（分頁）。對應後端 GET /api/InvoiceProcessQuery。 */
export function queryInvoices(
  query: InvoiceProcessQuery,
): Promise<ApiResponse<PagedResult<InvoiceItemDatatable>>> {
  return apiRequest<PagedResult<InvoiceItemDatatable>>('/InvoiceProcessQuery', {
    method: 'GET',
    params: toParams(query),
  })
}

/** 幣別統計。對應後端 GET /api/InvoiceProcessQuery/Summary。 */
export function getSummary(
  query: InvoiceProcessQuery,
): Promise<ApiResponse<CurrencySummary[]>> {
  return apiRequest<CurrencySummary[]>('/InvoiceProcessQuery/Summary', {
    method: 'GET',
    params: toParams(query),
  })
}

/** 取得發票明細。對應後端 GET /api/InvoiceProcessQuery/Detail。 */
export function getInvoiceDetail(keyId: string): Promise<ApiResponse<InvoiceDetail>> {
  return apiRequest<InvoiceDetail>('/InvoiceProcessQuery/Detail', {
    method: 'GET',
    params: { keyId },
  })
}

/** 搜尋開立人候選清單。對應後端 GET /api/InvoiceProcessQuery/Sellers。 */
export function searchSellers(
  keyword?: string,
): Promise<ApiResponse<InvoiceQuerySellerOption[]>> {
  return apiRequest<InvoiceQuerySellerOption[]>('/InvoiceProcessQuery/Sellers', {
    method: 'GET',
    params: keyword ? { keyword } : {},
  })
}

/** 搜尋代理業者候選清單。對應後端 GET /api/InvoiceProcessQuery/Agents。 */
export function searchAgents(
  keyword?: string,
): Promise<ApiResponse<InvoiceQueryAgentOption[]>> {
  return apiRequest<InvoiceQueryAgentOption[]>('/InvoiceProcessQuery/Agents', {
    method: 'GET',
    params: keyword ? { keyword } : {},
  })
}

/** 下載發票資料明細 Excel。對應後端 POST /api/InvoiceProcessQuery/ExportXlsx。 */
export function exportXlsx(query: InvoiceProcessQuery): Promise<Blob> {
  return apiDownloadBlob('/InvoiceProcessQuery/ExportXlsx', { method: 'POST', body: toParams(query) })
}

/** 下載買受人資料 Excel。對應後端 POST /api/InvoiceProcessQuery/ExportBuyer。 */
export function exportBuyer(query: InvoiceProcessQuery): Promise<Blob> {
  return apiDownloadBlob('/InvoiceProcessQuery/ExportBuyer', { method: 'POST', body: toParams(query) })
}

/** 下載 ERP 匯出檔（POSINV.dat）。對應後端 POST /api/InvoiceProcessQuery/ExportErp。 */
export function exportErp(query: InvoiceProcessQuery): Promise<Blob> {
  return apiDownloadBlob('/InvoiceProcessQuery/ExportErp', { method: 'POST', body: toParams(query) })
}
