/**
 * invoice-mig-api.ts — MIG 檔案下載 API
 *
 * 對應後端 InvoiceProcessQueryController.DownloadMig（POST /api/InvoiceProcessQuery/DownloadMig），
 * 遷移自舊版 InvoiceProcessController 的 InquireToMIG 頁面下載按鈕
 * （DownloadF0401 / DownloadF0701 / DownloadF0501 + zipItems）。
 *
 * 查詢條件與結果列沿用 invoice-process-api.ts（同一組查詢 API）；此處僅負責下載。
 */
import { apiDownloadBlob } from '@/services/api-service'

/** 可下載的 MIG 格式（對應後端 InvoiceProcessQueryService.MigDocTypes） */
export type MigDocType = 'F0401' | 'F0701' | 'F0501'

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

/**
 * 下載選取發票的 MIG XML 壓縮檔（回傳 zip Blob）。
 * keyIds 為結果列的 keyId（加密後 InvoiceID）；下載範圍由後端依登入者角色過濾。
 */
export async function downloadMig(docType: MigDocType, keyIds: string[]): Promise<Blob> {
  try {
    return await apiDownloadBlob('/InvoiceProcessQuery/DownloadMig', {
      method: 'POST',
      body: { docType, keyIds },
    })
  } catch (err) {
    throw new Error(await extractDownloadError(err, `${docType} 下載失敗`))
  }
}
