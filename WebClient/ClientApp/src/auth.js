/**
 * auth.js – lightweight reactive auth store (no Pinia required)
 *
 * Usage:
 *   import { useAuthStore } from '@/auth'
 *   const auth = useAuthStore()
 *   auth.user        // { pid, userName, roleId, roleName }
 *   auth.isLoggedIn  // computed boolean
 *   await auth.login(id, password, rememberMe)
 *   await auth.getMe()
 *   auth.logout()
 */
import { reactive, computed } from 'vue'
import axios from 'axios'

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

export function useAuthStore() {
  const isLoggedIn = computed(() => !!state.user)

  function setUser(data) {
    state.user = data ? {
      pid: data.pid,
      userName: data.userName,
      roleId: data.roleId,
      roleName: data.roleName,
    } : null
    if (state.user) {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(state.user))
    } else {
      localStorage.removeItem(STORAGE_KEY)
    }
  }

  async function login(id, password, rememberMe = false) {
    state.loading = true
    try {
      const { data } = await axios.post('/api/auth/login',
        { id, password, rememberMe },
        { withCredentials: true }
      )
      if (data?.success) {
        setUser(data)
      }
      return data
    } finally {
      state.loading = false
    }
  }

  async function getMe() {
    try {
      const { data } = await axios.get('/api/auth/me', { withCredentials: true })
      setUser(data)
      return data
    } catch {
      setUser(null)
      return null
    }
  }

  async function logout() {
    try {
      await axios.post('/api/auth/logout', {}, { withCredentials: true })
    } catch { /* ignore */ }
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
