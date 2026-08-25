<!--
  MonthlyReportIndex.vue — 下載發票月報表（每月統計報表匯出）。
  遷移自 WebHome InvoiceQueryController.MonthlyReport / InquireMonthlyReport
  （選單「下載發票月報表」/InvoiceQuery/MonthlyReport）。
  條件僅開立人 / 代理業者 / 發票日期起迄；按「製作報表」由後端逐月統計後直接下載 Excel
  （舊版為背景產檔 + 輪詢下載）。資料範圍由後端依登入者角色過濾。
-->
<script setup lang="ts">
import { useMonthlyReportIndex } from '../composables/useMonthlyReportIndex'

const {
  isAdmin,
  dateFrom,
  dateTo,
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
  exporting,
  error,
  message,
  createReport,
} = useMonthlyReportIndex()
</script>

<template>
  <div class="invoice-page">
    <div class="page-header">
      <h1>每月統計報表匯出</h1>
    </div>

    <div class="card query-card">
      <h2 class="card-title">查詢條件</h2>

      <div class="query-grid">
        <!-- 開立人 autocomplete -->
        <div class="form-group seller-group" @click.stop>
          <label>開立人統編<span class="required">*</span></label>
          <div class="seller-input">
            <input
              v-model="sellerKeyword"
              type="text"
              class="form-control"
              placeholder="輸入統編或名稱後選取"
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
          <label>代理業者統編<span class="required">*</span></label>
          <div class="seller-input">
            <input
              v-model="agentKeyword"
              type="text"
              class="form-control"
              placeholder="輸入統編或名稱後選取"
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
          <input v-model="dateFrom" type="date" class="form-control" />
        </div>
        <div class="form-group">
          <label>發票日期（迄）<span class="required">*</span></label>
          <input v-model="dateTo" type="date" class="form-control" />
        </div>
      </div>

      <p class="hint">
        開立人與代理業者至少擇一；統計區間以整月為單位（起日所屬月份 1 日 ~ 迄日所屬月份月底）。
      </p>

      <div v-if="error" class="alert-error">{{ error }}</div>
      <div v-if="message" class="alert-info">{{ message }}</div>

      <div class="query-actions">
        <button class="btn primary" :disabled="exporting" @click="createReport">
          {{ exporting ? '產製中…' : '製作報表' }}
        </button>
      </div>
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
.card-title { font-size: 1rem; color: var(--color-text); margin: 0 0 1rem; }
.query-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
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
.hint { margin: 1rem 0 0; font-size: 0.8rem; color: var(--color-muted); }
.query-actions { margin-top: 1.25rem; display: flex; justify-content: flex-end; gap: 0.6rem; flex-wrap: wrap; }
.btn:disabled { opacity: 0.6; cursor: not-allowed; }
.btn.ghost { background: transparent; border: 1px solid var(--color-border); color: var(--color-text); }
.btn.ghost:hover:not(:disabled) { border-color: var(--color-accent); }
.btn.sm { font-size: 0.8rem; padding: 0.3rem 0.7rem; }
.alert-error { color: #ff8585; margin-top: 1rem; white-space: pre-line; }
.alert-info { color: var(--color-accent); margin-top: 1rem; }
</style>
