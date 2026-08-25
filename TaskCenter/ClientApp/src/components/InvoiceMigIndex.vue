<!--
  InvoiceMigIndex.vue — 下載MIG檔案（MIG 格式下載）。
  遷移自 WebHome InvoiceProcessController.InquireToMIG（選單「下載MIG檔案」/InvoiceProcess/InquireToMIG）。
  原版 = Index 查詢頁（ResultAction=CreateMIG，查詢畫面 InvoiceQueryForMIG.cshtml）
        + 逐列勾選 + 下載F0401／下載F0701／下載F0501（DownloadMIG.cshtml）。
  查詢重用 useInvoiceProcessIndex；下載對象由後端依登入者角色範圍過濾。
-->
<script setup lang="ts">
import { ref, watch } from 'vue'
import { useInvoiceProcessIndex } from '../composables/useInvoiceProcessIndex'
import { useTableSelection } from '../composables/useTableSelection'
import { downloadMig, type MigDocType } from '@/services/invoice-mig-api'
import type { InvoiceItemDatatable } from '@/services/invoice-process-api'
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
  detailOpen,
  detail,
  detailLoading,
  detailError,
  openDetail,
  closeDetail,
  formatDate,
  formatAmount,
} = useInvoiceProcessIndex()

function sortArrow(key: string): string {
  if (sortName.value !== key || !sortType.value) return ''
  return sortType.value === 1 ? ' ▲' : ' ▼'
}

// ── 列勾選 ──────────────────────────────────────────────────
// 以 keyId（加密 InvoiceID）為選取鍵；無 keyId 者不可選。
function selectableKey(row: InvoiceItemDatatable): string | null {
  return row.keyId
}
const { selected, allChecked, indeterminate, toggleAll, toggle, isChecked, clear } =
  useTableSelection<InvoiceItemDatatable, string>(items, selectableKey)

// 換頁 / 重新查詢後清空選取，避免殘留舊 key。
watch(items, () => clear())

function onToggleAll(e: Event) {
  toggleAll((e.target as HTMLInputElement).checked)
}
function onToggleItem(row: InvoiceItemDatatable, e: Event) {
  toggle(row, (e.target as HTMLInputElement).checked)
}

// ── MIG 下載 ────────────────────────────────────────────────
const downloading = ref<MigDocType | ''>('')
const downloadError = ref('')

function triggerBlobDownload(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  document.body.removeChild(a)
  URL.revokeObjectURL(url)
}

async function onDownload(docType: MigDocType) {
  const keys = [...selected.value]
  if (!keys.length) {
    window.alert('請選擇下載資料!!')
    return
  }

  downloading.value = docType
  downloadError.value = ''
  try {
    const blob = await downloadMig(docType, keys)
    triggerBlobDownload(blob, `${docType}.zip`)
  } catch (e) {
    downloadError.value = e instanceof Error ? e.message : `${docType} 下載失敗`
  } finally {
    downloading.value = ''
  }
}
</script>

