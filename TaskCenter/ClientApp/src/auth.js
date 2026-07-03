/**
 * auth.js – 輕量響應式 auth store（不依賴 Pinia）
 *
 * 認證機制：
 *   - 登入成功後後端會同時設定 Cookie（SignInAsync）並回傳 JWT。
 *   - 前端將 JWT 存入 localStorage，後續請求由 api-service 自動附加 Bearer。
 *   - isLoggedIn 同時要求「有使用者資料」且「持有 access token」。
 *
 * 用法：
 *   import { useAuthStore } from '@/auth'
 *   const auth = useAuthStore()
 *   auth.user           // { uid, pid, userName, email }
 *   auth.isLoggedIn     // computed boolean
 *   await auth.login(id, password, rememberMe)
 *   await auth.getMe()
 *   await auth.logout()
 */
import { reactive, computed } from 'vue'
import {
  login as apiLogin,
  logout as apiLogout,
  getMe as apiGetMe,
} from '@/services/auth-api'
import { setTokens, clearTokens, getAccessToken } from '@/utils/token-utils'

const STORAGE_KEY = 'eivo_user'

function loadFromStorage() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? JSON.parse(raw) : null
  } catch {
    return null
  }
}

const state = reactive({
  user: loadFromStorage(),
  loading: false,
})

/** 將後端使用者物件正規化為前端形狀（兼容 PascalCase / camelCase） */
function normalizeUser(u) {
  if (!u) return null
  return {
    uid: u.UID ?? u.uid,
    pid: u.PID ?? u.pid,
    userName: u.UserName ?? u.userName,
    email: u.EMail ?? u.email,
    roleId: u.RoleID ?? u.roleId,
    roleName: u.RoleName ?? u.roleName,
  }
}

export function useAuthStore() {
  // 同時要求有使用者資料且仍持有 access token
  const isLoggedIn = computed(() => !!state.user && !!getAccessToken())

  function setUser(data) {
    state.user = normalizeUser(data)
    if (state.user) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(state.user))
    } else {
      localStorage.removeItem(STORAGE_KEY)
    }
  }

  /**
   * 登入。成功時儲存 token 與使用者資料。
   * @returns {Promise<{success: boolean, message: string, redirectUrl?: string}>}
   */
  async function login(id, password, rememberMe = false) {
    state.loading = true
    try {
      const res = await apiLogin({ id, password, rememberMe })
      if (res.success && res.data) {
        setTokens(res.data.accessToken, res.data.refreshToken, res.data.expiresAt)
        setUser(res.data.user)
        return {
          success: true,
          message: res.message,
          redirectUrl: res.data.redirectUrl ?? undefined,
        }
      }
      return {
        success: false,
        message: res.message || '登入失敗，請確認帳號密碼',
      }
    } finally {
      state.loading = false
    }
  }

  /** 取得目前登入者資訊（/auth/me 回傳頂層欄位） */
  async function getMe() {
    const res = await apiGetMe()
    if (res?.success && (res.id || res.pid)) {
      setUser({ uid: Number(res.id) || undefined, pid: res.pid, userName: res.name })
      return state.user
    }
    return state.user
  }

  /** 登出：呼叫後端清除 Cookie，並清除本地 token 與使用者資料 */
  async function logout() {
    try {
      await apiLogout()
    } catch {
      /* 忽略登出 API 錯誤，仍清除本地狀態 */
    }
    clearTokens()
    setUser(null)
  }

  return {
    state,
    user: computed(() => state.user),
    isLoggedIn,
    loading: computed(() => state.loading),
    setUser,
    login,
    getMe,
    logout,
  }
}
