<!-- OrganizationQueryIndex.vue – 營業人資料管理（查詢）。遷移自 WebHome OrganizationQuery/InquireOrganization。 -->
<script setup lang="ts">
import { computed, ref } from 'vue'
import { useOrganizationQueryIndex } from '../composables/useOrganizationQueryIndex'
import OrganizationEditForm from './OrganizationEditForm.vue'
import GatewaySettingsDialog from './GatewaySettingsDialog.vue'
import POSDeviceDialog from './POSDeviceDialog.vue'
import IssuerAgentDialog from './IssuerAgentDialog.vue'
import CustomSmtpDialog from './CustomSmtpDialog.vue'
import OrganizationResultTable from './OrganizationResultTable.vue'
import Pager from './Pager.vue'
import type { OrgRowAction } from './OrganizationResultTable.vue'
import type { OrganizationDatatable } from '../services/organization-api'

const {
  // 查詢條件
  receiptNo,
  companyName,
  organizationStatus,
  categoryId,
  categoryOptions,
  statusOptions,
  agentKeyword,
  agentSuggestions,
  agentSearching,
  showAgentSuggestions,
  onAgentInput,
  onAgentFocus,
  onAgentBlur,
  selectAgent,
  clearAgent,
  // 設為分支機構：主機構 autocomplete
  headquarterKeyId,
  headquarterKeyword,
  headquarterSuggestions,
  headquarterSearching,
  showHeadquarterSuggestions,
  headquarterApplying,
  onHeadquarterInput,
  onHeadquarterFocus,
  onHeadquarterBlur,
  selectHeadquarter,
  clearHeadquarter,
  applyHeadquarter,
  // 複製收費標準（沿用主機構選定的營業人作為複製來源）
  billingCloning,
  cloneBilling,
  // 列表狀態
  items,
  totalCount,
  page,
  loading,
  error,
  searched,
  totalPages,
  // 管理下拉選單
  openMenuId,
  toggleMenu,
  closeMenu,
  // 編輯營業人
  editVisible,
  editLoading,
  editSaving,
  editError,
  editData,
  editIsNew,
  addCompany,
  closeEdit,
  saveEdit,
  // 用戶端 G/W 設定
  processTypeOptions,
  gatewayVisible,
  gatewayLoading,
  gatewaySaving,
  gatewayError,
  gatewayCompanyName,
  gatewayProcessType,
  closeGateway,
  saveGateway,
  // PKCS12(PFX) 憑證上載
  certUploading,
  certError,
  certMessage,
  certKeyId,
  uploadCertificate,
  // 設定 POS 機號
  posVisible,
  posLoading,
  posSaving,
  posError,
  posCompanyName,
  posDevices,
  closePOS,
  savePOSDevice,
  removePOSDevice,
  // 設定發票經銷商
  agencyVisible,
  agencyLoading,
  agencySaving,
  agencyError,
  agencyCompanyName,
  agencyAgents,
  closeAgency,
  saveAgency,
  // 客製化服務設定（SMTP）
  customVisible,
  customLoading,
  customSaving,
  customError,
  customMessage,
  customCompanyName,
  customData,
  closeCustom,
  saveCustom,
  disableCustom,
  // 管理功能
  editCompany,
  disableCompany,
  enableCompany,
  gatewaySettings,
  applyPOS,
  applyAgency,
  applyRelationship,
  commitMaster,
  inquireUser,
  customSettings,
  // 工具 / 事件
  formatDate,
  onSearch,
  goToPage,
} = useOrganizationQueryIndex()

// 結果表格勾選的列（以 keyId 為鍵），由子元件 v-model:selected 回寫
const selectedKeys = ref<string[]>([])
const hasSelection = computed(() => selectedKeys.value.length > 0)

// 批次操作示範：讀取子元件回寫的勾選結果 selectedKeys
function onBatchDisable() {
  if (!selectedKeys.value.length) return
  const keys = [...selectedKeys.value]
  if (!window.confirm(`確定要停用選取的 ${keys.length} 筆營業人？`)) return
  // TODO: 呼叫後端批次 API（例如 disableOrganizations(keys)）；此處僅示範讀取選取結果
  console.log('批次停用 keyId：', keys)
  window.alert(`已送出批次停用：\n${keys.join('\n')}`)
}