<template>
  <div class="invoice-page">
    <div class="page-header">
      <h1>MIG 格式下載</h1>
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
          <label>發票日期（起）</label>
          <input v-model="filters.dateFrom" type="date" class="form-control" />
        </div>
        <div class="form-group">
          <label>發票日期（迄）</label>
          <input v-model="filters.dateTo" type="date" class="form-control" />
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
          <label>單據號碼</label>
          <input v-model="filters.dataNo" type="text" maxlength="64" class="form-control" @keyup.enter="onSearch" />
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
        <button class="btn primary" :disabled="loading" @click="onSearch">
          {{ loading ? '查詢中…' : '查詢' }}
        </button>
      </div>
    </div>

    <!-- 查詢結果 -->
    <div class="card result-card">
      <div v-if="error" class="alert-error">{{ error }}</div>
      <div v-if="downloadError" class="alert-error">{{ downloadError }}</div>
      <div v-if="loading" class="state-msg">資料載入中…</div>

      <div v-else>
        <div class="batch-bar">
          <span class="batch-count">已選 {{ selected.length }} 筆</span>
          <button class="btn ghost sm" :disabled="!selected.length || !!downloading" @click="onDownload('F0401')">
            {{ downloading === 'F0401' ? '下載中…' : '下載F0401' }}
          </button>
          <button class="btn ghost sm" :disabled="!selected.length || !!downloading" @click="onDownload('F0701')">
            {{ downloading === 'F0701' ? '下載中…' : '下載F0701' }}
          </button>
          <!-- 原版僅於查詢條件「已作廢」時顯示 F0501（作廢 MIG 僅已作廢發票適用） -->
          <button
            v-if="filters.cancelled === 'true'"
            class="btn ghost sm"
            :disabled="!selected.length || !!downloading"
            @click="onDownload('F0501')"
          >
            {{ downloading === 'F0501' ? '下載中…' : '下載F0501' }}
          </button>
        </div>
        <div class="table-wrap">
        <table class="result-table">
          <thead>
            <tr>
              <th class="check-col">
                <input
                  type="checkbox"
                  :checked="allChecked"
                  :indeterminate="indeterminate"
                  @change="onToggleAll"
                />
              </th>
              <th class="sortable" @click="toggleSort('CompanyName')">開立發票營業人{{ sortArrow('CompanyName') }}</th>
              <th class="sortable" @click="toggleSort('ReceiptNo')">營業人統編{{ sortArrow('ReceiptNo') }}</th>
              <th class="sortable" @click="toggleSort('InvoiceNo')">發票號碼{{ sortArrow('InvoiceNo') }}</th>
              <th>傳輸類型</th>
              <th class="sortable" @click="toggleSort('InvoiceDate')">日期{{ sortArrow('InvoiceDate') }}</th>
              <th>買受人名稱</th>
              <th class="sortable" @click="toggleSort('BuyerNo')">買受人統編{{ sortArrow('BuyerNo') }}</th>
              <th class="sortable" @click="toggleSort('OrderNo')">序號{{ sortArrow('OrderNo') }}</th>
              <th>發票狀態</th>
              <th>大平台狀態</th>
              <th>幣別</th>
              <th class="num sortable" @click="toggleSort('SalesAmount')">未稅金額{{ sortArrow('SalesAmount') }}</th>
              <th>稅別</th>
              <th class="num sortable" @click="toggleSort('TaxAmount')">稅額{{ sortArrow('TaxAmount') }}</th>
              <th class="num sortable" @click="toggleSort('TotalAmount')">含稅金額{{ sortArrow('TotalAmount') }}</th>
              <th>備註</th>
              <th>載具號碼</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in items" :key="row.invoiceId" :class="{ 'row-void': row.isCancelled }">
              <td class="check-col">
                <input
                  type="checkbox"
                  :checked="isChecked(row)"
                  :disabled="!row.keyId"
                  @change="onToggleItem(row, $event)"
                />
              </td>
              <td>{{ row.sellerName }}</td>
              <td>{{ row.sellerReceiptNo }}</td>
              <td><a class="link" @click="openDetail(row.keyId)">{{ row.invoiceNo }}</a></td>
              <td>{{ row.transTypeLabel }}</td>
              <td>{{ formatDate(row.invoiceDate) }}</td>
              <td>{{ row.buyerName }}</td>
              <td>{{ row.buyerReceiptNo }}</td>
              <td>{{ row.orderNo }}</td>
              <td :class="{ 'text-void': row.isCancelled }">{{ row.statusLabel }}</td>
              <td>{{ row.migStatus }}</td>
              <td>{{ row.currency }}</td>
              <td class="num">{{ formatAmount(row.salesAmount) }}</td>
              <td>{{ row.taxTypeLabel }}</td>
              <td class="num">{{ formatAmount(row.taxAmount) }}</td>
              <td class="num">{{ formatAmount(row.totalAmount) }}</td>
              <td>{{ row.remark }}</td>
              <td>{{ row.carrierNo }}</td>
            </tr>
            <tr v-if="searched && !items.length" class="empty-row">
              <td colspan="18" class="state-msg">查無資料!!</td>
            </tr>
          </tbody>
        </table>
        </div>
      </div>

      <Pager :page="page" :total-pages="totalPages" :total-count="totalCount" @change="goToPage" />
    </div>

    <!-- 發票明細 Modal -->
    <div v-if="detailOpen" class="modal-backdrop" @click.self="closeDetail">
      <div class="modal-box">
        <div class="modal-header">
          <h3>發票明細</h3>
          <button class="btn ghost sm" @click="closeDetail">✕</button>
        </div>
        <div class="modal-body">
          <div v-if="detailLoading" class="state-msg">載入中…</div>
          <div v-else-if="detailError" class="alert-error">{{ detailError }}</div>
          <template v-else-if="detail">
            <div class="detail-grid">
              <div><span class="lbl">發票號碼</span>{{ detail.invoiceNo }}</div>
              <div><span class="lbl">發票日期</span>{{ formatDate(detail.invoiceDate) }}</div>
              <div><span class="lbl">隨機碼</span>{{ detail.randomNo }}</div>
              <div><span class="lbl">狀態</span>{{ detail.statusLabel }}</div>
              <div><span class="lbl">開立人</span>{{ detail.sellerName }}（{{ detail.sellerReceiptNo }}）</div>
              <div><span class="lbl">買受人</span>{{ detail.buyerName }}（{{ detail.buyerReceiptNo }}）</div>
              <div><span class="lbl">買受人地址</span>{{ detail.buyerAddress }}</div>
              <div><span class="lbl">買受人 email</span>{{ detail.buyerEmail }}</div>
              <div><span class="lbl">載具</span>{{ detail.carrierType }} {{ detail.carrierNo }}</div>
              <div><span class="lbl">愛心碼</span>{{ detail.agencyCode }}</div>
            </div>

            <table class="result-table detail-lines">
              <thead>
                <tr><th>#</th><th>品名</th><th class="num">數量</th><th>單位</th><th class="num">單價</th><th class="num">金額</th><th>備註</th></tr>
              </thead>
              <tbody>
                <tr v-for="line in detail.lines" :key="line.seq">
                  <td>{{ line.seq }}</td>
                  <td>{{ line.description }}</td>
                  <td class="num">{{ formatAmount(line.quantity) }}</td>
                  <td>{{ line.unit }}</td>
                  <td class="num">{{ formatAmount(line.unitPrice) }}</td>
                  <td class="num">{{ formatAmount(line.amount) }}</td>
                  <td>{{ line.remark }}</td>
                </tr>
                <tr v-if="!detail.lines.length"><td colspan="7" class="state-msg">無品項資料</td></tr>
              </tbody>
            </table>

            <div class="detail-amounts">
              <span>未稅：{{ formatAmount(detail.salesAmount) }}</span>
              <span>稅額：{{ formatAmount(detail.taxAmount) }}</span>
              <span>含稅：{{ formatAmount(detail.totalAmount) }}</span>
              <span>幣別：{{ detail.currency }}</span>
              <span>稅別：{{ detail.taxTypeLabel }}</span>
            </div>
          </template>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.invoice-page { max-width: 1400px; margin: 0 auto; }
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
.batch-bar { display: flex; align-items: center; gap: 0.75rem; margin-bottom: 0.75rem; flex-wrap: wrap; }
.batch-count { font-size: 0.85rem; color: var(--color-muted); }
.table-wrap { overflow-x: auto; }
.result-table th.check-col, .result-table td.check-col { width: 1%; text-align: center; padding-right: 0.4rem; }
.result-table td.check-col input, .result-table th.check-col input { cursor: pointer; }
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
.result-table tbody tr.row-void { opacity: 0.6; }
.link { color: var(--color-accent); cursor: pointer; }
.link:hover { text-decoration: underline; }
.text-void { color: #ff8585; }
.state-msg { text-align: center; color: var(--color-muted); padding: 2.5rem 0; }
.empty-row td { border-bottom: none; }
.alert-error { color: #ff8585; margin-bottom: 1rem; white-space: pre-line; }

/* 明細 Modal */
.modal-backdrop { position: fixed; inset: 0; z-index: 50; background: rgba(0, 0, 0, 0.5); display: flex; align-items: center; justify-content: center; }
.modal-box { width: min(760px, 94vw); max-height: 88vh; overflow-y: auto; background: var(--color-surface); border: 1px solid var(--color-border); border-radius: 0.75rem; padding: 1.25rem 1.5rem; box-shadow: 0 12px 32px rgba(0, 0, 0, 0.35); }
.modal-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1rem; }
.modal-header h3 { margin: 0; font-size: 1.1rem; color: var(--color-text); }
.detail-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 0.5rem 1.25rem; margin-bottom: 1rem; color: var(--color-text); font-size: 0.88rem; }
.detail-grid .lbl { display: inline-block; min-width: 6.5rem; color: var(--color-muted); }
.detail-lines { margin-top: 0.5rem; }
.detail-amounts { display: flex; flex-wrap: wrap; gap: 1.25rem; margin-top: 1rem; color: var(--color-text); font-size: 0.9rem; font-weight: 500; }
</style>
