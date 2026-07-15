<!--
  BusinessRelationshipIndex.vue – 相對營業人資料維護（查詢 + 列內修改 + 列管理動作 + 新增 + 範本下載 / Excel 匯入）。
  遷移自 WebHome BusinessRelationshipController（MaintainRelationship / InquireBusinessRelationship 等）。
  關係以複合鍵（masterId + relativeId + businessId）識別，列表分頁呈現，可就地修改可編輯欄位。
-->
<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { useBusinessRelationshipIndex } from '../composables/useBusinessRelationshipIndex'
import Pager from './Pager.vue'

const {
  // 選項
  groupMembers,
  businessTypeOptions,
  salesBusinessType,
  // 查詢條件
  companyId,
  receiptNo,
  companyName,
  businessType,
  // 列表狀態
  items,
  totalCount,
  page,
  loading,
  error,
  searched,
  totalPages,
  // 列內編輯
  editingKey,
  editCompanyName,
  editContactEmail,
  editAddr,
  editPhone,
  editCustomerNo,
  saving,
  rowKey,
  startEdit,
  cancelEdit,
  saveEdit,
  // 列管理動作
  removeItem,
  toggleActivation,
  toggleEntrusting,
  toggleEntrustToPrint,
  manageUsers,
  // 新增
  addMasterCompanyId,
  addReceiptNo,
  addCompanyName,
  addBusinessType,
  addContactEmail,
  addAddr,
  addPhone,
  addCustomerNo,
  adding,
  addItem,
  // 範本下載 / Excel 匯入
  downloadingTemplate,
  uploading,
  uploadError,
  uploadMessage,
  downloadTemplate,
  uploadCounterpartFile,
  // 事件 / 生命週期
  loadGroupMembers,
  onSearch,
  goToPage,
} = useBusinessRelationshipIndex()

// 隱藏的檔案輸入：點「立即傳送」開啟選檔，選畢即上傳（沿用舊版 FileUpload.cshtml 之隱藏 input 行為）。
const fileInput = ref<HTMLInputElement | null>(null)
function pickFile() {
  fileInput.value?.click()
}
function onFileChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) {
    uploadCounterpartFile(file)
  }
  input.value = ''
}

// 管理下拉選單：紀錄目前展開的列（以複合鍵字串為鍵），點擊外部即關閉。
const openMenuKey = ref<string | null>(null)
function toggleMenu(key: string) {
  openMenuKey.value = openMenuKey.value === key ? null : key
}
function closeMenu() {
  openMenuKey.value = null
}

// 新增卡片展開狀態。
const showAdd = ref(false)

onMounted(() => {
  document.addEventListener('click', closeMenu)
  loadGroupMembers()
})
onUnmounted(() => document.removeEventListener('click', closeMenu))

function entrustingLabel(row: { businessId: number; entrusting: boolean | null }): string {
  return row.businessId === salesBusinessType && row.entrusting === true ? '自動接收' : ''
}
function printLabel(row: { entrustToPrint: boolean | null }): string {
  if (row.entrustToPrint === null) return '未設定'
  return row.entrustToPrint ? '主動列印' : '停用列印'
}
</script>

