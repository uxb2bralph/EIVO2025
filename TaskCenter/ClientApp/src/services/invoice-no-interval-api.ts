/**
 * invoice-no-interval-api.ts — 電子發票配號區間維護 API
 *
 * 對應後端 InvoiceNoIntervalController（/api/InvoiceNoInterval），
 * 遷移自舊版 InvoiceNoController.MaintainInvoiceNoInterval 之核心動作。
 * 第一階段：查詢清單 / 新增 / 修改 / 刪除 / 鎖定，及開立人、字軌選擇器。
 * 開立人以加密後的 sellerKey 傳遞（沿用 OrganizationQuery 做法）。
 */
import { apiRequest, apiDownloadBlob } from '@/services/api-service'
import type { ApiResponse, PagedResult } from '@/interfaces/api-response'

/** 配號區間列表項目（對應後端 InvoiceNoIntervalDatatableDto） */
export interface InvoiceNoIntervalDatatable {
  intervalId: number
  /** 開立人統一編號 */
  receiptNo: string | null
  /** 發票年度（西元年；顯示以民國年 = year - 1911） */
  year: number
  /** 發票期別（1~6，對應雙月） */
  periodNo: number
  /** 字軌（二位英文字母） */
  trackCode: string | null
  /** 發票類別（7 一般 / 8 特種） */
  invoiceType: number | null
  /** 發票號碼起（8 位） */
  startNo: number
  /** 發票號碼迄（8 位） */
  endNo: number
  /** 指定 POS 機號碼 */
  deviceName: string | null
  /** 配號總數（endNo - startNo + 1） */
  totalCount: number
  /** 配號本數（totalCount / 50） */
  bookletCount: number
  /** 目前給號 */
  currentNo: number
  /** 剩餘可配號 */
  remaining: number
  /** 是否已鎖定 */
  locked: boolean
  /** 是否可編輯 / 刪除（尚無號碼被配發或指派） */
  editable: boolean
  /** 開立人是否為主機構（有下轄分支機構） */
  isMaster: boolean
  /** 是否已設定涵蓋本區間之主機構配號 */
  hasMainAssignment: boolean
}

/** 查詢條件（對應後端 InvoiceNoIntervalQueryDto） */
export interface InvoiceNoIntervalQuery {
  /** 開立人（加密後的 CompanyID，必填） */
  sellerKey: string
  /** 發票年度（西元年，必填） */
  year: number
  /** 發票期別（1~6；省略表示全部） */
  periodNo?: number
  /** 是否含分支機構 */
  branchRelation?: boolean
  page?: number
  pageSize?: number
}

/** 新增 / 修改配號區間（對應後端 InvoiceNoIntervalEditDto） */
export interface InvoiceNoIntervalEdit {
  /** 配號區間識別碼；新增時為 null */
  intervalId: number | null
  /** 字軌識別碼；新增時必填 */
  trackId: number | null
  /** 開立人（加密後的 CompanyID）；新增時必填 */
  sellerKey: string | null
  /** 發票號碼起（8 位整數） */
  startNo: number | null
  /** 發票號碼迄（8 位整數；迄 > 起，差距為 50 倍數） */
  endNo: number | null
  /** 指定 POS 機號碼；空白表示清除 */
  deviceName: string | null
}

/** 開立人候選項目（對應後端 InvoiceNoSellerOptionDto） */
export interface InvoiceNoSellerOption {
  sellerKey: string
  receiptNo: string | null
  companyName: string | null
}

/** 字軌候選項目（對應後端 InvoiceTrackCodeOptionDto） */
export interface InvoiceTrackCodeOption {
  trackId: number
  trackCode: string | null
  invoiceType: number | null
}

/** 查詢配號區間（分頁）。對應後端 GET /api/InvoiceNoInterval。 */
export function queryIntervals(
  query: InvoiceNoIntervalQuery,
): Promise<ApiResponse<PagedResult<InvoiceNoIntervalDatatable>>> {
  const params: Record<string, unknown> = {}
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null && value !== '') params[key] = value
  }
  return apiRequest<PagedResult<InvoiceNoIntervalDatatable>>('/InvoiceNoInterval', {
    method: 'GET',
    params,
  })
}

/** 搜尋開立人候選清單。對應後端 GET /api/InvoiceNoInterval/Sellers。 */
export function searchSellers(
  keyword?: string,
): Promise<ApiResponse<InvoiceNoSellerOption[]>> {
  return apiRequest<InvoiceNoSellerOption[]>('/InvoiceNoInterval/Sellers', {
    method: 'GET',
    params: keyword ? { keyword } : {},
  })
}

