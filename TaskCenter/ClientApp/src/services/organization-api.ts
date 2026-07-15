/**
 * organization-api.ts — 營業人資料查詢 API
 *
 * 對應後端 OrganizationQueryController（GET /api/OrganizationQuery）。
 */
import { apiRequest } from '@/services/api-service'
import type { ApiResponse, PagedResult } from '@/interfaces/api-response'

/** 營業人列表項目（對應後端 OrganizationDatatableDto） */
export interface OrganizationDatatable {
  companyId: number
  /** 加密後的 CompanyID（沿用舊版 KeyID 做法），供編輯等動作傳遞 */
  keyId: string | null
  companyName: string | null
  receiptNo: string | null
  undertakerName: string | null
  contactEmail: string | null
  statusLevel: number | null
  statusName: string | null
  goLiveDate: string | null
  expirationDate: string | null
  /** 是否為主機構（對應後端 Organization.MasterOrganization 是否存在） */
  isMaster: boolean
}

/**
 * 營業人編輯資料（對應後端 OrganizationEditDto / 舊版 OrganizationViewModel）。
 * 日期欄位以 ISO 字串往返；前端表單以 yyyy-MM-dd 呈現。
 */
export interface OrganizationEdit {
  keyId: string | null
  companyId: number | null
  // 基本資料
  receiptNo: string | null
  companyName: string | null
  addr: string | null
  phone: string | null
  fax: string | null
  undertakerName: string | null
  categoryId: number | null
  customerNo: string | null
  // 聯絡方式
  contactName: string | null
  contactTitle: string | null
  contactPhone: string | null
  contactMobilePhone: string | null
  contactEmail: string | null
  // 日期
  creationDate: string | null
  expirationDate: string | null
  authorizationNotBefore: string | null
  authorizationNotAfter: string | null
  invoiceRequestNotBefore: string | null
  invoiceRequestNotAfter: string | null
  taxNo: string | null
  // 設定項目
  setToPrintInvoice: boolean | null
  invoicePrintView: string | null
  c0401POSView: string | null
  allowancePrintView: string | null
  authorizationNo: string | null
  entrustToPrint: boolean | null
  downloadDataNumber: boolean | null
  uploadBranchTrackBlank: boolean | null
  autoBlankTrack: boolean | null
  autoBlankTrackEmittance: boolean | null
  printAll: boolean | null
  settingInvoiceType: number | null
  subscribeB2BInvoicePDF: boolean | null
  enableTrackCodeInvoiceNoValidation: boolean | null
  setToOutsourcingCS: boolean | null
  downloadDispatch: boolean | null
  setToNotifyCounterpartBySMS: boolean | null
  useB2BStandalone: boolean | null
  /** 字串型設定旗標（DisableC0401Template、ForcedAuditNo … 對應舊版 name="Settings" 複選） */
  settings: string[] | null
  // 訊息通知
  /** 已啟用的通知旗標位元值清單（對應舊版 name="NoticeStatus" 複選） */
  noticeStatus: number[] | null
  businessContactPhone: string | null
  customNotificationView: string | null
  customNotification: string | null
  /** 公司識別標章相對路徑（唯讀） */
  logoUrl: string | null
}

/** 營業人查詢條件（對應後端 OrganizationQueryDto） */
export interface OrganizationQuery {
  receiptNo?: string
  companyName?: string
  organizationStatus?: number
  categoryId?: number
  agentId?: number
  branchRelation?: boolean
  page?: number
  pageSize?: number
}

/**
 * 查詢營業人資料（分頁）。
 */
export function queryOrganizations(
  query: OrganizationQuery,
): Promise<ApiResponse<PagedResult<OrganizationDatatable>>> {
  // 移除 undefined / 空字串，避免送出多餘 query 參數
  const params: Record<string, unknown> = {}
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null && value !== '') {
      params[key] = value
    }
  }

  return apiRequest<PagedResult<OrganizationDatatable>>('/OrganizationQuery', {
    method: 'GET',
    params,
  })
}

/** 所屬經銷商下拉選項（對應後端 OrganizationAgentDto） */
export interface OrganizationAgent {
  /** 經銷商 CompanyID（作為 agentId 查詢值） */
  companyId: number
  receiptNo: string | null
  companyName: string | null
}

