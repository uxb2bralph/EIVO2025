// useBusinessRelationshipIndex.ts – BusinessRelationshipIndex.vue 的邏輯 composable。
// 遷移自 WebHome BusinessRelationshipController（MaintainRelationship / InquireBusinessRelationship、
// 列管理動作 CommitItem / DeleteItem / Activate / Deactivate / SetEntrusting / SetEntrustToPrint、
// 互動式新增，以及範本下載 / Excel 匯入 UploadCounterpartBusiness）。
// 關係以複合鍵（masterId + relativeId + businessId）識別，列表分頁呈現並可就地編輯。
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import {
  queryBusinessRelationships,
  queryGroupMembers,
  commitBusinessRelationship,
  addBusinessRelationship,
  deleteBusinessRelationship,
  activateBusinessRelationship,
  deactivateBusinessRelationship,
  setEntrusting,
  setEntrustToPrint,
  downloadCounterpartTemplate,
  uploadCounterpart,
} from '@/services/business-relationship-api'
import type { BusinessRelationshipDatatable, GroupMember } from '@/services/business-relationship-api'

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

// 營業人類別選項（對應 Naming.InvoiceCenterBusinessType）。
export const BUSINESS_TYPE_OPTIONS = [
  { value: 1, label: '銷項' },
  { value: 2, label: '進項' },
]

// 銷項類別識別碼（自動接收僅對銷項顯示，沿用舊版 DataItem.cshtml）。
export const BUSINESS_TYPE_SALES = 1

