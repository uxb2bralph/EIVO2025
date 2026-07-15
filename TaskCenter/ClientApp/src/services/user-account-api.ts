/**
 * user-account-api.ts — 使用者帳號管理 API
 *
 * 對應後端 UserAccountController（/api/UserAccount），遷移自舊版 AccountController。
 * 由營業人資料管理頁「管理使用者」進入，以加密 OrgKeyId 限定營業人；
 * 各列動作以加密的使用者 keyId（UID）傳遞。
 */
import { apiRequest } from '@/services/api-service'
import type { ApiResponse, PagedResult } from '@/interfaces/api-response'

/** 使用者帳號列表項目（對應後端 UserAccountDatatableDto） */
export interface UserAccountDatatable {
  /** 加密後的 UID（沿用舊版 KeyID 做法），供啟用 / 停用 / 刪除等動作傳遞 */
  keyId: string | null
  companyName: string | null
  roleId: number | null
  roleName: string | null
  userName: string | null
  pid: string | null
  email: string | null
  /** 會員狀態（UserProfile.LevelID）；1101 註記停用時列選單改顯示「啟用」 */
  levelId: number | null
}

/** 使用者帳號編輯 / 新增資料（對應後端 UserAccountEditDto） */
export interface UserAccountEdit {
  /** 加密後的 UID；新增帳號時為 null */
  keyId: string | null
  /** 加密後的所屬營業人 CompanyID */
  orgKeyId: string | null
  pid: string | null
  userName: string | null
  email: string | null
  address: string | null
  phone: string | null
  mobilePhone: string | null
  phone2: string | null
  roleId: number | null
  /** 密碼（留空表示不修改原密碼；新增時必填）。僅用於送出 */
  password?: string | null
  /** 確認密碼。僅用於送出 */
  password1?: string | null
  /** 所屬營業人名稱（唯讀，載入時回傳） */
  companyName?: string | null
}

/** 使用者帳號查詢條件（對應後端 UserAccountQueryDto） */
export interface UserAccountQuery {
  /** 加密後的營業人 CompanyID（來自營業人列表 keyId 欄位） */
  orgKeyId: string
  pid?: string
  userName?: string
  roleId?: number
  levelId?: number
  page?: number
  pageSize?: number
}

/**
 * 查詢指定營業人的使用者帳號（分頁）。
 * 對應後端 GET /api/UserAccount，遷移自舊版 AccountController.AccountIndex + Inquire。
 */
export function queryUserAccounts(
  query: UserAccountQuery,
): Promise<ApiResponse<PagedResult<UserAccountDatatable>>> {
  // 移除 undefined / 空字串，避免送出多餘 query 參數
  const params: Record<string, unknown> = {}
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null && value !== '') {
      params[key] = value
    }
  }

  return apiRequest<PagedResult<UserAccountDatatable>>('/UserAccount', {
    method: 'GET',
    params,
  })
}

/**
 * 載入單一使用者帳號編輯資料（對應後端 UserAccount/EditItem，遷移自舊版 UserProfile/EditItem）。
 * 沿用舊版以加密 keyId 傳遞 UID。
 */
export function getUserAccountForEdit(keyId: string): Promise<ApiResponse<UserAccountEdit>> {
  return apiRequest<UserAccountEdit>('/UserAccount/EditItem', { method: 'GET', params: { keyId } })
}

/**
 * 新增 / 修改使用者帳號（對應後端 UserAccount/Commit，遷移自舊版 UserProfile/Commit）。
 * keyId 為 null 時新增；所屬營業人以 orgKeyId 傳遞。
 */
export function commitUserAccount(payload: UserAccountEdit): Promise<ApiResponse<void>> {
  return apiRequest<void>('/UserAccount/Commit', { method: 'POST', data: payload })
}

/**
 * 啟用帳號（對應後端 UserAccount/Activate，遷移自舊版 Account/Activate）。
 * 沿用舊版以加密 keyId 傳遞 UID。
 */
export function activateUserAccount(keyId: string): Promise<ApiResponse<void>> {
  return apiRequest<void>('/UserAccount/Activate', { method: 'POST', params: { keyId } })
}

/**
 * 停用帳號（對應後端 UserAccount/Deactivate，遷移自舊版 Account/Deactivate）。
 * 沿用舊版以加密 keyId 傳遞 UID。
 */
export function deactivateUserAccount(keyId: string): Promise<ApiResponse<void>> {
  return apiRequest<void>('/UserAccount/Deactivate', { method: 'POST', params: { keyId } })
}

/**
 * 重送確認信（對應後端 UserAccount/SendConfirmation，遷移自舊版 Account/SendConfirmation）。
 * 沿用舊版以加密 keyId 傳遞 UID。
 */
export function sendUserConfirmation(keyId: string): Promise<ApiResponse<void>> {
  return apiRequest<void>('/UserAccount/SendConfirmation', { method: 'POST', params: { keyId } })
}

/**
 * 刪除帳號（對應後端 UserAccount/DeleteItem，遷移自舊版 Account/DeleteItem）。
 * 沿用舊版以加密 keyId 傳遞 UID。
 */
export function deleteUserAccount(keyId: string): Promise<ApiResponse<void>> {
  return apiRequest<void>('/UserAccount/DeleteItem', { method: 'POST', params: { keyId } })
}
