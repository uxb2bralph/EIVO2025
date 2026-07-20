<!--
  CreateInvoiceIndex.vue — 線上開立發票。
  遷移自 WebHome InvoiceBusinessController.CreateInvoice + Views/Forms/SimpleInvoice（F0401 存證）
  與 B2BInvoice（A0101 交換）（選單「線上開立發票」）。
  營業人 / 買受人 / 發票主檔 / 明細輸入 + 金額試算 + 產品快速查詢 + 內容預覽 + 開立。
  資料範圍由後端依角色（CanAccessSeller）守門。列印 / 立即檢視證明聯（需 QRCode 金鑰）延後。
-->
<script setup lang="ts">
import { PROCESS_TYPE } from '@/services/create-invoice-api'
import { useCreateInvoice } from '../composables/useCreateInvoice'

const {
  processType,
  isA0101,
  setProcessType,
  taxTypeOptions,
  invoiceTypeOptions,
  carrierTypeOptions,
  clearanceMarkF0401Options,
  clearanceMarkA0101Options,
  buyerRemarkOptions,
  form,
  lines,
  addLine,
  removeLine,
  onTaxTypeChange,
  sum,
  sellerKeyword,
  sellerOptions,
  selectedSellerKey,
  onSellerInput,
  runSellerSearch,
  pickSeller,
  clearSeller,
  buyerKeyword,
  buyerOptions,
  onBuyerInput,
  runBuyerSearch,
  applyCounterpart,
  productSearchOpen,
  productKeyword,
  productOptions,
  productLoading,
  openProductSearch,
  closeProductSearch,
  onProductInput,
  pickProduct,
  previewOpen,
  preview,
  doPreview,
  closePreview,
  resultOpen,
  result,
  doCommit,
  closeResult,
  submitting,
  error,
} = useCreateInvoice()

function fmt(v: number | null): string {
  if (v === null || v === undefined) return ''
  return v.toLocaleString('en-US')
}
</script>

