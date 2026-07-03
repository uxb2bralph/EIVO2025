/**
 * token-utils.ts — JWT access / refresh token 的儲存與解析
 *
 * 負責：
 *  - 將 access / refresh token 與到期時間存入 localStorage
 *  - 解析 JWT payload（前端輕量驗證，僅用於判斷到期，不做簽章驗證）
 *  - 提供 `getAccessToken()` / `isExpired()` 供 api-service 使用
 */

const ACCESS_TOKEN_KEY = 'eivo_access_token'
const REFRESH_TOKEN_KEY = 'eivo_refresh_token'
const EXPIRES_AT_KEY = 'eivo_expires_at'

interface JwtPayload {
  exp?: number
  [key: string]: unknown
}

/**
 * 解析 JWT payload（base64url）。失敗回傳 null。
 * 注意：此處僅解碼，不驗證簽章——簽章驗證由後端負責。
 */
export function decodeJwt(token: string): JwtPayload | null {
  try {
    const payload = token.split('.')[1]
    if (!payload) return null
    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/')
    const json = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join(''),
    )
    return JSON.parse(json) as JwtPayload
  } catch {
    return null
  }
}

/**
 * 儲存登入取得的 token。
 * 到期時間優先採用後端回傳的 `expiresAt`，否則從 JWT 的 `exp` 推算。
 */
export function setTokens(accessToken: string, refreshToken?: string, expiresAt?: string): void {
  localStorage.setItem(ACCESS_TOKEN_KEY, accessToken)
  if (refreshToken) {
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken)
  }

  let expiryMs: number | null = null
  if (expiresAt) {
    const parsed = new Date(expiresAt).getTime()
    if (!Number.isNaN(parsed)) expiryMs = parsed
  }
  if (expiryMs === null) {
    const payload = decodeJwt(accessToken)
    if (payload?.exp) expiryMs = payload.exp * 1000
  }
  if (expiryMs !== null) {
    localStorage.setItem(EXPIRES_AT_KEY, String(expiryMs))
  }
}

export function getAccessToken(): string | null {
  return localStorage.getItem(ACCESS_TOKEN_KEY)
}

export function getRefreshToken(): string | null {
  return localStorage.getItem(REFRESH_TOKEN_KEY)
}

/** 取得 access token 到期時間（epoch ms）；未知時回傳 null。 */
export function getExpiresAt(): number | null {
  const value = localStorage.getItem(EXPIRES_AT_KEY)
  return value ? Number(value) : null
}

export function clearTokens(): void {
  localStorage.removeItem(ACCESS_TOKEN_KEY)
  localStorage.removeItem(REFRESH_TOKEN_KEY)
  localStorage.removeItem(EXPIRES_AT_KEY)
}

/**
 * 判斷 access token 是否已過期。
 * 到期時間未知時回傳 false（交由後端 401 反應式處理）。
 */
export function isExpired(): boolean {
  const expiry = getExpiresAt()
  if (expiry === null) return false
  return Date.now() >= expiry
}
