/**
 * winning-invoice-report-api.ts — 中獎統計表 API
 *
 * 對應後端 WinningInvoiceReportController（/api/WinningInvoiceReport），
 * 遷移自舊版 WinningInvoiceController.ReportIndex / InquireReport / CreateXlsx
 * （選單「中獎統計表」/WinningInvoice/ReportIndex）。
 * 查詢條件沿用發票查詢管線（InvoiceProcessQuery），結果僅計中獎發票並依開立人彙總；
 * 開立人 / 代理業者候選清單直接重用 invoice-process-api 的 searchSellers / searchAgents。
 */
import { apiRequest, apiDownloadBlob } from '@/services/api-service'
import type { ApiResponse, PagedResult } from '@/interfaces/api-response'
import type { InvoiceProcessQuery } from '@/services/invoice-process-api'

/** 查詢條件（對應後端 WinningInvoiceReportQueryDto；欄位同 InvoiceProcessQueryDto）。 */
export type WinningInvoiceReportQuery = InvoiceProcessQuery

/** 中獎統計結果列（對應後端 WinningInvoiceReportRowDto） */
export interface WinningInvoiceReportRow {
  sellerKey: string | null
  sellerReceiptNo: string | null
  sellerName: string | null
  addr: string | null
  winningCount: number
  donationCount: number
}

/** 移除空值，避免以空字串覆蓋後端可選條件 */
function toParams(query: WinningInvoiceReportQuery): Record<string, unknown> {
  const params: Record<string, unknown> = {}
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null && value !== '') params[key] = value
  }
  return params
}

/** 依開立人彙總中獎 / 捐贈張數（分頁）。對應後端 GET /api/WinningInvoiceReport。 */
export function queryWinningInvoiceReport(
  query: WinningInvoiceReportQuery,
): Promise<ApiResponse<PagedResult<WinningInvoiceReportRow>>> {
  return apiRequest<PagedResult<WinningInvoiceReportRow>>('/WinningInvoiceReport', {
    method: 'GET',
    params: toParams(query),
  })
}

/**
 * 匯出失敗時後端回的是 JSON 錯誤，但 responseType 為 blob，故需自行解析出訊息。
 * 解析不出訊息時退回預設文字。
 */
async function extractDownloadError(err: unknown, fallback: string): Promise<string> {
  const data = (err as { response?: { data?: unknown } })?.response?.data
  if (data instanceof Blob) {
    try {
      const json = JSON.parse(await data.text()) as { message?: string; errors?: string[] }
      if (json.errors?.length) return json.errors.join('\n')
      if (json.message) return json.message
    } catch {
      // 非 JSON（可能真的是檔案內容）→ 用預設訊息
    }
  }
  if (err instanceof Error && err.message) return err.message
  return fallback
}

/** 下載中獎統計表 Excel。對應後端 POST /api/WinningInvoiceReport/Export。 */
export async function exportWinningInvoiceReport(query: WinningInvoiceReportQuery): Promise<Blob> {
  try {
    return await apiDownloadBlob('/WinningInvoiceReport/Export', {
      method: 'POST',
      body: toParams(query),
    })
  } catch (err) {
    throw new Error(await extractDownloadError(err, '匯出失敗'))
  }
}