<template>
  <div class="invoice-page">
    <div class="page-header">
      <h1>線上開立發票</h1>
    </div>

    <!-- 電子發票型式 -->
    <div class="type-card">
      <span class="type-label">電子發票型式：</span>
      <label class="radio-inline">
        <input type="radio" :checked="processType === PROCESS_TYPE.F0401" @change="setProcessType(PROCESS_TYPE.F0401)" />
        存證（F0401）
      </label>
      <label class="radio-inline">
        <input type="radio" :checked="processType === PROCESS_TYPE.A0101" @change="setProcessType(PROCESS_TYPE.A0101)" />
        B2B 交換（A0101）
      </label>
    </div>

    <div v-if="error" class="alert-error">{{ error }}</div>

    <!-- 營業人 -->
    <div class="card">
      <h2 class="section-title">營業人</h2>
      <div class="form-grid">
        <!-- 開立人 autocomplete -->
        <div class="form-group seller-group" @click.stop>
          <label>發票開立人 <span class="req">*</span></label>
          <div class="seller-input">
            <input
              v-model="sellerKeyword"
              type="text"
              class="form-control"
              placeholder="輸入統編或名稱"
              @input="onSellerInput"
              @keyup.enter="runSellerSearch"
            />
            <button v-if="selectedSellerKey" class="btn ghost sm" @click="clearSeller">清除</button>
          </div>
          <ul v-if="sellerOptions.length" class="ac-menu">
            <li v-for="opt in sellerOptions" :key="opt.sellerKey ?? ''">
              <a @click="pickSeller(opt)">{{ opt.receiptNo }} {{ opt.companyName }}</a>
            </li>
          </ul>
        </div>

        <!-- 買受人統編 / 帶入 -->
        <div class="form-group seller-group" @click.stop>
          <label>買受人統編{{ isA0101 ? '' : '／查詢帶入' }}</label>
          <input
            v-model="form.buyerReceiptNo"
            type="text"
            maxlength="11"
            class="form-control"
            placeholder="個人發票留空（0000000000）"
          />
          <div class="buyer-search">
            <input
              v-model="buyerKeyword"
              type="text"
              class="form-control"
              placeholder="輸入統編/名稱帶入相對營業人"
              @input="onBuyerInput"
              @keyup.enter="runBuyerSearch"
            />
          </div>
          <ul v-if="buyerOptions.length" class="ac-menu">
            <li v-for="(opt, i) in buyerOptions" :key="i">
              <a @click="applyCounterpart(opt)">{{ opt.receiptNo }} {{ opt.companyName }}</a>
            </li>
          </ul>
        </div>

        <div class="form-group">
          <label>分店號碼 / 客戶 ID</label>
          <input v-model="form.customerId" type="text" class="form-control" />
        </div>

        <div class="form-group">
          <label>買受人名稱</label>
          <input v-model="form.buyerName" type="text" class="form-control" />
        </div>
        <div class="form-group">
          <label>買受人地址</label>
          <input v-model="form.address" type="text" class="form-control" />
        </div>
        <div class="form-group">
          <label>買受人電話</label>
          <input v-model="form.phone" type="text" class="form-control" />
        </div>
        <div class="form-group">
          <label>買受人 email</label>
          <input v-model="form.email" type="text" class="form-control" />
        </div>

        <div class="form-group">
          <label>隨機碼 <span class="req">*</span></label>
          <input v-model="form.randomNo" type="text" maxlength="4" class="form-control" />
        </div>
        <div class="form-group">
          <label>銷售品項稅額</label>
          <div class="radio-row">
            <label class="radio-inline"><input v-model="form.taxCalc" type="radio" value="TaxIncluded" /> 含稅</label>
            <label class="radio-inline"><input v-model="form.taxCalc" type="radio" value="NonTaxed" /> 未稅</label>
          </div>
        </div>

        <!-- F0401 專屬：載具 / 愛心碼 -->
        <template v-if="!isA0101">
          <div class="form-group">
            <label>載具類型</label>
            <select v-model="form.carrierType" class="form-select">
              <option v-for="opt in carrierTypeOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
            </select>
          </div>
          <div class="form-group">
            <label>載具號碼</label>
            <input v-model="form.carrierId1" type="text" maxlength="32" class="form-control" />
          </div>
          <div class="form-group">
            <label>愛心碼</label>
            <input v-model="form.npoban" type="text" maxlength="7" class="form-control" />
          </div>
        </template>

        <!-- A0101 專屬：檢查碼 / 買受人註記 / 沖帳別 / 相關號碼 / 相對營業人 -->
        <template v-else>
          <div class="form-group">
            <label>發票檢查碼</label>
            <input v-model="form.checkNo" type="text" maxlength="10" class="form-control" />
          </div>
          <div class="form-group">
            <label>買受人註記</label>
            <select v-model="form.buyerRemark" class="form-select">
              <option v-for="opt in buyerRemarkOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
            </select>
          </div>
          <div class="form-group">
            <label>沖帳別</label>
            <input v-model="form.category" type="text" maxlength="2" class="form-control" />
          </div>
          <div class="form-group">
            <label>相關號碼</label>
            <input v-model="form.relateNumber" type="text" maxlength="20" class="form-control" />
          </div>
          <div class="form-group">
            <label>&nbsp;</label>
            <label class="check-inline"><input v-model="form.counterpart" type="checkbox" /> 設為相對營業人</label>
          </div>
        </template>

        <div class="form-group span-2">
          <label>備註</label>
          <input v-model="form.remark" type="text" class="form-control" />
        </div>
      </div>
    </div>

    <!-- 發票主檔 -->
    <div class="card">
      <h2 class="section-title">發票</h2>
      <div class="form-grid">
        <div class="form-group">
          <label>發票類別</label>
          <select v-model="form.invoiceType" class="form-select">
            <option v-for="opt in invoiceTypeOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
          </select>
        </div>
        <div class="form-group">
          <label>課稅別</label>
          <select v-model="form.taxType" class="form-select" @change="onTaxTypeChange">
            <option v-for="opt in taxTypeOptions" :key="opt.value" :value="opt.value">{{ opt.label }}</option>
          </select>
        </div>
        <div class="form-group">
          <label>{{ isA0101 ? '通關方式註記' : '買受人簽署適用零稅率註記' }}</label>
          <select v-model="form.customsClearanceMark" class="form-select">
            <option
              v-for="opt in (isA0101 ? clearanceMarkA0101Options : clearanceMarkF0401Options)"
              :key="String(opt.value)"
              :value="opt.value"
            >{{ opt.label }}</option>
          </select>
        </div>
        <div class="form-group">
          <label>稅率</label>
          <input v-model="form.taxRate" type="text" class="form-control" />
        </div>
        <div class="form-group">
          <label>銷售額合計（新台幣）</label>
          <input v-model="form.salesAmount" type="text" class="form-control" readonly />
        </div>
        <div class="form-group">
          <label>營業稅額</label>
          <input v-model="form.taxAmount" type="text" class="form-control" readonly />
        </div>
        <div class="form-group">
          <label>總計（含稅）</label>
          <input v-model="form.totalAmount" type="text" class="form-control" readonly />
        </div>
      </div>
    </div>

    <!-- 發票明細 -->
    <div class="card">
      <h2 class="section-title">發票明細</h2>
      <div class="table-wrap">
        <table class="result-table">
          <thead>
            <tr>
              <th>產品編號</th>
              <th>品名</th>
              <th class="num">數量</th>
              <th class="num">單價</th>
              <th class="num">金額</th>
              <th>備註</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(row, idx) in lines" :key="idx">
              <td><input v-model="row.itemNo" type="text" class="form-control" /></td>
              <td>
                <div class="brief-cell">
                  <input v-model="row.brief" type="text" class="form-control" placeholder="請輸入品名" />
                  <button type="button" class="btn ghost sm" @click="openProductSearch(idx)">查</button>
                </div>
              </td>
              <td><input v-model.number="row.piece" type="number" min="1" step="1" class="form-control num" /></td>
              <td><input v-model.number="row.unitCost" type="number" min="0" step="1" class="form-control num" /></td>
              <td><input :value="fmt(row.costAmount)" type="text" class="form-control num" readonly /></td>
              <td><input v-model="row.remark" type="text" class="form-control" /></td>
              <td class="row-actions">
                <button type="button" class="btn ghost sm" @click="addLine">＋</button>
                <button type="button" class="btn ghost sm" :disabled="lines.length <= 1" @click="removeLine(idx)">－</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- 操作按鈕 -->
    <div class="action-bar">
      <button type="button" class="btn ghost" @click="sum()">金額</button>
      <button type="button" class="btn ghost" :disabled="submitting" @click="doPreview">發票預覽</button>
      <button type="button" class="btn primary" :disabled="submitting" @click="doCommit">
        {{ submitting ? '處理中…' : '發票開立' }}
      </button>
    </div>

    <!-- 產品快速查詢 Modal -->
    <div v-if="productSearchOpen" class="modal-backdrop" @click.self="closeProductSearch">
      <div class="modal-box sm">
        <div class="modal-header">
          <h3>產品快速查詢</h3>
          <button class="btn ghost sm" @click="closeProductSearch">✕</button>
        </div>
        <div class="modal-body">
          <input
            v-model="productKeyword"
            type="text"
            class="form-control"
            placeholder="輸入品名關鍵字"
            @input="onProductInput"
          />
          <div v-if="productLoading" class="state-msg">查詢中…</div>
          <ul v-else class="product-list">
            <li v-for="opt in productOptions" :key="opt.productId">
              <a @click="pickProduct(opt)">
                <span class="p-name">{{ opt.productName }}</span>
                <span class="p-price">{{ fmt(opt.salePrice) }}</span>
              </a>
            </li>
            <li v-if="!productOptions.length" class="state-msg">查無產品</li>
          </ul>
        </div>
      </div>
    </div>

    <!-- 內容預覽 Modal -->
    <div v-if="previewOpen && preview" class="modal-backdrop" @click.self="closePreview">
      <div class="modal-box">
        <div class="modal-header">
          <h3>發票內容預覽</h3>
          <button class="btn ghost sm" @click="closePreview">✕</button>
        </div>
        <div class="modal-body">
          <div class="detail-grid">
            <div><span class="lbl">發票號碼</span>{{ preview.invoiceNo }}</div>
            <div><span class="lbl">隨機碼</span>{{ preview.randomNo }}</div>
            <div><span class="lbl">開立人</span>{{ preview.sellerName }}（{{ preview.sellerReceiptNo }}）</div>
            <div v-if="!preview.isB2C"><span class="lbl">買受人</span>{{ preview.buyerName }}（{{ preview.buyerReceiptNo }}）</div>
            <div v-if="preview.carrierNo"><span class="lbl">載具</span>{{ preview.carrierType }} {{ preview.carrierNo }}</div>
            <div v-if="preview.agencyCode"><span class="lbl">愛心碼</span>{{ preview.agencyCode }}</div>
          </div>
          <table class="result-table detail-lines">
            <thead>
              <tr><th>#</th><th>品名</th><th class="num">數量</th><th class="num">單價</th><th class="num">金額</th><th>備註</th></tr>
            </thead>
            <tbody>
              <tr v-for="line in preview.lines" :key="line.seq">
                <td>{{ line.seq }}</td>
                <td>{{ line.description }}</td>
                <td class="num">{{ fmt(line.quantity) }}</td>
                <td class="num">{{ fmt(line.unitPrice) }}</td>
                <td class="num">{{ fmt(line.amount) }}</td>
                <td>{{ line.remark }}</td>
              </tr>
            </tbody>
          </table>
          <div class="detail-amounts">
            <span>未稅：{{ fmt(preview.salesAmount) }}</span>
            <span>稅額：{{ fmt(preview.taxAmount) }}</span>
            <span>含稅：{{ fmt(preview.totalAmount) }}</span>
            <span>稅別：{{ preview.taxTypeLabel }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 開立結果 Modal -->
    <div v-if="resultOpen && result" class="modal-backdrop" @click.self="closeResult">
      <div class="modal-box sm">
        <div class="modal-header">
          <h3>開立完成</h3>
          <button class="btn ghost sm" @click="closeResult">✕</button>
        </div>
        <div class="modal-body">
          <p class="result-line">發票已開立，號碼：<strong>{{ result.invoiceNo }}</strong></p>
          <p v-if="result.hasCarrier" class="result-note">使用載具，不列印證明聯。</p>
          <p v-else-if="result.printMark === 'Y'" class="result-note">此發票需列印證明聯（列印功能後續提供）。</p>
          <div class="modal-actions">
            <button class="btn primary" @click="closeResult">繼續開立</button>
          </div>
        </div>
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
.type-card { display: flex; align-items: center; gap: 1.25rem; flex-wrap: wrap; }
.type-label { color: var(--color-text); font-weight: 600; }
.section-title { font-size: 1.05rem; color: var(--color-text); margin: 0 0 1rem; }
.form-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1rem;
}
.form-group { display: flex; flex-direction: column; gap: 0.4rem; position: relative; }
.form-group.span-2 { grid-column: span 2; }
.form-group label { font-size: 0.85rem; font-weight: 500; color: var(--color-muted); }
.req { color: #ff8585; }
.radio-row { display: flex; gap: 1rem; }
.radio-inline, .check-inline { display: inline-flex; align-items: center; gap: 0.4rem; color: var(--color-text); font-weight: 400; font-size: 0.9rem; cursor: pointer; }
.buyer-search { margin-top: 0.4rem; }
.seller-input { display: flex; gap: 0.4rem; }
.seller-input .form-control { flex: 1; }
.ac-menu {
  position: absolute; top: 100%; left: 0; right: 0; z-index: 20;
  margin: 0.25rem 0 0; padding: 0.25rem 0; list-style: none;
  max-height: 240px; overflow-y: auto;
  background: var(--color-surface); border: 1px solid var(--color-border);
  border-radius: 0.5rem; box-shadow: 0 6px 18px rgba(0, 0, 0, 0.25);
}
.ac-menu li a { display: block; padding: 0.45rem 0.9rem; font-size: 0.85rem; color: var(--color-text); cursor: pointer; }
.ac-menu li a:hover { background: rgba(59, 199, 255, 0.12); color: var(--color-accent); }
.table-wrap { overflow-x: auto; }
.result-table { width: 100%; border-collapse: collapse; color: var(--color-text); }
.result-table th, .result-table td {
  padding: 0.5rem 0.6rem; text-align: left; border-bottom: 1px solid var(--color-border);
  font-size: 0.85rem; vertical-align: middle;
}
.result-table th.num, .result-table td.num { text-align: right; }
.result-table thead th { color: var(--color-muted); font-weight: 600; white-space: nowrap; }
.brief-cell { display: flex; gap: 0.3rem; }
.brief-cell .form-control { flex: 1; }
.row-actions { white-space: nowrap; display: flex; gap: 0.3rem; }
.form-control, .form-select {
  width: 100%; padding: 0.45rem 0.6rem; font-size: 0.88rem;
  background: var(--color-bg, transparent); color: var(--color-text);
  border: 1px solid var(--color-border); border-radius: 0.4rem;
}
.form-control.num { text-align: right; }
.form-control[readonly] { opacity: 0.75; }
.action-bar { display: flex; flex-wrap: nowrap; align-items: center; justify-content: center; gap: 0.6rem; }
.btn { padding: 0.5rem 1.1rem; border-radius: 0.4rem; cursor: pointer; font-size: 0.9rem; border: 1px solid var(--color-border); background: transparent; color: var(--color-text); }
.btn.primary { background: var(--color-accent); border-color: var(--color-accent); color: #06121b; font-weight: 600; }
.btn.ghost { background: transparent; }
.btn.ghost:hover:not(:disabled) { border-color: var(--color-accent); }
.btn.sm { font-size: 0.8rem; padding: 0.3rem 0.55rem; }
.btn:disabled { opacity: 0.6; cursor: not-allowed; }
.alert-error { color: #ff8585; margin-bottom: 1rem; white-space: pre-line; }
.state-msg { text-align: center; color: var(--color-muted); padding: 1.5rem 0; }

/* Modal */
.modal-backdrop { position: fixed; inset: 0; z-index: 50; background: rgba(0, 0, 0, 0.5); display: flex; align-items: center; justify-content: center; }
.modal-box { width: min(760px, 94vw); max-height: 88vh; overflow-y: auto; background: var(--color-surface); border: 1px solid var(--color-border); border-radius: 0.75rem; padding: 1.25rem 1.5rem; box-shadow: 0 12px 32px rgba(0, 0, 0, 0.35); }
.modal-box.sm { width: min(420px, 94vw); }
.modal-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1rem; }
.modal-header h3 { margin: 0; font-size: 1.1rem; color: var(--color-text); }
.detail-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(260px, 1fr)); gap: 0.5rem 1.25rem; margin-bottom: 1rem; color: var(--color-text); font-size: 0.88rem; }
.detail-grid .lbl { display: inline-block; min-width: 5.5rem; color: var(--color-muted); }
.detail-lines { margin-top: 0.5rem; }
.detail-amounts { display: flex; flex-wrap: wrap; gap: 1.25rem; margin-top: 1rem; color: var(--color-text); font-size: 0.9rem; font-weight: 500; }
.product-list { list-style: none; margin: 0.75rem 0 0; padding: 0; max-height: 320px; overflow-y: auto; }
.product-list li a { display: flex; justify-content: space-between; gap: 1rem; padding: 0.5rem 0.75rem; border-bottom: 1px solid var(--color-border); color: var(--color-text); cursor: pointer; font-size: 0.88rem; }
.product-list li a:hover { background: rgba(59, 199, 255, 0.12); color: var(--color-accent); }
.p-price { color: var(--color-muted); }
.result-line { color: var(--color-text); font-size: 0.95rem; }
.result-note { color: var(--color-muted); font-size: 0.85rem; }
.modal-actions { display: flex; justify-content: flex-end; margin-top: 1rem; }
</style>
