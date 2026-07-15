// useInvoiceNumberApplyIndex.ts – InvoiceNumberApplyIndex.vue 的邏輯 composable。
// 遷移自 WebHome InvoiceNumberApplyController（QueryIndex / Query 頁 + 列動作 MoveFile / TransferOrganization / SetAll）。
// 申請資料以檔案系統上的 JSON 檔保存並以清單呈現（不分頁），列以加密後的檔案路徑（keyId）識別。
import { ref } from 'vue'
import {
  queryInvoiceNumberApplies,
  moveApplyFile,
  transferOrganization,
  downloadApplyWord,
} from '@/services/invoice-number-apply-api'
import type { InvoiceNumberApplyItem } from '@/services/invoice-number-apply-api'

/** 觸發瀏覽器下載 Blob（建立暫時性物件 URL 後點擊隱藏連結） */
function triggerBlobDownload(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  a.remove()
  URL.revokeObjectURL(url)
}

export function useInvoiceNumberApplyIndex() {
  // 查詢條件
  const businessId = ref('')

  // 列表狀態
  const items = ref<InvoiceNumberApplyItem[]>([])
  const loading = ref(false)
  const error = ref('')
  const message = ref('')
  const searched = ref(false)

  // 進行中的列動作 keyId（避免重複點擊）
  const busyKey = ref<string | null>(null)

  async function load() {
    loading.value = true
    error.value = ''
    message.value = ''
    try {
      const res = await queryInvoiceNumberApplies(businessId.value)
      if (res.success && res.data) {
        items.value = res.data
      } else {
        items.value = []
        error.value = res.errors?.length ? res.errors.join('\n') : res.message || '查詢失敗'
      }
    } finally {
      searched.value = true
      loading.value = false
    }
  }

  function onSearch() {
    load()
  }

  /** 歸檔（遷移自舊版 moveFile）。成功後由列表移除該列。 */
  async function archive(row: InvoiceNumberApplyItem) {
    if (!window.confirm(`確認將統編 ${row.businessId} 的申請資料歸檔?`)) {
      return
    }
    busyKey.value = row.keyId
    error.value = ''
    message.value = ''
    try {
      const res = await moveApplyFile(row.keyId)
      if (res.success) {
        items.value = items.value.filter((x) => x.keyId !== row.keyId)
        message.value = '歸檔完成.'
      } else {
        error.value = res.errors?.length ? res.errors.join('\n') : res.message || '歸檔失敗'
      }
    } finally {
      busyKey.value = null
    }
  }

  /** 轉營業人（遷移自舊版 transferCompany）。成功後由列表移除該列（後端已歸檔）。 */
  async function transfer(row: InvoiceNumberApplyItem) {
    if (!window.confirm(`確認將統編 ${row.businessId} 轉為營業人?`)) {
      return
    }
    busyKey.value = row.keyId
    error.value = ''
    message.value = ''
    try {
      const res = await transferOrganization(row.keyId)
      if (res.success) {
        items.value = items.value.filter((x) => x.keyId !== row.keyId)
        message.value = '營業人轉檔完成.'
      } else {
        error.value = res.errors?.length ? res.errors.join('\n') : res.message || '營業人轉換失敗'
      }
    } finally {
      busyKey.value = null
    }
  }

  /** 下載 Word zip（遷移自舊版 SetAll）。 */
  async function downloadWord(row: InvoiceNumberApplyItem) {
    busyKey.value = row.keyId
    error.value = ''
    message.value = ''
    try {
      const blob = await downloadApplyWord(row.businessId)
      triggerBlobDownload(blob, `ApplyWord_${row.businessId}.zip`)
    } catch {
      error.value = 'Word 下載失敗.'
    } finally {
      busyKey.value = null
    }
  }

  return {
    // 查詢條件
    businessId,
    // 列表狀態
    items,
    loading,
    error,
    message,
    searched,
    busyKey,
    // 事件 / 列動作
    load,
    onSearch,
    archive,
    transfer,
    downloadWord,
  }
}
