<!--
  OrganizationEditForm.vue – 編輯營業人資料表單。
  遷移自 WebHome Views/Organization/Module/EditItem.cshtml + ItemForm.cshtml。
  以對話框（modal）形式於 OrganizationQueryIndex.vue 中呈現。
-->
<script setup lang="ts">
import { reactive, watch } from 'vue'
import type { OrganizationEdit } from '@/services/organization-api'

const props = defineProps<{
  /** 後端載入的營業人編輯資料 */
  data: OrganizationEdit
  /** 是否儲存中 */
  saving?: boolean
}>()

const emit = defineEmits<{
  (e: 'save', payload: OrganizationEdit): void
  (e: 'cancel'): void
}>()

// 類別選項（對應舊版 OrganizationCategoryOptions.cshtml）
const categoryOptions = [
  { value: 15, label: '發票開立營業人' },
  { value: 20, label: '經銷商' },
  { value: 23, label: '境外電商' },
]

// 發票類別選項（對應舊版 InvoiceTypeOptions.cshtml）
const invoiceTypeOptions = [
  { value: 7, label: '一般稅額計算之電子發票' },
  { value: 8, label: '特種稅額計算之電子發票' },
]

// 訊息通知旗標（對應舊版 name="NoticeStatus" 複選，值為 Naming.InvoiceNoticeStatus 位元值）
const noticeFlags = [
  { value: 0x02, label: '寄送開立通知' }, // Issuing
  { value: 0x01, label: '寄送中獎通知' }, // Winning
  { value: 0x40, label: '使用境外電商郵件樣式' }, // UseCBEStyle
  { value: 0x04, label: '寄送作廢通知' }, // Cancelling
  { value: 0x08, label: '寄送折讓通知' }, // IssuingAllowance
  { value: 0x10, label: '寄送作廢折讓通知' }, // CancellingAllowance
  { value: 0x20, label: '使用客訂郵件樣式' }, // UseCustomStyle
  { value: 0x80, label: '寄送每日報表' }, // GetDailyReport
  { value: 0x100, label: '每日寄送未開立通知' }, // NoInvoiceIssued
]

const dateFields = [
  'creationDate',
  'expirationDate',
  'authorizationNotBefore',
  'authorizationNotAfter',
  'invoiceRequestNotBefore',
  'invoiceRequestNotAfter',
] as const

// ISO 日期字串 → yyyy-MM-dd（供 <input type="date">）
function toDateInput(value: string | null): string {
  if (!value) return ''
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return ''
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}

// 建立本地可編輯副本（陣列預設為 []，日期正規化為 yyyy-MM-dd）
function cloneToForm(src: OrganizationEdit): OrganizationEdit {
  const copy: OrganizationEdit = {
    ...src,
    settings: src.settings ? [...src.settings] : [],
    noticeStatus: src.noticeStatus ? [...src.noticeStatus] : [],
  }
  // 以 Record 投影避免跨 union key 指派的型別問題
  const rec = copy as unknown as Record<string, string | null>
  for (const f of dateFields) {
    rec[f] = toDateInput(src[f])
  }
  return copy
}

const form = reactive<OrganizationEdit>(cloneToForm(props.data))

// 父層重新載入資料時同步更新表單
watch(
  () => props.data,
  (next) => {
    Object.assign(form, cloneToForm(next))
  },
)

function onSubmit() {
  // 日期空字串還原為 null，避免後端解析失敗
  const payload: OrganizationEdit = { ...form }
  const rec = payload as unknown as Record<string, string | null>
  for (const f of dateFields) {
    rec[f] = rec[f] ? rec[f] : null
  }
  emit('save', payload)
}
</script>

