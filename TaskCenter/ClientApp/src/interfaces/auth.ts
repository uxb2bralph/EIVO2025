/**
 * auth.ts — 登入 / 認證相關型別定義
 *
 * 對應後端 TaskCenter.Core.DTOs.LoginDto / LoginResultDto。
 */

/** 登入請求 body（對應後端 LoginDto，後端 model binding 不分大小寫） */
export interface LoginRequest {
  id: string
  password: string
  rememberMe?: boolean
}

/**
 * 登入結果（對應後端 LoginResultDto，envelope 與 token 欄位皆為 camelCase）。
 *
 * 注意：`user` 內層欄位目前由後端以 PascalCase 輸出（UserProfileDto 未個別標註），
 * 因此型別保留 optional 兩種大小寫；store 層會做正規化對應。
 */
export interface LoginResultDto {
  redirectUrl?: string | null
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: BackendUserProfile
}

/** 後端 UserProfileDto（PascalCase 欄位，僅列出前端會用到的部分） */
export interface BackendUserProfile {
  UID?: number
  PID?: string
  UserName?: string
  EMail?: string
  RoleID?: number
  RoleName?: string
  [key: string]: unknown
}

/** 前端正規化後的使用者資訊（store 對外提供的形狀） */
export interface AuthUser {
  uid?: number
  pid?: string
  userName?: string
  email?: string
  roleId?: number
  roleName?: string
}
