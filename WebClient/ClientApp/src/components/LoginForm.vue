<script setup lang="ts">
import { ref, computed } from 'vue'
import axios from 'axios'

type State = 'idle' | 'submitting' | 'success' | 'error'

const id = ref('')
const password = ref('')
const rememberMe = ref(false)
const state = ref<State>('idle')
const errorMsg = ref('')

// 基本驗證
const idError = computed(() => {
  if (!id.value.trim()) return '請輸入 ID'
  return ''
})
const passwordError = computed(() => {
  if (!password.value) return '請輸入密碼'
  if (password.value.length < 6) return '密碼至少 6 碼'
  return ''
})
const hasErrors = computed(() => !!idError.value || !!passwordError.value)

// 模擬 API endpoint（請改成你的後端路由）
const endpoint = '/api/auth/login'

// 提交表單
const onSubmit = async () => {
  errorMsg.value = ''
  if (hasErrors.value) {
    state.value = 'error'
    return
  }
  try {
    state.value = 'submitting'
    const payload = {
      id: id.value.trim(),
      password: password.value,
      rememberMe: rememberMe.value,
    }
    const { data } = await axios.post(endpoint, payload, {
      // 需要跨域時可加 withCredentials 與適當 CORS 設定
      withCredentials: true,
    })
    // 假設後端回傳 { success: boolean, message?: string, redirectUrl?: string }
    if (data?.success) {
      state.value = 'success'
      // 選擇性導頁
      if (data.redirectUrl) {
        window.location.href = data.redirectUrl
      }
    } else {
      state.value = 'error'
      errorMsg.value = data?.message || '登入失敗，請確認帳密'
    }
  } catch (err: any) {
    state.value = 'error'
    errorMsg.value = err?.response?.data?.message || '系統發生錯誤，稍後再試'
  }
}
</script>

<template>
  <div class="container py-5">
    <div class="row justify-content-center">
      <div class="col-12 col-sm-10 col-md-6 col-lg-4">
        <div class="card shadow-sm">
          <div class="card-body">
            <h1 class="h4 mb-3 text-center">電子發票系統</h1>
            <form @submit.prevent="onSubmit" novalidate>
              <!-- ID -->
              <div class="mb-3">
                <label for="loginId" class="form-label">ID</label>
                <input
                  id="loginId"
                  type="text"
                  class="form-control"
                  :class="{ 'is-invalid': idError }"
                  v-model="id"
                  autocomplete="username"
                  placeholder="請輸入 ID"
                />
                <div class="invalid-feedback" v-if="idError">{{ idError }}</div>
              </div>

              <!-- Password -->
              <div class="mb-3">
                <label for="loginPassword" class="form-label">Password</label>
                <input
                  id="loginPassword"
                  type="password"
                  class="form-control"
                  :class="{ 'is-invalid': passwordError }"
                  v-model="password"
                  autocomplete="current-password"
                  placeholder="請輸入密碼"
                />
                <div class="invalid-feedback" v-if="passwordError">
                  {{ passwordError }}
                </div>
              </div>

              <!-- Remember me + Forgot password -->
              <div class="d-flex justify-content-between align-items-center mb-3">
                <div class="form-check">
                  <input
                    id="rememberMe"
                    class="form-check-input"
                    type="checkbox"
                    v-model="rememberMe"
                  />
                  <label class="form-check-label" for="rememberMe">記住我</label>
                </div>
                <a href="/Account/ForgotPassword" class="text-decoration-none">
                  Forgot password
                </a>
              </div>

              <!-- Submit -->
              <button
                type="submit"
                class="btn btn-primary w-100"
                :disabled="state === 'submitting'"
              >
                <span v-if="state !== 'submitting'">Sign In</span>
                <span v-else class="d-inline-flex align-items-center gap-2">
                  <span
                    class="spinner-border spinner-border-sm"
                    role="status"
                    aria-hidden="true"
                  ></span>
                  處理中…
                </span>
              </button>

              <!-- Error message -->
              <div
                v-if="state === 'error' && errorMsg"
                class="alert alert-danger mt-3"
                role="alert"
              >
                {{ errorMsg }}
              </div>

              <!-- Success message -->
              <div
                v-if="state === 'success'"
                class="alert alert-success mt-3"
                role="alert"
              >
                登入成功，正在為您導頁…
              </div>
            </form>
          </div>
        </div>

        <!-- 額外：公司/系統資訊 -->
        <p class="text-center text-muted mt-3 mb-0" style="font-size: 0.9rem;">
          © 2025 電子發票系統
        </p>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* 可選：微調卡片與間距 */
.card {
  border: none;
  border-radius: 0.75rem;
}
</style>
