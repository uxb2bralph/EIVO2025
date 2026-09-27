<!--
  E0501SettingsDialog.vue – E0501（配號結果檔）自動取號設定對話框。
  遷移自 WebHome Views/Organization/ApplyE0501Settings.cshtml，由 OrganizationEditForm.vue 開啟。
  沿用舊版以加密 KeyID 傳遞 CompanyID；設定值存於 OrganizationCustomSetting.SettingData(JSON)。
  另含「主機構批次配號」：以 Excel 範本下載 / 匯入維護 Settings.BranchInvoiceNoAssignments。
-->
<script setup lang="ts">
import { ref, watch } from 'vue'
import type { E0501Settings } from '@/services/organization-settings-api'

const props = defineProps<{
  /** 是否顯示對話框 */
  visible: boolean
  /** 營業人名稱（顯示用） */
  companyName?: string
  /** 是否載入中 */
  loading?: boolean
  /** 是否儲存中 */
  saving?: boolean
  /** 錯誤訊息 */
  error?: string
  /** 成功訊息 */
  message?: string
  /** 目前設定；載入中為 null */
  data: E0501Settings | null
  /** 主機構批次配號範本下載中 */
  branchDownloading?: boolean
  /** 主機構批次配號 Excel 匯入中 */
  branchUploading?: boolean
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'save', payload: {
    disableAutoUpdate: boolean
    initialLock: boolean
    reservedBooklets: number | null
  }): void
  /** 下載主機構批次配號範本 */
  (e: 'download-branch-template'): void
  /** 匯入主機構批次配號 Excel（選檔後立即傳送） */
  (e: 'upload-branch-assignments', file: File): void
}>()

// 表單欄位（對應舊版 DisableE0501AutoUpdate / E0501InitialLock / E0501ReservedBooklets）
const disableAutoUpdate = ref(false)
const initialLock = ref(false)
const reservedBooklets = ref<number | ''>('')

// data 變動時（開啟 / 載入完成 / 儲存後刷新）帶入表單。
watch(
  () => props.data,
  (data) => {
    disableAutoUpdate.value = data?.disableAutoUpdate ?? false
    initialLock.value = data?.initialLock ?? false
    reservedBooklets.value = data?.reservedBooklets ?? ''
  },
  { immediate: true },
)

function submit() {
  emit('save', {
    disableAutoUpdate: disableAutoUpdate.value,
    initialLock: initialLock.value,
    reservedBooklets: reservedBooklets.value === '' ? null : Number(reservedBooklets.value),
  })
}

// 主機構批次配號：選檔後立即傳送（沿用資料維護頁「下載範本 / 立即傳送」的操作方式）。
const branchFileInput = ref<HTMLInputElement | null>(null)

function pickBranchFile() {
  branchFileInput.value?.click()
}

function onBranchFileChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) {
    emit('upload-branch-assignments', file)
  }
  // 清除選取，讓同一個檔案修正後可再次上傳。
  input.value = ''
}
</script>