// 設為分支機構：將表格勾選的營業人（selectedKeys）設為選定主機構的分支機構。
// 成功後清除列表勾選（主機構輸入由 composable 內清除）。
async function onApplyHeadquarter() {
  const ok = await applyHeadquarter([...selectedKeys.value])
  if (ok) selectedKeys.value = []
}

// 複製收費標準：以主機構選定的營業人為複製來源，將其收費標準複製到表格勾選的營業人（selectedKeys）。
// 成功後清除列表勾選（來源選擇保留，方便繼續複製到其他目標）。
async function onCloneBilling() {
  const ok = await cloneBilling([...selectedKeys.value])
  if (ok) selectedKeys.value = []
}

// 結果表格每列管理選單動作分派：先關閉選單再執行對應功能
function onRowAction(action: OrgRowAction, org: OrganizationDatatable) {
  closeMenu()
  switch (action) {
    case 'edit':
      editCompany(org)
      break
    case 'disable':
      disableCompany(org)
      break
    case 'enable':
      enableCompany(org)
      break
    case 'gateway':
      gatewaySettings(org)
      break
    case 'pos':
      applyPOS(org)
      break
    case 'agency':
      applyAgency(org)
      break
    case 'relationship':
      applyRelationship(org)
      break
    case 'master':
      commitMaster(org)
      break
    case 'user':
      inquireUser(org)
      break
    case 'custom':
      customSettings(org)
      break
  }
}
</script>

