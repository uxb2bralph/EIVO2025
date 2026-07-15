/**
 * business-relationship-api.ts — 相對營業人資料維護 API
 *
 * 對應後端 BusinessRelationshipController（/api/BusinessRelationship），
 * 遷移自舊版 WebHome BusinessRelationshipController（MaintainRelationship / InquireBusinessRelationship 等）。
 * 關係以複合鍵（masterId + relativeId + businessId）識別。
 */
import { $api, apiRequest, apiDownloadBlob } from '@/services/api-service'
import type { ApiResponse, PagedResult } from '@/interfaces/api-response'

/** 相對營業人列表項目（對應後端 BusinessRelationshipDatatableDto） */
export interface BusinessRelationshipDatatable {
  /** 主營業人 CompanyID（複合鍵之一，供列動作傳遞） */
  masterId: number
  /** 相對營業人 CompanyID（複合鍵之一，供列動作傳遞） */
  relativeId: number
  /** 營業人類別識別碼（1=銷項、2=進項；複合鍵之一） */
  businessId: number
  /** 加密後的相對營業人 CompanyID（供「管理使用者」導向 UserAccount 頁使用） */
  relativeKeyId: string | null
  /** 主營業人名稱 */
  masterName: string | null
  /** 相對營業人名稱 */
  companyName: string | null
  /** 相對營業人統一編號 */
  receiptNo: string | null
  /** 營業人類別名稱（如「銷項」） */
  businessTypeName: string | null
  /** 聯絡人電子郵件 */
  contactEmail: string | null
  /** 地址 */
  addr: string | null
  /** 電話 */
  phone: string | null
  /** 客戶代碼 */
  customerNo: string | null
  /** 關係狀態顯示文字（null CurrentLevel 時為「已啟用」） */
  statusText: string | null
  /** 是否已註記停用（供列選單顯示「啟用 / 停用」） */
  deactivated: boolean
  /** 是否設定自動接收 */
  entrusting: boolean | null
  /** 是否設定主動列印（null 表示未設定） */
  entrustToPrint: boolean | null
}

/** 集團成員（主營業人）下拉選項（對應後端 GroupMemberDto） */
export interface GroupMember {
  companyId: number
  receiptNo: string | null
  companyName: string | null
}

/** 相對營業人查詢條件（對應後端 BusinessRelationshipQueryDto） */
export interface BusinessRelationshipQuery {
  /** 集團成員（主營業人 CompanyID）；省略表示全部 */
  companyId?: number
  /** 相對營業人統一編號（精確比對） */
  receiptNo?: string
  /** 相對營業人名稱（模糊比對） */
  companyName?: string
  /** 營業人類別（1=銷項、2=進項）；省略表示全部 */
  businessType?: number
  page?: number
  pageSize?: number
}

/** 修改相對營業人（對應後端 BusinessRelationshipEditDto） */
export interface BusinessRelationshipEdit {
  masterId: number
  relativeId: number
  businessId: number
  companyName: string | null
  contactEmail: string | null
  addr: string | null
  phone: string | null
  customerNo: string | null
}

/** 新增相對營業人（對應後端 BusinessRelationshipAddDto） */
export interface BusinessRelationshipAdd {
  /** 主營業人（集團成員）CompanyID */
  masterCompanyId: number | null
  /** 相對營業人統一編號（必填） */
  receiptNo: string | null
  /** 相對營業人名稱（必填） */
  companyName: string | null
  /** 營業人類別（1=銷項、2=進項）；省略預設銷項 */
  businessType: number | null
  contactEmail: string | null
  addr: string | null
  phone: string | null
  customerNo: string | null
}

/** 匯入相對營業人 Excel 的結果：成功回傳結果檔 Blob，失敗回傳錯誤訊息 */
export interface CounterpartUploadResult {
  /** 處理結果 Excel（含「處理狀態」欄）；失敗時為 undefined */
  blob?: Blob
  /** 錯誤訊息；成功時為 undefined */
  error?: string
}

/**
 * 查詢相對營業人關係（分頁）。
 * 對應後端 GET /api/BusinessRelationship，遷移自舊版 InquireBusinessRelationship。
 */
export function queryBusinessRelationships(
  query: BusinessRelationshipQuery,
): Promise<ApiResponse<PagedResult<BusinessRelationshipDatatable>>> {
  const params: Record<string, unknown> = {}
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null && value !== '') {
      params[key] = value
    }
  }

  return apiRequest<PagedResult<BusinessRelationshipDatatable>>('/BusinessRelationship', {
    method: 'GET',
    params,
  })
}