<template>
  <div class="edit-form">
    <!-- 營業人基本資料 -->
    <section class="form-section">
      <h3 class="section-title">基本資料</h3>
      <div class="field-grid">
        <div class="field">
          <label><span class="req">*</span>公司統一編號</label>
          <input v-model="form.receiptNo" type="text" class="form-control" />
        </div>
        <div class="field">
          <label><span class="req">*</span>名稱</label>
          <input v-model="form.companyName" type="text" class="form-control" />
        </div>
        <div class="field col-span-2">
          <label><span class="req">*</span>地址</label>
          <input v-model="form.addr" type="text" maxlength="68" class="form-control" />
        </div>
        <div class="field">
          <label><span class="req">*</span>電話</label>
          <input v-model="form.phone" type="text" class="form-control" />
        </div>
        <div class="field">
          <label>傳真</label>
          <input v-model="form.fax" type="text" class="form-control" />
        </div>
        <div class="field">
          <label>公司負責人</label>
          <input v-model="form.undertakerName" type="text" class="form-control" />
        </div>
        <div class="field">
          <label><span class="req">*</span>類別</label>
          <select v-model.number="form.categoryId" class="form-control">
            <option :value="null">請選擇</option>
            <option v-for="opt in categoryOptions" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </option>
          </select>
        </div>
        <div class="field col-span-2">
          <label>營業人店號／別名</label>
          <input v-model="form.customerNo" type="text" maxlength="16" class="form-control" />
        </div>
      </div>
    </section>

    <!-- 聯絡方式 -->
    <section class="form-section">
      <h3 class="section-title">聯絡方式</h3>
      <div class="field-grid">
        <div class="field">
          <label>聯絡人姓名</label>
          <input v-model="form.contactName" type="text" class="form-control" />
        </div>
        <div class="field">
          <label>聯絡人職稱</label>
          <input v-model="form.contactTitle" type="text" class="form-control" />
        </div>
        <div class="field">
          <label>聯絡人電話</label>
          <input v-model="form.contactPhone" type="text" class="form-control" />
        </div>
        <div class="field">
          <label>聯絡人行動電話</label>
          <input v-model="form.contactMobilePhone" type="text" class="form-control" />
        </div>
        <div class="field col-span-2">
          <label><span class="req">*</span>聯絡人電子郵件</label>
          <input v-model="form.contactEmail" type="text" maxlength="512" class="form-control" />
        </div>
        <div class="field">
          <label>建檔日期</label>
          <input v-model="form.creationDate" type="date" class="form-control" />
        </div>
        <div class="field">
          <label>註記停用日期</label>
          <input v-model="form.expirationDate" type="date" class="form-control" />
        </div>
        <div class="field">
          <label>授權期間(起)</label>
          <input v-model="form.authorizationNotBefore" type="date" class="form-control" />
        </div>
        <div class="field">
          <label>授權期間(迄)</label>
          <input v-model="form.authorizationNotAfter" type="date" class="form-control" />
        </div>
        <div class="field">
          <label>發票期間(起)</label>
          <input v-model="form.invoiceRequestNotBefore" type="date" class="form-control" />
        </div>
        <div class="field">
          <label>發票期間(迄)</label>
          <input v-model="form.invoiceRequestNotAfter" type="date" class="form-control" />
        </div>
        <div class="field col-span-2">
          <label>稅籍編號</label>
          <input v-model="form.taxNo" type="text" class="form-control" />
        </div>
      </div>
    </section>

    <!-- 設定項目 -->
    <section class="form-section">
      <h3 class="section-title">設定項目</h3>
      <div class="field-grid">
        <label class="check"><input v-model="form.setToPrintInvoice" type="checkbox" />營業人客製化列印</label>
        <label class="check"><input v-model="form.settings" type="checkbox" value="DisableC0401Template" />停用C0401系統樣板</label>
        <div class="field col-span-2">
          <label>發票列印樣式</label>
          <input v-model="form.invoicePrintView" type="text" maxlength="160" class="form-control" />
          <label>熱感紙發票列印樣式</label>
          <input v-model="form.c0401POSView" type="text" maxlength="160" class="form-control" />
          <label>折讓單列印樣式</label>
          <input v-model="form.allowancePrintView" type="text" maxlength="160" class="form-control" />
        </div>
        <div class="field">
          <label>電子發票核准函號</label>
          <input v-model="form.authorizationNo" type="text" class="form-control" />
        </div>
        <label class="check"><input v-model="form.entrustToPrint" type="checkbox" />B2B發票買受人可重複列印</label>
        <label class="check"><input v-model="form.downloadDataNumber" type="checkbox" />使用傳送企業發票對應資料</label>
        <label class="check"><input v-model="form.settings" type="checkbox" value="ForcedAuditNo" />單據號碼強制唯一</label>
        <label class="check"><input v-model="form.settings" type="checkbox" value="IgnoreDuplicatedNo" />傳輸開立忽略檢核發票號碼重複</label>
        <label class="check"><input v-model="form.uploadBranchTrackBlank" type="checkbox" />上期發票空白號碼用戶端自動下載</label>
        <label class="check"><input v-model="form.autoBlankTrack" type="checkbox" />由系統自動結算</label>
        <label class="check"><input v-model="form.autoBlankTrackEmittance" type="checkbox" />由系統自動輸出E0402</label>
        <label class="check"><input v-model="form.printAll" type="checkbox" />設定發票資料全列印</label>
        <div class="field">
          <label><span class="req">*</span>設定發票類別</label>
          <select v-model.number="form.settingInvoiceType" class="form-control">
            <option :value="null">請選擇</option>
            <option v-for="opt in invoiceTypeOptions" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </option>
          </select>
        </div>
        <label class="check"><input v-model="form.subscribeB2BInvoicePDF" type="checkbox" />設定接收B2B發票(PDF檔)</label>
        <label class="check"><input v-model="form.enableTrackCodeInvoiceNoValidation" type="checkbox" />開立發票字軌號碼檢核</label>
        <label class="check"><input v-model="form.settings" type="checkbox" value="InvoiceExchange" />啟用發票交換</label>
        <label class="check"><input v-model="form.settings" type="checkbox" value="AllB2B" />只開立B2B發票</label>
        <label class="check"><input v-model="form.setToOutsourcingCS" type="checkbox" />使用委外客服</label>
        <label class="check"><input v-model="form.downloadDispatch" type="checkbox" />設定接收MIG</label>
        <label class="check"><input v-model="form.settings" type="checkbox" value="SendAllowanceMIGManually" />手動傳送折讓單MIG</label>
        <label class="check"><input v-model="form.settings" type="checkbox" value="InvoiceNotUploadedAlert" />通知發票未傳送</label>
      </div>
    </section>

    <!-- 訊息通知 -->
    <section class="form-section">
      <h3 class="section-title">訊息通知</h3>
      <div class="field-grid">
        <label v-for="flag in noticeFlags" :key="flag.value" class="check">
          <input v-model="form.noticeStatus" type="checkbox" :value="flag.value" />{{ flag.label }}
        </label>
        <label class="check"><input v-model="form.setToNotifyCounterpartBySMS" type="checkbox" />使用簡訊通知買受人</label>
        <label class="check"><input v-model="form.useB2BStandalone" type="checkbox" />B2B發票開立通知以鏈結取代附件</label>
        <label class="check"><input v-model="form.settings" type="checkbox" value="HybridB2B" />以A0401格式寄送B2B發票</label>
        <div class="field">
          <label>營業人連絡電話</label>
          <input v-model="form.businessContactPhone" type="text" class="form-control" />
        </div>
        <div class="field col-span-2">
          <label>客訂樣式檔名</label>
          <input v-model="form.customNotificationView" type="text" maxlength="160" class="form-control" />
        </div>
        <div class="field col-span-2">
          <label>郵件附言</label>
          <textarea v-model="form.customNotification" class="form-control" rows="5"></textarea>
        </div>
      </div>
    </section>

    <div class="form-actions">
      <button type="button" class="btn primary" :disabled="saving" @click="onSubmit">
        {{ saving ? '儲存中…' : '確定' }}
      </button>
      <button type="button" class="btn ghost" :disabled="saving" @click="emit('cancel')">取消</button>
    </div>
  </div>
</template>

<style scoped>
.edit-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}
.form-section {
  border: 1px solid var(--color-border);
  border-radius: 0.5rem;
  padding: 1rem 1.25rem;
}
.section-title {
  margin: 0 0 1rem;
  font-size: 1rem;
  color: var(--color-accent);
  border-bottom: 1px solid var(--color-border);
  padding-bottom: 0.5rem;
}
.field-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 0.75rem 1.25rem;
  align-items: start;
}
.field {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}
.field.col-span-2 {
  grid-column: 1 / -1;
}
.field label {
  font-size: 0.82rem;
  color: var(--color-muted);
}
.req {
  color: #ff6b6b;
  margin-right: 0.15rem;
}
.check {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.85rem;
  color: var(--color-text);
}
.check input {
  flex: none;
}
.form-actions {
  display: flex;
  justify-content: center;
  gap: 1rem;
  padding-top: 0.5rem;
}
.btn.ghost {
  background: transparent;
  border: 1px solid var(--color-border);
  color: var(--color-text);
}
.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
