<!--
  PeriodicalExchangeRateIndex.vue – 期別匯率維護（查詢 + 列內新增 / 修改 / 刪除 + 範本下載 / Excel 匯入）。
  遷移自 WebHome PeriodicalExchangeRateController.Index / Inquire、列管理動作，及 GetExchangeRateSample / UploadExchangeRate。
  查詢一律帶入發票年度；匯率以複合鍵（期別 + 幣別）識別，列表分頁呈現，幣別與匯率可就地編輯，並提供固定新增列。
-->
<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { usePeriodicalExchangeRateIndex } from '../composables/usePeriodicalExchangeRateIndex'
import Pager from './Pager.vue'

const {
  // 選項
  yearOptions,
  periodOptions,
  // 查詢條件
  year,
  periodNo,
  currency,
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
  editCurrency,
  editExchangeRate,
  saving,
  rowKey,
  startEdit,
  cancelEdit,
  saveEdit,
  removeItem,
  // 新增
  addPeriodNo,
  addCurrency,
  addExchangeRate,
  adding,
  addItem,
  // 匯率資料：範本下載 / Excel 匯入
  downloadingSample,
  uploading,
  uploadError,
  uploadMessage,
  downloadSample,
  uploadExchangeRateFile,
  // 事件 / 工具
  onSearch,
  goToPage,
  toRocYear,
  periodLabel,
} = usePeriodicalExchangeRateIndex()

// 隱藏的檔案輸入：點「立即傳送」開啟選檔，選畢即上傳（沿用舊版 UploadExchangeRate.cshtml 之隱藏 input 行為）。
const fileInput = ref<HTMLInputElement | null>(null)
function pickFile() {
  fileInput.value?.click()
}
function onFileChange(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) {
    uploadExchangeRateFile(file)
  }
  // 清空以便重新選擇同一檔案時仍能觸發 change。
  input.value = ''
}

// 管理下拉選單：紀錄目前展開的列（以複合鍵字串為鍵），點擊外部即關閉
const openMenuKey = ref<string | null>(null)
function toggleMenu(key: string) {
  openMenuKey.value = openMenuKey.value === key ? null : key
}
function closeMenu() {
  openMenuKey.value = null
}

onMounted(() => document.addEventListener('click', closeMenu))
onUnmounted(() => document.removeEventListener('click', closeMenu))
</script>

<template>
  <div class="exchange-rate-page">
    <div class="page-header">
      <h1>期別匯率維護</h1>
    </div>

    <!-- 資料維護：匯率資料（範本下載 / Excel 匯入；遷移自舊版 UploadExchangeRate.cshtml） -->
    <div class="card maintenance-card">
      <h2 class="card-title">資料維護</h2>
      <div class="maintenance-row">
        <span class="maintenance-label">匯入匯率資料</span>
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
          <button class="btn primary" :disabled="uploading" @click="pickFile">
            <span v-if="uploading">處理中…</span>
            <span v-else>立即傳送</span>
          </button>
        </div>
      </div>
      <div v-if="uploadError" class="alert-error maintenance-alert">{{ uploadError }}</div>
      <div v-if="uploadMessage" class="alert-success maintenance-alert">{{ uploadMessage }}</div>
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
        <div class="form-group">
          <label>幣別</label>
          <input
            v-model="currency"
            type="text"
            class="form-control"
            placeholder="幣別代碼（如 USD）"
            @keyup.enter="onSearch"
          />
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
              <th>幣別</th>
              <th>匯率</th>
              <th class="col-action">管理</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in items" :key="rowKey(row)">
              <td>{{ toRocYear(row.year) }}</td>
              <td>{{ periodLabel(row.periodNo) }}</td>

              <!-- 編輯模式：幣別與匯率可修改；年度 / 期別維持唯讀 -->
              <template v-if="editingKey === rowKey(row)">
                <td>
                  <input
                    v-model="editCurrency"
                    type="text"
                    class="form-control"
                    placeholder="幣別代碼"
                    :disabled="saving"
                  />
                </td>
                <td>
                  <input
                    v-model.number="editExchangeRate"
                    type="number"
                    step="0.000001"
                    min="0"
                    class="form-control"
                    placeholder="請輸入匯率"
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
                <td>{{ row.currency }}<span v-if="row.currencyName" class="currency-name">（{{ row.currencyName }}）</span></td>
                <td>{{ row.exchangeRate }}</td>
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

          <!-- 新增列（沿用舊版新增功能）：年度固定為目前查詢年度 -->
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
                  v-model="addCurrency"
                  type="text"
                  class="form-control"
                  placeholder="幣別代碼"
                  :disabled="adding"
                />
              </td>
              <td>
                <input
                  v-model.number="addExchangeRate"
                  type="number"
                  step="0.000001"
                  min="0"
                  class="form-control"
                  placeholder="請輸入匯率"
                  :disabled="adding"
                  @keyup.enter="addItem"
                />
              </td>
              <td class="col-action">
                <button class="btn primary sm" :disabled="adding" @click="addItem">
                  <span v-if="!adding">新增匯率</span>
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
.exchange-rate-page {
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
/* 資料維護：匯率資料 */
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
.currency-name {
  color: var(--color-muted);
  font-size: 0.85rem;
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
.alert-success {
  color: #5ad19a;
  margin-bottom: 1rem;
  white-space: pre-line;
}
</style>
