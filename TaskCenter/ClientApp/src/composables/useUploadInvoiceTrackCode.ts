// useUploadInvoiceTrackCode.ts – 「上傳發票字軌號碼」匯入區塊的邏輯 composable。
// 遷移自 WebHome Views/InvoiceNo/Module/UploadInvoiceTrackCode.cshtml
// （下載範本 / 立即傳送）與 Module/PreviewInvoiceTrackCode.cshtml、EditInvoiceTrackCodeNo.cshtml
// （預覽逐列驗證結果、刪除不匯入之列、確定上傳）。
//
// 流程沿用舊版：上傳 Excel → 後端逐列驗證回傳預覽 → 前端可移除個別列 →
// 以通過驗證之 rowKey 回送匯入 → 以回傳結果覆寫預覽（顯示「已匯入成功」或錯誤原因）。
import { ref, computed } from 'vue'
import {
  downloadUploadTrackCodeSample,
  uploadTrackCodeToPreview,
  commitUploadedTrackCodes,
} from '@/services/upload-track-code-api'
import type { UploadTrackCodeRow } from '@/services/upload-track-code-api'

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

/** 期別 → 雙月標籤（如 1 → "01-02月"，沿用舊版 EditInvoiceTrackCodeNo.cshtml 格式） */
export function uploadPeriodLabel(periodNo: number | null): string {
  if (!periodNo) return ''
  const from = String(periodNo * 2 - 1).padStart(2, '0')
  const to = String(periodNo * 2).padStart(2, '0')
  return `${from}-${to}月`
}

/** 發票號碼 → 8 位補零（沿用舊版 {0:00000000} 顯示格式） */
export function formatInvoiceNo(no: number | null): string {
  return no === null || no === undefined ? '' : String(no).padStart(8, '0')
}

export function useUploadInvoiceTrackCode() {
  // 預覽 / 匯入結果列
  const rows = ref<UploadTrackCodeRow[]>([])
  const downloadingSample = ref(false)
  const uploading = ref(false)
  const committing = ref(false)
  const error = ref('')
  const message = ref('')

  /** 尚可匯入的列（預覽通過且未匯入）；沿用舊版「有可匯入列才顯示確定上傳」 */
  const committableKeys = computed(() =>
    rows.value.filter((r) => !r.message && r.rowKey).map((r) => r.rowKey as string),
  )
  const hasCommittable = computed(() => committableKeys.value.length > 0)

  /** 下載範本（對應舊版 GetUploadInvoiceTrackCodeSample） */
  async function downloadSample() {
    downloadingSample.value = true
    error.value = ''
    message.value = ''
    try {
      const blob = await downloadUploadTrackCodeSample()
      triggerBlobDownload(blob, 'UploadInvoiceTrackCodeSample.xlsx')
    } catch {
      error.value = '範本下載失敗'
    } finally {
      downloadingSample.value = false
    }
  }

  /** 上傳 Excel 取得預覽（對應舊版 UploadToPreview） */
  async function uploadFile(file: File) {
    uploading.value = true
    error.value = ''
    message.value = ''
    try {
      const res = await uploadTrackCodeToPreview(file)
      if (res.success && res.data) {
        rows.value = res.data
        if (!rows.value.length) {
          message.value = '上傳檔案未包含任何資料列。'
        }
      } else {
        rows.value = []
        error.value = res.errors?.length ? res.errors.join('\n') : res.message || '檔案上傳失敗'
      }
    } finally {
      uploading.value = false
    }
  }

  /** 移除預覽中的單一列（對應舊版每列「刪除」按鈕，僅自畫面移除、不匯入） */
  function removeRow(index: number) {
    rows.value.splice(index, 1)
  }

  /** 清除預覽結果 */
  function clearPreview() {
    rows.value = []
    error.value = ''
    message.value = ''
  }

  /** 匯入預覽通過之資料（對應舊版 CommitUpload），並以回傳結果覆寫預覽 */
  async function commitRows() {
    const keys = committableKeys.value
    if (!keys.length) {
      return false
    }

    committing.value = true
    error.value = ''
    message.value = ''
    try {
      const res = await commitUploadedTrackCodes(keys)
      if (res.success && res.data) {
        // 已匯入列不會再帶 rowKey，未匯入成功者保留訊息供修正後重新上傳。
        rows.value = res.data
        const succeeded = res.data.filter((r) => r.committed).length
        const failed = res.data.length - succeeded
        message.value = failed
          ? `匯入完成：成功 ${succeeded} 筆、失敗 ${failed} 筆，請確認各列訊息。`
          : `匯入完成：共 ${succeeded} 筆。`
        return true
      }
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '匯入失敗'
      return false
    } finally {
      committing.value = false
    }
  }

  return {
    // 狀態
    rows,
    downloadingSample,
    uploading,
    committing,
    error,
    message,
    hasCommittable,
    // 動作
    downloadSample,
    uploadFile,
    removeRow,
    clearPreview,
    commitRows,
    // 工具
    uploadPeriodLabel,
    formatInvoiceNo,
  }
}
