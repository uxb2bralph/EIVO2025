/**
 * auth-utils.ts — 認證流程輔助（token 即將過期判斷、刷新、導向登入）
 *
 * 提供 api-service 的 interceptor 使用：
 *  - isExpiringSoon()    token 是否即將過期（< 2 分鐘）
 *  - refreshAccessToken() 以 refresh token 換新 token（去重，避免並行重複刷新）
 *  - redirectToLogin()   清除 token 並導回登入頁
 */
import axios from 'axios'
import type { ApiResponse } from '@/interfaces/api-response'
import type { LoginResultDto } from '@/interfaces/auth'
import {
  getExpiresAt,
  getRefreshToken,
  setTokens,
  clearTokens,
} from '@/utils/token-utils'
import { appBase } from '@/utils/app-base'

/** token 提前刷新閾值：到期前 2 分鐘 */
const EXPIRING_SOON_MS = 2 * 60 * 1000

// appBase 為部署根路徑（'/' 或 '/TaskCenter2025/'），API 與登入頁 URL 都要帶上它。
const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? `${appBase}api`
/** 登入頁的「應用內」路由路徑（不含部署 base）。 */
const LOGIN_ROUTE = '/Login'

/** access token 是否即將過期（到期前 2 分鐘內）。到期時間未知時回傳 false。 */
export function isExpiringSoon(): boolean {
  const expiry = getExpiresAt()
  if (expiry === null) return false
  return expiry - Date.now() < EXPIRING_SOON_MS
}

// 確保同一時間只有一個刷新請求在進行（多個並行請求共用同一 Promise）
let refreshPromise: Promise<boolean> | null = null

/**
 * 以 refresh token 向後端換取新的 access token。
 *
 * 使用裸 axios（非 $api）呼叫，避免觸發 $api 的 request interceptor 造成無限遞迴。
 * 成功時更新 token 並回傳 true；失敗時導回登入頁並回傳 false。
 */
export function refreshAccessToken(): Promise<boolean> {
  if (refreshPromise) return refreshPromise
  refreshPromise = doRefresh().finally(() => {
    refreshPromise = null
  })
  return refreshPromise
}

async function doRefresh(): Promise<boolean> {
  const refreshToken = getRefreshToken()
  if (!refreshToken) {
    redirectToLogin()
    return false
  }

  try {
    // 後端 RefreshToken 接收 [FromBody] string，需傳 JSON 字串字面值
    const { data } = await axios.post<ApiResponse<LoginResultDto>>(
      `${API_BASE_URL}/auth/refresh`,
      JSON.stringify(refreshToken),
      {
        headers: { 'Content-Type': 'application/json' },
        withCredentials: true,
      },
    )

    if (data?.success && data.data?.accessToken) {
      setTokens(data.data.accessToken, data.data.refreshToken, data.data.expiresAt)
      return true
    }

    redirectToLogin()
    return false
  } catch {
    redirectToLogin()
    return false
  }
}

/**
 * 清除 token 並導向登入頁（保留 returnUrl 以便登入後返回）。
 * 已在登入頁時不重複導向，避免迴圈。
 */
export function redirectToLogin(): void {
  clearTokens()
  if (typeof window === 'undefined') return

  // 目前網址去掉部署 base，還原成「應用內」路由路徑（例如 /OrganizationQuery）。
  const baseNoSlash = appBase.replace(/\/$/, '') // '' 或 '/TaskCenter2025'
  let route = window.location.pathname
  if (baseNoSlash && route.startsWith(baseNoSlash)) {
    route = route.slice(baseNoSlash.length) || '/'
  }
  // 已在登入頁 → 不重複導向，避免迴圈。
  if (route === LOGIN_ROUTE) return

  const returnUrl = encodeURIComponent(route + window.location.search)
  // 硬導向須帶上部署 base（appBase 結尾必為 '/'），否則會打到站台根 → 404。
  window.location.assign(`${appBase}Login?returnUrl=${returnUrl}`)
}