/**
 * 依關鍵字搜尋所屬經銷商候選清單（類別為經銷商的營業人），供查詢條件的 autocomplete 使用
 * （對應後端 OrganizationQuery/Agents，遷移自舊版 InquireOrganization.cshtml 之 AgentID 下拉）。
 * keyword 為統編前綴或名稱關鍵字；後端限制回傳筆數。
 */
export function getOrganizationAgents(keyword?: string): Promise<ApiResponse<OrganizationAgent[]>> {
  return apiRequest<OrganizationAgent[]>('/OrganizationQuery/Agents', {
    method: 'GET',
    params: keyword ? { keyword } : {},
  })
}

/** 主機構下拉選項（對應後端 HeadquarterDto） */
export interface Headquarter {
  /** 加密後的主機構 CompanyID（沿用舊版 KeyID 做法），供設為分支機構時傳遞 */
  keyId: string | null
  receiptNo: string | null
  companyName: string | null
}

/**
 * 依關鍵字搜尋主機構候選清單（已設定為主機構的營業人），供「設為分支機構」的 autocomplete 使用
 * （對應後端 OrganizationQuery/Headquarters，遷移自舊版 Home/SearchHeadquarter）。
 * keyword 為統編前綴或名稱關鍵字；為空時後端回傳空集合。
 */
export function searchHeadquarters(keyword: string): Promise<ApiResponse<Headquarter[]>> {
  return apiRequest<Headquarter[]>('/OrganizationQuery/Headquarters', {
    method: 'GET',
    params: { keyword },
  })
}

/**
 * 將勾選的營業人設為指定主機構的分支機構
 * （對應後端 OrganizationQuery/ApplyHeadquarter，遷移自舊版 Organization/ApplyHeadquarter）。
 * 主機構與各分支機構均以加密 KeyID 傳遞 CompanyID。
 */
export function applyHeadquarter(
  headquarterKeyId: string,
  branchKeyIds: string[],
): Promise<ApiResponse<void>> {
  return apiRequest<void>('/OrganizationQuery/ApplyHeadquarter', {
    method: 'POST',
    data: { headquarterKeyId, branchKeyIds },
  })
}

/**
 * 將來源營業人的收費標準複製到勾選的目標營業人
 * （對應後端 OrganizationQuery/CloneBillingPlan，遷移自舊版 Organization/CloneBillingPlan）。
 * 目標營業人原有的收費設定將被取代；來源與各目標均以加密 KeyID 傳遞 CompanyID。
 */
export function cloneBillingPlan(
  sourceKeyId: string,
  targetKeyIds: string[],
): Promise<ApiResponse<void>> {
  return apiRequest<void>('/OrganizationQuery/CloneBillingPlan', {
    method: 'POST',
    data: { sourceKeyId, targetKeyIds },
  })
}

/**
 * 載入單一營業人編輯資料（對應後端 OrganizationQuery/EditItem）。
 * 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
 */
export function getOrganizationForEdit(keyId: string): Promise<ApiResponse<OrganizationEdit>> {
  return apiRequest<OrganizationEdit>('/OrganizationQuery/EditItem', {
    method: 'GET',
    params: { keyId },
  })
}

/**
 * 儲存營業人編輯資料（對應後端 OrganizationQuery/CommitItem）。
 */
export function commitOrganization(payload: OrganizationEdit): Promise<ApiResponse<void>> {
  return apiRequest<void>('/OrganizationQuery/CommitItem', {
    method: 'POST',
    data: payload,
  })
}

/**
 * 停用營業人（對應後端 OrganizationQuery/DisableItem，遷移自舊版 Handling/DisableCompany）。
 * 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
 */
export function disableOrganization(keyId: string): Promise<ApiResponse<void>> {
  return apiRequest<void>('/OrganizationQuery/DisableItem', {
    method: 'POST',
    params: { keyId },
  })
}

/**
 * 啟用營業人（對應後端 OrganizationQuery/EnableItem）。
 * 將已註記停用（Mark_To_Delete）的營業人狀態改回啟用（Checked）。
 * 沿用停用做法，以加密 KeyID 傳遞 CompanyID。
 */
