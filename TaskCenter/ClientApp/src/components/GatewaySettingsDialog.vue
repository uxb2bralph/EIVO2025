<!--
  GatewaySettingsDialog.vue – 用戶端 G/W 設定對話框。
  遷移自舊版 Organization/GatewaySettings，抽離自 OrganizationQueryIndex.vue。
-->
<script setup lang="ts">
import { ref, watch } from 'vue'

interface ProcessTypeOption {
  value: number
  label: string
}

const props = defineProps<{
  /** 是否顯示對話框 */
  visible: boolean
  /** 營業人名稱 */
  companyName?: string
  /** 是否載入中 */
  loading?: boolean
  /** 是否儲存中 */
  saving?: boolean
  /** 錯誤訊息 */
  error?: string
  /** 傳送 Excel 發票開立方式選項 */
  processTypeOptions: ProcessTypeOption[]
  /** 目前選取的開立方式（v-model） */
  modelValue: number | ''
  /** 憑證上載中 */
  certUploading?: boolean
  /** 憑證上載錯誤訊息 */
  certError?: string
  /** 憑證上載成功訊息 */
  certMessage?: string
  /** 目前已設定的憑證金鑰（OrganizationToken.KeyID）；未設定時為空 */
  certKeyId?: string | null
}>()

const emit = defineEmits<{
  (e: 'update:modelValue', value: number | ''): void
  (e: 'close'): void
  (e: 'save'): void
  (e: 'uploadCertificate', payload: { file: File; pin: string }): void
}>()

// PKCS12(PFX) 憑證上載表單狀態（於對話框內就地管理）
const certFileInput = ref<HTMLInputElement | null>(null)
const certFile = ref<File | null>(null)
const certPin = ref('')

function onCertFileChange(event: Event) {
  const files = (event.target as HTMLInputElement).files
  certFile.value = files && files.length > 0 ? files[0] : null
}

function onUploadCertificate() {
  if (!certFile.value) return
  emit('uploadCertificate', { file: certFile.value, pin: certPin.value })
}

function resetCertForm() {
  certFile.value = null
  certPin.value = ''
  if (certFileInput.value) {
    certFileInput.value.value = ''
  }
}

// 上載成功（收到成功訊息）後清空表單，避免重複送出同一檔案。
watch(
  () => props.certMessage,
  (msg) => {
    if (msg) resetCertForm()
  },
)

// 對話框關閉時一併清空憑證表單。
watch(
  () => props.visible,
  (visible) => {
    if (!visible) resetCertForm()
  },
)
</script>

<template>
  <div v-if="visible" class="eivo-modal-overlay">
    <div class="eivo-modal-dialog gw-dialog">
      <div class="eivo-modal-header">
        <h2>用戶端 G/W 設定</h2>
        <button type="button" class="eivo-modal-close" aria-label="關閉" @click="$emit('close')">×</button>
      </div>
      <div class="eivo-modal-body">
        <div v-if="companyName" class="gw-company">{{ companyName }}</div>
        <div v-if="error" class="alert-error">{{ error }}</div>
        <div v-if="loading" class="state-msg">資料載入中…</div>
        <template v-else>
          <div class="form-group">
            <label>選擇傳送 Excel 發票開立方式：</label>
            <select
              :value="modelValue"
              class="form-select"
              :disabled="saving"
              @change="$emit('update:modelValue', ($event.target as HTMLSelectElement).value === '' ? '' : Number(($event.target as HTMLSelectElement).value))"
            >
              <option value="">請選擇...</option>
              <option v-for="opt in processTypeOptions" :key="opt.value" :value="opt.value">
                {{ opt.label }}
              </option>
            </select>
          </div>
          <!-- PKCS12(PFX) 憑證上載（遷移自舊版 CertificateIdentityController.CommitItem） -->
          <div class="cert-section">
            <div class="cert-title">憑證設定</div>
            <div class="form-group">
              <label>PKCS12(PFX)憑證檔：</label>
              <input
                ref="certFileInput"
                type="file"
                class="form-control"
                :disabled="certUploading"
                @change="onCertFileChange"
              />
            </div>
            <div class="form-group">
              <label>PIN Code：</label>
              <input
                v-model="certPin"
                type="password"
                class="form-control"
                autocomplete="new-password"
                :disabled="certUploading"
              />
            </div>
            <div v-if="certKeyId" class="cert-keyid">原憑證金鑰:{{ certKeyId }}</div>
            <div v-if="certError" class="alert-error">{{ certError }}</div>
            <div v-if="certMessage" class="cert-success">{{ certMessage }}</div>
            <div class="cert-actions">
              <button
                type="button"
                class="btn primary"
                :disabled="certUploading || !certFile"
                @click="onUploadCertificate"
              >
                <span v-if="!certUploading">上載</span>
                <span v-else>上載中…</span>
              </button>
            </div>
          </div>
          <div class="gw-actions">
            <button type="button" class="btn ghost" :disabled="saving" @click="$emit('close')">
              取消
            </button>
            <button type="button" class="btn primary" :disabled="saving" @click="$emit('save')">
              <span v-if="!saving">儲存</span>
              <span v-else>儲存中…</span>
            </button>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* 對話框（與 OrganizationQueryIndex 共用樣式，scoped 需各自定義） */
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
.state-msg {
  text-align: center;
  color: var(--color-muted);
  padding: 2.5rem 0;
}
.alert-error {
  color: #ff8585;
  margin-bottom: 1rem;
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

/* 用戶端 G/W 設定對話框 */
.gw-dialog {
  max-width: 520px;
}
.gw-company {
  font-weight: 600;
  color: var(--color-text);
  margin-bottom: 1rem;
}
.gw-actions {
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
  margin-top: 1.5rem;
}

/* PKCS12(PFX) 憑證設定區塊 */
.cert-section {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin-top: 1.5rem;
  padding-top: 1.25rem;
  border-top: 1px solid var(--color-border);
}
.cert-title {
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--color-text);
}
.cert-success {
  color: var(--color-accent);
  font-size: 0.85rem;
}
.cert-keyid {
  color: var(--color-muted);
  font-size: 0.85rem;
  word-break: break-all;
}
.cert-actions {
  display: flex;
  justify-content: flex-end;
}
</style>