<template>
  <div class="org-query-page">
    <div class="page-header">
      <h1>營業人資料管理</h1>
    </div>

    <!-- 查詢條件 -->
    <div class="card query-card">
      <div class="query-grid">
        <div class="form-group">
          <label>統編</label>
          <input
            v-model="receiptNo"
            type="text"
            class="form-control"
            placeholder="請輸入統一編號"
            @keyup.enter="onSearch"
          />
        </div>
        <div class="form-group">
          <label>營業人名稱</label>
          <input
            v-model="companyName"
            type="text"
            class="form-control"
            placeholder="請輸入營業人名稱"
            @keyup.enter="onSearch"
          />
        </div>
        <div class="form-group">
          <label>營業人狀態</label>
          <select v-model="organizationStatus" class="form-select">
            <option value="">全部</option>
            <option v-for="opt in statusOptions" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </option>
          </select>
        </div>
        <div class="form-group">
          <label>營業人類別</label>
          <select v-model="categoryId" class="form-select">
            <option value="">全部</option>
            <option v-for="opt in categoryOptions" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </option>
          </select>
        </div>
        <div class="form-group agent-group">
          <label>所屬經銷商</label>
          <div class="autocomplete">
            <input
              v-model="agentKeyword"
              type="text"
              class="form-control"
              placeholder="輸入統編或名稱查詢經銷商（留空為全部）"
              autocomplete="off"
              @input="onAgentInput"
              @focus="onAgentFocus"
              @blur="onAgentBlur"
              @keyup.enter="onSearch"
            />
            <button
              v-if="agentKeyword"
              type="button"
              class="autocomplete-clear"
              aria-label="清除"
              @click="clearAgent"
            >
              ×
            </button>
            <ul
              v-if="showAgentSuggestions && agentSuggestions.length"
              class="autocomplete-list"
            >
              <li
                v-for="opt in agentSuggestions"
                :key="opt.companyId"
                class="autocomplete-item"
                @mousedown.prevent="selectAgent(opt)"
              >
                ({{ opt.receiptNo }}){{ opt.companyName }}
              </li>
            </ul>
            <div
              v-else-if="showAgentSuggestions && !agentSearching && agentKeyword.trim()"
              class="autocomplete-empty"
            >
              查無符合的經銷商
            </div>
          </div>
        </div>
      </div>
      <div class="query-actions">
        <button class="btn primary" :disabled="loading" @click="onSearch">
          <span v-if="!loading">查詢</span>
          <span v-else>查詢中…</span>
        </button>
        <button class="btn ghost" :disabled="loading" @click="addCompany">新增</button>
      </div>
    </div>

    <!-- 查詢結果 -->
    <div class="card result-card">
      <div v-if="error" class="alert-error">{{ error }}</div>

      <div v-if="loading" class="state-msg">資料載入中…</div>

      <template v-else-if="items.length">
        <!-- 批次操作工具列：讀取表格勾選結果 selectedKeys -->
        <div class="batch-toolbar">
          <span class="batch-count">已選 {{ selectedKeys.length }} 筆</span>
          <button class="btn ghost" :disabled="!hasSelection" @click="onBatchDisable">
            批次停用
          </button>
          <!-- 主機構 autocomplete 同時作為「複製收費標準」的複製來源與「設為分支機構」的主機構 -->
          <!-- 設為分支機構：選擇主機構（autocomplete），將勾選的營業人設為其分支機構 -->
          <div class="headquarter-picker autocomplete">
            <input
              v-model="headquarterKeyword"
              type="text"
              class="form-control"
              placeholder="選擇主機構：輸入統編或名稱"
              autocomplete="off"
              @input="onHeadquarterInput"
              @focus="onHeadquarterFocus"
              @blur="onHeadquarterBlur"
            />
            <button
              v-if="headquarterKeyword"
              type="button"
              class="autocomplete-clear"
              aria-label="清除"
              @click="clearHeadquarter"
            >
              ×
            </button>
            <ul
              v-if="showHeadquarterSuggestions && headquarterSuggestions.length"
              class="autocomplete-list"
            >
              <li
                v-for="opt in headquarterSuggestions"
                :key="opt.keyId ?? ''"
                class="autocomplete-item"
                @mousedown.prevent="selectHeadquarter(opt)"
              >
                ({{ opt.receiptNo }}){{ opt.companyName }}
              </li>
            </ul>
            <div
              v-else-if="showHeadquarterSuggestions && !headquarterSearching && headquarterKeyword.trim()"
              class="autocomplete-empty"
            >
              查無符合的主機構
            </div>
          </div>
          <button
            class="btn primary"
            :disabled="!hasSelection || !headquarterKeyId || headquarterApplying"
            @click="onApplyHeadquarter"
          >
            <span v-if="!headquarterApplying">設為分支機構</span>
            <span v-else>設定中…</span>
          </button>
          <!-- 複製收費標準：以主機構選定的營業人為來源，複製收費標準到勾選的營業人 -->
          <button
            class="btn ghost"
            :disabled="!hasSelection || !headquarterKeyId || billingCloning"
            @click="onCloneBilling"
          >
            <span v-if="!billingCloning">複製收費標準</span>
            <span v-else>複製中…</span>
          </button>
        </div>

        <OrganizationResultTable
          v-model:selected="selectedKeys"
          :items="items"
          :open-menu-id="openMenuId"
          :format-date="formatDate"
          @toggle-menu="toggleMenu"
          @action="onRowAction"
        />
      </template>

      <div v-else-if="searched" class="state-msg">查無資料!!</div>

      <!-- 分頁 -->
      <Pager
        :page="page"
        :total-pages="totalPages"
        :total-count="totalCount"
        @change="goToPage"
      />
    </div>

    <!-- 編輯營業人對話框（遷移自舊版 editOrganization tab） -->
    <div v-if="editVisible" class="eivo-modal-overlay">
      <div class="eivo-modal-dialog">
        <div class="eivo-modal-header">
          <h2>{{ editIsNew ? '新增營業人資料' : '編輯營業人資料' }}</h2>
          <button type="button" class="eivo-modal-close" aria-label="關閉" @click="closeEdit">×</button>
        </div>
        <div class="eivo-modal-body">
          <div v-if="editError" class="alert-error">{{ editError }}</div>
          <div v-if="editLoading" class="state-msg">資料載入中…</div>
          <OrganizationEditForm
            v-else-if="editData"
            :data="editData"
            :saving="editSaving"
            @save="saveEdit"
            @cancel="closeEdit"
          />
        </div>
      </div>
    </div>

    <!-- 用戶端 G/W 設定對話框（遷移自舊版 Organization/GatewaySettings） -->
    <GatewaySettingsDialog
      v-model="gatewayProcessType"
      :visible="gatewayVisible"
      :company-name="gatewayCompanyName"
      :loading="gatewayLoading"
      :saving="gatewaySaving"
      :error="gatewayError"
      :process-type-options="processTypeOptions"
      :cert-uploading="certUploading"
      :cert-error="certError"
      :cert-message="certMessage"
      :cert-key-id="certKeyId"
      @close="closeGateway"
      @save="saveGateway"
      @upload-certificate="uploadCertificate($event.file, $event.pin)"
    />

    <!-- 店家 POS 機維護對話框（遷移自舊版 InvoiceBusiness/ApplyPOSDevice） -->
    <POSDeviceDialog
      :visible="posVisible"
      :company-name="posCompanyName"
      :loading="posLoading"
      :saving="posSaving"
      :error="posError"
      :devices="posDevices"
      @close="closePOS"
      @save="savePOSDevice($event.deviceId, $event.posNo)"
      @delete="removePOSDevice"
    />

    <!-- 設定發票經銷商對話框（遷移自舊版 Organization/ApplyIssuerAgent） -->
    <IssuerAgentDialog
      :visible="agencyVisible"
      :company-name="agencyCompanyName"
      :loading="agencyLoading"
      :saving="agencySaving"
      :error="agencyError"
      :agents="agencyAgents"
      @close="closeAgency"
      @save="saveAgency"
    />

    <!-- 客製化服務設定（SMTP）對話框（遷移自舊版 Organization/CustomSettings） -->
    <CustomSmtpDialog
      :visible="customVisible"
      :company-name="customCompanyName"
      :loading="customLoading"
      :saving="customSaving"
      :error="customError"
      :message="customMessage"
      :data="customData"
      @close="closeCustom"
      @save="saveCustom"
      @disable="disableCustom"
    />
  </div>