/** 取得集團成員（主營業人）下拉選項（對應後端 BusinessRelationship/GroupMembers）。 */
export function queryGroupMembers(): Promise<ApiResponse<GroupMember[]>> {
  return apiRequest<GroupMember[]>('/BusinessRelationship/GroupMembers', { method: 'GET' })
}

/** 修改相對營業人（對應後端 BusinessRelationship/CommitItem）。 */
export function commitBusinessRelationship(
  payload: BusinessRelationshipEdit,
): Promise<ApiResponse<void>> {
  return apiRequest<void>('/BusinessRelationship/CommitItem', {
    method: 'POST',
    data: payload,
  })
}

/** 新增相對營業人（對應後端 BusinessRelationship/AddItem）。 */
export function addBusinessRelationship(
  payload: BusinessRelationshipAdd,
): Promise<ApiResponse<void>> {
  return apiRequest<void>('/BusinessRelationship/AddItem', {
    method: 'POST',
    data: payload,
  })
}

/** 刪除相對營業人關係（對應後端 BusinessRelationship/DeleteItem）。 */
export function deleteBusinessRelationship(
  businessId: number,
  masterId: number,
  relativeId: number,
): Promise<ApiResponse<void>> {
  return apiRequest<void>('/BusinessRelationship/DeleteItem', {
    method: 'POST',
    params: { businessId, masterId, relativeId },
  })
}

/** 啟用相對營業人關係（對應後端 BusinessRelationship/Activate）。 */
export function activateBusinessRelationship(
  businessId: number,
  masterId: number,
  relativeId: number,
): Promise<ApiResponse<void>> {
  return apiRequest<void>('/BusinessRelationship/Activate', {
    method: 'POST',
    params: { businessId, masterId, relativeId },
  })
}

/** 停用相對營業人關係（對應後端 BusinessRelationship/Deactivate）。 */
export function deactivateBusinessRelationship(
  businessId: number,
  masterId: number,
  relativeId: number,
): Promise<ApiResponse<void>> {
  return apiRequest<void>('/BusinessRelationship/Deactivate', {
    method: 'POST',
    params: { businessId, masterId, relativeId },
  })
}

/** 設定相對營業人自動接收（對應後端 BusinessRelationship/SetEntrusting）。 */
export function setEntrusting(
  businessId: number,
  masterId: number,
  relativeId: number,
  status: boolean,
): Promise<ApiResponse<void>> {
  return apiRequest<void>('/BusinessRelationship/SetEntrusting', {
    method: 'POST',
    params: { businessId, masterId, relativeId, status },
  })
}

/** 設定相對營業人主動列印（對應後端 BusinessRelationship/SetEntrustToPrint）。 */
export function setEntrustToPrint(
  businessId: number,
  masterId: number,
  relativeId: number,
  status: boolean,
): Promise<ApiResponse<void>> {
  return apiRequest<void>('/BusinessRelationship/SetEntrustToPrint', {
    method: 'POST',
    params: { businessId, masterId, relativeId, status },
  })
}

/**
 * 下載相對營業人資料範本（對應後端 BusinessRelationship/DownloadTemplate）。
 * 回傳 Excel Blob；呼叫方負責觸發瀏覽器下載。可能拋出例外。
 */
export function downloadCounterpartTemplate(): Promise<Blob> {
  return apiDownloadBlob('/BusinessRelationship/DownloadTemplate')
}

/**
 * 匯入相對營業人 Excel（對應後端 BusinessRelationship/UploadCounterpart，遷移自舊版 UploadCounterpartBusiness）。
 * 以 multipart/form-data 上傳單一 Excel 檔（須含「相對營業人」工作表），後端同步處理並回傳含「處理狀態」欄的結果檔。
 * 後端於錯誤時改回傳 JSON（非 Excel），此處以 content-type 判別並轉為錯誤訊息。
 */
export async function uploadCounterpart(file: File, businessType?: number): Promise<CounterpartUploadResult> {
  const formData = new FormData()
  formData.append('excelFile', file)
  if (businessType !== undefined && businessType !== null) {
    formData.append('businessType', String(businessType))
  }

  try {
    const { data } = await $api('/BusinessRelationship/UploadCounterpart', {
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
async function interpretUploadResponse(blob: Blob): Promise<CounterpartUploadResult> {
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
