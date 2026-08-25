<!--
  InvoiceSummaryIndex.vue — 發票統計表。
  遷移自 WebHome InvoiceQueryController.InvoiceSummary / InquireSummary / CreateMonthlyReportXlsx
  （選單「發票統計表」/InvoiceQuery/InvoiceSummary）。
  以發票查詢條件過濾後，依「開立發票營業人」彙總資料筆數 + 分頁 + 排序 + 下載月報表 Excel。
  發票日期起迄為必填；資料範圍由後端依登入者角色過濾（系統管理看全部、開立人看自家、代理看旗下）。
-->
<script setup lang="ts">
import { useInvoiceSummaryIndex } from '../composables/useInvoiceSummaryIndex'
import Pager from './Pager.vue'

const {
  isAdmin,
  carrierTypeOptions,
  businessTypeOptions,
  filters,
  sellerKeyword,
  sellerOptions,
  selectedSellerKey,
  onSellerInput,
  runSellerSearch,
  pickSeller,
  clearSeller,
  agentKeyword,
  agentOptions,
  selectedAgentKey,
  onAgentInput,
  runAgentSearch,
  pickAgent,
  clearAgent,
  items,
  totalCount,
  page,
  loading,
  error,
  searched,
  totalPages,
  onSearch,
  goToPage,
  sortName,
  sortType,
  toggleSort,
  exporting,
  doExportMonthlyReport,
  formatDate,
  formatAmount,
} = useInvoiceSummaryIndex()

function sortArrow(key: string): string {
  if (sortName.value !== key || !sortType.value) return ''
  return sortType.value === 1 ? ' ▲' : ' ▼'
}
</script>

<template>
  <div class="invoice-page">
    <div class="page-header">
      <h1>發票統計表</h1>
    </div>

    <!-- 查詢條件 -->
    <div class="card query-card">
      <div class="query-grid">
        <div class="form-group">
          <label>交易類型</label>
          <select v-model="filters.businessType" class="form-select">
            <option value="">全部</option>
            <option v-for="opt in businessTypeOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
          </select>
        </div>

        <!-- 開立人 autocomplete -->
        <div class="form-group seller-group" @click.stop>
          <label>開立人統編</label>
          <div class="seller-input">
            <input
              v-model="sellerKeyword"
              type="text"
              class="form-control"
              placeholder="輸入統編或名稱（空白=全部）"
              @input="onSellerInput"
              @keyup.enter="runSellerSearch"
            />
            <button v-if="selectedSellerKey" class="btn ghost sm" @click="clearSeller">清除</button>
          </div>
          <ul v-if="sellerOptions.length" class="seller-menu">
            <li v-for="opt in sellerOptions" :key="opt.sellerKey ?? ''">
              <a @click="pickSeller(opt)">{{ opt.receiptNo }} {{ opt.companyName }}</a>
            </li>
          </ul>
        </div>

        <!-- 代理業者（僅系統管理） -->
        <div v-if="isAdmin" class="form-group seller-group" @click.stop>
          <label>代理業者統編</label>
          <div class="seller-input">
            <input
              v-model="agentKeyword"
              type="text"
              class="form-control"
              placeholder="輸入統編或名稱（空白=全部）"
              @input="onAgentInput"
              @keyup.enter="runAgentSearch"
            />
            <button v-if="selectedAgentKey" class="btn ghost sm" @click="clearAgent">清除</button>
          </div>
          <ul v-if="agentOptions.length" class="seller-menu">
            <li v-for="opt in agentOptions" :key="opt.agentKey ?? ''">
              <a @click="pickAgent(opt)">{{ opt.receiptNo }} {{ opt.companyName }}</a>
            </li>
          </ul>
        </div>

        <div class="form-group">
          <label>發票日期（起）<span class="required">*</span></label>
          <input v-model="filters.dateFrom" type="date" class="form-control" />
        </div>
        <div class="form-group">
          <label>發票日期（迄）<span class="required">*</span></label>
          <input v-model="filters.dateTo" type="date" class="form-control" />
        </div>

        <div class="form-group">
          <label>買受人統編</label>
          <input v-model="filters.buyerReceiptNo" type="text" maxlength="11" class="form-control"
                 placeholder="個人發票請輸入 0000000000" @keyup.enter="onSearch" />
        </div>
        <div class="form-group">
          <label>買受人名稱</label>
          <input v-model="filters.buyerName" type="text" class="form-control" @keyup.enter="onSearch" />
        </div>
        <div class="form-group">
          <label>客戶 ID</label>
          <input v-model="filters.customerId" type="text" class="form-control" @keyup.enter="onSearch" />
        </div>

        <div class="form-group">
          <label>發票號碼（起）</label>
          <input v-model="filters.invoiceNo" type="text" maxlength="12" class="form-control" @keyup.enter="onSearch" />
        </div>
        <div class="form-group">
          <label>發票號碼（迄）</label>
          <input v-model="filters.endNo" type="text" maxlength="12" class="form-control" @keyup.enter="onSearch" />
        </div>

        <div class="form-group">
          <label>附件檔</label>
          <select v-model="filters.attachment" class="form-select">
            <option value="">全部</option>
            <option :value="1">有</option>
            <option :value="0">無</option>
          </select>
        </div>
        <div class="form-group">
          <label>是否中獎</label>
          <select v-model="filters.winning" class="form-select">
            <option value="">全部</option>
            <option :value="1">是</option>
          </select>
        </div>
        <div class="form-group">
          <label>單據狀態</label>
          <select v-model="filters.cancelled" class="form-select">
            <option value="">全部</option>
            <option value="false">未作廢</option>
            <option value="true">已作廢</option>
          </select>
        </div>
        <div class="form-group">
          <label>列印註記</label>
          <select v-model="filters.printMark" class="form-select">
            <option value="">全部</option>
            <option value="Y">Y</option>
            <option value="N">N</option>
          </select>
        </div>
        <div class="form-group">
          <label>列印狀態</label>
          <select v-model="filters.printed" class="form-select">
            <option value="">全部</option>
            <option value="false">未列印</option>
            <option value="true">已列印</option>
          </select>
        </div>
        <div class="form-group">
          <label>載具類型</label>
          <select v-model="filters.carrierType" class="form-select">
            <option value="">全部</option>
            <option v-for="opt in carrierTypeOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
          </select>
        </div>
        <div class="form-group">
          <label>載具號碼</label>
          <input v-model="filters.carrierNo" type="text" maxlength="48" class="form-control" @keyup.enter="onSearch" />
        </div>
      </div>

      <div class="query-actions">
        <button class="btn ghost" :disabled="exporting" @click="doExportMonthlyReport">
          {{ exporting ? '產製中…' : '下載月報表' }}
        </button>
        <button class="btn primary" :disabled="loading" @click="onSearch">
          {{ loading ? '查詢中…' : '查詢' }}
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
              <th class="sortable" @click="toggleSort('CompanyName')">營業人名稱{{ sortArrow('CompanyName') }}</th>
              <th class="sortable" @click="toggleSort('ReceiptNo')">統一編號{{ sortArrow('ReceiptNo') }}</th>
              <th class="num">資料筆數</th>
              <th>註記停用</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in items" :key="row.sellerKey ?? row.receiptNo ?? ''">
              <td>{{ row.companyName }}</td>
              <td>{{ row.receiptNo }}</td>
              <td class="num">{{ formatAmount(row.invoiceCount) }}</td>
              <td>{{ formatDate(row.expirationDate) }}</td>
            </tr>
            <tr v-if="searched && !items.length" class="empty-row">
              <td colspan="4" class="state-msg">查無資料!!</td>
            </tr>
          </tbody>
        </table>
      </div>

      <Pager :page="page" :total-pages="totalPages" :total-count="totalCount" @change="goToPage" />
    </div>
  </div>
