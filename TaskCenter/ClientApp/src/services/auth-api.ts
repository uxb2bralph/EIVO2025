/**
 * auth-api.ts — 認證相關前台 API（對應後端 AuthController）
 *
 * 全部透過 `apiRequest` 呼叫，回傳統一的 `ApiResponse<T>`，永不拋出例外。
 * 對應後端路由：
 *   POST /api/auth/login    登入
 *   POST /api/auth/refresh  刷新 token
 *   GET  /api/auth/logout   登出（後端為 HttpGet + [Authorize]）
 *   GET  /api/auth/me       取得目前登入者資訊（[Authorize]）
 */
import { apiRequest } from '@/services/api-service'
import type { ApiResponse } from '@/interfaces/api-response'
import type { LoginRequest, LoginResultDto } from '@/interfaces/auth'

/** 後端 /auth/me 回傳的（非 ApiResponse 包裝的）使用者資訊 */
export interface MeResult {
  id?: string
  name?: string
  pid?: string
  realName?: string
}

/** 登入：POST /api/auth/login */
export function login(payload: LoginRequest): Promise<ApiResponse<LoginResultDto>> {
  return apiRequest<LoginResultDto>('/auth/login', {
    method: 'POST',
    data: payload,
  })
}

/** 刷新 token：POST /api/auth/refresh（body 為 refresh token 字串） */
export function refresh(refreshToken: string): Promise<ApiResponse<LoginResultDto>> {
  return apiRequest<LoginResultDto>('/auth/refresh', {
    method: 'POST',
    data: JSON.stringify(refreshToken),
  })
}

/** 登出：GET /api/auth/logout */
export function logout(): Promise<ApiResponse> {
  return apiRequest('/auth/logout', { method: 'GET' })
}

/**
 * 取得目前登入者：GET /api/auth/me
 *
 * 此端點直接回傳匿名物件（非 ApiResponse 包裝），
 * `apiRequest` 會補上 success=true，故使用者欄位位於回應頂層。
 */
export function getMe(): Promise<ApiResponse<unknown> & MeResult> {
  return apiRequest('/auth/me', { method: 'GET' }) as Promise<ApiResponse<unknown> & MeResult>
}
