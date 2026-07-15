/**
 * track-code-api.ts — 電子發票字軌維護 API
 *
 * 對應後端 TrackCodeController（/api/TrackCode），遷移自舊版 TrackCodeController。
 * 由系統管理維護入口進入；字軌以原始 trackId 傳遞（沿用舊版做法）。
 */
import { apiRequest } from '@/services/api-service'
import type { ApiResponse, PagedResult } from '@/interfaces/api-response'

/** 電子發票字軌列表項目（對應後端 TrackCodeDatatableDto） */
export interface TrackCodeDatatable {
  /** 字軌識別碼（InvoiceTrackCode.TrackID），供修改 / 刪除傳遞 */
  trackId: number
  /** 發票年度（西元年；顯示時以民國年 = year - 1911） */
  year: number
  /** 發票期別（1~6，對應雙月） */
  periodNo: number
  /** 字軌（二位英文字母） */
  trackCode: string | null
  /** 發票類別（Naming.InvoiceTypeDefinition：7 一般 / 8 特種） */
  invoiceType: number | null
}

/** 新增 / 修改字軌資料（對應後端 TrackCodeEditDto） */
export interface TrackCodeEdit {
  /** 字軌識別碼；新增時為 null */
  trackId: number | null
  /** 發票年度（西元年）；新增時必填 */
  year: number | null
  /** 發票期別（1~6）；新增時必填 */
  periodNo: number | null
  /** 字軌（二位英文字母） */
  trackCode: string | null
  /** 發票類別（7 一般 / 8 特種）；未指定時後端預設一般 */
  invoiceType: number | null
}

/** 字軌查詢條件（對應後端 TrackCodeQueryDto） */
export interface TrackCodeQuery {
  /** 發票年度（西元年，必填） */
  year: number
  /** 發票期別（1~6；省略表示全部） */
  periodNo?: number
  page?: number
  pageSize?: number
}

/**
 * 查詢電子發票字軌（分頁）。
 * 對應後端 GET /api/TrackCode，遷移自舊版 TrackCodeController.Inquire。
 */
export function queryTrackCodes(
  query: TrackCodeQuery,
): Promise<ApiResponse<PagedResult<TrackCodeDatatable>>> {
  // 移除 undefined / 空字串，避免送出多餘 query 參數
  const params: Record<string, unknown> = {}
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null && value !== '') {
      params[key] = value
    }
  }

  return apiRequest<PagedResult<TrackCodeDatatable>>('/TrackCode', {
    method: 'GET',
    params,
  })
}

/**
 * 新增 / 修改字軌（對應後端 TrackCode/CommitItem，遷移自舊版 TrackCode/CommitItem）。
 * trackId 為 null 時新增，否則修改；成功時回傳儲存後的列資料。
 */
export function commitTrackCode(
  payload: TrackCodeEdit,
): Promise<ApiResponse<TrackCodeDatatable>> {
  return apiRequest<TrackCodeDatatable>('/TrackCode/CommitItem', {
    method: 'POST',
    data: payload,
  })
}

/**
 * 刪除字軌（對應後端 TrackCode/DeleteItem，遷移自舊版 TrackCode/DeleteItem）。
 * 沿用舊版以原始 trackId 傳遞。
 */
export function deleteTrackCode(trackId: number): Promise<ApiResponse<void>> {
  return apiRequest<void>('/TrackCode/DeleteItem', {
    method: 'POST',
    params: { id: trackId },
  })
}