</template>

<style scoped>
.org-query-page {
  max-width: 1200px;
  margin: 0 auto;
}
.page-header {
  margin-bottom: 1.25rem;
}
.page-header h1 {
  font-size: 1.5rem;
  color: var(--color-text);
  margin: 0;
}
.card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 0.75rem;
  padding: 1.5rem;
  margin-bottom: 1.25rem;
}
.query-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1rem;
}
.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}
/* 所屬經銷商佔滿整列（對應舊版 colspan=3 的全寬呈現） */
.agent-group {
  grid-column: 1 / -1;
}

/* 所屬經銷商 autocomplete */
.autocomplete {
  position: relative;
}
.autocomplete .form-control {
  width: 100%;
  padding-right: 2rem; /* 預留清除按鈕空間 */
}
.autocomplete-clear {
  position: absolute;
  top: 50%;
  right: 0.5rem;
  transform: translateY(-50%);
  background: transparent;
  border: none;
  color: var(--color-muted);
  font-size: 1.15rem;
  line-height: 1;
  cursor: pointer;
  padding: 0;
}
.autocomplete-clear:hover {
  color: var(--color-text);
}
.autocomplete-list {
  position: absolute;
  z-index: 20;
  top: calc(100% + 0.25rem);
  left: 0;
  right: 0;
  margin: 0;
  padding: 0.25rem 0;
  list-style: none;
  max-height: 16rem;
  overflow-y: auto;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 0.5rem;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.35);
}
.autocomplete-item {
  padding: 0.5rem 0.75rem;
  color: var(--color-text);
  cursor: pointer;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.autocomplete-item:hover {
  background: var(--color-accent);
  color: #fff;
}
.autocomplete-empty {
  position: absolute;
  z-index: 20;
  top: calc(100% + 0.25rem);
  left: 0;
  right: 0;
  padding: 0.6rem 0.75rem;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 0.5rem;
  color: var(--color-muted);
  font-size: 0.9rem;
}
.form-group label {
  font-size: 0.85rem;
  font-weight: 500;
  color: var(--color-muted);
}
.query-actions {
  margin-top: 1.25rem;
  display: flex;
  justify-content: flex-end;
  gap: 0.75rem;
}
.batch-toolbar {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}
.batch-count {
  font-size: 0.85rem;
  color: var(--color-muted);
}
/* 設為分支機構：主機構 autocomplete；靠右與批次停用分開 */
.headquarter-picker {
  width: 260px;
  margin-left: auto;
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

.state-msg {
  text-align: center;
  color: var(--color-muted);
  padding: 2.5rem 0;
}
.alert-error {
  color: #ff8585;
  margin-bottom: 1rem;
}
/* 編輯對話框 */
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
</style>
