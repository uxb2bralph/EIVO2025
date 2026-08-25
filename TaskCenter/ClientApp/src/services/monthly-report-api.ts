/**
 * monthly-report-api.ts — 發票月報表 API
 *
 * 對應後端 MonthlyReportController（POST /api/MonthlyReport/Export），
 * 遷移自舊版 InvoiceQueryController.MonthlyReport / InquireMonthlyReport
 * （選單「下載發票月報表」）。
 *
 * 舊版是「送出 → 建 ProcessRequest → 背景產檔 → 輪詢下載」；此處後端同步產檔，
 * 直接以 Blob 回傳 Excel。開立人 / 代理業者候選清單重用 invoice-process-api 的
 * searchSellers / searchAgents，不另開端點。
 */
import { apiDownloadBlob } from '@/services/api-service'

/** 查詢條件（對應後端 MonthlyReportQueryDto） */
export interface MonthlyReportQuery {
  /** 開立人（加密後 CompanyID）；與 agentKey 至少擇一 */
  sellerKey?: string
  /** 代理業者（加密後 CompanyID）；與 sellerKey 至少擇一 */
  agentKey?: string
  /** 統計起日（yyyy-MM-dd） */
  dateFrom?: string
  /** 統計迄日（yyyy-MM-dd） */
  dateTo?: string
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

/** 製作並下載發票月報表 Excel。對應後端 POST /api/MonthlyReport/Export。 */
export async function exportMonthlyReport(query: MonthlyReportQuery): Promise<Blob> {
  try {
    return await apiDownloadBlob('/MonthlyReport/Export', { method: 'POST', body: query })
  } catch (err) {
    throw new Error(await extractDownloadError(err, '月報表製作失敗'))
  }
}