<template>
  <div class="relationship-page">
    <div class="page-header">
      <h1>相對營業人資料維護</h1>
    </div>

    <!-- 資料維護：範本下載 / Excel 匯入（遷移自舊版維護頁「匯入相對營業人」區塊） -->
    <div class="card maintenance-card">
      <h2 class="card-title">資料維護</h2>
      <div class="maintenance-row">
        <span class="maintenance-label">匯入相對營業人</span>
        <div class="maintenance-actions">
          <button class="btn ghost" :disabled="downloadingTemplate" @click="downloadTemplate">
            <span v-if="!downloadingTemplate">下載範本</span>
            <span v-else>下載中…</span>
          </button>
          <input
            ref="fileInput"
            type="file"
            accept=".xlsx,.xls"
            class="hidden-file"
            @change="onFileChange"
          />
          <button class="btn primary" :disabled="uploading" @click="pickFile">
            <span v-if="uploading">處理中…</span>
            <span v-else>立即傳送</span>
          </button>
        </div>
      </div>
      <p class="maintenance-hint">匯入類別依查詢條件之「營業人類別」，未選則預設為銷項。</p>
      <div v-if="uploadError" class="alert-error maintenance-alert">{{ uploadError }}</div>
      <div v-if="uploadMessage" class="alert-success maintenance-alert">{{ uploadMessage }}</div>
    </div>

    <!-- 查詢條件 -->
    <div class="card query-card">
      <div class="query-grid">
        <div class="form-group">
          <label>集團成員</label>
          <select v-model="companyId" class="form-select">
            <option value="">全部</option>
            <option v-for="m in groupMembers" :key="m.companyId" :value="m.companyId">
              {{ m.receiptNo }} {{ m.companyName }}
            </option>
          </select>
        </div>
        <div class="form-group">
          <label>相對營業人統編</label>
          <input
            v-model="receiptNo"
            type="text"
            class="form-control"
            placeholder="統一編號"
            @keyup.enter="onSearch"
          />
        </div>
        <div class="form-group">
          <label>相對營業人名稱</label>
          <input
            v-model="companyName"
            type="text"
            class="form-control"
            placeholder="營業人名稱"
            @keyup.enter="onSearch"
          />
        </div>
        <div class="form-group">
          <label>營業人類別</label>
          <select v-model="businessType" class="form-select">
            <option value="">全部</option>
            <option v-for="opt in businessTypeOptions" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </option>
          </select>
        </div>
      </div>
      <div class="query-actions">
        <button class="btn ghost" @click="showAdd = !showAdd">
          {{ showAdd ? '收合新增' : '新增相對營業人' }}
        </button>
        <button class="btn primary" :disabled="loading" @click="onSearch">
          <span v-if="!loading">查詢</span>
          <span v-else>查詢中…</span>
        </button>
      </div>
    </div>

    <!-- 新增相對營業人（遷移自舊版 AddItem 列） -->
    <div v-if="showAdd" class="card add-card">
      <h2 class="card-title">新增相對營業人</h2>
      <div class="query-grid">
        <div class="form-group">
          <label>集團成員<span class="req">*</span></label>
          <select v-model="addMasterCompanyId" class="form-select" :disabled="adding">
            <option value="">請選擇</option>
            <option v-for="m in groupMembers" :key="m.companyId" :value="m.companyId">
              {{ m.receiptNo }} {{ m.companyName }}
            </option>
          </select>
        </div>
        <div class="form-group">
          <label>相對營業人統編<span class="req">*</span></label>
          <input v-model="addReceiptNo" type="text" class="form-control" placeholder="統一編號" :disabled="adding" />
        </div>
        <div class="form-group">
          <label>相對營業人名稱<span class="req">*</span></label>
          <input v-model="addCompanyName" type="text" class="form-control" placeholder="營業人名稱" :disabled="adding" />
        </div>
        <div class="form-group">
          <label>營業人類別</label>
          <select v-model.number="addBusinessType" class="form-select" :disabled="adding">
            <option v-for="opt in businessTypeOptions" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </option>
          </select>
        </div>
        <div class="form-group">
          <label>聯絡人電子郵件</label>
          <input v-model="addContactEmail" type="text" class="form-control" placeholder="電子郵件" :disabled="adding" />
        </div>
        <div class="form-group">
          <label>地址</label>
          <input v-model="addAddr" type="text" class="form-control" placeholder="地址" :disabled="adding" />
        </div>
        <div class="form-group">
          <label>電話</label>
          <input v-model="addPhone" type="text" class="form-control" placeholder="電話" :disabled="adding" />
        </div>
        <div class="form-group">
          <label>客戶代碼</label>
          <input v-model="addCustomerNo" type="text" class="form-control" placeholder="客戶代碼" :disabled="adding" />
        </div>
      </div>
      <div class="query-actions">
        <button class="btn primary" :disabled="adding" @click="addItem">
          <span v-if="!adding">新增相對營業人</span>
          <span v-else>新增中…</span>
        </button>
      </div>
    </div>

    <!-- 查詢結果 -->
    <div class="card result-card">
      <div v-if="error" class="alert-error">{{ error }}</div>

      <div v-if="loading" class="state-msg">資料載入中…</div>

      <div v-else class="table-wrap">
        <table class="result-table">
          <thead>
            <tr>
              <th>集團成員</th>
              <th>相對營業人名稱</th>
              <th>統一編號</th>
              <th>類別</th>
              <th>聯絡人電子郵件</th>
              <th>地址</th>
              <th>電話</th>
              <th>客戶代碼</th>
              <th>狀態</th>
              <th class="col-action">管理</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in items" :key="rowKey(row)">
              <td>{{ row.masterName }}</td>

              <!-- 編輯模式：名稱 / 電子郵件 / 地址 / 電話 / 客戶代碼 可修改 -->
              <template v-if="editingKey === rowKey(row)">
                <td><input v-model="editCompanyName" type="text" class="form-control" :disabled="saving" /></td>
                <td>{{ row.receiptNo }}</td>
                <td>{{ row.businessTypeName }}</td>
                <td><input v-model="editContactEmail" type="text" class="form-control" :disabled="saving" /></td>
                <td><input v-model="editAddr" type="text" class="form-control" :disabled="saving" /></td>
                <td><input v-model="editPhone" type="text" class="form-control" :disabled="saving" /></td>
                <td><input v-model="editCustomerNo" type="text" class="form-control" :disabled="saving" /></td>
                <td>{{ row.statusText }}</td>
                <td class="col-action">
                  <div class="row-actions">
                    <button class="btn primary sm" :disabled="saving" @click="saveEdit(row)">
                      <span v-if="!saving">確定</span>
                      <span v-else>儲存中…</span>
                    </button>
                    <button class="btn ghost sm" :disabled="saving" @click="cancelEdit">取消</button>
                  </div>
                </td>
              </template>

              <!-- 檢視模式 -->
              <template v-else>
                <td>{{ row.companyName }}</td>
                <td>{{ row.receiptNo }}</td>
                <td>{{ row.businessTypeName }}</td>
                <td>{{ row.contactEmail }}</td>
                <td>{{ row.addr }}</td>
                <td>{{ row.phone }}</td>
                <td>{{ row.customerNo }}</td>
                <td>
                  <div>{{ row.statusText }}</div>
                  <div v-if="entrustingLabel(row)" class="status-sub">{{ entrustingLabel(row) }}</div>
                  <div class="status-sub">{{ printLabel(row) }}</div>
                </td>
                <td class="col-action">
                  <!-- @click.stop 避免冒泡到 document 的關閉監聽 -->
                  <div class="action-dropdown" @click.stop>
                    <button
                      type="button"
                      class="btn action-toggle"
                      :aria-expanded="openMenuKey === rowKey(row)"
                      @click="toggleMenu(rowKey(row))"
                    >
                      請選擇功能 <span class="caret"></span>
                    </button>
                    <ul v-if="openMenuKey === rowKey(row)" class="action-menu">
                      <li><a @click="closeMenu(); startEdit(row)">修改</a></li>
                      <li v-if="row.deactivated"><a @click="closeMenu(); toggleActivation(row)">啟用</a></li>
                      <li v-else><a @click="closeMenu(); toggleActivation(row)">停用</a></li>
                      <li><a @click="closeMenu(); toggleEntrusting(row)">{{ row.entrusting === true ? '停用自動接收' : '設定自動接收' }}</a></li>
                      <li><a @click="closeMenu(); toggleEntrustToPrint(row)">{{ row.entrustToPrint === true ? '停用列印' : '啟用列印' }}</a></li>
                      <li><a @click="closeMenu(); removeItem(row)">刪除</a></li>
                      <li><a @click="closeMenu(); manageUsers(row)">管理使用者</a></li>
                    </ul>
                  </div>
                </td>
              </template>
            </tr>

            <tr v-if="searched && !items.length" class="empty-row">
              <td colspan="10" class="state-msg">查無資料!!</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- 分頁 -->
      <Pager
        :page="page"
        :total-pages="totalPages"
        :total-count="totalCount"
        @change="goToPage"
      />
    </div>
  </div>
