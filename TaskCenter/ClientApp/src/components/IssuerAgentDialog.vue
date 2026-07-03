<!--
  IssuerAgentDialog.vue – 設定發票經銷商對話框。
  遷移自舊版 Organization/Module/ApplyIssuerAgent.cshtml，抽離自 OrganizationQueryIndex.vue。
  以勾選方式指派「發票開立代理」類別的營業人為此開立人的經銷商；
  沿用舊版以加密 KeyID 傳遞 CompanyID 的做法，勾選結果以 keyId 清單回傳父層。
-->
<script setup lang="ts">
import { ref, watch } from 'vue'
import type { IssuerAgent } from '@/services/organization-api'

const props = defineProps<{
  /** 是否顯示對話框 */
  visible: boolean
  /** 開立人名稱 */
  companyName?: string
  /** 是否載入中 */
  loading?: boolean
  /** 是否儲存中 */
  saving?: boolean
  /** 錯誤訊息 */
  error?: string
  /** 經銷商候選清單（含目前是否已指派） */
  agents: IssuerAgent[]
}>()

const emit = defineEmits<{
  (e: 'close'): void
  (e: 'save', selectedKeyIds: string[]): void
}>()

// 目前勾選的經銷商 keyId 集合。
const selected = ref<Set<string>>(new Set())

// 以候選清單目前的指派狀態初始化勾選（載入完成或重開對話框時）。
function resetSelection() {
  selected.value = new Set(
    props.agents.filter((a) => a.selected && a.keyId).map((a) => a.keyId as string),
  )
}

function toggle(keyId: string | null) {
  if (!keyId) return
  const next = new Set(selected.value)
  if (next.has(keyId)) {
    next.delete(keyId)
  } else {
    next.add(keyId)
  }
  selected.value = next
}

function submit() {
  emit('save', Array.from(selected.value))
}

// 候選清單載入後同步勾選狀態。
watch(() => props.agents, resetSelection, { deep: true })

// 對話框開啟時重設勾選狀態。
watch(
  () => props.visible,
  (visible) => {
    if (visible) resetSelection()
  },
)
</script>

<template>
  <div v-if="visible" class="eivo-modal-overlay">
    <div class="eivo-modal-dialog agency-dialog">
      <div class="eivo-modal-header">
        <h2>設定發票經銷商</h2>
        <button type="button" class="eivo-modal-close" aria-label="關閉" @click="$emit('close')">×</button>
      </div>
      <div class="eivo-modal-body">
        <div v-if="companyName" class="agency-company">{{ companyName }}</div>
        <div v-if="error" class="alert-error">{{ error }}</div>
        <div v-if="loading" class="state-msg">資料載入中…</div>
        <template v-else>
          <div class="agency-list">
            <label v-for="agent in agents" :key="agent.keyId ?? ''" class="agency-item">
              <input
                type="checkbox"
                :checked="agent.keyId ? selected.has(agent.keyId) : false"
                :disabled="saving || !agent.keyId"
                @change="toggle(agent.keyId)"
              />
              <span>({{ agent.receiptNo }}){{ agent.companyName }}</span>
            </label>
            <div v-if="!agents.length" class="agency-empty">尚無可指派的發票經銷商</div>
          </div>
          <div class="agency-footer">
            <button type="button" class="btn ghost" :disabled="saving" @click="resetSelection">重填</button>
            <button type="button" class="btn primary" :disabled="saving" @click="submit">
              <span v-if="!saving">確定</span>
              <span v-else>儲存中…</span>
            </button>
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

/* 設定發票經銷商對話框 */
.agency-dialog {
  max-width: 640px;
}
.agency-company {
  font-weight: 600;
  color: var(--color-text);
  margin-bottom: 1rem;
}
.agency-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
  gap: 0.5rem 1rem;
  max-height: 55vh;
  overflow-y: auto;
}
.agency-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.9rem;
  color: var(--color-text);
  cursor: pointer;
}
.agency-empty {
  grid-column: 1 / -1;
  text-align: center;
  color: var(--color-muted);
  padding: 1.25rem 0;
}
.agency-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 1.5rem;
  padding-top: 1.25rem;
  border-top: 1px solid var(--color-border);
}
</style>
