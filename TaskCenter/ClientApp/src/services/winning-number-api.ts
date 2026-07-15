/**
 * winning-number-api.ts — 中獎號碼維護 API
 *
 * 對應後端 WinningNumberController（/api/WinningNumber），遷移自舊版 WinningNumberController。
 * 中獎號碼以原始 winningId 傳遞（沿用舊版 WinningID，為非敏感之流水鍵）。
 */
import { apiRequest, apiDownloadBlob } from '@/services/api-service'
import type { ApiResponse } from '@/interfaces/api-response'

/** 中獎號碼列表項目（對應後端 WinningNumberDatatableDto） */
export interface WinningNumberDatatable {
  /** 中獎號碼識別碼（UniformInvoiceWinningNumber.WinningID），供修改 / 刪除傳遞 */
  winningId: number
  /** 發票年度（西元年；顯示時以民國年 = year - 1911） */
  year: number
  /** 發票期別（1~6，對應雙月） */
  period: number
  /** 獎別代碼（Naming.WinningPrizeType） */
  rank: number
  /** 獎別名稱（如「頭獎」） */
  prizeType: string | null
  /** 中獎金額 */
  bonus: number | null
  /** 中獎號碼 */
  winningNo: string | null
  /** 是否可修改 / 刪除（二~六獎為頭獎自動衍生，不可直接維護） */
  editable: boolean
}

/** 新增 / 修改中獎號碼（對應後端 WinningNumberEditDto） */
export interface WinningNumberEdit {
  /** 中獎號碼識別碼；新增時為 null */
  winningId: number | null
  /** 發票年度（西元年）；新增時必填 */
  year: number | null
  /** 發票期別（1~6）；新增時必填 */
  period: number | null
  /** 獎別代碼（1 特別獎 / 2 特獎 / 3 頭獎 / 9 增開六獎） */
  rank: number | null
  /** 中獎號碼（特別獎 / 特獎 / 頭獎為 8 碼數字；增開六獎為 3 碼數字） */
  winningNo: string | null
}

/** 中獎號碼查詢條件（對應後端 WinningNumberQueryDto） */
export interface WinningNumberQuery {
  /** 發票年度（西元年，必填） */
  year: number
  /** 發票期別（1~6，必填） */
  periodNo: number
}

/** 對獎 / 清除作業條件（對應後端 WinningActionDto） */
export interface WinningActionPayload {
  /** 發票年度（西元年） */
  year: number
  /** 發票期別（1~6） */
  periodNo: number
}

/** 雲端發票中獎清冊上傳結果（對應後端 WinningNoUploadResultDto） */
export interface WinningNoUploadResult {
  /** 處理工作識別碼（proc.ProcessRequest.TaskID），供輪詢 / 下載使用 */
  taskId: number
  /** 結果檔下載時的建議檔名 */
  fileDownloadName: string | null
}

/** 雲端發票中獎清冊處理狀態（對應後端 WinningNoProcessStatusDto） */
export interface WinningNoProcessStatus {
  /** 是否已完成（結果檔已產生，可供下載） */
  completed: boolean
  /** 處理過程是否有例外（仍可下載含處理狀態之結果檔） */
  failed: boolean
  /** 例外訊息（無則為 null） */
  message: string | null
}

/**
 * 查詢中獎號碼（依年度 + 期別）。
 * 對應後端 GET /api/WinningNumber，遷移自舊版 WinningNumberController.Inquire。
 */
export function queryWinningNumbers(
  query: WinningNumberQuery,
): Promise<ApiResponse<WinningNumberDatatable[]>> {
  return apiRequest<WinningNumberDatatable[]>('/WinningNumber', {
    method: 'GET',
    params: { year: query.year, periodNo: query.periodNo },
  })
}

/**
 * 新增 / 修改中獎號碼（對應後端 WinningNumber/CommitItem）。
 * winningId 為 null 時新增，否則修改；成功時回傳儲存後的列資料。
 * 頭獎會由後端自動衍生二~六獎，故前端應於成功後重新查詢。
 */
export function commitWinningNumber(
  payload: WinningNumberEdit,
): Promise<ApiResponse<WinningNumberDatatable>> {
  return apiRequest<WinningNumberDatatable>('/WinningNumber/CommitItem', {
    method: 'POST',
    data: payload,
  })
}

/**
 * 刪除中獎號碼（對應後端 WinningNumber/DeleteItem）。
 * 刪除頭獎時後端會一併刪除其衍生的二~六獎。
 */
export function deleteWinningNumber(winningId: number): Promise<ApiResponse<void>> {
  return apiRequest<void>('/WinningNumber/DeleteItem', {
    method: 'POST',
    params: { id: winningId },
  })
}

/**
 * 執行發票對獎（對應後端 WinningNumber/MatchWinningInvoiceNo）。
 * 呼叫儲存程序建立中獎發票對應並發送中獎通知。
 */
export function matchWinningInvoiceNo(
  payload: WinningActionPayload,
): Promise<ApiResponse<void>> {
  return apiRequest<void>('/WinningNumber/MatchWinningInvoiceNo', {
    method: 'POST',
    data: payload,
  })
}

/**
 * 清除中獎發票（對應後端 WinningNumber/ClearWinningInvoiceNo）。
 * 刪除指定年度 / 期別已建立的中獎發票對應。
 */
export function clearWinningInvoiceNo(
  payload: WinningActionPayload,
): Promise<ApiResponse<void>> {
  return apiRequest<void>('/WinningNumber/ClearWinningInvoiceNo', {
    method: 'POST',
    data: payload,
  })
}

/**
 * 下載雲端發票中獎清冊範本（對應後端 WinningNumber/DownloadSample，遷移自舊版 GetSample?data=WinningNo）。
 * 回傳 Excel Blob；呼叫方負責觸發瀏覽器下載。可能拋出例外。
 */
export function downloadWinningSample(): Promise<Blob> {
  return apiDownloadBlob('/WinningNumber/DownloadSample')
}

/**
 * 上傳雲端發票中獎清冊 Excel（對應後端 WinningNumber/UploadWinningNo，遷移自舊版 UploadWinningNo）。
 * 以 multipart/form-data 傳送單一 Excel 檔；成功後回傳 taskId 供輪詢處理狀態。
 */
export function uploadWinningNo(file: File): Promise<ApiResponse<WinningNoUploadResult>> {
  const formData = new FormData()
  formData.append('excelFile', file)

  return apiRequest<WinningNoUploadResult>('/WinningNumber/UploadWinningNo', {
    method: 'POST',
    data: formData,
    headers: { 'Content-Type': 'multipart/form-data' },
  })
}

/**
 * 查詢雲端發票中獎清冊處理狀態（對應後端 WinningNumber/CheckProcess，遷移自舊版 CheckResource 輪詢）。
 */
export function checkWinningProcess(
  taskId: number,
): Promise<ApiResponse<WinningNoProcessStatus>> {
  return apiRequest<WinningNoProcessStatus>('/WinningNumber/CheckProcess', {
    method: 'GET',
    params: { taskId },
  })
}

/**
 * 下載雲端發票中獎清冊處理結果檔（對應後端 WinningNumber/DownloadResult，遷移自舊版 DownloadResource）。
 * 回傳 Excel Blob；呼叫方負責觸發瀏覽器下載。可能拋出例外。
 */
export function downloadWinningResult(taskId: number): Promise<Blob> {
  return apiDownloadBlob(`/WinningNumber/DownloadResult?taskId=${taskId}`)
}
