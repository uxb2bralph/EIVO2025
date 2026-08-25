/**
 * invoice-summary-api.ts — 發票統計表 API
 *
 * 對應後端 InvoiceSummaryController（/api/InvoiceSummary），
 * 遷移自舊版 InvoiceQueryController.InvoiceSummary / InquireSummary / CreateMonthlyReportXlsx
 * （選單「發票統計表」）。查詢條件與「資料查詢／列印／匯出」相同，故沿用 InvoiceProcessQuery；
 * 開立人 / 代理業者候選清單亦直接重用 invoice-process-api 的 searchSellers / searchAgents。
 */
import { apiRequest, apiDownloadBlob } from '@/services/api-service'
import type { ApiResponse, PagedResult } from '@/interfaces/api-response'
import type { InvoiceProcessQuery } from '@/services/invoice-process-api'

/** 查詢條件（對應後端 InvoiceSummaryQueryDto；欄位同 InvoiceProcessQueryDto）。 */
export type InvoiceSummaryQuery = InvoiceProcessQuery

/** 統計結果列（對應後端 InvoiceSummaryRowDto） */
export interface InvoiceSummaryRow {
  sellerKey: string | null
  companyName: string | null
  receiptNo: string | null
  invoiceCount: number
  expirationDate: string | null
}

/** 移除空值，避免以空字串覆蓋後端可選條件 */
function toParams(query: InvoiceSummaryQuery): Record<string, unknown> {
  const params: Record<string, unknown> = {}
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null && value !== '') params[key] = value
  }
  return params
}

/** 依開立發票營業人彙總查詢（分頁）。對應後端 GET /api/InvoiceSummary。 */
export function queryInvoiceSummary(
  query: InvoiceSummaryQuery,
): Promise<ApiResponse<PagedResult<InvoiceSummaryRow>>> {
  return apiRequest<PagedResult<InvoiceSummaryRow>>('/InvoiceSummary', {
    method: 'GET',
    params: toParams(query),
  })
}

/** 下載開立發票月報表 Excel。對應後端 POST /api/InvoiceSummary/MonthlyReport。 */
export function exportMonthlyReport(query: InvoiceSummaryQuery): Promise<Blob> {
  return apiDownloadBlob('/InvoiceSummary/MonthlyReport', { method: 'POST', body: toParams(query) })
}
