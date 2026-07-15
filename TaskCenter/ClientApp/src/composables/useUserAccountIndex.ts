// useUserAccountIndex.ts – UserAccountIndex.vue 的邏輯 composable。
// 遷移自 WebHome AccountController.AccountIndex / Inquire 及列管理動作。
// 由營業人資料管理頁「管理使用者」進入；營業人以加密 orgKeyId 限定，各列動作以加密 keyId（UID）傳遞。
import { ref, computed } from 'vue'
import {
  queryUserAccounts,
  activateUserAccount,
  deactivateUserAccount,
  sendUserConfirmation,
  deleteUserAccount,
  getUserAccountForEdit,
  commitUserAccount,
} from '@/services/user-account-api'
import type { UserAccountDatatable, UserAccountEdit } from '@/services/user-account-api'

// 對應後端 Naming.MemberStatusDefinition.Mark_To_Delete（1101 註記停用）
export const MEMBER_STATUS_MARK_TO_DELETE = 1101

export function useUserAccountIndex(orgKeyId: string, companyName: string) {
  // 會員狀態選項（對應舊版 AccountQuery.cshtml 之 Naming.BusinessRelationshipStatus）
  const statusOptions = [
    { value: 1101, label: '註記停用' },
    { value: 1102, label: '等待回覆確認' },
    { value: 1103, label: '人員已確認' },
  ]

  // 身份設定選項（對應舊版 EditUserProfile 之 Naming.RoleID：營業人 / 資料稽核員 / 平台系統管理員）
  const roleOptions = [
    { value: 51, label: '營業人' },
    { value: 64, label: '資料稽核員' },
    { value: 1, label: '平台系統管理員' },
  ]

  // 所屬營業人（顯示用）
  const orgName = ref(companyName)

  // 查詢條件
  const pid = ref('')
  const userName = ref('')
  const levelId = ref<number | ''>('')

  // 列表狀態
  const items = ref<UserAccountDatatable[]>([])
  const totalCount = ref(0)
  const page = ref(1)
  const pageSize = ref(10)
  const loading = ref(false)
  const error = ref('')
  const searched = ref(false)

  const totalPages = computed(() =>
    totalCount.value > 0 ? Math.ceil(totalCount.value / pageSize.value) : 0,
  )

  // 管理下拉選單：紀錄目前展開的列（以 keyId 為鍵），null 表示皆關閉
  const openMenuKey = ref<string | null>(null)

  function toggleMenu(keyId: string | null) {
    openMenuKey.value = openMenuKey.value === keyId ? null : keyId
  }

  function closeMenu() {
    openMenuKey.value = null
  }

  async function load() {
    if (!orgKeyId) {
      error.value = '無法取得營業人識別碼'
      searched.value = true
      return
    }
    loading.value = true
    error.value = ''
    try {
      const res = await queryUserAccounts({
        orgKeyId,
        pid: pid.value.trim() || undefined,
        userName: userName.value.trim() || undefined,
        levelId: levelId.value === '' ? undefined : levelId.value,
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

  function goToPage(target: number) {
    if (target >= 1 && target <= totalPages.value && target !== page.value) {
      page.value = target
      load()
    }
  }

  // --- 列管理動作（遷移自舊版 Account/Activate、Deactivate、SendConfirmation、DeleteItem）---
  // 沿用舊版以加密 keyId 傳遞 UID；破壞性 / 狀態變更動作先行確認，完成後重新載入列表。

  async function activate(user: UserAccountDatatable) {
    if (!user.keyId) return
    if (!window.confirm('確認啟用此帳號?')) return
    error.value = ''
    const res = await activateUserAccount(user.keyId)
    if (res.success) {
      await load()
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '啟用失敗'
    }
  }

  async function deactivate(user: UserAccountDatatable) {
    if (!user.keyId) return
    if (!window.confirm('確認停用此帳號?')) return
    error.value = ''
    const res = await deactivateUserAccount(user.keyId)
    if (res.success) {
      await load()
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '停用失敗'
    }
  }

  async function sendConfirmation(user: UserAccountDatatable) {
    if (!user.keyId) return
    error.value = ''
    const res = await sendUserConfirmation(user.keyId)
    if (res.success) {
      window.alert(res.message || '確認信已送出!!')
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '重送確認信失敗'
    }
  }

  async function removeUser(user: UserAccountDatatable) {
    if (!user.keyId) return
    if (!window.confirm('確認刪除此帳號?')) return
    error.value = ''
    const res = await deleteUserAccount(user.keyId)
    if (res.success) {
      await load()
    } else {
      error.value = res.errors?.length ? res.errors.join('\n') : res.message || '刪除失敗'
    }
  }

  // --- 編輯 / 新增帳號（遷移自舊版 UserProfileController.EditItem / Commit）---
  // 沿用舊版以加密 KeyID 傳遞識別碼；表單以對話框呈現，所屬營業人固定為目前管理的營業人。
  const editVisible = ref(false)
  const editLoading = ref(false)
  const editSaving = ref(false)
  const editError = ref('')
  const editData = ref<UserAccountEdit | null>(null)

  // 建立空白表單（新增用），所屬營業人帶入目前的 orgKeyId。
  function blankEdit(): UserAccountEdit {
    return {
      keyId: null,
      orgKeyId,
      pid: '',
      userName: '',
      email: '',
      address: '',
      phone: '',
      mobilePhone: '',
      phone2: '',
      roleId: null,
      password: '',
      password1: '',
      companyName: orgName.value,
    }
  }

  // 開啟編輯（傳入使用者）或新增（不傳）對話框。
  async function openEdit(user?: UserAccountDatatable) {
    editVisible.value = true
    editError.value = ''
    if (!user) {
      editData.value = blankEdit()
      editLoading.value = false
      return
    }
    if (!user.keyId) {
      editError.value = '無法取得帳號識別碼'
      return
    }
    editLoading.value = true
    editData.value = null
    try {
      const res = await getUserAccountForEdit(user.keyId)
      if (res.success && res.data) {
        // 帶入所屬營業人 orgKeyId 與空白密碼欄位供送出使用。
        editData.value = { ...res.data, orgKeyId, password: '', password1: '' }
      } else {
        editError.value = res.message || '載入帳號資料失敗'
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

  // 儲存編輯 / 新增（對應後端 Commit）。成功回傳 true。
  async function saveEdit(payload: UserAccountEdit) {
    editSaving.value = true
    editError.value = ''
    try {
      const res = await commitUserAccount({ ...payload, orgKeyId })
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

  return {
    // 顯示 / 查詢條件
    orgName,
    statusOptions,
    roleOptions,
    pid,
    userName,
    levelId,
    // 列表狀態
    items,
    totalCount,
    page,
    loading,
    error,
    searched,
    totalPages,
    // 管理下拉選單
    openMenuKey,
    toggleMenu,
    closeMenu,
    // 列管理動作
    activate,
    deactivate,
    sendConfirmation,
    removeUser,
    // 編輯 / 新增
    editVisible,
    editLoading,
    editSaving,
    editError,
    editData,
    openEdit,
    closeEdit,
    saveEdit,
    // 事件
    load,
    onSearch,
    goToPage,
  }
}
