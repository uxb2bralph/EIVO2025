<!--
  InvoiceNoIntervalIndex.vue — 電子發票號碼維護（配號區間查詢 + 新增 / 修改 / 刪除 / 鎖定）。
  遷移自 WebHome InvoiceNoController.MaintainInvoiceNoInterval 之核心動作（第一階段）。
  查詢須先選擇開立人與年度；列表以雙月期別呈現，起迄與 POS 機號可就地編輯，並提供固定新增列。
  進階動作（分割 / 均分 / POS 本組數 / 主機構 / 分支機構 / E0401 匯出）另行分階段遷移。
-->
<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import { useInvoiceNoIntervalIndex } from '../composables/useInvoiceNoIntervalIndex'
import Pager from './Pager.vue'

const {
  yearOptions,
  periodOptions,
  year,
  periodNo,
  branchRelation,
  sellerKeyword,
  sellerOptions,
  sellerLoading,
  selectedSellerKey,
  searchSellerOptions,
  pickSeller,
  clearSeller,
  items,
  totalCount,
  page,
  loading,
  error,
  searched,
  totalPages,
  onSearch,
  goToPage,
  trackOptions,
  loadTrackOptions,
  editingId,
  editStartNo,
  editEndNo,
  editDeviceName,
  saving,
  startEdit,
  cancelEdit,
  saveEdit,
  removeItem,
  toggleLock,
  doSplit,
  doAllot,
  doApplyHeadquarter,
  branchDialogOpen,
  branchKeyword,
  branchOptions,
  branchLoading,
  branchSaving,
  openBranchDialog,
  closeBranchDialog,
  searchBranchOptions,
  confirmBranch,
  exporting,
  exportE0401,
  safetyStock,
  safetyStockSaving,
  saveSafetyStock,
  addTrackId,
  addStartNo,
  addEndNo,
  addDeviceName,
  adding,
  addItem,
  toRocYear,
  periodLabel,
  invoiceTypeLabel,
  padInvoiceNo,
} = useInvoiceNoIntervalIndex()

// 每列「管理」下拉選單
const openMenuId = ref<number | null>(null)
function toggleMenu(id: number) {
  openMenuId.value = openMenuId.value === id ? null : id
}
function closeMenu() {
  openMenuId.value = null
}

function doSearch() {
  onSearch()
  loadTrackOptions()
}

onMounted(() => document.addEventListener('click', closeMenu))
onUnmounted(() => document.removeEventListener('click', closeMenu))
</script>

