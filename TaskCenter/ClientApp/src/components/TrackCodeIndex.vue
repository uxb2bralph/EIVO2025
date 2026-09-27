<!--
  TrackCodeIndex.vue – 電子發票字軌維護（查詢 + 列內新增 / 修改 / 刪除）。
  遷移自 WebHome TrackCodeController.Index / Inquire 及 CommitItem / DeleteItem；
  另含「上傳發票字軌號碼」匯入區塊（遷移自 InvoiceNo/Module/UploadInvoiceTrackCode.cshtml）。
  查詢一律帶入發票年度；列表以雙月期別呈現，字軌與類別可就地編輯，並提供固定新增列。
-->
<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { useTrackCodeIndex } from '../composables/useTrackCodeIndex'
import { useUploadInvoiceTrackCode } from '../composables/useUploadInvoiceTrackCode'
import Pager from './Pager.vue'

const {
  // 選項
  yearOptions,
  periodOptions,
  invoiceTypeOptions,
  // 查詢條件
  year,
  periodNo,
  // 列表狀態
  items,
  totalCount,
  page,
  loading,
  error,
  searched,
  totalPages,
  // 列內編輯
  editingId,
  editTrackCode,
  editInvoiceType,
  saving,
  startEdit,
  cancelEdit,
  saveEdit,
  removeItem,
  // 新增
  addPeriodNo,
  addTrackCode,
  addInvoiceType,
  adding,
  addItem,
  // 事件 / 工具
  onSearch,
  goToPage,
  toRocYear,
  periodLabel,
  invoiceTypeLabel,
} = useTrackCodeIndex()

// 上傳發票字軌號碼（遷移自 InvoiceNo/Module/UploadInvoiceTrackCode.cshtml）：
// 下載範本 / 立即傳送 → 預覽逐列驗證結果 → 確定上傳。
const {
  rows: uploadRows,
  downloadingSample,
  uploading,
  committing,
  error: uploadError,
  message: uploadMessage,
  hasCommittable,
  downloadSample,
  uploadFile,
  removeRow,
  commitRows,
  uploadPeriodLabel,
  formatInvoiceNo,
} = useUploadInvoiceTrackCode()

const fileInput = ref<HTMLInputElement | null>(null)
function pickFile() {
  fileInput.value?.click()
}
function onFileChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) {
    uploadFile(file)
  }
  // 清空選取值，讓同一檔案可再次選取（沿用舊版每次上傳前 $file.val('')）
  input.value = ''
}

// 管理下拉選單：紀錄目前展開的列（以 trackId 為鍵），點擊外部即關閉
const openMenuId = ref<number | null>(null)
function toggleMenu(trackId: number) {
  openMenuId.value = openMenuId.value === trackId ? null : trackId
}
function closeMenu() {
  openMenuId.value = null
}

onMounted(() => {
  document.addEventListener('click', closeMenu)
  // load()
})
onUnmounted(() => document.removeEventListener('click', closeMenu))
</script>

