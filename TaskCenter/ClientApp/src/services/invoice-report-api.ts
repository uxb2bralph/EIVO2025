/**
 * invoice-report-api.ts — 發票明細查詢 API
 *
 * 對應後端 InvoiceReportController（/api/InvoiceReport），
 * 遷移自舊版 InvoiceQueryController.InvoiceReport 頁面（選單「發票明細查詢」）之
 * Inquire / CreateXlsxAsync / DownloadCSV / DownloadAttachment / DownloadAll。
 *
 * 查詢條件與「資料查詢／列印／匯出」相同，故沿用 InvoiceProcessQuery；
 * 開立人 / 代理業者候選清單亦直接重用 invoice-process-api 的 searchSellers / searchAgents。
 */
import { apiRequest, apiDownloadBlob } from '@/services/api-service'
import type { ApiResponse, PagedResult } from '@/interfaces/api-response'
import type { InvoiceProcessQuery } from '@/services/invoice-process-api'

/** 查詢條件（對應後端 InvoiceReportQueryDto；欄位同 InvoiceProcessQueryDto）。 */
export type InvoiceReportQuery = InvoiceProcessQuery

/** 發票明細結果列（對應後端 InvoiceReportRowDto；欄位＝匯出 Excel / CSV 欄位）。 */
export interface InvoiceReportRow {
  keyId: string | null
  invoiceNo: string | null
  invoiceDate: string | null
  attachmentName: string | null
  attachmentCount: number
  customerId: string | null
  orderNo: string | null
  sellerName: string | null
  sellerReceiptNo: string | null
  salesAmount: number | null
  taxAmount: number | null
  totalAmount: number | null
  buyerName: string | null
  buyerReceiptNo: string | null
  contactName: string | null
  address: string | null
  email: string | null
  agencyCode: string | null
  winningLabel: string | null
  carrierType: string | null
  carrierNo: string | null
  isCancelled: boolean
}

/** 移除空值，避免以空字串覆蓋後端可選條件 */
function toParams(query: InvoiceReportQuery): Record<string, unknown> {
  const params: Record<string, unknown> = {}
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null && value !== '') params[key] = value
  }
  return params
}

/**
 * 下載失敗時後端回的是 JSON 錯誤，但 responseType 為 blob，故需自行解析出訊息。
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

/** 查詢發票明細（分頁）。對應後端 GET /api/InvoiceReport。 */
export function queryInvoiceReport(
  query: InvoiceReportQuery,
): Promise<ApiResponse<PagedResult<InvoiceReportRow>>> {
  return apiRequest<PagedResult<InvoiceReportRow>>('/InvoiceReport', {
    method: 'GET',
    params: toParams(query),
  })
}

/** 下載發票資料明細 Excel。對應後端 POST /api/InvoiceReport/ExportXlsx。 */
export async function exportReportXlsx(query: InvoiceReportQuery): Promise<Blob> {
  try {
    return await apiDownloadBlob('/InvoiceReport/ExportXlsx', { method: 'POST', body: toParams(query) })
  } catch (err) {
    throw new Error(await extractDownloadError(err, 'Excel 匯出失敗'))
  }
}

/** 下載發票資料明細 CSV。對應後端 POST /api/InvoiceReport/ExportCsv。 */
export async function exportReportCsv(query: InvoiceReportQuery): Promise<Blob> {
  try {
    return await apiDownloadBlob('/InvoiceReport/ExportCsv', { method: 'POST', body: toParams(query) })
  } catch (err) {
    throw new Error(await extractDownloadError(err, 'CSV 匯出失敗'))
  }
}

/**
 * 下載選取發票的附件壓縮檔。對應後端 POST /api/InvoiceReport/DownloadAttachments。
 * keyIds 為結果列的 keyId（加密後 InvoiceID）；下載範圍由後端依登入者角色過濾。
 */
export async function downloadSelectedAttachments(keyIds: string[]): Promise<Blob> {
  try {
    return await apiDownloadBlob('/InvoiceReport/DownloadAttachments', {
      method: 'POST',
      body: { keyIds },
    })
  } catch (err) {
    throw new Error(await extractDownloadError(err, '附件下載失敗'))
  }
}

/** 下載查詢結果全部發票的附件壓縮檔。對應後端 POST /api/InvoiceReport/DownloadAllAttachments。 */
export async function downloadAllAttachments(query: InvoiceReportQuery): Promise<Blob> {
  try {
    return await apiDownloadBlob('/InvoiceReport/DownloadAllAttachments', {
      method: 'POST',
      body: toParams(query),
    })
  } catch (err) {
    throw new Error(await extractDownloadError(err, '附件下載失敗'))
  }
}
