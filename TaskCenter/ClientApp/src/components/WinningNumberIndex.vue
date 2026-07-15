<!--
  WinningNumberIndex.vue – 中獎號碼維護（查詢 + 列內新增 / 修改 / 刪除 + 發票對獎 / 清除）。
  遷移自 WebHome WinningNumberController.Index / Inquire、列管理動作及 MatchWinningInvoiceNo / ClearWinningInvoiceNo。
  查詢須同時帶入發票年度與期別；列表以獎別排序，頭獎會由後端自動衍生二~六獎（不可直接維護）。
  註：Excel 中獎清冊上傳 / 範本下載暫未遷移。
-->
<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { useWinningNumberIndex } from '../composables/useWinningNumberIndex'

const {
  // 選項
  yearOptions,
  periodOptions,
  rankOptions,
  // 查詢條件
  year,
  periodNo,
  // 列表狀態
  items,
  loading,
  error,
  message,
  searched,
  hasResult,
  // 列內編輯
  editingId,
  editRank,
  editWinningNo,
  saving,
  startEdit,
  cancelEdit,
  saveEdit,
  removeItem,
  // 新增
  addRank,
  addWinningNo,
  adding,
  addItem,
  // 對獎 / 清除
  matching,
  clearing,
  doMatch,
  doClear,
  // 雲端發票中獎清冊：範本下載 / Excel 上傳
  downloadingSample,
  uploading,
  uploadError,
  processing,
  processMessage,
  resultReady,
  resultFileName,
  downloadingResult,
  downloadSample,
  uploadWinningNoFile,
  downloadResult,
  // 事件 / 工具
  onSearch,
  toRocYear,
  periodLabel,
  bonusLabel,
  winningNoLength,
} = useWinningNumberIndex()

// 隱藏的檔案輸入：點「立即傳送」開啟選檔，選畢即上傳（沿用舊版 FileUpload.cshtml 之隱藏 input 行為）。
const fileInput = ref<HTMLInputElement | null>(null)
function pickFile() {
  fileInput.value?.click()
}
function onFileChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) {
    uploadWinningNoFile(file)
  }
  // 清空以便重新選擇同一檔案時仍能觸發 change。
  input.value = ''
}

// 管理下拉選單：紀錄目前展開的列（以 winningId 為鍵），點擊外部即關閉
const openMenuId = ref<number | null>(null)
function toggleMenu(winningId: number) {
  openMenuId.value = openMenuId.value === winningId ? null : winningId
}
function closeMenu() {
  openMenuId.value = null
}

onMounted(() => document.addEventListener('click', closeMenu))
onUnmounted(() => document.removeEventListener('click', closeMenu))
</script>

