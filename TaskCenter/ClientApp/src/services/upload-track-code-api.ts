/**
 * upload-track-code-api.ts — 上傳發票字軌號碼 API
 *
 * 對應後端 InvoiceNoIntervalController 之 UploadTrackCodeSample / UploadTrackCodeToPreview /
 * CommitUploadTrackCode（/api/InvoiceNoInterval），遷移自舊版 WebHome
 * Views/InvoiceNo/UploadInvoiceTrackCode.cshtml 與 InvoiceNoController 之
 * GetUploadInvoiceTrackCodeSample / UploadToPreview / CommitUpload。
 *
 * 匯入內容為「配號區間」（InvoiceNoInterval）：先上傳 Excel 取得逐列驗證後的預覽，
 * 通過者以加密後的 rowKey 回送匯入（沿用舊版 KeyItems 做法，不信任前端輸入）。
 */
import { apiRequest, apiDownloadBlob } from '@/services/api-service'
import type { ApiResponse } from '@/interfaces/api-response'

/** 上傳發票字軌號碼之預覽 / 匯入結果列（對應後端 UploadTrackCodeRowDto） */
export interface UploadTrackCodeRow {
  /** 加密後的匯入內容（沿用舊版 KeyItems）；驗證失敗或已匯入時為 null */
  rowKey: string | null
  /** 營業人統一編號 */
  receiptNo: string | null
  /** 營業人名稱 */
  companyName: string | null
  /** 營業人註記停用日期（yyyy/MM/dd）；未註記停用為 null */
  expirationDate: string | null
  /** 發票年度（民國年，即上傳檔案內容） */
  year: number | null
  /** 發票期別（1~6，對應雙月） */
  periodNo: number | null
  /** 字軌（二位英文字母） */
  trackCode: string | null
  /** 發票號碼起（8 位） */
  startNo: number | null
  /** 發票號碼迄（8 位） */
  endNo: number | null
  /** 驗證 / 匯入訊息；預覽時為 null 表示可匯入 */
  message: string | null
  /** 是否已成功匯入 */
  committed: boolean
}

/**
 * 下載上傳發票字軌號碼範本（Excel）。
 * 對應後端 GET /api/InvoiceNoInterval/UploadTrackCodeSample；以 Blob 回傳（可能拋出例外）。
 */
export function downloadUploadTrackCodeSample(): Promise<Blob> {
  return apiDownloadBlob('/InvoiceNoInterval/UploadTrackCodeSample')
}

/**
 * 上傳發票字軌號碼 Excel 並取得預覽（含逐列驗證結果）。
 * 對應後端 POST /api/InvoiceNoInterval/UploadTrackCodeToPreview。
 */
export function uploadTrackCodeToPreview(file: File): Promise<ApiResponse<UploadTrackCodeRow[]>> {
  const formData = new FormData()
  formData.append('excelFile', file)

  return apiRequest<UploadTrackCodeRow[]>('/InvoiceNoInterval/UploadTrackCodeToPreview', {
    method: 'POST',
    data: formData,
    headers: { 'Content-Type': 'multipart/form-data' },
  })
}

/**
 * 匯入預覽通過之發票字軌號碼（以預覽回傳之 rowKey 傳遞）。
 * 對應後端 POST /api/InvoiceNoInterval/CommitUploadTrackCode。
 */
export function commitUploadedTrackCodes(
  rowKeys: string[],
): Promise<ApiResponse<UploadTrackCodeRow[]>> {
  return apiRequest<UploadTrackCodeRow[]>('/InvoiceNoInterval/CommitUploadTrackCode', {
    method: 'POST',
    data: { rowKeys },
  })
}
