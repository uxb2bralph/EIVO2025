/**
 * periodical-exchange-rate-api.ts — 期別匯率維護 API
 *
 * 對應後端 PeriodicalExchangeRateController（/api/PeriodicalExchangeRate），遷移自舊版 PeriodicalExchangeRateController。
 * 匯率以複合鍵（periodId + currencyId）識別；periodId = 年度*100 + 期別（期別 1~6 為雙月）。
 */
import { $api, apiRequest, apiDownloadBlob } from '@/services/api-service'
import type { ApiResponse, PagedResult } from '@/interfaces/api-response'

/** 期別匯率列表項目（對應後端 ExchangeRateDatatableDto） */
export interface ExchangeRateDatatable {
  /** 期別識別碼（= 年度*100 + 期別）；與 currencyId 共同組成複合鍵 */
  periodId: number
  /** 發票年度（西元年；顯示時以民國年 = year - 1911） */
  year: number
  /** 發票期別（1~6，對應雙月） */
  periodNo: number
  /** 幣別識別碼（CurrencyType.CurrencyID）；與 periodId 共同組成複合鍵 */
  currencyId: number
  /** 幣別代碼（如「USD」） */
  currency: string | null
  /** 幣別名稱 */
  currencyName: string | null
  /** 匯率 */
  exchangeRate: number
}

/** 新增 / 修改期別匯率（對應後端 ExchangeRateEditDto） */
export interface ExchangeRateEdit {
  /** 修改前的期別識別碼；新增時為 null */
  origPeriodId: number | null
  /** 修改前的幣別識別碼；新增時為 null */
  origCurrencyId: number | null
  /** 發票年度（西元年）；與 periodNo 共同組成 periodId */
  year: number | null
  /** 發票期別（1~6）；與 year 共同組成 periodId */
  periodNo: number | null
  /** 幣別代碼（AbbrevName；NTD 視同 TWD） */
  currency: string | null
  /** 匯率（須大於 0） */
  exchangeRate: number | null
}

/** 期別匯率查詢條件（對應後端 ExchangeRateQueryDto） */
export interface ExchangeRateQuery {
  /** 發票年度（西元年，必填） */
  year: number
  /** 發票期別（1~6；省略表示該年度全部期別） */
  periodNo?: number
  /** 幣別代碼（前綴比對；省略表示全部幣別） */
  currency?: string
  page?: number
  pageSize?: number
}

/** 匯入匯率 Excel 的結果：成功回傳結果檔 Blob，失敗回傳錯誤訊息 */
export interface ExchangeRateUploadResult {
  /** 處理結果 Excel（含「處理狀態」欄）；失敗時為 undefined */
  blob?: Blob
  /** 錯誤訊息；成功時為 undefined */
  error?: string
}

/**
 * 查詢期別匯率（分頁）。
 * 對應後端 GET /api/PeriodicalExchangeRate，遷移自舊版 PeriodicalExchangeRateController.Inquire。
 */
export function queryExchangeRates(
  query: ExchangeRateQuery,
): Promise<ApiResponse<PagedResult<ExchangeRateDatatable>>> {
  // 移除 undefined / null / 空字串，避免送出多餘 query 參數
  const params: Record<string, unknown> = {}
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null && value !== '') {
      params[key] = value
    }
  }

  return apiRequest<PagedResult<ExchangeRateDatatable>>('/PeriodicalExchangeRate', {
    method: 'GET',
    params,
  })
}

/**
 * 新增 / 修改期別匯率（對應後端 PeriodicalExchangeRate/CommitItem）。
 * origPeriodId / origCurrencyId 皆為 null 時新增；修改時帶入原複合鍵，
 * 若幣別 / 期別變更後端會將資料列搬移至新複合鍵。成功時回傳存檔後的列資料。
 */
export function commitExchangeRate(
  payload: ExchangeRateEdit,
): Promise<ApiResponse<ExchangeRateDatatable>> {
  return apiRequest<ExchangeRateDatatable>('/PeriodicalExchangeRate/CommitItem', {
    method: 'POST',
    data: payload,
  })
}

/**
 * 刪除期別匯率（對應後端 PeriodicalExchangeRate/DeleteItem）。
 * 以複合鍵（periodId + currencyId）刪除。
 */
export function deleteExchangeRate(
  periodId: number,
  currencyId: number,
): Promise<ApiResponse<void>> {
  return apiRequest<void>('/PeriodicalExchangeRate/DeleteItem', {
    method: 'POST',
    params: { periodId, currencyId },
  })
}

/**
 * 下載匯率資料範本（對應後端 PeriodicalExchangeRate/GetExchangeRateSample，遷移自舊版 GetExchangeRateSampleAsync）。
 * 回傳 Excel Blob；呼叫方負責觸發瀏覽器下載。可能拋出例外。
 */
export function downloadExchangeRateSample(): Promise<Blob> {
  return apiDownloadBlob('/PeriodicalExchangeRate/GetExchangeRateSample')
}

/**
 * 匯入匯率 Excel（對應後端 PeriodicalExchangeRate/UploadExchangeRate，遷移自舊版 UploadExchangeRate）。
 * 以 multipart/form-data 上傳單一 Excel 檔（須含「匯率」工作表），後端同步處理並回傳含「處理狀態」欄的結果檔。
 * 後端於錯誤時改回傳 JSON（非 Excel），此處以 content-type 判別並轉為錯誤訊息。
 */
export async function uploadExchangeRate(file: File): Promise<ExchangeRateUploadResult> {
  const formData = new FormData()
  formData.append('excelFile', file)

  try {
    const { data } = await $api('/PeriodicalExchangeRate/UploadExchangeRate', {
      method: 'POST',
      data: formData,
      responseType: 'blob',
    })
    return await interpretUploadResponse(data as Blob)
  } catch (error) {
    // axios 於 4xx / 5xx 會拋例外；responseType 為 blob 時，錯誤內容亦為 Blob。
    const responseData = (error as { response?: { data?: unknown } })?.response?.data
    if (responseData instanceof Blob) {
      return await interpretUploadResponse(responseData)
    }
    return { error: error instanceof Error ? error.message : '匯入失敗' }
  }
}

/** 判別上傳回應：Excel 檔（成功）或 JSON 錯誤訊息（失敗）。 */
async function interpretUploadResponse(blob: Blob): Promise<ExchangeRateUploadResult> {
  if (blob.type && blob.type.includes('application/json')) {
    try {
      const parsed = JSON.parse(await blob.text()) as Partial<ApiResponse>
      const detail = parsed.errors?.length ? parsed.errors.join('\n') : undefined
      return { error: detail || parsed.message || '匯入失敗' }
    } catch {
      return { error: '匯入失敗' }
    }
  }
  return { blob }
}
