<!-- OrganizationQueryIndex.vue – 營業人資料管理（查詢）。遷移自 WebHome OrganizationQuery/InquireOrganization。 -->
<script setup lang="ts">
import { useOrganizationQueryIndex } from '../composables/useOrganizationQueryIndex'
import OrganizationEditForm from './OrganizationEditForm.vue'
import GatewaySettingsDialog from './GatewaySettingsDialog.vue'
import POSDeviceDialog from './POSDeviceDialog.vue'
import IssuerAgentDialog from './IssuerAgentDialog.vue'
import OrganizationResultTable from './OrganizationResultTable.vue'
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
  // 列表狀態
  items,
  totalCount,
  page,
  loading,
  error,
  searched,
  totalPages,
  hasPrev,
  hasNext,
  pageNumbers,
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
  prevPage,
  nextPage,
  goToPage,
} = useOrganizationQueryIndex()

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
      </div>
      <div class="query-actions">
        <button class="btn primary" :disabled="loading" @click="onSearch">
          <span v-if="!loading">查詢</span>
          <span v-else>查詢中…</span>
        </button>
      </div>
    </div>

    <!-- 查詢結果 -->
    <div class="card result-card">
      <div v-if="error" class="alert-error">{{ error }}</div>

      <div v-if="loading" class="state-msg">資料載入中…</div>

      <OrganizationResultTable
        v-else-if="items.length"
        :items="items"
        :open-menu-id="openMenuId"
        :format-date="formatDate"
        @toggle-menu="toggleMenu"
        @action="onRowAction"
      />

      <div v-else-if="searched" class="state-msg">查無資料!!</div>

      <!-- 分頁 -->
      <nav v-if="totalPages > 1" class="pager">
        <button class="btn ghost" :disabled="!hasPrev" @click="prevPage">上一頁</button>
        <span class="page-numbers">
          <button
            v-for="p in pageNumbers"
            :key="p"
            class="btn ghost page-num"
            :class="{ active: p === page }"
            @click="goToPage(p)"
          >
            {{ p }}
          </button>
        </span>
        <button class="btn ghost" :disabled="!hasNext" @click="nextPage">下一頁</button>
        <span class="pager-info">
          第 {{ page }} / {{ totalPages }} 頁（共 {{ totalCount }} 筆）
        </span>
      </nav>
    </div>

    <!-- 編輯營業人對話框（遷移自舊版 editOrganization tab） -->
    <div v-if="editVisible" class="eivo-modal-overlay">
      <div class="eivo-modal-dialog">
        <div class="eivo-modal-header">
          <h2>編輯營業人資料</h2>
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
.form-group label {
  font-size: 0.85rem;
  font-weight: 500;
  color: var(--color-muted);
}
.query-actions {
  margin-top: 1.25rem;
  display: flex;
  justify-content: flex-end;
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
.pager {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  margin-top: 1.25rem;
  padding-top: 1.25rem;
  border-top: 1px solid var(--color-border);
}
.pager-info {
  color: var(--color-muted);
  font-size: 0.9rem;
}
.page-numbers {
  display: flex;
  align-items: center;
  gap: 0.4rem;
}
.page-num {
  min-width: 2.25rem;
  padding: 0.4rem 0.6rem;
  text-align: center;
}
.page-num.active {
  background: var(--color-accent);
  border-color: var(--color-accent);
  color: #fff;
  cursor: default;
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
