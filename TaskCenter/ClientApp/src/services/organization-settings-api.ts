/**
 * organization-settings-api.ts — 營業人客製化設定 API
 *
 * 對應後端 OrganizationSettingsController（/api/OrganizationSettings）。
 * 提供 E0501 自動取號設定（遷移自舊版 Organization/ApplyE0501Settings），
 * 以及主機構批次配號設定的 Excel 範本下載 / 匯入。
 */
import { apiRequest, apiDownloadBlob } from '@/services/api-service'
import type { ApiResponse } from '@/interfaces/api-response'

/** 主機構批次配號的單一分支機構設定（對應後端 BranchInvoiceNoAssignmentDto） */
export interface BranchInvoiceNoAssignment {
  /** 分支機構統一編號 */
  receiptNo: string | null
  /** 分支機構名稱（後端由統編對應帶出；查無對應時為 null） */
  companyName: string | null
  /** 配號本組數（每組 50 號）；空值表示未指定 */
  booklets: number | null
  /** 配號後預設鎖定字軌號碼區間 */
  initialLock: boolean
}

/** E0501 自動取號設定（對應後端 E0501SettingsDto） */
export interface E0501Settings {
  /** 加密後的 CompanyID（沿用舊版 KeyID 做法） */
  keyId: string | null
  /** 停用 E0501 自動取號 */
  disableAutoUpdate: boolean
  /** 自動取號後預設鎖定字軌號碼區間 */
  initialLock: boolean
  /** 保留本組數（每組 50 號）；空值表示不保留 */
  reservedBooklets: number | null
  /** 主機構批次配號設定；僅供顯示，由 Excel 匯入維護 */
  branchAssignments: BranchInvoiceNoAssignment[]
}

/**
 * 載入 E0501 自動取號設定（對應後端 OrganizationSettings/E0501）。
 * 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
 */
export function getE0501Settings(keyId: string): Promise<ApiResponse<E0501Settings>> {
  return apiRequest<E0501Settings>('/OrganizationSettings/E0501', {
    method: 'GET',
    params: { keyId },
  })
}

/**
 * 儲存 E0501 自動取號設定（對應後端 OrganizationSettings/CommitE0501）。
 * 後端僅異動 E0501 相關欄位，同一份設定 JSON 內的其餘項目原樣保留。
 */
export function commitE0501Settings(payload: {
  keyId: string
  disableAutoUpdate: boolean
  initialLock: boolean
  reservedBooklets: number | null
}): Promise<ApiResponse<void>> {
  return apiRequest<void>('/OrganizationSettings/CommitE0501', {
    method: 'POST',
    data: payload,
  })
}

/**
 * 下載主機構批次配號範本（對應後端 OrganizationSettings/BranchAssignmentTemplate）。
 * 「配號設定」工作表已帶出現行設定（或以該主機構的分支機構清單預填），另附「分支機構」工作表供對照。
 * 回傳 Excel Blob；呼叫方負責觸發瀏覽器下載。可能拋出例外。
 */
export function downloadBranchAssignmentTemplate(keyId: string): Promise<Blob> {
  return apiDownloadBlob(
    `/OrganizationSettings/BranchAssignmentTemplate?keyId=${encodeURIComponent(keyId)}`,
  )
}

/**
 * 匯入主機構批次配號 Excel（對應後端 OrganizationSettings/UploadBranchAssignments）。
 * 以 multipart/form-data 上傳單一 Excel 檔（須含「配號設定」工作表），後端同步處理後
 * 【整批取代】該營業人的配號設定；任一列有誤即整批不儲存，錯誤明細回傳於 errors。
 */
export function uploadBranchAssignments(
  keyId: string,
  file: File,
): Promise<ApiResponse<BranchInvoiceNoAssignment[]>> {
  const formData = new FormData()
  formData.append('keyId', keyId)
  formData.append('excelFile', file)

  return apiRequest<BranchInvoiceNoAssignment[]>('/OrganizationSettings/UploadBranchAssignments', {
    method: 'POST',
    data: formData,
    headers: { 'Content-Type': 'multipart/form-data' },
  })
}
