/**
 * invoice-number-apply-api.ts — 電子發票字軌號碼申請查詢 API
 *
 * 對應後端 InvoiceNumberApplyController（/api/InvoiceNumberApply），
 * 遷移自舊版 WebHome InvoiceNumberApplyController（QueryIndex / Query 頁 + 列動作 MoveFile / TransferOrganization / SetAll）。
 * 申請資料以檔案系統上的 JSON 檔保存；列以加密後的檔案路徑（keyId）識別。
 */
import { apiRequest, apiDownloadBlob } from '@/services/api-service'
import type { ApiResponse } from '@/interfaces/api-response'

/** 申請 JSON 檔列表項目（對應後端 InvoiceNumberApplyItemDto） */
export interface InvoiceNumberApplyItem {
  /** 統一編號（由檔名解析而得） */
  businessId: string
  /** 最新填表日（檔案最後寫入時間，ISO 字串） */
  applyUpdateTime: string
  /** 加密後的申請 JSON 檔路徑（供歸檔 / 轉營業人動作傳遞） */
  keyId: string
}

/**
 * 查詢申請 JSON 檔列表（對應後端 GET /api/InvoiceNumberApply，遷移自舊版 Query）。
 * @param businessId 統一編號（部分比對）；省略表示全部。
 */
export function queryInvoiceNumberApplies(
  businessId?: string,
): Promise<ApiResponse<InvoiceNumberApplyItem[]>> {
  const params: Record<string, unknown> = {}
  if (businessId && businessId.trim() !== '') {
    params['businessId'] = businessId.trim()
  }

  return apiRequest<InvoiceNumberApplyItem[]>('/InvoiceNumberApply', {
    method: 'GET',
    params,
  })
}

/** 歸檔：將申請 JSON 檔搬移至備份資料夾（對應後端 MoveFile）。 */
export function moveApplyFile(keyId: string): Promise<ApiResponse<void>> {
  return apiRequest<void>('/InvoiceNumberApply/MoveFile', {
    method: 'POST',
    data: { keyId },
  })
}

/** 轉營業人：將申請資料轉為營業人並提交（對應後端 TransferOrganization）。 */
export function transferOrganization(keyId: string): Promise<ApiResponse<void>> {
  return apiRequest<void>('/InvoiceNumberApply/TransferOrganization', {
    method: 'POST',
    data: { keyId },
  })
}

/**
 * 下載 Word：依統一編號取得打包後的 Word（.doc）zip（對應後端 DownloadWord，遷移自舊版 SetAll）。
 * 回傳 Blob；呼叫方負責觸發瀏覽器下載。可能拋出例外。
 */
export function downloadApplyWord(businessId: string): Promise<Blob> {
  return apiDownloadBlob(`/InvoiceNumberApply/DownloadWord?businessId=${encodeURIComponent(businessId)}`)
}