export function enableOrganization(keyId: string): Promise<ApiResponse<void>> {
  return apiRequest<void>('/OrganizationQuery/EnableItem', {
    method: 'POST',
    params: { keyId },
  })
}

/** 用戶端 G/W 設定（對應後端 GatewaySettingsDto） */
export interface GatewaySettings {
  keyId: string | null
  /** 傳送 Excel 發票開立方式（對應 Naming.InvoiceProcessType 之 Xlsx 系列） */
  defaultProcessType: number | null
  /** 目前已設定的 PKCS12(PFX) 憑證金鑰（對應 OrganizationToken.KeyID）；null 表示尚未設定 */
  certificateKeyId: string | null
}

/**
 * 載入用戶端 G/W 設定（對應後端 OrganizationQuery/GatewaySettings）。
 * 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
 */
export function getGatewaySettings(keyId: string): Promise<ApiResponse<GatewaySettings>> {
  return apiRequest<GatewaySettings>('/OrganizationQuery/GatewaySettings', {
    method: 'GET',
    params: { keyId },
  })
}

/**
 * 儲存「傳送 Excel 發票開立方式」（對應後端 OrganizationQuery/CommitDefaultProcessType）。
 * 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
 */
export function commitDefaultProcessType(
  keyId: string,
  defaultProcessType: number,
): Promise<ApiResponse<void>> {
  return apiRequest<void>('/OrganizationQuery/CommitDefaultProcessType', {
    method: 'POST',
    params: { keyId, defaultProcessType },
  })
}

/**
 * 上載並更新營業人 PKCS12(PFX) 憑證
 * （對應後端 OrganizationQuery/CommitCertificate，遷移自舊版 CertificateIdentityController.CommitItem）。
 * 以 multipart/form-data 傳送憑證檔與 PIN Code；沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
 */
export function commitCertificate(
  keyId: string,
  pin: string,
  pfxFile: File,
): Promise<ApiResponse<string>> {
  const formData = new FormData()
  formData.append('keyId', keyId)
  formData.append('pin', pin)
  formData.append('pfxFile', pfxFile)

  return apiRequest<string>('/OrganizationQuery/CommitCertificate', {
    method: 'POST',
    data: formData,
    headers: { 'Content-Type': 'multipart/form-data' },
  })
}

/** 店家 POS 機（對應後端 POSDeviceDto） */
export interface POSDevice {
  /** POS 機序號（新增時由後端配號） */
  deviceId: number
  /** POS 機編號 */
  posNo: string | null
}

/**
 * 載入店家 POS 機清單（對應後端 OrganizationQuery/POSDevices，遷移自舊版 InvoiceBusiness/ApplyPOSDevice）。
 * 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
 */
export function getPOSDevices(keyId: string): Promise<ApiResponse<POSDevice[]>> {
  return apiRequest<POSDevice[]>('/OrganizationQuery/POSDevices', {
    method: 'GET',
    params: { keyId },
  })
}

/**
 * 新增或編輯店家 POS 機編號（對應後端 OrganizationQuery/CommitPOS）。
 * deviceId 為 null 時視為新增；沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
 */
export function commitPOSDevice(
  keyId: string,
  deviceId: number | null,
  posNo: string,
): Promise<ApiResponse<POSDevice>> {
  return apiRequest<POSDevice>('/OrganizationQuery/CommitPOS', {
    method: 'POST',
    data: { keyId, deviceId, posNo },
  })
}

/**
 * 刪除店家 POS 機（對應後端 OrganizationQuery/DeletePOS）。
 * 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
 */
export function deletePOSDevice(keyId: string, deviceId: number): Promise<ApiResponse<void>> {
  return apiRequest<void>('/OrganizationQuery/DeletePOS', {
    method: 'POST',
    params: { keyId, deviceId },
  })
}

/** 發票經銷商候選項目（對應後端 IssuerAgentDto） */
export interface IssuerAgent {
  /** 加密後的經銷商 CompanyID（沿用舊版 KeyID 做法） */
  keyId: string | null
  receiptNo: string | null
  companyName: string | null
  /** 是否已指派給目標開立人 */
  selected: boolean
}

