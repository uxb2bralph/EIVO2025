<!--
  POSDeviceDialog.vue – 店家 POS 機維護對話框。
  遷移自舊版 InvoiceBusiness/ApplyPOSDevice（POSDeviceList），抽離自 OrganizationQueryIndex.vue。
-->
<script setup lang="ts">
import { ref, watch } from 'vue'
import type { POSDevice } from '@/services/organization-api'

const props = defineProps<{
  /** 是否顯示對話框 */
  visible: boolean
  /** 營業人名稱 */
  companyName?: string
  /** 是否載入中 */
  loading?: boolean
  /** 是否儲存 / 刪除中 */
  saving?: boolean
  /** 錯誤訊息 */
  error?: string
  /** POS 機清單 */
  devices: POSDevice[]
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'save', payload: { deviceId: number | null; posNo: string }): void
  (e: 'delete', device: POSDevice): void
}>()

// 編輯中的列（以 deviceId 為鍵；null 代表正在編輯「新增」列）
const editingId = ref<number | null | undefined>(undefined)
const editingPosNo = ref('')
// 新增列的輸入
const newPosNo = ref('')

function startEdit(device: POSDevice) {
  editingId.value = device.deviceId
  editingPosNo.value = device.posNo ?? ''
}

function cancelEdit() {
  editingId.value = undefined
  editingPosNo.value = ''
}

function submitEdit() {
  if (editingId.value === undefined) return
  emit('save', { deviceId: editingId.value, posNo: editingPosNo.value })
}

function submitNew() {
  emit('save', { deviceId: null, posNo: newPosNo.value })
}

// 儲存成功後（清單長度或內容變化）由父層更新 devices；此處於清單變動時收合新增/編輯狀態。
watch(
  () => props.devices,
  () => {
    cancelEdit()
    newPosNo.value = ''
  },
  { deep: true },
)

// 對話框關閉時重設就地編輯狀態。
watch(
  () => props.visible,
  (visible) => {
    if (!visible) {
      cancelEdit()
      newPosNo.value = ''
    }
  },
)
</script>

<template>
  <div v-if="visible" class="eivo-modal-overlay">
    <div class="eivo-modal-dialog pos-dialog">
      <div class="eivo-modal-header">
        <h2>店家 POS 機維護</h2>
        <button type="button" class="eivo-modal-close" aria-label="關閉" @click="$emit('close')">×</button>
      </div>
      <div class="eivo-modal-body">
        <div v-if="companyName" class="pos-company">{{ companyName }}</div>
        <div v-if="error" class="alert-error">{{ error }}</div>
        <div v-if="loading" class="state-msg">資料載入中…</div>
        <template v-else>
          <table class="pos-table">
            <thead>
              <tr>
                <th>POS機編號</th>
                <th class="pos-actions-col">管理</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="device in devices" :key="device.deviceId">
                <td>
                  <template v-if="editingId === device.deviceId">
                    <input
                      v-model="editingPosNo"
                      type="text"
                      class="form-control"
                      :disabled="saving"
                      @keyup.enter="submitEdit"
                    />
                  </template>
                  <template v-else>{{ device.posNo }}</template>
                </td>
                <td class="pos-actions-col">
                  <template v-if="editingId === device.deviceId">
                    <button type="button" class="btn primary sm" :disabled="saving" @click="submitEdit">
                      儲存
                    </button>
                    <button type="button" class="btn ghost sm" :disabled="saving" @click="cancelEdit">
                      取消
                    </button>
                  </template>
                  <template v-else>
                    <button type="button" class="btn ghost sm" :disabled="saving" @click="startEdit(device)">
                      編輯
                    </button>
                    <button type="button" class="btn ghost sm" :disabled="saving" @click="$emit('delete', device)">
                      刪除
                    </button>
                  </template>
                </td>
              </tr>
              <tr v-if="!devices.length">
                <td colspan="2" class="pos-empty">尚未設定 POS 機</td>
              </tr>
              <!-- 新增列 -->
              <tr class="pos-add-row">
                <td>
                  <input
                    v-model="newPosNo"
                    type="text"
                    class="form-control"
                    placeholder="輸入新的 POS 機編號"
                    :disabled="saving"
                    @keyup.enter="submitNew"
                  />
                </td>
                <td class="pos-actions-col">
                  <button
                    type="button"
                    class="btn primary sm"
                    :disabled="saving || !newPosNo.trim()"
                    @click="submitNew"
                  >
                    新增
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
          <div class="pos-footer">
            <button type="button" class="btn ghost" :disabled="saving" @click="$emit('close')">關閉</button>
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
.btn.sm {
  font-size: 0.8rem;
  padding: 0.25rem 0.6rem;
  margin-left: 0.35rem;
}
.btn.sm:first-child {
  margin-left: 0;
}

/* 店家 POS 機維護對話框 */
.pos-dialog {
  max-width: 560px;
}
.pos-company {
  font-weight: 600;
  color: var(--color-text);
  margin-bottom: 1rem;
}
.pos-table {
  width: 100%;
  border-collapse: collapse;
  color: var(--color-text);
}
.pos-table th,
.pos-table td {
  padding: 0.5rem 0.6rem;
  text-align: left;
  border-bottom: 1px solid var(--color-border);
  font-size: 0.9rem;
  vertical-align: middle;
}
.pos-table thead th {
  color: var(--color-muted);
  font-weight: 600;
}
.pos-actions-col {
  width: 12rem;
  white-space: nowrap;
  text-align: right;
}
.pos-empty {
  text-align: center;
  color: var(--color-muted);
  padding: 1.25rem 0;
}
.pos-add-row td {
  border-bottom: none;
  padding-top: 0.9rem;
}
.pos-footer {
  display: flex;
  justify-content: flex-end;
  margin-top: 1.5rem;
}
.form-control {
  width: 100%;
}
</style>
