/**
 * useE0501Settings.ts — E0501 自動取號設定對話框狀態
 *
 * 遷移自舊版 Organization/ApplyE0501Settings.cshtml（開啟時載入、變更後回寫）。
 * 由 OrganizationEditForm.vue 以編輯中營業人的加密 KeyID 開啟。
 */
import { ref } from 'vue'
import {
  getE0501Settings,
  commitE0501Settings,
  downloadBranchAssignmentTemplate,
  uploadBranchAssignments,
  type E0501Settings,
} from '@/services/organization-settings-api'

/** 觸發瀏覽器下載 Blob（建立暫時性物件 URL 後點擊隱藏連結） */
function triggerBlobDownload(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}

export function useE0501Settings() {
  const e0501Visible = ref(false)
  const e0501Loading = ref(false)
  const e0501Saving = ref(false)
  const e0501Error = ref('')
  const e0501Message = ref('')
  const e0501CompanyName = ref('')
  const e0501Data = ref<E0501Settings | null>(null)
  const e0501KeyId = ref<string | null>(null)
  // 主機構批次配號（範本下載 / Excel 匯入）；與上方 E0501 表單的存檔分開。
  const branchDownloading = ref(false)
  const branchUploading = ref(false)

  /** 開啟對話框並載入指定營業人的 E0501 設定 */
  async function openE0501(keyId: string | null, companyName?: string | null) {
    if (!keyId) {
      return
    }
    e0501Visible.value = true
    e0501Loading.value = true
    e0501Error.value = ''
    e0501Message.value = ''
    e0501CompanyName.value = companyName ?? ''
    e0501KeyId.value = keyId
    e0501Data.value = null
    try {
      const res = await getE0501Settings(keyId)
      if (res.success && res.data) {
        e0501Data.value = res.data
      } else {
        e0501Error.value = res.message || '載入 E0501 取號設定失敗'
      }
    } finally {
      e0501Loading.value = false
    }
  }

  function closeE0501() {
    e0501Visible.value = false
    e0501KeyId.value = null
    e0501Data.value = null
    e0501Error.value = ''
    e0501Message.value = ''
  }

  /** 儲存設定（對應後端 CommitE0501）。成功回傳 true。 */
  async function saveE0501(payload: {
    disableAutoUpdate: boolean
    initialLock: boolean
    reservedBooklets: number | null
  }): Promise<boolean> {
    if (!e0501KeyId.value) {
      e0501Error.value = '無法取得營業人識別碼'
      return false
    }
    e0501Saving.value = true
    e0501Error.value = ''
    e0501Message.value = ''
    try {
      const res = await commitE0501Settings({ keyId: e0501KeyId.value, ...payload })
      if (res.success) {
        e0501Message.value = res.message || '資料已儲存!!'
        // 反映後端實際存下的內容（沿用舊版儲存後重新載入）
        const reload = await getE0501Settings(e0501KeyId.value)
        if (reload.success && reload.data) {
          e0501Data.value = reload.data
        }
        return true
      }
      e0501Error.value = res.errors?.length ? res.errors.join('\n') : res.message || '儲存失敗'
      return false
    } finally {
      e0501Saving.value = false
    }
  }

  /** 下載主機構批次配號範本（對應後端 BranchAssignmentTemplate） */
  async function downloadBranchTemplate() {
    if (!e0501KeyId.value) {
      e0501Error.value = '無法取得營業人識別碼'
      return
    }
    branchDownloading.value = true
    e0501Error.value = ''
    e0501Message.value = ''
    try {
      const blob = await downloadBranchAssignmentTemplate(e0501KeyId.value)
      triggerBlobDownload(blob, '主機構批次配號範本.xlsx')
    } catch {
      e0501Error.value = '範本下載失敗'
    } finally {
      branchDownloading.value = false
    }
  }

  /**
   * 匯入主機構批次配號 Excel（對應後端 UploadBranchAssignments）。
   * 後端整批取代設定；成功後重新載入以反映實際存下的內容。成功回傳 true。
   */
  async function uploadBranchAssignmentsFile(file: File): Promise<boolean> {
    if (!e0501KeyId.value) {
      e0501Error.value = '無法取得營業人識別碼'
      return false
    }
    branchUploading.value = true
    e0501Error.value = ''
    e0501Message.value = ''
    try {
      const res = await uploadBranchAssignments(e0501KeyId.value, file)
      if (res.success) {
        e0501Message.value = res.message || '配號設定已匯入!!'
        const reload = await getE0501Settings(e0501KeyId.value)
        if (reload.success && reload.data) {
          e0501Data.value = reload.data
        }
        return true
      }
      // 後端以 errors 逐列回報，訊息以換行串接（對話框以 white-space: pre-line 顯示）。
      e0501Error.value = res.errors?.length
        ? `${res.message}\n${res.errors.join('\n')}`
        : res.message || '匯入失敗'
      return false
    } finally {
      branchUploading.value = false
    }
  }

  return {
    e0501Visible,
    e0501Loading,
    e0501Saving,
    e0501Error,
    e0501Message,
    e0501CompanyName,
    e0501Data,
    branchDownloading,
    branchUploading,
    openE0501,
    closeE0501,
    saveE0501,
    downloadBranchTemplate,
    uploadBranchAssignmentsFile,
  }
}
