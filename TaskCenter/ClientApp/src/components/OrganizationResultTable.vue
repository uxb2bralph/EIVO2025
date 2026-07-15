<!--
  OrganizationResultTable.vue – 營業人查詢結果表格（含每列管理下拉選單）。
  抽離自 OrganizationQueryIndex.vue，為純呈現元件：資料與動作皆由父層透過 props / events 提供。
-->
<script setup lang="ts">
import { toRef, watch } from 'vue'
import type { OrganizationDatatable } from '../services/organization-api'
import { useTableSelection } from '../composables/useTableSelection'

/** 每列管理選單可觸發的動作 */
export type OrgRowAction =
  | 'edit'
  | 'disable'
  | 'enable'
  | 'gateway'
  | 'pos'
  | 'agency'
  | 'relationship'
  | 'master'
  | 'user'
  | 'custom'

// 對應後端 Naming.MemberStatusDefinition.Mark_To_Delete（1101 註記停用）
const MEMBER_STATUS_MARK_TO_DELETE = 1101

const props = defineProps<{
  /** 查詢結果列表 */
  items: OrganizationDatatable[]
  /** 目前展開管理選單的列（以 companyId 為鍵），null 表示皆關閉 */
  openMenuId: number | null
  /** 日期格式化函式（沿用父層 composable 提供的實作） */
  formatDate: (value: string | null) => string
}>()

const emit = defineEmits<{
  (e: 'toggle-menu', companyId: number): void
  (e: 'action', action: OrgRowAction, org: OrganizationDatatable): void
}>()

/** 勾選結果（以 keyId 為鍵），父層以 v-model:selected 取得 */
const selected = defineModel<string[]>('selected', { default: () => [] })

// 勾選邏輯：全選 / 半選 / 單選皆由響應式狀態推導，不再直接操作 DOM
const {
  selected: selectedKeys,
  allChecked,
  indeterminate,
  toggleAll,
  toggle,
  isChecked,
  clear,
} = useTableSelection(toRef(props, 'items'), (org) => org.keyId)

// composable 內部選取變動時回寫 v-model，讓父層取得選取結果
watch(selectedKeys, (keys) => {
  selected.value = [...keys]
})

// 換頁 / 重新查詢載入新資料列時清空選取，避免殘留舊 key 造成錯誤的半選狀態
watch(
  () => props.items,
  () => clear(),
)

function onToggleAll(event: Event) {
  toggleAll((event.target as HTMLInputElement).checked)
}

function onToggleItem(org: OrganizationDatatable, event: Event) {
  toggle(org, (event.target as HTMLInputElement).checked)
}

// 選單項目點擊：先關閉選單再觸發動作（父層 action 處理器負責關閉）
function onAction(action: OrgRowAction, org: OrganizationDatatable) {
  emit('action', action, org)
}
</script>