</template>

<style scoped>
.relationship-page {
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
.req {
  color: #ff8585;
}
.query-actions {
  margin-top: 1.25rem;
  display: flex;
  justify-content: flex-end;
  gap: 0.6rem;
}
.card-title {
  font-size: 1.05rem;
  font-weight: 600;
  color: var(--color-text);
  margin: 0 0 1rem;
}
.maintenance-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}
.maintenance-label {
  font-size: 0.95rem;
  font-weight: 500;
  color: var(--color-text);
}
.maintenance-actions {
  display: flex;
  gap: 0.6rem;
  align-items: center;
}
.maintenance-hint {
  margin: 0.75rem 0 0;
  font-size: 0.8rem;
  color: var(--color-muted);
}
.hidden-file {
  display: none;
}
.maintenance-alert {
  margin-top: 1rem;
  margin-bottom: 0;
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
  padding: 0.3rem 0.7rem;
}
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
  vertical-align: middle;
}
.result-table thead th {
  color: var(--color-muted);
  font-weight: 600;
}
.result-table tbody tr:hover {
  background: rgba(59, 199, 255, 0.06);
}
.status-sub {
  font-size: 0.78rem;
  color: var(--color-muted);
}
.col-action {
  min-width: 150px;
}
.row-actions {
  display: flex;
  gap: 0.4rem;
}
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
  min-width: 9rem;
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
.state-msg {
  text-align: center;
  color: var(--color-muted);
  padding: 2.5rem 0;
}
.empty-row td {
  border-bottom: none;
}
.alert-error {
  color: #ff8585;
  margin-bottom: 1rem;
  white-space: pre-line;
}
.alert-success {
  color: #5ad19a;
  margin-bottom: 1rem;
  white-space: pre-line;
}
</style>
