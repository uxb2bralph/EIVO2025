<!--
  InvoiceNumberApplyIndex.vue – 電子發票字軌號碼申請查詢（查詢 + 歸檔 / 轉營業人 / 下載Word）。
  遷移自 WebHome InvoiceNumberApplyController（QueryIndex / Query 頁 + 列動作 MoveFile / TransferOrganization / SetAll）。
  申請資料以檔案系統上的 JSON 檔保存並以清單呈現（不分頁），列以加密後的檔案路徑（keyId）識別。
-->
<script setup lang="ts">
import { useInvoiceNumberApplyIndex } from '../composables/useInvoiceNumberApplyIndex'

const {
  businessId,
  items,
  loading,
  error,
  message,
  searched,
  busyKey,
  onSearch,
  archive,
  transfer,
  downloadWord,
} = useInvoiceNumberApplyIndex()

// 格式化最新填表日（以在地時間顯示）。
function formatDateTime(value: string): string {
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return value
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}/${pad(d.getMonth() + 1)}/${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`
}
</script>

<template>
  <div class="apply-page">
    <div class="page-header">
      <h1>新登錄營業人資料受理</h1>
    </div>

    <!-- 查詢條件 -->
    <div class="card query-card">
      <div class="query-grid">
        <div class="form-group">
          <label>統一編號</label>
          <input
            v-model="businessId"
            type="text"
            class="form-control"
            placeholder="統一編號"
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
      <div v-if="message" class="alert-success">{{ message }}</div>

      <div v-if="loading" class="state-msg">資料載入中…</div>

      <div v-else class="table-wrap">
        <table class="result-table">
          <thead>
            <tr>
              <th>統一編號</th>
              <th>最新填表日</th>
              <th class="col-action">歸檔</th>
              <th class="col-action">轉營業人檔</th>
              <th class="col-action">下載Word</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in items" :key="row.keyId">
              <td>{{ row.businessId }}</td>
              <td>{{ formatDateTime(row.applyUpdateTime) }}</td>
              <td class="col-action">
                <button class="btn ghost sm" :disabled="busyKey === row.keyId" @click="archive(row)">
                  歸檔
                </button>
              </td>
              <td class="col-action">
                <button class="btn ghost sm" :disabled="busyKey === row.keyId" @click="transfer(row)">
                  轉營業人
                </button>
              </td>
              <td class="col-action">
                <button class="btn primary sm" :disabled="busyKey === row.keyId" @click="downloadWord(row)">
                  zip
                </button>
              </td>
            </tr>

            <tr v-if="searched && !items.length" class="empty-row">
              <td colspan="5" class="state-msg">查無資料!!</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<style scoped>
.apply-page {
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
.query-actions {
  margin-top: 1.25rem;
  display: flex;
  justify-content: flex-end;
  gap: 0.6rem;
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
  width: 1%;
  text-align: center;
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
