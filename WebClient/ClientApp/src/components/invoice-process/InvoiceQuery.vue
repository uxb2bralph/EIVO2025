<template>
  <div class="container py-3">
    <!-- 頁面標題 -->
    <header class="mb-3">
      <nav class="navbar navbar-expand-lg navbar-light bg-light rounded px-3">
        <span class="navbar-brand fw-semibold">電子發票系統</span>
        <span class="ms-2 text-muted">首頁 ＞ 發票作業 ＞ 發票資料查詢／列印／匯出</span>
      </nav>
    </header>

    <!-- 查詢條件 -->
    <section class="card mb-3">
      <div class="card-header fw-semibold">查詢條件</div>
      <div class="card-body">
        <div class="row g-3">
          <!-- 關鍵字 -->
          <div class="col-12 col-md-6">
            <label class="form-label">請輸入檢索關鍵字</label>
            <input v-model.trim="form.keyword" type="text" class="form-control" placeholder="關鍵字" />
          </div>

          <!-- 營業人統編／買受人統編 -->
          <div class="col-12 col-md-3">
            <label class="form-label">營業人統編</label>
            <input v-model.trim="form.sellerId" type="text" class="form-control" placeholder="營業人統編" />
          </div>
          <div class="col-12 col-md-3">
            <label class="form-label">買受人統編</label>
            <input v-model.trim="form.buyerId" type="text" class="form-control" placeholder="買受人統編" />
          </div>

          <!-- 客戶 ID／個人識別碼 -->
          <div class="col-12 col-md-3">
            <label class="form-label">客戶 ID</label>
            <input v-model.trim="form.customerId" type="text" class="form-control" placeholder="客戶 ID" />
          </div>
          <div class="col-12 col-md-3">
            <label class="form-label">個人識別碼</label>
            <input v-model.trim="form.personalId" type="text" class="form-control" placeholder="0000000000" />
          </div>

          <!-- 日期區間 -->
          <div class="col-12 col-md-6">
            <label class="form-label">日期區間</label>
            <div class="input-group">
              <input v-model="form.dateFrom" type="date" class="form-control" />
              <span class="input-group-text">至</span>
              <input v-model="form.dateTo" type="date" class="form-control" />
            </div>
          </div>

          <!-- 發票號碼／單據號碼 -->
          <div class="col-12 col-md-3">
            <label class="form-label">發票號碼</label>
            <input v-model.trim="form.invoiceNo" type="text" class="form-control" placeholder="AB12345678" />
          </div>
          <div class="col-12 col-md-3">
            <label class="form-label">單據號碼</label>
            <input v-model.trim="form.docNo" type="text" class="form-control" placeholder="單據號碼" />
          </div>

          <!-- 代理業者統編 -->
          <div class="col-12 col-md-3">
            <label class="form-label">代理業者統編</label>
            <input v-model.trim="form.agentId" type="text" class="form-control" placeholder="代理業者統編" />
          </div>

          <!-- 是否中獎／單據狀態／列印狀態 -->
          <div class="col-12 col-md-3">
            <label class="form-label">是否中獎</label>
            <select v-model="form.isWinning" class="form-select">
              <option value="">全部</option>
              <option value="Y">是</option>
              <option value="N">否</option>
            </select>
          </div>
          <div class="col-12 col-md-3">
            <label class="form-label">單據狀態</label>
            <select v-model="form.docStatus" class="form-select">
              <option value="">全部</option>
              <option value="ACTIVE">未作廢</option>
              <option value="VOID">已作廢</option>
            </select>
          </div>
          <div class="col-12 col-md-3">
            <label class="form-label">列印狀態</label>
            <select v-model="form.printStatus" class="form-select">
              <option value="">全部</option>
              <option value="PRINTED">已列印</option>
              <option value="UNPRINTED">未列印</option>
            </select>
          </div>

          <!-- 列印註記／是否顯示買受人地址 -->
          <div class="col-12 col-md-3">
            <label class="form-label">列印註記</label>
            <select v-model="form.printNote" class="form-select">
              <option value="">全部</option>
              <option value="WITH_NOTE">有註記</option>
              <option value="NO_NOTE">無註記</option>
            </select>
          </div>
          <div class="col-12 col-md-3">
            <label class="form-label">篩選有買受人地址</label>
            <select v-model="form.hasBuyerAddress" class="form-select">
              <option value="">全部</option>
              <option value="Y">是</option>
              <option value="N">否</option>
            </select>
          </div>

          <!-- 載具類型／載具號碼 -->
          <div class="col-12 col-md-3">
            <label class="form-label">載具類型</label>
            <select v-model="form.carrierType" class="form-select">
              <option value="">全部</option>
              <option value="PHONE_BARCODE">手機條碼</option>
              <option value="CITIZEN">自然人憑證</option>
              <option value="MEMBER">會員載具</option>
            </select>
          </div>
          <div class="col-12 col-md-3">
            <label class="form-label">載具號碼</label>
            <input v-model.trim="form.carrierNo" type="text" class="form-control" placeholder="/XXXXXX" />
          </div>

          <!-- 開立通知未送出 -->
          <div class="col-12 col-md-3">
            <label class="form-label">開立通知未送出</label>
            <select v-model="form.issueNotifyPending" class="form-select">
              <option value="">全部</option>
              <option value="Y">是</option>
              <option value="N">否</option>
            </select>
          </div>

          <!-- 額外顯示選項 -->
          <div class="col-12">
            <div class="form-check form-check-inline">
              <input v-model="form.showCurrencyStat" class="form-check-input" type="checkbox" id="currencyStat" />
              <label class="form-check-label" for="currencyStat">顯示幣別統計</label>
            </div>
            <div class="form-check form-check-inline">
              <input v-model="form.showPlatformStatus" class="form-check-input" type="checkbox" id="platformStatus" />
              <label class="form-check-label" for="platformStatus">
                顯示大平台處理狀態（C:完成、E:失敗、P:處理中）
              </label>
            </div>
          </div>
        </div>

        <!-- 操作列 -->
        <div class="d-flex gap-2 mt-3">
          <button class="btn btn-primary" @click="onSearch" :disabled="loading">查詢</button>

          <div class="dropdown">
            <button class="btn btn-outline-secondary dropdown-toggle" type="button" data-bs-toggle="dropdown">
              列印類型
            </button>
            <ul class="dropdown-menu">
              <li><a class="dropdown-item" href="#" @click.prevent="onPrint('voucher')">折讓單</a></li>
              <li><a class="dropdown-item" href="#" @click.prevent="onPrint('invoice')">發票</a></li>
              <li><a class="dropdown-item" href="#" @click.prevent="onPrint('qrcode')">QR Code 證明聯</a></li>
              <li><a class="dropdown-item" href="#" @click.prevent="onPrint('link')">聯類型</a></li>
              <li><a class="dropdown-item" href="#" @click.prevent="onPrint('trade')">交易類型</a></li>
            </ul>
          </div>

          <button class="btn btn-outline-success" @click="onExport" :disabled="loading || results.length === 0">
            匯出
          </button>
        </div>
      </div>
    </section>

    <!-- 查詢結果 -->
    <section class="card">
      <div class="card-header d-flex justify-content-between align-items-center">
        <span class="fw-semibold">查詢項目</span>
        <small class="text-muted">共 {{ total }} 筆</small>
      </div>

      <div class="table-responsive">
        <table class="table table-sm table-hover mb-0">
          <thead class="table-light">
            <tr>
              <th>發票號碼</th>
              <th>日期</th>
              <th>買受人統編</th>
              <th>客戶ID</th>
              <th>金額</th>
              <th>幣別</th>
              <th>列印狀態</th>
              <th v-if="form.showPlatformStatus">平台狀態</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in results" :key="row.id">
              <td>{{ row.invoiceNo }}</td>
              <td>{{ formatDate(row.date) }}</td>
              <td>{{ row.buyerId }}</td>
              <td>{{ row.customerId }}</td>
              <td class="text-end">{{ formatMoney(row.amount) }}</td>
              <td>{{ row.currency }}</td>
              <td>
                <span :class="row.printed ? 'badge bg-primary' : 'badge bg-secondary'">
                  {{ row.printed ? '已列印' : '未列印' }}
                </span>
              </td>
              <td v-if="form.showPlatformStatus">
                <span
                  :class="{
                    'badge bg-success': row.platformStatus === 'C',
                    'badge bg-danger': row.platformStatus === 'E',
                    'badge bg-warning text-dark': row.platformStatus === 'P'
                  }"
                >
                  {{ platformStatusText(row.platformStatus) }}
                </span>
              </td>
            </tr>
            <tr v-if="!loading && results.length === 0">
              <td colspan="8" class="text-center text-muted py-4">沒有符合條件的資料</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- 分頁 -->
      <div class="card-footer d-flex justify-content-between align-items-center">
        <div class="text-muted">第 {{ page }} / {{ totalPages }} 頁</div>
        <div class="btn-group">
          <button class="btn btn-outline-secondary btn-sm" @click="prevPage" :disabled="page <= 1 || loading">上一頁</button>
          <button class="btn btn-outline-secondary btn-sm" @click="nextPage" :disabled="page >= totalPages || loading">下一頁</button>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, computed } from 'vue'

