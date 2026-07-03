/**
 * api-service.ts — 前端 HTTP 請求統一入口
 *
 * 功能：
 *  - 自動附加 JWT Bearer Token
 *  - Token 過期前主動刷新（proactive refresh）
 *  - 收到 401 時自動重試一次（reactive refresh）
 *  - 統一錯誤格式回傳（ApiResponse<T>），永不拋出例外
 *  - 檔案 Blob 下載
 */
import axios, { type AxiosRequestConfig, type InternalAxiosRequestConfig } from 'axios'
import type { ApiResponse } from '@/interfaces/api-response'
import { getAccessToken, isExpired } from '@/utils/token-utils'
import { isExpiringSoon, refreshAccessToken, redirectToLogin } from '@/utils/auth-utils'

// 擴充 Axios 請求設定，追蹤 401 重試狀態（避免無限迴圈）
declare module 'axios' {
  interface InternalAxiosRequestConfig {
    _retried?: boolean
  }
}

// ── $api instance ─────────────────────────────────────────────────────────────

/**
 * 預設 Axios 實例，已掛載 token 管理與 401 重試 interceptor。
 * 通常不直接使用，請透過 `apiRequest` / `apiDownloadBlob` 呼叫。
 */
export const $api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? '/api',
  timeout: 30000,
  withCredentials: true, // 傳送 HttpOnly cookie（後端優先讀取 Authorization header）
  headers: {
    'Content-Type': 'application/json',
  },
})

// ── Request interceptor：token 生命週期管理 ────────────────────────────────────

$api.interceptors.request.use(async (config: InternalAxiosRequestConfig) => {
  const token = getAccessToken()
  if (!token) return config

  // token 已過期或即將過期 → 先刷新，再附加最新 token
  if (isExpired() || isExpiringSoon()) {
    await refreshAccessToken()
  }

  // 附加最新 token（refresh 後可能已更新；refresh 失敗時已清除，會回退為無 token）
  const latestToken = getAccessToken()
  if (latestToken) {
    config.headers.Authorization = `Bearer ${latestToken}`
  }

  return config
})

// ── Response interceptor：業務錯誤記錄 & 401 重試 ──────────────────────────────

$api.interceptors.response.use(
  (response) => {
    // 記錄 API 層業務錯誤（success=false，HTTP 200 但業務失敗）
    const data = response.data as ApiResponse
    if (data && typeof data === 'object' && 'success' in data && !data.success) {
      console.warn('[api-service] Business error:', data.message, data.errors)
    }
    return response
  },
  async (error) => {
    const originalRequest = error.config as InternalAxiosRequestConfig

    if (error.response?.status === 401 && !originalRequest._retried) {
      originalRequest._retried = true
      const refreshed = await refreshAccessToken()
      if (refreshed) {
        // 刷新成功 → 用新 token 重試原請求
        const newToken = getAccessToken()
        if (newToken) {
          originalRequest.headers.Authorization = `Bearer ${newToken}`
        }
        return $api(originalRequest)
      } else {
        // 刷新失敗 → 已被 refreshAccessToken 重導登入頁
        return Promise.reject(error)
      }
    }

    if (error.response?.status !== 401) {
      console.error('[api-service] Request error:', {
        status: error.response?.status,
        url: error.config?.url,
        method: error.config?.method,
        data: error.response?.data,
      })
    } else {
      // 重試後仍 401
      redirectToLogin()
    }

    return Promise.reject(error)
  },
)

// ── 公開 API ──────────────────────────────────────────────────────────────────

/**
 * 發送 JSON 請求，回傳 `ApiResponse<T>`，**永不拋出例外**。
 *
 * @example
 * const res = await apiRequest<Product[]>('/products')
 * if (res.success) console.log(res.data)
 * else console.error(res.message)
 */
export async function apiRequest<T = unknown>(
  url: string,
  options?: AxiosRequestConfig,
): Promise<ApiResponse<T>> {
  try {
    const { data: response } = await $api<ApiResponse<T>>(url, options)
    response.success = response.success ?? true // 預設 success=true（適用於非 ApiResponse 的純資料回應）
    console.debug('[api-service] Response:', { url, method: options?.method ?? 'GET', response: response })
    return response
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.data) {
      const responseData = error.response.data as unknown
      if (typeof responseData === 'object' && responseData !== null && 'success' in responseData) {
        return responseData as ApiResponse<T>
      }
      const partialData = responseData as Partial<ApiResponse<T>>
      return {
        success: false,
        message: partialData.message ?? error.message,
        errors: partialData.errors,
      }
    }
    return {
      success: false,
      message: error instanceof Error ? error.message : '請求發生未知錯誤',
    }
  }
}

/**
 * 發送請求並以 `Blob` 回傳，適用於 PDF / Excel 等檔案下載。
 * 認證 token 由 `$api` onRequest 自動處理。
 *
 * **注意：此函式可能拋出例外，呼叫方需自行處理。**
 *
 * @example
 * const blob = await apiDownloadBlob('/reports/export', { method: 'POST', body: { ... } })
 * const url = URL.createObjectURL(blob)
 * // 觸發下載 ...
 * URL.revokeObjectURL(url)
 */
export async function apiDownloadBlob(
  url: string,
  options?: { method?: string; body?: unknown },
): Promise<Blob> {
  const { data } = await $api<Blob>(url, {
    method: options?.method ?? 'GET',
    data: options?.body,
    responseType: 'blob',
  })
  return data
}