<template>
  <div class="interval-page">
    <div class="page-header">
      <h1>電子發票號碼維護</h1>
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

        <!-- 開立人選擇（autocomplete） -->
        <div class="form-group seller-group" @click.stop>
          <label>開立人<span class="req">*</span></label>
          <div class="seller-input">
            <input
              v-model="sellerKeyword"
              type="text"
              class="form-control"
              placeholder="輸入統編或名稱後搜尋"
              @keyup.enter="searchSellerOptions"
            />
            <button class="btn ghost sm" :disabled="sellerLoading" @click="searchSellerOptions">
              {{ sellerLoading ? '搜尋中…' : '搜尋' }}
            </button>
            <button v-if="selectedSellerKey" class="btn ghost sm" @click="clearSeller">清除</button>
          </div>
          <ul v-if="sellerOptions.length" class="seller-menu">
            <li v-for="opt in sellerOptions" :key="opt.sellerKey">
              <a @click="pickSeller(opt)">{{ opt.receiptNo }} {{ opt.companyName }}</a>
            </li>
          </ul>
        </div>

        <div class="form-group">
          <label>&nbsp;</label>
          <label class="check-inline">
            <input v-model="branchRelation" type="checkbox" />
            查詢分支機構發票號碼區間
          </label>
        </div>
      </div>
      <div class="query-actions">
        <button
          class="btn ghost"
          :disabled="exporting || !selectedSellerKey"
          title="下載分支機構配號 E0401 XML（需指定期別）"
          @click="exportE0401"
        >
          {{ exporting ? '匯出中…' : '下載E0401' }}
        </button>
        <button class="btn primary" :disabled="loading || !selectedSellerKey" @click="doSearch">
          <span v-if="!loading">查詢</span>
          <span v-else>查詢中…</span>
        </button>
      </div>
    </div>

    <!-- 查詢結果 -->
    <div class="card result-card">
      <div v-if="error" class="alert-error">{{ error }}</div>

      <!-- 可用配號存量警戒值（該開立人；沿用舊版 QueryResult 之欄位） -->
      <div v-if="searched && selectedSellerKey" class="safety-stock">
        <label>可用配號存量警戒值</label>
        <input
          v-model.number="safetyStock"
          type="number"
          class="form-control num"
          placeholder="（可空白）"
          :disabled="safetyStockSaving"
          @change="saveSafetyStock"
        />
        <span v-if="safetyStockSaving" class="state-msg-inline">儲存中…</span>
      </div>

      <div v-if="loading" class="state-msg">資料載入中…</div>

      <div v-else class="table-wrap">
        <table class="result-table">
          <thead>
            <tr>
              <th>統一編號</th>
              <th>年度</th>
              <th>期別</th>
              <th>字軌</th>
              <th>號碼起</th>
              <th>號碼迄</th>
              <th>POS機號</th>
              <th>配號總數/剩餘</th>
              <th>給號</th>
              <th>狀態</th>
              <th class="col-action">管理</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in items" :key="row.intervalId">
              <td>{{ row.receiptNo }}</td>
              <td>{{ toRocYear(row.year) }}</td>
              <td>{{ periodLabel(row.periodNo) }}</td>
              <td>{{ row.trackCode }}<small v-if="row.invoiceType" class="type-tag">（{{ invoiceTypeLabel(row.invoiceType) }}）</small></td>

              <!-- 編輯模式：起迄與 POS 機號可改 -->
              <template v-if="editingId === row.intervalId">
                <td>
                  <input v-model.number="editStartNo" type="number" class="form-control num" :disabled="saving" />
                </td>
                <td>
                  <input v-model.number="editEndNo" type="number" class="form-control num" :disabled="saving" />
                </td>
                <td>
                  <input v-model="editDeviceName" type="text" class="form-control" :disabled="saving" placeholder="（可空白）" />
                </td>
                <td colspan="2" class="state-msg-inline">編輯中…</td>
                <td class="col-action">
                  <div class="row-actions">
                    <button class="btn primary sm" :disabled="saving" @click="saveEdit(row)">
                      {{ saving ? '儲存中…' : '確定' }}
                    </button>
                    <button class="btn ghost sm" :disabled="saving" @click="cancelEdit">取消</button>
                  </div>
                </td>
              </template>

              <!-- 檢視模式 -->
              <template v-else>
                <td>{{ padInvoiceNo(row.startNo) }}</td>
                <td>{{ padInvoiceNo(row.endNo) }}</td>
                <td>{{ row.deviceName }}</td>
                <td>{{ row.totalCount }}（{{ row.bookletCount }} 本）／{{ row.remaining }}</td>
                <td>{{ padInvoiceNo(row.currentNo) }}</td>
                <td>
                  <span :class="row.locked ? 'badge-locked' : 'badge-open'">
                    {{ row.locked ? '已鎖定' : '可配號' }}
                  </span>
                </td>
                <td class="col-action">
                  <div class="action-dropdown" @click.stop>
                    <button
                      type="button"
                      class="btn action-toggle"
                      :aria-expanded="openMenuId === row.intervalId"
                      @click="toggleMenu(row.intervalId)"
                    >
                      請選擇功能 <span class="caret"></span>
                    </button>
                    <ul v-if="openMenuId === row.intervalId" class="action-menu">
                      <li v-if="row.editable"><a @click="closeMenu(); startEdit(row)">修改</a></li>
                      <li v-if="row.editable"><a @click="closeMenu(); removeItem(row)">刪除</a></li>
                      <li v-if="row.editable"><a @click="closeMenu(); doAllot(row)">本組數均分</a></li>
                      <li v-if="!row.editable && row.remaining > 100"><a @click="closeMenu(); doSplit(row)">分割</a></li>
                      <li v-if="row.isMaster && !row.hasMainAssignment"><a @click="closeMenu(); doApplyHeadquarter(row)">主機構配號</a></li>
                      <li v-if="row.isMaster && row.editable"><a @click="closeMenu(); openBranchDialog(row)">指派分支機構</a></li>
                      <li><a @click="closeMenu(); toggleLock(row)">{{ row.locked ? '解除鎖定' : '鎖定' }}</a></li>
                    </ul>
                  </div>
                </td>
              </template>
            </tr>

            <tr v-if="searched && !items.length" class="empty-row">
              <td colspan="11" class="state-msg">查無資料!!</td>
            </tr>
          </tbody>

          <!-- 新增列（沿用舊版 AddItem）：開立人 = 目前所選開立人 -->
          <tfoot v-if="selectedSellerKey">
            <tr class="add-row">
              <td colspan="3">新增配號區間</td>
              <td>
                <select v-model="addTrackId" class="form-select" :disabled="adding">
                  <option value="">選擇字軌</option>
                  <option v-for="t in trackOptions" :key="t.trackId" :value="t.trackId">
                    {{ t.trackCode }}
                  </option>
                </select>
              </td>
              <td><input v-model.number="addStartNo" type="number" class="form-control num" placeholder="起號" :disabled="adding" /></td>
              <td><input v-model.number="addEndNo" type="number" class="form-control num" placeholder="迄號" :disabled="adding" /></td>
              <td><input v-model="addDeviceName" type="text" class="form-control" placeholder="POS機號(可空白)" :disabled="adding" /></td>
              <td colspan="3"></td>
              <td class="col-action">
                <button class="btn primary sm" :disabled="adding" @click="addItem">
                  {{ adding ? '新增中…' : '新增' }}
                </button>
              </td>
            </tr>
          </tfoot>
        </table>
      </div>

      <Pager :page="page" :total-pages="totalPages" :total-count="totalCount" @change="goToPage" />
    </div>

    <!-- 指派分支機構對話框 -->
    <div v-if="branchDialogOpen" class="modal-backdrop" @click.self="closeBranchDialog">
      <div class="modal-box">
        <div class="modal-header">
          <h3>指派分支機構</h3>
          <button class="btn ghost sm" @click="closeBranchDialog">✕</button>
        </div>
        <div class="modal-body">
          <div class="seller-input">
            <input
              v-model="branchKeyword"
              type="text"
              class="form-control"
              placeholder="輸入分支機構統編或名稱後搜尋"
              :disabled="branchSaving"
              @keyup.enter="searchBranchOptions"
            />
            <button class="btn ghost sm" :disabled="branchLoading || branchSaving" @click="searchBranchOptions">
              {{ branchLoading ? '搜尋中…' : '搜尋' }}
            </button>
          </div>
          <ul v-if="branchOptions.length" class="branch-list">
            <li v-for="opt in branchOptions" :key="opt.sellerKey">
              <button class="branch-item" :disabled="branchSaving" @click="confirmBranch(opt)">
                {{ opt.receiptNo }} {{ opt.companyName }}
              </button>
            </li>
          </ul>
          <p v-else class="state-msg-inline">請搜尋並點選要指派的分支機構開立人。</p>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.interval-page { max-width: 1200px; margin: 0 auto; }
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
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1rem;
}
.form-group { display: flex; flex-direction: column; gap: 0.4rem; position: relative; }
.form-group label { font-size: 0.85rem; font-weight: 500; color: var(--color-muted); }
.req { color: #ff8585; }
.check-inline { flex-direction: row; align-items: center; gap: 0.4rem; color: var(--color-text); font-weight: 400; }
.seller-input { display: flex; gap: 0.4rem; }
.seller-input .form-control { flex: 1; }
.seller-menu {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  z-index: 20;
  margin: 0.25rem 0 0;
  padding: 0.25rem 0;
  list-style: none;
  max-height: 260px;
  overflow-y: auto;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 0.5rem;
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.25);
}
.seller-menu li a {
  display: block;
  padding: 0.45rem 0.9rem;
  font-size: 0.85rem;
  color: var(--color-text);
  cursor: pointer;
}
.seller-menu li a:hover { background: rgba(59, 199, 255, 0.12); color: var(--color-accent); }
.query-actions { margin-top: 1.25rem; display: flex; justify-content: flex-end; gap: 0.6rem; }
.btn:disabled { opacity: 0.6; cursor: not-allowed; }
.btn.ghost { background: transparent; border: 1px solid var(--color-border); color: var(--color-text); }
.btn.ghost:hover:not(:disabled) { border-color: var(--color-accent); }
.btn.sm { font-size: 0.8rem; padding: 0.3rem 0.7rem; }
.table-wrap { overflow-x: auto; }
.result-table { width: 100%; border-collapse: collapse; color: var(--color-text); }
.result-table th, .result-table td {
  padding: 0.6rem 0.75rem;
  text-align: left;
  border-bottom: 1px solid var(--color-border);
  white-space: nowrap;
  font-size: 0.88rem;
  vertical-align: middle;
}
.result-table thead th { color: var(--color-muted); font-weight: 600; }
.result-table tbody tr:hover { background: rgba(59, 199, 255, 0.06); }
.type-tag { color: var(--color-muted); font-size: 0.75rem; }
.form-control.num { width: 8.5rem; }
.col-action { min-width: 150px; }
.row-actions { display: flex; gap: 0.4rem; }
.add-row { background: rgba(59, 199, 255, 0.04); }
.add-row td { border-top: 2px solid var(--color-border); border-bottom: none; }
.badge-locked { color: #ff8585; }
.badge-open { color: #5fd0a0; }
.action-dropdown { position: relative; display: inline-block; }
.action-toggle { background: var(--color-accent); border: 1px solid var(--color-accent); color: #fff; font-size: 0.85rem; padding: 0.35rem 0.7rem; }
.action-toggle .caret {
  display: inline-block; margin-left: 0.35rem;
  border-top: 4px solid currentColor; border-right: 4px solid transparent; border-left: 4px solid transparent;
  vertical-align: middle;
}
.action-menu {
  position: absolute; right: 0; z-index: 10; min-width: 8rem;
  margin: 0.25rem 0 0; padding: 0.25rem 0; list-style: none;
  background: var(--color-surface); border: 1px solid var(--color-border);
  border-radius: 0.5rem; box-shadow: 0 6px 18px rgba(0, 0, 0, 0.25);
}
.action-menu li a { display: block; padding: 0.45rem 0.9rem; font-size: 0.85rem; color: var(--color-text); white-space: nowrap; cursor: pointer; }
.action-menu li a:hover { background: rgba(59, 199, 255, 0.12); color: var(--color-accent); }
.state-msg { text-align: center; color: var(--color-muted); padding: 2.5rem 0; }
.state-msg-inline { color: var(--color-muted); }
.safety-stock { display: flex; align-items: center; gap: 0.6rem; margin-bottom: 1rem; }
.safety-stock label { font-size: 0.85rem; color: var(--color-muted); }
.empty-row td { border-bottom: none; }
.alert-error { color: #ff8585; margin-bottom: 1rem; white-space: pre-line; }

/* 指派分支機構對話框 */
.modal-backdrop {
  position: fixed; inset: 0; z-index: 50;
  background: rgba(0, 0, 0, 0.5);
  display: flex; align-items: center; justify-content: center;
}
.modal-box {
  width: min(560px, 92vw);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 0.75rem;
  padding: 1.25rem 1.5rem;
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.35);
}
.modal-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1rem; }
.modal-header h3 { margin: 0; font-size: 1.1rem; color: var(--color-text); }
.modal-body .seller-input { display: flex; gap: 0.4rem; }
.modal-body .seller-input .form-control { flex: 1; }
.branch-list { list-style: none; margin: 0.75rem 0 0; padding: 0; max-height: 300px; overflow-y: auto; }
.branch-item {
  display: block; width: 100%; text-align: left;
  padding: 0.5rem 0.75rem; margin-bottom: 0.25rem;
  background: transparent; border: 1px solid var(--color-border); border-radius: 0.4rem;
  color: var(--color-text); font-size: 0.85rem; cursor: pointer;
}
.branch-item:hover:not(:disabled) { border-color: var(--color-accent); background: rgba(59, 199, 255, 0.08); }
</style>