export function useBusinessRelationshipIndex() {
  const router = useRouter()

  // 集團成員（主營業人）下拉選項。
  const groupMembers = ref<GroupMember[]>([])

  // 查詢條件
  const companyId = ref<number | ''>('')
  const receiptNo = ref('')
  const companyName = ref('')
  const businessType = ref<number | ''>('')

  // 列表狀態
  const items = ref<BusinessRelationshipDatatable[]>([])
  const totalCount = ref(0)
  const page = ref(1)
  const pageSize = ref(10)
  const loading = ref(false)
  const error = ref('')
  const searched = ref(false)

  const totalPages = computed(() =>
    totalCount.value > 0 ? Math.ceil(totalCount.value / pageSize.value) : 0,
  )

  async function loadGroupMembers() {
    const res = await queryGroupMembers()
    if (res.success && res.data) {
      groupMembers.value = res.data
    }
  }

  async function load() {
    loading.value = true
    error.value = ''
    cancelEdit() // 重新查詢時結束任何進行中的列編輯
    try {
      const res = await queryBusinessRelationships({
        companyId: companyId.value === '' ? undefined : companyId.value,
        receiptNo: receiptNo.value.trim() || undefined,
        companyName: companyName.value.trim() || undefined,
        businessType: businessType.value === '' ? undefined : businessType.value,
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
        error.value = res.errors?.length ? res.errors.join('\n') : res.message || '查詢失敗'
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

  function goToPage(target: number) {
    if (target >= 1 && target <= totalPages.value && target !== page.value) {
      page.value = target
      load()
    }
  }

  /** 複合鍵字串（masterId + relativeId + businessId），供列識別 */
  function rowKey(row: BusinessRelationshipDatatable): string {
    return `${row.masterId}-${row.relativeId}-${row.businessId}`
  }

  // --- 列內編輯（遷移自舊版 CommitItem）：名稱 / 電子郵件 / 地址 / 電話 / 客戶代碼 ---
  const editingKey = ref<string | null>(null)
  const editCompanyName = ref('')
  const editContactEmail = ref('')
  const editAddr = ref('')
  const editPhone = ref('')
  const editCustomerNo = ref('')
  const saving = ref(false)

  function startEdit(row: BusinessRelationshipDatatable) {
    editingKey.value = rowKey(row)
    editCompanyName.value = row.companyName ?? ''
    editContactEmail.value = row.contactEmail ?? ''
    editAddr.value = row.addr ?? ''
    editPhone.value = row.phone ?? ''
    editCustomerNo.value = row.customerNo ?? ''
    error.value = ''
  }

  function cancelEdit() {
    editingKey.value = null
  }

  async function saveEdit(row: BusinessRelationshipDatatable) {
    saving.value = true
    error.value = ''
    try {
      const res = await commitBusinessRelationship({
        masterId: row.masterId,
        relativeId: row.relativeId,
        businessId: row.businessId,
        companyName: editCompanyName.value.trim(),
        contactEmail: editContactEmail.value.trim() || null,
        addr: editAddr.value.trim() || null,
        phone: editPhone.value.trim() || null,
        customerNo: editCustomerNo.value.trim() || null,
      })
      if (res.success) {
        cancelEdit()
        await load()
        return true
      }
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '儲存失敗'
      return false
    } finally {
      saving.value = false
    }
  }

  // --- 列管理動作（刪除 / 啟用 / 停用 / 自動接收 / 主動列印）---
  async function removeItem(row: BusinessRelationshipDatatable) {
    if (!window.confirm('確認刪除此營業人關係?')) {
      return
    }
    error.value = ''
    const res = await deleteBusinessRelationship(row.businessId, row.masterId, row.relativeId)
    if (res.success) {
      await load()
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '刪除失敗'
    }
  }

  async function toggleActivation(row: BusinessRelationshipDatatable) {
    const confirmMsg = row.deactivated ? '確認啟用此營業人?' : '確認停用此營業人?'
    if (!window.confirm(confirmMsg)) {
      return
    }
    error.value = ''
    const res = row.deactivated
      ? await activateBusinessRelationship(row.businessId, row.masterId, row.relativeId)
      : await deactivateBusinessRelationship(row.businessId, row.masterId, row.relativeId)
    if (res.success) {
      await load()
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '設定失敗'
    }
  }

  async function toggleEntrusting(row: BusinessRelationshipDatatable) {
    const next = row.entrusting !== true
    const confirmMsg = next ? '確認啟用此營業人自動接收?' : '確認停用此營業人自動接收?'
    if (!window.confirm(confirmMsg)) {
      return
    }
    error.value = ''
    const res = await setEntrusting(row.businessId, row.masterId, row.relativeId, next)
    if (res.success) {
      await load()
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '設定失敗'
    }
  }

  async function toggleEntrustToPrint(row: BusinessRelationshipDatatable) {
    const next = row.entrustToPrint !== true
    const confirmMsg = next ? '確認啟用主動列印?' : '確認停用主動列印?'
    if (!window.confirm(confirmMsg)) {
      return
    }
    error.value = ''
    const res = await setEntrustToPrint(row.businessId, row.masterId, row.relativeId, next)
    if (res.success) {
      await load()
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '設定失敗'
    }
  }

  // --- 管理使用者（遷移自舊版 inquireUser）：導向使用者帳號管理頁，以相對營業人為範圍 ---
  function manageUsers(row: BusinessRelationshipDatatable) {
    if (!row.relativeKeyId) {
      error.value = '無法取得相對營業人識別碼'
      return
    }
    router.push({ name: 'UserAccount', query: { org: row.relativeKeyId, name: row.companyName ?? '' } })
  }

  // --- 新增相對營業人（遷移自舊版 AddItem 列 → CommitBusinessRelationshipViewModel）---
  const addMasterCompanyId = ref<number | ''>('')
  const addReceiptNo = ref('')
  const addCompanyName = ref('')
  const addBusinessType = ref<number>(BUSINESS_TYPE_SALES)
  const addContactEmail = ref('')
  const addAddr = ref('')
  const addPhone = ref('')
  const addCustomerNo = ref('')
  const adding = ref(false)

  async function addItem() {
    adding.value = true
    error.value = ''
    try {
      const res = await addBusinessRelationship({
        masterCompanyId: addMasterCompanyId.value === '' ? null : addMasterCompanyId.value,
        receiptNo: addReceiptNo.value.trim() || null,
        companyName: addCompanyName.value.trim() || null,
        businessType: addBusinessType.value,
        contactEmail: addContactEmail.value.trim() || null,
        addr: addAddr.value.trim() || null,
        phone: addPhone.value.trim() || null,
        customerNo: addCustomerNo.value.trim() || null,
      })
      if (res.success) {
        addReceiptNo.value = ''
        addCompanyName.value = ''
        addContactEmail.value = ''
        addAddr.value = ''
        addPhone.value = ''
        addCustomerNo.value = ''
        await load()
        return true
      }
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '新增失敗'
      return false
    } finally {
      adding.value = false
    }
  }

  // --- 範本下載 / Excel 匯入（遷移自舊版資料維護區塊）---
  const downloadingTemplate = ref(false)
  const uploading = ref(false)
  const uploadError = ref('')
  const uploadMessage = ref('')

  async function downloadTemplate() {
    downloadingTemplate.value = true
    uploadError.value = ''
    uploadMessage.value = ''
    try {
      const blob = await downloadCounterpartTemplate()
      triggerBlobDownload(blob, '相對營業人範本.xlsx')
    } catch {
      uploadError.value = '範本下載失敗'
    } finally {
      downloadingTemplate.value = false
    }
  }

  // 匯入相對營業人 Excel（對應後端 UploadCounterpart）。同步處理後下載含「處理狀態」欄的結果檔，並重新查詢列表。
  // 匯入的營業人類別沿用查詢條件之營業人類別（未選則預設銷項）。
  async function uploadCounterpartFile(file: File) {
    uploading.value = true
    uploadError.value = ''
    uploadMessage.value = ''
    try {
      const bt = businessType.value === '' ? undefined : businessType.value
      const result = await uploadCounterpart(file, bt)
      if (result.blob) {
        triggerBlobDownload(result.blob, '相對營業人(回應).xlsx')
        uploadMessage.value = '匯入完成，已下載處理結果檔，請確認各列處理狀態。'
        if (searched.value) {
          await load()
        }
      } else {
        uploadError.value = result.error || '匯入失敗'
      }
    } finally {
      uploading.value = false
    }
  }

  return {
    // 選項
    groupMembers,
    businessTypeOptions: BUSINESS_TYPE_OPTIONS,
    salesBusinessType: BUSINESS_TYPE_SALES,
    // 查詢條件
    companyId,
    receiptNo,
    companyName,
    businessType,
    // 列表狀態
    items,
    totalCount,
    page,
    loading,
    error,
    searched,
    totalPages,
    // 列內編輯
    editingKey,
    editCompanyName,
    editContactEmail,
    editAddr,
    editPhone,
    editCustomerNo,
    saving,
    rowKey,
    startEdit,
    cancelEdit,
    saveEdit,
    // 列管理動作
    removeItem,
    toggleActivation,
    toggleEntrusting,
    toggleEntrustToPrint,
    manageUsers,
    // 新增
    addMasterCompanyId,
    addReceiptNo,
    addCompanyName,
    addBusinessType,
    addContactEmail,
    addAddr,
    addPhone,
    addCustomerNo,
    adding,
    addItem,
    // 範本下載 / Excel 匯入
    downloadingTemplate,
    uploading,
    uploadError,
    uploadMessage,
    downloadTemplate,
    uploadCounterpartFile,
    // 事件 / 生命週期
    load,
    loadGroupMembers,
    onSearch,
    goToPage,
  }
}