/**
 * 載入發票經銷商候選清單
 * （對應後端 OrganizationQuery/IssuerAgents，遷移自舊版 Organization/ApplyIssuerAgent）。
 * 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
 */
export function getIssuerAgents(keyId: string): Promise<ApiResponse<IssuerAgent[]>> {
  return apiRequest<IssuerAgent[]>('/OrganizationQuery/IssuerAgents', {
    method: 'GET',
    params: { keyId },
  })
}

/**
 * 設定發票經銷商（對應後端 OrganizationQuery/CommitIssuerAgent）。
 * 開立人與各經銷商均以加密 KeyID 傳遞 CompanyID；agentKeyIds 為勾選的經銷商 keyId 清單。
 */
export function commitIssuerAgent(
  keyId: string,
  agentKeyIds: string[],
): Promise<ApiResponse<void>> {
  return apiRequest<void>('/OrganizationQuery/CommitIssuerAgent', {
    method: 'POST',
    data: { keyId, agentKeyIds },
  })
}

/**
 * 設定為 B2B 營業人
 * （對應後端 OrganizationQuery/ApplyRelationship，遷移自舊版 Handling/ApplyRelationship）。
 * 將營業人加入企業群組成為 B2B 營業人；沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
 */
export function setB2BRelationship(keyId: string): Promise<ApiResponse<void>> {
  return apiRequest<void>('/OrganizationQuery/ApplyRelationship', {
    method: 'POST',
    params: { keyId },
  })
}

/**
 * 切換主機構設定
 * （對應後端 OrganizationQuery/CommitMaster，遷移自舊版 Handling/CommitMasterOrganization）。
 * 舊版為切換行為：尚未設定則設為主機構，已設定則取消；回傳切換後是否為主機構。
 * 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
 */
export function commitMasterOrganization(keyId: string): Promise<ApiResponse<boolean>> {
  return apiRequest<boolean>('/OrganizationQuery/CommitMaster', {
    method: 'POST',
    params: { keyId },
  })
}

/** 客製化服務設定 — SMTP 郵件伺服器（對應後端 CustomSmtpSettingsDto） */
export interface CustomSmtpSettings {
  keyId: string | null
  /** 是否已有啟用中的 SMTP 設定（Disabled 視為未設定） */
  configured: boolean
  host: string | null
  port: number | null
  enableSsl: boolean | null
  userName: string | null
  mailFrom: string | null
  /** 是否已設定登入密碼（密碼本身不回傳） */
  hasPassword: boolean
}

/**
 * 載入客製化 SMTP 設定
 * （對應後端 OrganizationQuery/CustomSmtpSettings，遷移自舊版 Organization/CustomSettings）。
 * 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法。
 */
export function getCustomSmtpSettings(keyId: string): Promise<ApiResponse<CustomSmtpSettings>> {
  return apiRequest<CustomSmtpSettings>('/OrganizationQuery/CustomSmtpSettings', {
    method: 'GET',
    params: { keyId },
  })
}

/**
 * 儲存客製化 SMTP 設定（對應後端 OrganizationQuery/CommitCustomSmtp）。
 * password 留空表示不變更原密碼；沿用舊版以加密 KeyID 傳遞 CompanyID。
 */
export function commitCustomSmtp(payload: {
  keyId: string
  host: string
  port: number | null
  enableSsl: boolean
  userName: string | null
  password: string | null
  mailFrom: string
}): Promise<ApiResponse<void>> {
  return apiRequest<void>('/OrganizationQuery/CommitCustomSmtp', {
    method: 'POST',
    data: payload,
  })
}

/**
 * 停用客製化 SMTP 設定（對應後端 OrganizationQuery/DisableCustomSmtp）。
 * 沿用舊版以加密 KeyID 傳遞 CompanyID。
 */
export function disableCustomSmtp(keyId: string): Promise<ApiResponse<void>> {
  return apiRequest<void>('/OrganizationQuery/DisableCustomSmtp', {
    method: 'POST',
    params: { keyId },
  })
}