<template>
  <div class="track-code-page">
    <div class="page-header">
      <h1>電子發票字軌維護</h1>
    </div>

    <!-- 上傳發票字軌號碼（遷移自舊版 InvoiceNo/UploadInvoiceTrackCode 頁面之 Excel 匯入區塊）。
         匯入內容為各營業人之發票號碼配號區間。 -->
    <div class="card upload-card">
      <h2 class="card-title">上傳發票字軌號碼</h2>
      <div class="maintenance-row">
        <span class="maintenance-label">發票字軌號碼</span>
        <div class="maintenance-actions">
          <button class="btn ghost" :disabled="downloadingSample" @click="downloadSample">
            <span v-if="!downloadingSample">下載範本</span>
            <span v-else>下載中…</span>
          </button>
          <input
            ref="fileInput"
            type="file"
            accept=".xlsx,.xlsm"
            class="hidden-file"
            @change="onFileChange"
          />
          <button class="btn primary" :disabled="uploading" @click="pickFile">
            <span v-if="!uploading">立即傳送</span>
            <span v-else>處理中…</span>
          </button>
        </div>
      </div>
      <p class="maintenance-hint">
        欄位順序：營業人統編 / 年份（民國年）/ 發票期別 / 字軌 / 發票起號 / 發票迄號，可先下載範本填寫。
      </p>

      <div v-if="uploadError" class="alert-error maintenance-alert">{{ uploadError }}</div>
      <div v-if="uploadMessage" class="alert-success maintenance-alert">{{ uploadMessage }}</div>

      <!-- 上傳預覽 / 匯入結果（遷移自 Module/PreviewInvoiceTrackCode.cshtml） -->
      <div v-if="uploadRows.length" class="upload-preview">
        <div class="table-wrap">
          <table class="result-table">
            <thead>
              <tr>
                <th>營業人統一編號</th>
                <th>發票年度</th>
                <th>月份</th>
                <th>字軌</th>
                <th>發票號碼起</th>
                <th>發票號碼迄</th>
                <th class="col-action">處理狀態</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(row, idx) in uploadRows" :key="`${row.receiptNo}-${row.startNo}-${idx}`">
                <td>
                  {{ row.receiptNo }}
                  <span v-if="row.companyName" class="upload-company">{{ row.companyName }}</span>
                  <span v-if="row.expirationDate" class="upload-expired">
                    (註記停用:{{ row.expirationDate }})
                  </span>
                </td>
                <td>{{ row.year }}</td>
                <td>{{ uploadPeriodLabel(row.periodNo) }}</td>
                <td>{{ row.trackCode }}</td>
                <td>{{ formatInvoiceNo(row.startNo) }}</td>
                <td>{{ formatInvoiceNo(row.endNo) }}</td>
                <td class="col-action">
                  <!-- 可匯入列提供「刪除」自本次匯入排除（沿用舊版做法，僅移除畫面上的列） -->
                  <button
                    v-if="!row.message"
                    class="btn ghost sm"
                    :disabled="committing"
                    @click="removeRow(idx)"
                  >
                    刪除
                  </button>
                  <span v-else :class="row.committed ? 'upload-ok' : 'upload-error'">
                    {{ row.message }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <div v-if="hasCommittable" class="upload-actions">
          <button class="btn primary" :disabled="committing" @click="commitRows">
            <span v-if="!committing">確定上傳</span>
            <span v-else>上傳中…</span>
          </button>
        </div>
      </div>
    </div>

    <!-- 查詢條件 -->
    <div class="card query-card">
      <div class="query-grid">
        <div class="form-group">
          <label>發票年度（民國年）<span class="req">*</span></label>
          <select v-model.number="year" class="form-select">
            <option v-for="y in yearOptions" :key="y" :value="y">{{ toRocYear(y) }}</option>
          </select>
        </div>
        <div class="form-group">
          <label>期別</label>
          <select v-model="periodNo" class="form-select">
            <option value="">全部</option>
            <option v-for="opt in periodOptions" :key="opt.value" :value="opt.value">
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

      <div v-else class="table-wrap">
        <table class="result-table">
          <thead>
            <tr>
              <th>發票年度</th>
              <th>發票期別</th>
              <th>字軌</th>
              <th>類別</th>
              <th class="col-action">管理</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in items" :key="row.trackId">
              <td>{{ toRocYear(row.year) }}</td>
              <td>{{ periodLabel(row.periodNo) }}</td>

              <!-- 編輯模式：字軌與類別可修改；年度 / 期別維持唯讀 -->
              <template v-if="editingId === row.trackId">
                <td>
                  <input
                    v-model="editTrackCode"
                    type="text"
                    class="form-control"
                    placeholder="請輸入字軌"
                    maxlength="2"
                    :disabled="saving"
                  />
                </td>
                <td>
                  <select v-model.number="editInvoiceType" class="form-select" :disabled="saving">
                    <option v-for="opt in invoiceTypeOptions" :key="opt.value" :value="opt.value">
                      {{ opt.label }}
                    </option>
                  </select>
                </td>
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
                <td>{{ row.trackCode }}</td>
                <td>{{ invoiceTypeLabel(row.invoiceType) }}</td>
                <td class="col-action">
                  <!-- @click.stop 避免冒泡到 document 的關閉監聽 -->
                  <div class="action-dropdown" @click.stop>
                    <button
                      type="button"
                      class="btn action-toggle"
                      :aria-expanded="openMenuId === row.trackId"
                      @click="toggleMenu(row.trackId)"
                    >
                      請選擇功能 <span class="caret"></span>
                    </button>
                    <ul v-if="openMenuId === row.trackId" class="action-menu">
                      <li><a @click="closeMenu(); startEdit(row)">修改</a></li>
                      <li><a @click="closeMenu(); removeItem(row)">刪除</a></li>
                    </ul>
                  </div>
                </td>
              </template>
            </tr>

            <tr v-if="searched && !items.length" class="empty-row">
              <td colspan="5" class="state-msg">查無資料!!</td>
            </tr>
          </tbody>

          <!-- 新增列（沿用舊版 AddItem.cshtml）：年度固定為目前查詢年度 -->
          <tfoot>
            <tr class="add-row">
              <td>{{ toRocYear(year) }}</td>
              <td>
                <select v-model.number="addPeriodNo" class="form-select" :disabled="adding">
                  <option v-for="opt in periodOptions" :key="opt.value" :value="opt.value">
                    {{ opt.label }}
                  </option>
                </select>
              </td>
              <td>
                <input
                  v-model="addTrackCode"
                  type="text"
                  class="form-control"
                  placeholder="請輸入字軌"
                  maxlength="2"
                  :disabled="adding"
                  @keyup.enter="addItem"
                />
              </td>
              <td>
                <select v-model.number="addInvoiceType" class="form-select" :disabled="adding">
                  <option v-for="opt in invoiceTypeOptions" :key="opt.value" :value="opt.value">
                    {{ opt.label }}
                  </option>
                </select>
              </td>
              <td class="col-action">
                <button class="btn primary sm" :disabled="adding" @click="addItem">
                  <span v-if="!adding">新增字軌</span>
                  <span v-else>新增中…</span>
                </button>
              </td>
            </tr>
          </tfoot>
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
.track-code-page {
  max-width: 1000px;
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
.col-action {
  min-width: 150px;
}
.row-actions {
  display: flex;
  gap: 0.4rem;
}
.add-row {
  background: rgba(59, 199, 255, 0.04);
}
.add-row td {
  border-top: 2px solid var(--color-border);
  border-bottom: none;
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
  min-width: 8rem;
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
/* 上傳發票字軌號碼 */
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
.alert-success {
  color: #7ddca4;
  white-space: pre-line;
}
.upload-preview {
  margin-top: 1.25rem;
}
.upload-actions {
  margin-top: 1rem;
  display: flex;
  justify-content: flex-end;
}
.upload-company {
  color: var(--color-muted);
  margin-left: 0.35rem;
}
.upload-expired {
  color: #ffb457;
  margin-left: 0.35rem;
}
.upload-ok {
  color: #7ddca4;
}
.upload-error {
  color: #ff8585;
  white-space: pre-line;
}
</style>