/** 取得字軌候選清單（依年度 + 期別）。對應後端 GET /api/InvoiceNoInterval/TrackCodes。 */
export function getTrackCodeOptions(
  year: number,
  periodNo?: number,
): Promise<ApiResponse<InvoiceTrackCodeOption[]>> {
  const params: Record<string, unknown> = { year }
  if (periodNo !== undefined && periodNo !== null) params.periodNo = periodNo
  return apiRequest<InvoiceTrackCodeOption[]>('/InvoiceNoInterval/TrackCodes', {
    method: 'GET',
    params,
  })
}

/** 新增 / 修改配號區間。對應後端 POST /api/InvoiceNoInterval/CommitItem。 */
export function commitInterval(payload: InvoiceNoIntervalEdit): Promise<ApiResponse<void>> {
  return apiRequest<void>('/InvoiceNoInterval/CommitItem', {
    method: 'POST',
    data: payload,
  })
}

/** 刪除配號區間。對應後端 POST /api/InvoiceNoInterval/DeleteItem。 */
export function deleteInterval(intervalId: number): Promise<ApiResponse<void>> {
  return apiRequest<void>('/InvoiceNoInterval/DeleteItem', {
    method: 'POST',
    params: { id: intervalId },
  })
}

/** 鎖定 / 解除鎖定配號區間。對應後端 POST /api/InvoiceNoInterval/LockItem。 */
export function lockInterval(intervalId: number, locked: boolean): Promise<ApiResponse<void>> {
  return apiRequest<void>('/InvoiceNoInterval/LockItem', {
    method: 'POST',
    data: { intervalId, locked },
  })
}

/** 分割配號區間（切出尾段為新區間）。對應後端 POST /api/InvoiceNoInterval/SplitItem。 */
export function splitInterval(intervalId: number): Promise<ApiResponse<void>> {
  return apiRequest<void>('/InvoiceNoInterval/SplitItem', {
    method: 'POST',
    params: { id: intervalId },
  })
}

/** 本組數均分（每份 parts*50 個號碼）。對應後端 POST /api/InvoiceNoInterval/AllotItem。 */
export function allotInterval(intervalId: number, parts: number): Promise<ApiResponse<void>> {
  return apiRequest<void>('/InvoiceNoInterval/AllotItem', {
    method: 'POST',
    data: { intervalId, parts },
  })
}

/** 主機構配號（登錄涵蓋本區間之主機構配號）。對應後端 POST /api/InvoiceNoInterval/ApplyHeadquarter。 */
export function applyHeadquarter(intervalId: number): Promise<ApiResponse<void>> {
  return apiRequest<void>('/InvoiceNoInterval/ApplyHeadquarter', {
    method: 'POST',
    params: { id: intervalId },
  })
}

/** 指派分支機構（將本區間改配給指定分支機構開立人）。對應後端 POST /api/InvoiceNoInterval/CommitBranch。 */
export function commitBranch(intervalId: number, sellerKey: string): Promise<ApiResponse<void>> {
  return apiRequest<void>('/InvoiceNoInterval/CommitBranch', {
    method: 'POST',
    data: { intervalId, sellerKey },
  })
}

/** 取得可用配號存量警戒值。對應後端 GET /api/InvoiceNoInterval/SafetyStock。 */
export function getSafetyStock(sellerKey: string): Promise<ApiResponse<{ safetyStock: number | null }>> {
  return apiRequest<{ safetyStock: number | null }>('/InvoiceNoInterval/SafetyStock', {
    method: 'GET',
    params: { sellerKey },
  })
}

/** 設定可用配號存量警戒值（null 清除）。對應後端 POST /api/InvoiceNoInterval/SafetyStock。 */
export function setSafetyStock(sellerKey: string, safetyStock: number | null): Promise<ApiResponse<void>> {
  return apiRequest<void>('/InvoiceNoInterval/SafetyStock', {
    method: 'POST',
    data: { sellerKey, safetyStock },
  })
}

/**
 * 下載 E0401 分支機構配號 XML zip。對應後端 GET /api/InvoiceNoInterval/DownloadE0401。
 * 以 Blob 回傳（可能拋出例外，呼叫方需自行處理）。
 */
export function downloadE0401(sellerKey: string, year: number, periodNo: number): Promise<Blob> {
  const qs = new URLSearchParams({
    sellerKey,
    year: String(year),
    periodNo: String(periodNo),
  }).toString()
  return apiDownloadBlob(`/InvoiceNoInterval/DownloadE0401?${qs}`)
}
