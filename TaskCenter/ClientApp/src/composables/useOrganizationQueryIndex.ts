// useOrganizationQueryIndex.ts – OrganizationQueryIndex.vue 的邏輯 composable。
// 遷移自 WebHome OrganizationQuery/InquireOrganization。
import { ref, computed, onMounted, onUnmounted } from 'vue'
import {
  queryOrganizations,
  getOrganizationForEdit,
  commitOrganization,
  disableOrganization,
  enableOrganization,
  getGatewaySettings,
  commitDefaultProcessType,
  commitCertificate,
  getPOSDevices,
  commitPOSDevice,
  deletePOSDevice,
  getIssuerAgents,
  commitIssuerAgent,
  setB2BRelationship,
  commitMasterOrganization,
} from '@/services/organization-api'
import type {
  OrganizationDatatable,
  OrganizationEdit,
  POSDevice,
  IssuerAgent,
} from '@/services/organization-api'

export function useOrganizationQueryIndex() {
  // 營業人類別（對應 CategoryDefinition.CategoryEnum）
  const categoryOptions = [
    { value: 15, label: '發票開立營業人' },
    { value: 20, label: '經銷商' },
    { value: 23, label: '境外電商' },
    { value: 18, label: '集團成員' },
    { value: 16, label: '相對營業人' },
  ]

  // 營業人狀態（對應 OrganizationStatus.CurrentLevel）
  const statusOptions = [
    { value: 1103, label: '已啟用' },
    { value: 1101, label: '已停用' },
  ]

  // 查詢條件
  const receiptNo = ref('')
  const companyName = ref('')
  const organizationStatus = ref<number | ''>('')
  const categoryId = ref<number | ''>('')

  // 列表狀態
  const items = ref<OrganizationDatatable[]>([])
  const totalCount = ref(0)
  const page = ref(1)
  const pageSize = ref(10)
  const pageWindowSize = ref(10) // 分頁器一次顯示的頁碼數量
  const loading = ref(false)
  const error = ref('')
  const searched = ref(false)

  const totalPages = computed(() =>
    totalCount.value > 0 ? Math.ceil(totalCount.value / pageSize.value) : 0,
  )
  const hasPrev = computed(() => page.value > 1)
  const hasNext = computed(() => page.value < totalPages.value)

  // 以目前頁碼為中心，計算要顯示的頁碼清單（夾在 1 ~ totalPages 之間）
  const pageNumbers = computed(() => {
    const total = totalPages.value
    const windowSize = pageWindowSize.value
    if (total <= 0) return []
    let start = page.value - Math.floor(windowSize / 2)
    start = Math.max(1, Math.min(start, total - windowSize + 1))
    start = Math.max(1, start)
    const end = Math.min(total, start + windowSize - 1)
    const result: number[] = []
    for (let p = start; p <= end; p++) result.push(p)
    return result
  })

  // 管理下拉選單：紀錄目前展開的列（以 companyId 為鍵），點擊外部即關閉
  const openMenuId = ref<number | null>(null)

  function toggleMenu(companyId: number) {
    openMenuId.value = openMenuId.value === companyId ? null : companyId
  }

  function closeMenu() {
    openMenuId.value = null
  }

  onMounted(() => document.addEventListener('click', closeMenu))
  onUnmounted(() => document.removeEventListener('click', closeMenu))

  // --- 編輯營業人（遷移自舊版 editCompany → Organization/EditItem）---
  // 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法；表單在本頁以對話框呈現。
  const editVisible = ref(false)
  const editLoading = ref(false)
  const editSaving = ref(false)
  const editError = ref('')
  const editData = ref<OrganizationEdit | null>(null)

  async function editCompany(org: OrganizationDatatable) {
    if (!org.keyId) {
      editError.value = '無法取得營業人識別碼'
      return
    }
    editVisible.value = true
    editLoading.value = true
    editError.value = ''
    editData.value = null
    try {
      const res = await getOrganizationForEdit(org.keyId)
      if (res.success && res.data) {
        editData.value = res.data
      } else {
        editError.value = res.message || '載入營業人資料失敗'
      }
    } finally {
      editLoading.value = false
    }
  }

  function closeEdit() {
    editVisible.value = false
    editData.value = null
    editError.value = ''
  }

  // 儲存編輯表單（對應舊版 commitOrganization → Organization/CommitItem）。
  async function saveEdit(payload: OrganizationEdit) {
    editSaving.value = true
    editError.value = ''
    try {
      const res = await commitOrganization(payload)
      if (res.success) {
        closeEdit()
        await load() // 重新載入列表反映變更
        return true
      }
      editError.value = res.errors?.length ? res.errors.join('\n') : res.message || '儲存失敗'
      return false
    } finally {
      editSaving.value = false
    }
  }

  // --- 停用營業人（遷移自舊版 disableCompany → Handling/DisableCompany）---
  // 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法；此為破壞性動作，先行確認。
  async function disableCompany(org: OrganizationDatatable) {
    if (!org.keyId) {
      error.value = '無法取得營業人識別碼'
      return
    }
    if (!window.confirm(`確定要停用「${org.companyName}」嗎？`)) {
      return
    }
    error.value = ''
    const res = await disableOrganization(org.keyId)
    if (res.success) {
      await load() // 重新載入列表反映狀態變更
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '停用失敗'
    }
  }

  // --- 標示並設定啟用（將已註記停用的營業人狀態改回啟用）---
  // 沿用停用做法，以加密 KeyID 傳遞 CompanyID。
  async function enableCompany(org: OrganizationDatatable) {
    if (!org.keyId) {
      error.value = '無法取得營業人識別碼'
      return
    }
    if (!window.confirm(`確定要將「${org.companyName}」設定為啟用嗎？`)) {
      return
    }
    error.value = ''
    const res = await enableOrganization(org.keyId)
    if (res.success) {
      await load() // 重新載入列表反映狀態變更
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '啟用失敗'
    }
  }

  // --- 用戶端 G/W 設定（遷移自舊版 gatewaySettings → Organization/GatewaySettings）---
  // 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法；此頁僅涵蓋「傳送 Excel 發票開立方式」。
  // 傳送 Excel 發票開立方式選項（對應舊版 GatewaySettings.cshtml 之 Naming.InvoiceProcessType）
  const processTypeOptions = [
    { value: 101, label: 'Excel通用格式' }, // F0401_Xlsx
    { value: 201, label: 'Excel格式加值中心配號' }, // F0401_Xlsx_Allocation_ByVAC
    { value: 301, label: 'Excel格式開立人配號' }, // F0401_Xlsx_Allocation_ByIssuer
    { value: 401, label: 'Excel格式境外電商專用' }, // F0401_Xlsx_CBE
    { value: 321, label: 'Excel格式A0401開立人配號' }, // A0101_Xlsx_Allocation_ByIssuer
  ]

  const gatewayVisible = ref(false)
  const gatewayLoading = ref(false)
  const gatewaySaving = ref(false)
  const gatewayError = ref('')
  const gatewayCompanyName = ref('')
  const gatewayKeyId = ref<string | null>(null)
  const gatewayProcessType = ref<number | ''>('')
  // 目前已設定的 PKCS12(PFX) 憑證金鑰（對應 OrganizationToken.KeyID）；null 表示尚未設定。
  const certKeyId = ref<string | null>(null)

  async function gatewaySettings(org: OrganizationDatatable) {
    if (!org.keyId) {
      error.value = '無法取得營業人識別碼'
      return
    }
    gatewayVisible.value = true
    gatewayLoading.value = true
    gatewayError.value = ''
    gatewayCompanyName.value = org.companyName ?? ''
    gatewayKeyId.value = org.keyId
    gatewayProcessType.value = ''
    certKeyId.value = null
    try {
      const res = await getGatewaySettings(org.keyId)
      if (res.success && res.data) {
        gatewayProcessType.value = res.data.defaultProcessType ?? ''
        certKeyId.value = res.data.certificateKeyId
      } else {
        gatewayError.value = res.message || '載入 G/W 設定失敗'
      }
    } finally {
      gatewayLoading.value = false
    }
  }

  function closeGateway() {
    gatewayVisible.value = false
    gatewayKeyId.value = null
    gatewayError.value = ''
    certUploading.value = false
    certError.value = ''
    certMessage.value = ''
    certKeyId.value = null
  }

  // --- PKCS12(PFX) 憑證上載（遷移自舊版 CertificateIdentityController.CommitItem）---
  // 與用戶端 G/W 設定共用同一對話框與 gatewayKeyId。
  const certUploading = ref(false)
  const certError = ref('')
  const certMessage = ref('')

  // 上載並更新 PFX 憑證（對應後端 CommitCertificate）。成功回傳 true。
  async function uploadCertificate(pfxFile: File | null, pin: string) {
    if (!gatewayKeyId.value) {
      certError.value = '無法取得營業人識別碼'
      return false
    }
    if (!pfxFile) {
      certError.value = '未選取檔案或檔案上傳失敗'
      return false
    }
    certUploading.value = true
    certError.value = ''
    certMessage.value = ''
    try {
      const res = await commitCertificate(gatewayKeyId.value, pin, pfxFile)
      if (res.success) {
        certMessage.value = res.message || '憑證更新成功'
        // 以後端回傳的新金鑰更新「原憑證金鑰」顯示。
        if (res.data) certKeyId.value = res.data
        return true
      }
      certError.value = res.errors?.length ? res.errors.join('\n') : res.message || '憑證上載失敗'
      return false
    } finally {
      certUploading.value = false
    }
  }

  // 儲存「傳送 Excel 發票開立方式」（對應舊版 CommitDefaultProcessType）。
  async function saveGateway() {
    if (!gatewayKeyId.value) {
      gatewayError.value = '無法取得營業人識別碼'
      return false
    }
    if (gatewayProcessType.value === '') {
      gatewayError.value = '請選擇傳送 Excel 發票開立方式'
      return false
    }
    gatewaySaving.value = true
    gatewayError.value = ''
    try {
      const res = await commitDefaultProcessType(gatewayKeyId.value, gatewayProcessType.value)
      if (res.success) {
        closeGateway()
        return true
      }
      gatewayError.value = res.errors?.length ? res.errors.join('\n') : res.message || '儲存失敗'
      return false
    } finally {
      gatewaySaving.value = false
    }
  }

  // --- 設定 POS 機號（遷移自舊版 applyPOS → InvoiceBusiness/ApplyPOSDevice）---
  // 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法；此頁以對話框呈現店家 POS 機維護。
  const posVisible = ref(false)
  const posLoading = ref(false)
  const posSaving = ref(false)
  const posError = ref('')
  const posCompanyName = ref('')
  const posKeyId = ref<string | null>(null)
  const posDevices = ref<POSDevice[]>([])

  async function applyPOS(org: OrganizationDatatable) {
    if (!org.keyId) {
      error.value = '無法取得營業人識別碼'
      return
    }
    posVisible.value = true
    posLoading.value = true
    posError.value = ''
    posCompanyName.value = org.companyName ?? ''
    posKeyId.value = org.keyId
    posDevices.value = []
    try {
      const res = await getPOSDevices(org.keyId)
      if (res.success && res.data) {
        posDevices.value = res.data
      } else {
        posError.value = res.message || '載入 POS 機資料失敗'
      }
    } finally {
      posLoading.value = false
    }
  }

  function closePOS() {
    posVisible.value = false
    posKeyId.value = null
    posDevices.value = []
    posError.value = ''
  }

  // 新增或編輯 POS 機編號（對應後端 CommitPOS）。deviceId 為 null 時新增。成功回傳 true。
  async function savePOSDevice(deviceId: number | null, posNo: string) {
    if (!posKeyId.value) {
      posError.value = '無法取得營業人識別碼'
      return false
    }
    const trimmed = posNo.trim()
    if (!trimmed) {
      posError.value = 'POS機編號錯誤!!'
      return false
    }
    posSaving.value = true
    posError.value = ''
    try {
      const res = await commitPOSDevice(posKeyId.value, deviceId, trimmed)
      if (res.success && res.data) {
        // 以後端回傳結果更新清單：編輯時取代原列，新增時附加。
        const idx = posDevices.value.findIndex((d) => d.deviceId === res.data!.deviceId)
        if (idx >= 0) {
          posDevices.value[idx] = res.data
        } else {
          posDevices.value.push(res.data)
        }
        return true
      }
      posError.value = res.errors?.length ? res.errors.join('\n') : res.message || '儲存失敗'
      return false
    } finally {
      posSaving.value = false
    }
  }

  // 刪除 POS 機（對應後端 DeletePOS）。成功回傳 true。
  async function removePOSDevice(device: POSDevice) {
    if (!posKeyId.value) {
      posError.value = '無法取得營業人識別碼'
      return false
    }
    if (!window.confirm(`確定要刪除 POS 機「${device.posNo}」嗎？`)) {
      return false
    }
    posSaving.value = true
    posError.value = ''
    try {
      const res = await deletePOSDevice(posKeyId.value, device.deviceId)
      if (res.success) {
        posDevices.value = posDevices.value.filter((d) => d.deviceId !== device.deviceId)
        return true
      }
      posError.value = res.errors?.length ? res.errors.join('\n') : res.message || '刪除失敗'
      return false
    } finally {
      posSaving.value = false
    }
  }

  // --- 設定發票經銷商（遷移自舊版 applyAgency → Organization/ApplyIssuerAgent）---
  // 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法；開立人與各經銷商均以 KeyID 往返。
  const agencyVisible = ref(false)
  const agencyLoading = ref(false)
  const agencySaving = ref(false)
  const agencyError = ref('')
  const agencyCompanyName = ref('')
  const agencyKeyId = ref<string | null>(null)
  const agencyAgents = ref<IssuerAgent[]>([])

  async function applyAgency(org: OrganizationDatatable) {
    if (!org.keyId) {
      error.value = '無法取得營業人識別碼'
      return
    }
    agencyVisible.value = true
    agencyLoading.value = true
    agencyError.value = ''
    agencyCompanyName.value = org.companyName ?? ''
    agencyKeyId.value = org.keyId
    agencyAgents.value = []
    try {
      const res = await getIssuerAgents(org.keyId)
      if (res.success && res.data) {
        agencyAgents.value = res.data
      } else {
        agencyError.value = res.message || '載入發票經銷商資料失敗'
      }
    } finally {
      agencyLoading.value = false
    }
  }

  function closeAgency() {
    agencyVisible.value = false
    agencyKeyId.value = null
    agencyAgents.value = []
    agencyError.value = ''
  }

  // 儲存勾選的經銷商（對應舊版 CommitIssuerAgent）；selectedKeyIds 為勾選經銷商的加密 KeyID。
  // 後端會做循環經銷檢查，失敗時以 error 訊息回報。成功回傳 true。
  async function saveAgency(selectedKeyIds: string[]) {
    if (!agencyKeyId.value) {
      agencyError.value = '無法取得營業人識別碼'
      return false
    }
    agencySaving.value = true
    agencyError.value = ''
    try {
      const res = await commitIssuerAgent(agencyKeyId.value, selectedKeyIds)
      if (res.success) {
        closeAgency()
        return true
      }
      agencyError.value = res.errors?.length ? res.errors.join('\n') : res.message || '儲存失敗'
      return false
    } finally {
      agencySaving.value = false
    }
  }

  // --- 設定為 B2B 營業人（遷移自舊版 applyRelationship → Handling/ApplyRelationship）---
  // 沿用舊版以加密 KeyID 傳遞 CompanyID 的做法；此為確認動作，成功後以訊息回報。
  async function applyRelationship(org: OrganizationDatatable) {
    if (!org.keyId) {
      error.value = '無法取得營業人識別碼'
      return
    }
    if (!window.confirm(`確定要將「${org.companyName}」設定為 B2B 營業人嗎？`)) {
      return
    }
    error.value = ''
    const res = await setB2BRelationship(org.keyId)
    if (res.success) {
      // 後端回傳「設定完成!!」或「該開立人已是B2B營業人!!」，直接提示使用者。
      window.alert(res.message || '設定完成!!')
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '設定失敗'
    }
  }

  // --- 切換主機構設定（遷移自舊版 commitMaster → Handling/CommitMasterOrganization）---
  // 沿用舊版切換行為：尚未設定則設為主機構，已設定則取消；以加密 KeyID 傳遞 CompanyID。
  // 列表以切換開關呈現 isMaster，故各分支皆重新載入列表以還原/反映開關狀態。
  async function commitMaster(org: OrganizationDatatable) {
    if (!org.keyId) {
      error.value = '無法取得營業人識別碼'
      return
    }
    const willBeMaster = !org.isMaster
    // const confirmMsg = willBeMaster
    //   ? `確定要將「${org.companyName}」設定為主機構嗎？`
    //   : `確定要取消「${org.companyName}」的主機構設定嗎？`
    // if (!window.confirm(confirmMsg)) {
    //   await load() // 使用者取消：重新載入以還原切換開關的顯示狀態
    //   return
    // }
    error.value = ''
    const res = await commitMasterOrganization(org.keyId)
    if (res.success) {
      // await load() // 重新載入列表反映主機構狀態變更
      org.isMaster = willBeMaster // 直接更新切換開關的顯示狀態
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '設定失敗'
      await load() // 還原切換開關的顯示狀態
    }
  }

  function inquireUser(org: OrganizationDatatable) {
    // TODO: 管理使用者（舊版 inquireUser）
    console.warn('inquireUser 尚未實作', org.companyId)
  }

  function customSettings(org: OrganizationDatatable) {
    // TODO: 客製化服務設定（舊版 customSettings，需加密 KeyID）
    console.warn('customSettings 尚未實作', org.companyId)
  }

  function formatDate(value: string | null): string {
    if (!value) return ''
    const d = new Date(value)
    if (Number.isNaN(d.getTime())) return ''
    const y = d.getFullYear()
    const m = String(d.getMonth() + 1).padStart(2, '0')
    const day = String(d.getDate()).padStart(2, '0')
    return `${y}/${m}/${day}`
  }

  async function load() {
    loading.value = true
    error.value = ''
    try {
      const res = await queryOrganizations({
        receiptNo: receiptNo.value.trim() || undefined,
        companyName: companyName.value.trim() || undefined,
        organizationStatus: organizationStatus.value === '' ? undefined : organizationStatus.value,
        categoryId: categoryId.value === '' ? undefined : categoryId.value,
        page: page.value,
        pageSize: pageSize.value,
      })
      if (res.success && res.data) {
        items.value = res.data.items
        totalCount.value = res.data.totalCount
        page.value = res.data.pageNumber
      } else {
        items.value = []
        totalCount.value = 0
        error.value = res.message || '查詢失敗'
      }
    } finally {
      searched.value = true
      loading.value = false
    }
  }

  function onSearch() {
    page.value = 1
    load()
  }

  function prevPage() {
    if (hasPrev.value) {
      page.value--
      load()
    }
  }

  function nextPage() {
    if (hasNext.value) {
      page.value++
      load()
    }
  }

  function goToPage(target: number) {
    if (target >= 1 && target <= totalPages.value && target !== page.value) {
      page.value = target
      load()
    }
  }

  return {
    // 查詢條件
    receiptNo,
    companyName,
    organizationStatus,
    categoryId,
    categoryOptions,
    statusOptions,
    // 列表狀態
    items,
    totalCount,
    page,
    loading,
    error,
    searched,
    totalPages,
    hasPrev,
    hasNext,
    pageNumbers,
    // 管理下拉選單
    openMenuId,
    toggleMenu,
    closeMenu,
    // 編輯營業人
    editVisible,
    editLoading,
    editSaving,
    editError,
    editData,
    closeEdit,
    saveEdit,
    // 用戶端 G/W 設定
    processTypeOptions,
    gatewayVisible,
    gatewayLoading,
    gatewaySaving,
    gatewayError,
    gatewayCompanyName,
    gatewayProcessType,
    closeGateway,
    saveGateway,
    // PKCS12(PFX) 憑證上載
    certUploading,
    certError,
    certMessage,
    certKeyId,
    uploadCertificate,
    // 設定 POS 機號
    posVisible,
    posLoading,
    posSaving,
    posError,
    posCompanyName,
    posDevices,
    closePOS,
    savePOSDevice,
    removePOSDevice,
    // 設定發票經銷商
    agencyVisible,
    agencyLoading,
    agencySaving,
    agencyError,
    agencyCompanyName,
    agencyAgents,
    closeAgency,
    saveAgency,
    // 管理功能
    editCompany,
    disableCompany,
    enableCompany,
    gatewaySettings,
    applyPOS,
    applyAgency,    
    applyRelationship,
    commitMaster,
    inquireUser,
    customSettings,
    // 工具 / 事件
    formatDate,
    onSearch,
    prevPage,
    nextPage,
    goToPage,
  }
}