<template>
  <div class="table-wrap">
    <table class="result-table">
      <thead>
        <tr>
          <th>
            <input
              name="checkAll"
              type="checkbox"
              :checked="allChecked"
              :indeterminate="indeterminate"
              @change="onToggleAll"
            />
          </th>
          <th>營業人名稱</th>
          <th>統編</th>
          <th>負責人</th>
          <th>連絡人信箱</th>
          <th>狀態</th>
          <th>上線日期</th>
          <th>到期日期</th>
          <th>主機構</th>
          <th>管理</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="org in items" :key="org.companyId">
          <td>
            <input
              type="checkbox"
              name="checkItem"
              :value="org.keyId"
              :checked="isChecked(org)"
              @change="onToggleItem(org, $event)"
            />
          </td>
          <td>{{ org.companyName }}</td>
          <td>{{ org.receiptNo }}</td>
          <td>{{ org.undertakerName }}</td>
          <td>{{ org.contactEmail }}</td>
          <td>{{ org.statusName }}</td>
          <td>{{ formatDate(org.goLiveDate) }}</td>
          <td>{{ formatDate(org.expirationDate) }}</td>
          <td>
            <!-- 主機構開關：切換時透過 master 動作觸發父層 commitMaster -->
            <span class="master-cell" :title="org.isMaster ? '是' : '否'">
              <label class="switch">
                <input
                  type="checkbox"
                  :checked="org.isMaster"
                  @change="onAction('master', org)"
                />
                <span class="slider round"></span>
              </label>
              <span class="master-label">{{ org.isMaster ? '是' : '否' }}</span>
            </span>
          </td>
          <td>
            <!-- @click.stop 避免冒泡到 document 的關閉監聽 -->
            <div class="action-dropdown" @click.stop>
              <button
                type="button"
                class="btn action-toggle"
                :aria-expanded="openMenuId === org.companyId"
                @click="emit('toggle-menu', org.companyId)"
              >
                請選擇功能 <span class="caret"></span>
              </button>
              <ul v-if="openMenuId === org.companyId" class="action-menu">
                <!-- 已註記停用（Mark_To_Delete）時，僅提供「標示並設定啟用」 -->
                <template v-if="org.statusLevel === MEMBER_STATUS_MARK_TO_DELETE">
                  <li><a @click="onAction('enable', org)">啟用</a></li>
                </template>
                <template v-else>
                  <li><a @click="onAction('edit', org)">編輯</a></li>
                  <li><a @click="onAction('disable', org)">停用</a></li>
                  <li><a @click="onAction('gateway', org)">用戶端G/W設定</a></li>
                  <li><a @click="onAction('pos', org)">設定POS機號</a></li>
                  <li><a @click="onAction('agency', org)">設定發票經銷商</a></li>
                  <li><a @click="onAction('relationship', org)">設定為B2B營業人</a></li>
                  <li><a @click="onAction('user', org)">管理使用者</a></li>
                  <li><a @click="onAction('custom', org)">客製化服務設定</a></li>
                </template>
              </ul>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.table-wrap {
  overflow-x: auto;
}
.result-table {
  width: 100%;
  border-collapse: collapse;
  color: var(--color-text);
}
.result-table th,
.result-table td {
  padding: 0.6rem 0.75rem;
  text-align: left;
  border-bottom: 1px solid var(--color-border);
  white-space: nowrap;
  font-size: 0.9rem;
}
.result-table thead th {
  color: var(--color-muted);
  font-weight: 600;
}
.result-table tbody tr:hover {
  background: rgba(59, 199, 255, 0.06);
}
/* 主機構開關（toggle switch） */
.master-cell {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  user-select: none;
}
.switch {
  position: relative;
  display: inline-block;
  width: 44px;
  height: 24px;
  flex-shrink: 0;
}
.switch input {
  opacity: 0;
  width: 0;
  height: 0;
}
.slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: var(--color-border);
  transition: 0.4s;
}
.slider::before {
  position: absolute;
  content: '';
  height: 18px;
  width: 18px;
  left: 3px;
  bottom: 3px;
  background-color: #fff;
  transition: 0.4s;
}
.switch input:checked + .slider {
  background-color: var(--color-accent);
}
.switch input:focus + .slider {
  box-shadow: 0 0 1px var(--color-accent);
}
.switch input:checked + .slider::before {
  transform: translateX(20px);
}
/* Rounded slider */
.slider.round {
  border-radius: 24px;
}
.slider.round::before {
  border-radius: 50%;
}
.master-label {
  font-size: 0.85rem;
  color: var(--color-text);
}

/* 管理下拉選單 */
.action-dropdown {
  position: relative;
  display: inline-block;
}
.action-toggle {
  background: var(--color-accent);
  border: 1px solid var(--color-accent);
  color: #fff;
  font-size: 0.85rem;
  padding: 0.35rem 0.7rem;
}
.action-toggle .caret {
  display: inline-block;
  margin-left: 0.35rem;
  border-top: 4px solid currentColor;
  border-right: 4px solid transparent;
  border-left: 4px solid transparent;
  vertical-align: middle;
}
.action-menu {
  position: absolute;
  right: 0;
  z-index: 10;
  min-width: 9.5rem;
  margin: 0.25rem 0 0;
  padding: 0.25rem 0;
  list-style: none;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 0.5rem;
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.25);
}
.action-menu li a {
  display: block;
  padding: 0.45rem 0.9rem;
  font-size: 0.85rem;
  color: var(--color-text);
  white-space: nowrap;
  cursor: pointer;
}
.action-menu li a:hover {
  background: rgba(59, 199, 255, 0.12);
  color: var(--color-accent);
}
</style>