<template>
  <div class="winning-number-page">
    <div class="page-header">
      <h1>中獎號碼維護</h1>
    </div>
    <!-- 資料維護：雲端發票中獎清冊（範本下載 / Excel 上傳；遷移自舊版 WinningNoQuery.cshtml L22-L45） -->
    <div class="card maintenance-card">
      <h2 class="card-title">資料維護</h2>
      <div class="maintenance-row">
        <span class="maintenance-label">雲端發票中獎清冊</span>
        <div class="maintenance-actions">
          <button class="btn ghost" :disabled="downloadingSample" @click="downloadSample">
            <span v-if="!downloadingSample">下載範本</span>
            <span v-else>下載中…</span>
          </button>
          <input
            ref="fileInput"
            type="file"
            accept=".xlsx,.xls"
            class="hidden-file"
            @change="onFileChange"
          />
          <button class="btn primary" :disabled="uploading || processing" @click="pickFile">
            <span v-if="uploading">上傳中…</span>
            <span v-else-if="processing">處理中…</span>
            <span v-else>立即傳送</span>
          </button>
        </div>
      </div>

      <!-- 上傳 / 處理狀態 -->
      <div v-if="uploadError" class="alert-error maintenance-alert">{{ uploadError }}</div>
      <div v-if="processing" class="alert-info maintenance-alert">
        下載資料準備中，您可以繼續等待，完成後即可下載結果。
      </div>
      <div v-if="processMessage" class="alert-warning maintenance-alert">{{ processMessage }}</div>
      <div v-if="resultReady" class="result-download">
        <span class="ready-text">處理完成！</span>
        <button class="btn primary sm" :disabled="downloadingResult" @click="downloadResult">
          <span v-if="!downloadingResult">下載結果（{{ resultFileName }}）</span>
          <span v-else>下載中…</span>
        </button>
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
          <label>發票期別<span class="req">*</span></label>
          <select v-model.number="periodNo" class="form-select">
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
    <div v-if="searched" class="card result-card">
      <div v-if="error" class="alert-error">{{ error }}</div>
      <div v-if="message" class="alert-success">{{ message }}</div>

      <div v-if="loading" class="state-msg">資料載入中…</div>

      <div v-else class="table-wrap">
        <table class="result-table">
          <thead>
            <tr>
              <th>發票年度</th>
              <th>發票期別</th>
              <th>獎別</th>
              <th>中獎金額</th>
              <th>中獎號碼</th>
              <th class="col-action">管理</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in items" :key="row.winningId">
              <td>{{ toRocYear(row.year) }}</td>
              <td>{{ periodLabel(row.period) }}</td>

              <!-- 編輯模式：獎別與號碼可修改（年度 / 期別維持唯讀） -->
              <template v-if="editingId === row.winningId">
                <td>
                  <select v-model.number="editRank" class="form-select" :disabled="saving">
                    <option v-for="opt in rankOptions" :key="opt.value" :value="opt.value">
                      {{ opt.label }}
                    </option>
                  </select>
                </td>
                <td>{{ bonusLabel(row.bonus) }}</td>
                <td>
                  <input
                    v-model="editWinningNo"
                    type="text"
                    class="form-control"
                    placeholder="請輸入中獎號碼"
                    :maxlength="winningNoLength(editRank)"
                    :disabled="saving"
                  />
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
                <td>{{ row.prizeType }}</td>
                <td>{{ bonusLabel(row.bonus) }}</td>
                <td>{{ row.winningNo }}</td>
                <td class="col-action">
                  <!-- 僅可維護獎別（特別獎 / 特獎 / 頭獎 / 增開六獎）顯示管理選單；衍生獎項不可維護 -->
                  <div v-if="row.editable" class="action-dropdown" @click.stop>
                    <button
                      type="button"
                      class="btn action-toggle"
                      :aria-expanded="openMenuId === row.winningId"
                      @click="toggleMenu(row.winningId)"
                    >
                      請選擇功能 <span class="caret"></span>
                    </button>
                    <ul v-if="openMenuId === row.winningId" class="action-menu">
                      <li><a @click="closeMenu(); startEdit(row)">修改</a></li>
                      <li><a @click="closeMenu(); removeItem(row)">刪除</a></li>
                    </ul>
                  </div>
                </td>
              </template>
            </tr>

            <tr v-if="!items.length" class="empty-row">
              <td colspan="6" class="state-msg">查無資料!!</td>
            </tr>
          </tbody>

          <!-- 新增列（沿用舊版 AddItem.cshtml）：年度 / 期別固定為目前查詢條件 -->
          <tfoot>
            <tr class="add-row">
              <td>{{ toRocYear(year) }}</td>
              <td>{{ periodLabel(periodNo) }}</td>
              <td>
                <select v-model.number="addRank" class="form-select" :disabled="adding">
                  <option v-for="opt in rankOptions" :key="opt.value" :value="opt.value">
                    {{ opt.label }}
                  </option>
                </select>
              </td>
              <td></td>
              <td>
                <input
                  v-model="addWinningNo"
                  type="text"
                  class="form-control"
                  placeholder="請輸入中獎號碼"
                  :maxlength="winningNoLength(addRank)"
                  :disabled="adding"
                  @keyup.enter="addItem"
                />
              </td>
              <td class="col-action">
                <button class="btn primary sm" :disabled="adding" @click="addItem">
                  <span v-if="!adding">新增中獎號碼</span>
                  <span v-else>新增中…</span>
                </button>
              </td>
            </tr>
          </tfoot>
        </table>
      </div>

      <!-- 對獎作業（沿用舊版 DoMatch.cshtml，僅在有中獎號碼時顯示） -->
      <div v-if="hasResult && !loading" class="result-actions">
        <button class="btn primary" :disabled="matching || clearing" @click="doMatch">
          <span v-if="!matching">執行發票對獎</span>
          <span v-else>對獎中…</span>
        </button>
        <button class="btn danger" :disabled="matching || clearing" @click="doClear">
          <span v-if="!clearing">清除中獎發票</span>
          <span v-else>清除中…</span>
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.winning-number-page {
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
/* 資料維護：雲端發票中獎清冊 */
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
.hidden-file {
  display: none;
}
.maintenance-alert {
  margin-top: 1rem;
  margin-bottom: 0;
}
.alert-info {
  color: var(--color-accent);
  white-space: pre-line;
}
.alert-warning {
  color: #e0a955;
  white-space: pre-line;
}
.result-download {
  margin-top: 1rem;
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
}
.ready-text {
  color: #5ad19a;
  font-weight: 500;
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
.btn.danger {
  background: #e05555;
  border: 1px solid #e05555;
  color: #fff;
}
.btn.danger:hover:not(:disabled) {
  background: #cc4646;
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
.result-actions {
  margin-top: 1.25rem;
  display: flex;
  gap: 0.75rem;
  justify-content: flex-end;
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
