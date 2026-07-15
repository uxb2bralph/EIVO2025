<!--
  CustomSmtpDialog.vue – 客製化服務設定（發送通知郵件伺服器 SMTP）對話框。
  遷移自舊版 Organization/Module/CustomSettings.cshtml + CustomSmtp.cshtml，抽離自 OrganizationQueryIndex.vue。
  沿用舊版以加密 KeyID 傳遞 CompanyID；密碼留空表示不變更原密碼。
-->
<script setup lang="ts">
import { ref, watch } from 'vue'
import type { CustomSmtpSettings } from '@/services/organization-api'

const props = defineProps<{
  /** 是否顯示對話框 */
  visible: boolean
  /** 營業人名稱 */
  companyName?: string
  /** 是否載入中 */
  loading?: boolean
  /** 是否儲存 / 停用中 */
  saving?: boolean
  /** 錯誤訊息 */
  error?: string
  /** 成功訊息 */
  message?: string
  /** 目前 SMTP 設定（configured=false 表示尚未設定 / 已停用） */
  data: CustomSmtpSettings | null
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'save', payload: {
    host: string
    port: number | null
    enableSsl: boolean
    userName: string | null
    password: string | null
    mailFrom: string
  }): void
  (e: 'disable'): void
}>()

// 表單欄位
const host = ref('')
const port = ref<number | ''>('')
const enableSsl = ref(false)
const userName = ref('')
const password = ref('')
const mailFrom = ref('')

// data 變動時（開啟 / 載入完成 / 儲存後刷新）帶入表單；密碼一律留空（不回傳）。
watch(
  () => props.data,
  (data) => {
    host.value = data?.host ?? ''
    port.value = data?.port ?? ''
    enableSsl.value = data?.enableSsl ?? false
    userName.value = data?.userName ?? ''
    mailFrom.value = data?.mailFrom ?? ''
    password.value = ''
  },
  { immediate: true },
)

function submit() {
  emit('save', {
    host: host.value.trim(),
    port: port.value === '' ? null : Number(port.value),
    enableSsl: enableSsl.value,
    userName: userName.value.trim() || null,
    password: password.value === '' ? null : password.value,
    mailFrom: mailFrom.value.trim(),
  })
}
</script>

<template>
  <div v-if="visible" class="eivo-modal-overlay">
    <div class="eivo-modal-dialog smtp-dialog">
      <div class="eivo-modal-header">
        <h2>客製化服務設定</h2>
        <button type="button" class="eivo-modal-close" aria-label="關閉" @click="$emit('close')">×</button>
      </div>
      <div class="eivo-modal-body">
        <div v-if="companyName" class="smtp-company">{{ companyName }}</div>
        <div v-if="error" class="alert-error">{{ error }}</div>
        <div v-if="message" class="alert-success">{{ message }}</div>
        <div v-if="loading" class="state-msg">資料載入中…</div>
        <form v-else class="smtp-form" @submit.prevent="submit">
          <div class="smtp-caption">設定發送通知郵件伺服器(SMTP)</div>
          <div class="form-row">
            <div class="form-group">
              <label><span class="req">*</span>伺服器 domain name 或 IP</label>
              <input v-model="host" type="text" class="form-control" :disabled="saving" required />
            </div>
            <div class="form-group">
              <label><span class="req">*</span>Port</label>
              <input v-model="port" type="number" class="form-control" placeholder="預設 25" :disabled="saving" />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>登入帳號</label>
              <input
                v-model="userName"
                type="text"
                class="form-control"
                placeholder="請輸入 smtp 登入帳號"
                :disabled="saving"
              />
            </div>
            <div class="form-group">
              <label>密碼</label>
              <input
                v-model="password"
                type="password"
                class="form-control"
                autocomplete="new-password"
                :placeholder="data?.hasPassword ? '已設定，留空表示不變更' : '請輸入 smtp 登入密碼'"
                :disabled="saving"
              />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label><span class="req">*</span>寄件人 email</label>
              <input v-model="mailFrom" type="text" class="form-control" :disabled="saving" required />
            </div>
            <div class="form-group ssl-group">
              <label>使用 SSL</label>
              <label class="ssl-check">
                <input v-model="enableSsl" type="checkbox" :disabled="saving" /> 使用 SSL 連線
              </label>
            </div>
          </div>
          <div class="smtp-footer">
            <button type="submit" class="btn primary" :disabled="saving">
              <span v-if="!saving">設定使用</span>
              <span v-else>處理中…</span>
            </button>
            <button
              v-if="data?.configured"
              type="button"
              class="btn ghost"
              :disabled="saving"
              @click="$emit('disable')"
            >
              停用
            </button>
            <button type="button" class="btn ghost" :disabled="saving" @click="$emit('close')">關閉</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.eivo-modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 1000;
  display: flex;
  align-items: flex-start;
  justify-content: center;
  padding: 2.5rem 1rem;
  background: rgba(0, 0, 0, 0.55);
  overflow-y: auto;
}
.eivo-modal-dialog {
  width: 100%;
  max-width: 960px;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 0.75rem;
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.4);
}
.eivo-modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.5rem;
  border-bottom: 1px solid var(--color-border);
}
.eivo-modal-header h2 {
  margin: 0;
  font-size: 1.15rem;
  color: var(--color-text);
}
.eivo-modal-close {
  background: transparent;
  border: none;
  color: var(--color-muted);
  font-size: 1.5rem;
  line-height: 1;
  cursor: pointer;
}
.eivo-modal-close:hover {
  color: var(--color-text);
}
.eivo-modal-body {
  padding: 1.5rem;
}
.state-msg {
  text-align: center;
  color: var(--color-muted);
  padding: 2.5rem 0;
}
.alert-error {
  color: #ff8585;
  margin-bottom: 1rem;
  white-space: pre-line;
}
.alert-success {
  color: #4ade80;
  margin-bottom: 1rem;
}
.smtp-dialog {
  max-width: 620px;
}
.smtp-company {
  font-weight: 600;
  color: var(--color-text);
  margin-bottom: 1rem;
}
.smtp-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.smtp-caption {
  font-weight: 600;
  color: var(--color-text);
}
.form-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}
.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}
.form-group label {
  font-size: 0.85rem;
  font-weight: 500;
  color: var(--color-muted);
}
.req {
  color: #ff8585;
  margin-right: 0.15rem;
}
.ssl-group .ssl-check {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  color: var(--color-text);
  font-weight: 400;
  padding-top: 0.4rem;
}
.form-control:disabled {
  opacity: 0.7;
}
.smtp-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 0.5rem;
  padding-top: 1.25rem;
  border-top: 1px solid var(--color-border);
}
.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.btn.ghost {
  background: transparent;
  border: 1px solid var(--color-border);
  color: var(--color-text);
}
.btn.ghost:hover:not(:disabled) {
  border-color: var(--color-accent);
}
</style>