</template>

<style scoped>
.invoice-page { max-width: 1200px; margin: 0 auto; }
.page-header { margin-bottom: 1.25rem; }
.page-header h1 { font-size: 1.5rem; color: var(--color-text); margin: 0; }
.card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 0.75rem;
  padding: 1.5rem;
  margin-bottom: 1.25rem;
}
.query-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}
.form-group { display: flex; flex-direction: column; gap: 0.4rem; position: relative; }
.form-group label { font-size: 0.85rem; font-weight: 500; color: var(--color-muted); }
.required { color: #ff8585; margin-left: 0.2rem; }
.seller-input { display: flex; gap: 0.4rem; }
.seller-input .form-control { flex: 1; }
.seller-menu {
  position: absolute; top: 100%; left: 0; right: 0; z-index: 20;
  margin: 0.25rem 0 0; padding: 0.25rem 0; list-style: none;
  max-height: 260px; overflow-y: auto;
  background: var(--color-surface); border: 1px solid var(--color-border);
  border-radius: 0.5rem; box-shadow: 0 6px 18px rgba(0, 0, 0, 0.25);
}
.seller-menu li a { display: block; padding: 0.45rem 0.9rem; font-size: 0.85rem; color: var(--color-text); cursor: pointer; }
.seller-menu li a:hover { background: rgba(59, 199, 255, 0.12); color: var(--color-accent); }
.query-actions { margin-top: 1.25rem; display: flex; justify-content: flex-end; gap: 0.6rem; flex-wrap: wrap; }
.btn:disabled { opacity: 0.6; cursor: not-allowed; }
.btn.ghost { background: transparent; border: 1px solid var(--color-border); color: var(--color-text); }
.btn.ghost:hover:not(:disabled) { border-color: var(--color-accent); }
.btn.sm { font-size: 0.8rem; padding: 0.3rem 0.7rem; }
.table-wrap { overflow-x: auto; }
.result-table { width: 100%; border-collapse: collapse; color: var(--color-text); }
.result-table th, .result-table td {
  padding: 0.6rem 0.75rem; text-align: left; border-bottom: 1px solid var(--color-border);
  white-space: nowrap; font-size: 0.85rem; vertical-align: middle;
}
.result-table th.num, .result-table td.num { text-align: right; }
.result-table thead th { color: var(--color-muted); font-weight: 600; }
.result-table th.sortable { cursor: pointer; user-select: none; }
.result-table th.sortable:hover { color: var(--color-accent); }
.result-table tbody tr:hover { background: rgba(59, 199, 255, 0.06); }
.state-msg { text-align: center; color: var(--color-muted); padding: 2.5rem 0; }
.empty-row td { border-bottom: none; }
.alert-error { color: #ff8585; margin-bottom: 1rem; white-space: pre-line; }
</style>