type InvoiceRow = {
  id: string
  invoiceNo: string
  date: string // ISO date
  buyerId: string
  customerId: string
  amount: number
  currency: string
  printed: boolean
  platformStatus?: 'C' | 'E' | 'P'
}

const form = reactive({
  keyword: '',
  sellerId: '',
  buyerId: '',
  customerId: '',
  personalId: '',
  dateFrom: '',
  dateTo: '',
  invoiceNo: '',
  docNo: '',
  agentId: '',
  isWinning: '',
  docStatus: '',
  printStatus: '',
  printNote: '',
  hasBuyerAddress: '',
  carrierType: '',
  carrierNo: '',
  issueNotifyPending: '',
  showCurrencyStat: false,
  showPlatformStatus: true
})

const loading = ref(false)
const results = ref<InvoiceRow[]>([])
const total = ref(0)
const page = ref(1)
const pageSize = ref(20)

const totalPages = computed(() => Math.max(1, Math.ceil(total.value / pageSize.value)))

function formatDate(iso: string) {
  const d = new Date(iso)
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}
function formatMoney(n: number) {
  return new Intl.NumberFormat('zh-TW', { minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(n)
}
function platformStatusText(s?: 'C' | 'E' | 'P') {
  if (s === 'C') return '完成'
  if (s === 'E') return '失敗'
  if (s === 'P') return '處理中'
  return '-'
}

async function onSearch() {
  page.value = 1
  await fetchInvoices()
}

async function nextPage() {
  if (page.value >= totalPages.value) return
  page.value += 1
  await fetchInvoices()
}
async function prevPage() {
  if (page.value <= 1) return
  page.value -= 1
  await fetchInvoices()
}

async function fetchInvoices() {
  loading.value = true
  try {
    // TODO: 以實際 API 端點替換，下方為範例
    // const res = await fetch('/api/invoices/search', {
    //   method: 'POST',
    //   headers: { 'Content-Type': 'application/json' },
    //   body: JSON.stringify({ ...form, page: page.value, pageSize: pageSize.value })
    // })
    // const data = await res.json()
    // results.value = data.items
    // total.value = data.total

    // Mock 資料（請移除）
    await new Promise(r => setTimeout(r, 600))
    const mock: InvoiceRow[] = Array.from({ length: pageSize.value }, (_, i) => ({
      id: `${page.value}-${i}`,
      invoiceNo: `AB${String(12345678 + i).padStart(8, '0')}`,
      date: new Date(Date.now() - i * 86400000).toISOString(),
      buyerId: form.buyerId || '12345678',
      customerId: form.customerId || 'CUST-001',
      amount: 1000 + i * 10,
      currency: 'TWD',
      printed: i % 2 === 0,
      platformStatus: (['C', 'E', 'P'] as const)[i % 3]
    }))
    results.value = mock
    total.value = 120
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

async function onPrint(type: 'voucher' | 'invoice' | 'qrcode' | 'link' | 'trade') {
  // TODO: 依列印類型呼叫對應服務（可回傳PDF或觸發後端列印佇列）
  // 例：window.open(`/api/invoices/print?type=${type}&q=${encodeURIComponent(JSON.stringify(form))}`, '_blank')
  console.log('print:', type, form)
}

async function onExport() {
  // TODO: 呼叫匯出（CSV/Excel）
  // 例：
  // const res = await fetch('/api/invoices/export', { method: 'POST', body: JSON.stringify({ ...form }) })
  // const blob = await res.blob()
  // const url = URL.createObjectURL(blob)
  // const a = document.createElement('a')
  // a.href = url
  // a.download = `invoices_${Date.now()}.csv`
  // a.click()
  console.log('export:', form)
}

// 初次載入可選擇是否預設查詢
// fetchInvoices()
</script>

<style scoped>
.table thead th {
  white-space: nowrap;
}
.badge {
  font-size: 0.75rem;
}
</style>
