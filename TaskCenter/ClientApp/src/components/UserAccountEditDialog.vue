<!--
  UserAccountEditDialog.vue – 使用者帳號編輯 / 新增對話框。
  遷移自舊版 UserProfile/EditUserProfile.cshtml（ItemForm + ChangePass），抽離自 UserAccountIndex.vue。
  所屬營業人固定為目前管理的營業人（唯讀顯示），沿用舊版以加密 KeyID 傳遞識別碼。
-->
<script setup lang="ts">
import { ref, watch } from 'vue'
import type { UserAccountEdit } from '@/services/user-account-api'

const props = defineProps<{
  /** 是否顯示對話框 */
  visible: boolean
  /** 載入中（編輯時載入原資料） */
  loading?: boolean
  /** 儲存中 */
  saving?: boolean
  /** 錯誤訊息 */
  error?: string
  /** 表單資料（新增時為空白表單，編輯時為載入的資料） */
  data: UserAccountEdit | null
  /** 身份設定選項 */
  roleOptions: { value: number; label: string }[]
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'save', payload: UserAccountEdit): void
}>()

// 表單本地副本（避免直接改動父層 props）
const form = ref<UserAccountEdit | null>(null)

// data 變動時（開啟 / 載入完成）複製一份到本地表單。
watch(
  () => props.data,
  (data) => {
    form.value = data ? { ...data } : null
  },
  { immediate: true },
)

// 是否為新增（無 keyId）
const isNew = () => !form.value?.keyId

function submit() {
  if (form.value) emit('save', form.value)
}
</script>

<template>
  <div v-if="visible" class="eivo-modal-overlay">
    <div class="eivo-modal-dialog account-edit-dialog">
      <div class="eivo-modal-header">
        <h2>{{ isNew() ? '新增帳號' : '修改帳號' }}</h2>
        <button type="button" class="eivo-modal-close" aria-label="關閉" @click="$emit('close')">×</button>
      </div>
      <div class="eivo-modal-body">
        <div v-if="error" class="alert-error">{{ error }}</div>
        <div v-if="loading" class="state-msg">資料載入中…</div>
        <form v-else-if="form" class="edit-form" @submit.prevent="submit">
          <div class="form-group">
            <label>會員</label>
            <input type="text" class="form-control" :value="form.companyName ?? ''" disabled />
          </div>
          <div class="form-group">
            <label>帳號 <span class="req">*</span></label>
            <input v-model="form.pid" type="text" class="form-control" :disabled="saving" required />
          </div>
          <div class="form-group">
            <label>姓名 <span class="req">*</span></label>
            <input v-model="form.userName" type="text" class="form-control" :disabled="saving" required />
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>密碼<span v-if="isNew()" class="req">*</span></label>
              <input
                v-model="form.password"
                type="password"
                class="form-control"
                autocomplete="new-password"
                :placeholder="isNew() ? '' : '留空表示不修改'"
                :disabled="saving"
              />
            </div>
            <div class="form-group">
              <label>請再確認密碼</label>
              <input
                v-model="form.password1"
                type="password"
                class="form-control"
                autocomplete="new-password"
                :disabled="saving"
              />
            </div>
          </div>
          <div class="form-group">
            <label>常用電子郵件 <span class="req">*</span></label>
            <input v-model="form.email" type="text" class="form-control" :disabled="saving" required />
          </div>
          <div class="form-group">
            <label>住址</label>
            <input v-model="form.address" type="text" class="form-control" :disabled="saving" />
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>電話（日）</label>
              <input v-model="form.phone" type="text" class="form-control" :disabled="saving" />
            </div>
            <div class="form-group">
              <label>電話（夜）</label>
              <input v-model="form.phone2" type="text" class="form-control" :disabled="saving" />
            </div>
            <div class="form-group">
              <label>行動電話</label>
              <input v-model="form.mobilePhone" type="text" class="form-control" :disabled="saving" />
            </div>
          </div>
          <div class="form-group">
            <label>身份設定 <span class="req">*</span></label>
            <select v-model="form.roleId" class="form-select" :disabled="saving" required>
              <option :value="null" disabled>請選擇身份</option>
              <option v-for="opt in roleOptions" :key="opt.value" :value="opt.value">
                {{ opt.label }}
              </option>
            </select>
          </div>
          <div class="edit-footer">
            <button type="button" class="btn ghost" :disabled="saving" @click="$emit('close')">取消</button>
            <button type="submit" class="btn primary" :disabled="saving">
              <span v-if="!saving">儲存</span>
              <span v-else>儲存中…</span>
            </button>
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
.account-edit-dialog {
  max-width: 620px;
}
.edit-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.form-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
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
}
.form-control:disabled {
  opacity: 0.7;
}
.edit-footer {
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