<template>
  <div v-if="visible" class="eivo-modal-overlay">
    <div class="eivo-modal-dialog e0501-dialog">
      <div class="eivo-modal-header">
        <h2>E0501取號設定</h2>
        <button type="button" class="eivo-modal-close" aria-label="關閉" @click="emit('close')">×</button>
      </div>
      <div class="eivo-modal-body">
        <div v-if="companyName" class="e0501-company">{{ companyName }}</div>
        <div v-if="error" class="alert-error">{{ error }}</div>
        <div v-if="message" class="alert-success">{{ message }}</div>
        <div v-if="loading" class="state-msg">資料載入中…</div>
        <form v-else class="e0501-form" @submit.prevent="submit">
          <div class="form-row">
            <label class="row-label">E0501自動取號設定</label>
            <div class="row-value checks">
              <label class="check">
                <input v-model="disableAutoUpdate" type="checkbox" :disabled="saving" />停用
              </label>
              <label class="check">
                <input v-model="initialLock" type="checkbox" :disabled="saving" />預設鎖定
              </label>
            </div>
          </div>
          <div class="form-row">
            <label class="row-label" for="e0501-reserved-booklets">保留本組數</label>
            <div class="row-value">
              <input
                id="e0501-reserved-booklets"
                v-model="reservedBooklets"
                type="number"
                min="0"
                step="1"
                class="form-control"
                placeholder="每組 50 號；留空表示不保留"
                :disabled="saving"
              />
            </div>
          </div>

          <!--
            主機構批次配號：以 Excel 交換 Settings.BranchInvoiceNoAssignments。
            範本已帶出現行設定（或以分支機構清單預填）；上傳後整批取代，與上方 E0501 表單的存檔分開。
          -->
          <div class="branch-section">
            <div class="branch-head">
              <span class="row-label">主機構批次配號</span>
              <div class="branch-actions">
                <button
                  type="button"
                  class="btn ghost"
                  :disabled="branchDownloading || branchUploading"
                  @click="emit('download-branch-template')"
                >
                  {{ branchDownloading ? '下載中…' : '下載範本' }}
                </button>
                <input
                  ref="branchFileInput"
                  type="file"
                  accept=".xlsx,.xlsm"
                  class="hidden-file"
                  @change="onBranchFileChange"
                />
                <button
                  type="button"
                  class="btn primary"
                  :disabled="branchUploading || branchDownloading"
                  @click="pickBranchFile"
                >
                  {{ branchUploading ? '處理中…' : '立即傳送' }}
                </button>
              </div>
            </div>
            <p class="branch-hint">
              範本「配號設定」工作表每列為一個分支機構：分支機構統一編號 / 配號本組數（每組 50 號）/ 預設鎖定（是、否）。
              上傳後整批取代現行配號設定；任一列有誤則整批不儲存。
            </p>
            <table v-if="data?.branchAssignments?.length" class="branch-table">
              <thead>
                <tr>
                  <th>統一編號</th>
                  <th>分支機構</th>
                  <th class="num">本組數</th>
                  <th>預設鎖定</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="item in data.branchAssignments" :key="item.receiptNo ?? ''">
                  <td>{{ item.receiptNo }}</td>
                  <td>{{ item.companyName }}</td>
                  <td class="num">{{ item.booklets ?? '' }}</td>
                  <td>{{ item.initialLock ? '是' : '否' }}</td>
                </tr>
              </tbody>
            </table>
            <p v-else class="branch-hint empty">尚未設定批次配號。</p>
          </div>

          <div class="e0501-footer">
            <button type="submit" class="btn primary" :disabled="saving || !!loading">
              {{ saving ? '處理中…' : '確定' }}
            </button>
            <button type="button" class="btn ghost" :disabled="saving" @click="emit('close')">關閉</button>
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
  z-index: 1010;
  display: flex;
  align-items: flex-start;
  justify-content: center;
  padding: 2.5rem 1rem;
  background: rgba(0, 0, 0, 0.55);
  overflow-y: auto;
}
.eivo-modal-dialog {
  width: 100%;
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
.e0501-dialog {
  max-width: 560px;
}
.e0501-company {
  font-weight: 600;
  color: var(--color-text);
  margin-bottom: 1rem;
}
.e0501-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.form-row {
  display: grid;
  grid-template-columns: minmax(8rem, 10rem) 1fr;
  align-items: center;
  gap: 1rem;
}
.row-label {
  font-size: 0.85rem;
  font-weight: 500;
  color: var(--color-muted);
}
.checks {
  display: flex;
  gap: 1.25rem;
}
.check {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.85rem;
  color: var(--color-text);
}
.form-control:disabled {
  opacity: 0.7;
}
.branch-section {
  padding-top: 1rem;
  border-top: 1px solid var(--color-border);
  display: flex;
  flex-direction: column;
  gap: 0.6rem;
}
.branch-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}
.branch-actions {
  display: flex;
  gap: 0.5rem;
  align-items: center;
}
.hidden-file {
  display: none;
}
.branch-hint {
  margin: 0;
  font-size: 0.78rem;
  line-height: 1.5;
  color: var(--color-muted);
}
.branch-hint.empty {
  font-style: italic;
}
.branch-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.82rem;
  color: var(--color-text);
}
.branch-table th,
.branch-table td {
  padding: 0.35rem 0.5rem;
  border-bottom: 1px solid var(--color-border);
  text-align: left;
}
.branch-table th {
  color: var(--color-muted);
  font-weight: 500;
}
.branch-table .num {
  text-align: right;
}
.e0501-footer {
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
