<!--
  UserAccountIndex.vue – 使用者帳號管理（查詢 + 列管理）。
  遷移自 WebHome AccountController.AccountIndex / Inquire。
  由營業人資料管理頁「管理使用者」進入：orgKeyId（加密 CompanyID）與 name（營業人名稱）以路由 query 傳入。
-->
<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useUserAccountIndex, MEMBER_STATUS_MARK_TO_DELETE } from '../composables/useUserAccountIndex'
import UserAccountEditDialog from './UserAccountEditDialog.vue'
import Pager from './Pager.vue'

const route = useRoute()
const router = useRouter()
const orgKeyId = String(route.query['org'] ?? '')
const companyName = String(route.query['name'] ?? '')

const {
  orgName,
  statusOptions,
  roleOptions,
  pid,
  userName,
  levelId,
  items,
  totalCount,
  page,
  loading,
  error,
  searched,
  totalPages,
  openMenuKey,
  toggleMenu,
  closeMenu,
  activate,
  deactivate,
  sendConfirmation,
  removeUser,
  editVisible,
  editLoading,
  editSaving,
  editError,
  editData,
  openEdit,
  closeEdit,
  saveEdit,
  load,
  onSearch,
  goToPage,
} = useUserAccountIndex(orgKeyId, companyName)

onMounted(() => {
  document.addEventListener('click', closeMenu)
  load()
})
onUnmounted(() => document.removeEventListener('click', closeMenu))

function goBack() {
  router.push({ name: 'OrganizationQuery' })
}
</script>

<template>
  <div class="user-account-page">
    <div class="page-header">
      <div>
        <h1>使用者管理</h1>
        <p v-if="orgName" class="org-name">{{ orgName }}</p>
      </div>
      <button class="btn ghost" @click="goBack">← 返回營業人管理</button>
    </div>

    <!-- 查詢條件 -->
    <div class="card query-card">
      <div class="query-grid">
        <div class="form-group">
          <label>帳號</label>
          <input
            v-model="pid"
            type="text"
            class="form-control"
            placeholder="請輸入帳號"
            @keyup.enter="onSearch"
          />
        </div>
        <div class="form-group">
          <label>姓名</label>
          <input
            v-model="userName"
            type="text"
            class="form-control"
            placeholder="請輸入會員名稱"
            @keyup.enter="onSearch"
          />
        </div>
        <div class="form-group">
          <label>會員狀態</label>
          <select v-model="levelId" class="form-select">
            <option value="">全部</option>
            <option v-for="opt in statusOptions" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </option>
          </select>
        </div>
      </div>
      <div class="query-actions">
        <button class="btn primary" :disabled="loading" @click="onSearch">
          <span v-if="!loading">查詢</span>
          <span v-else>查詢中…</span>
        </button>
      </div>
    </div>

    <!-- 查詢結果 -->
    <div class="card result-card">
      <div class="result-toolbar">
        <button class="btn primary" @click="openEdit()">＋ 新增帳號</button>
      </div>

      <div v-if="error" class="alert-error">{{ error }}</div>

      <div v-if="loading" class="state-msg">資料載入中…</div>

      <div v-else-if="items.length" class="table-wrap">
        <table class="result-table">
          <thead>
            <tr>
              <th>公司名稱</th>
              <th>角色</th>
              <th>會員名稱</th>
              <th>ID</th>
              <th>電子郵件</th>
              <th>管理</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in items" :key="user.keyId ?? ''">
              <td>{{ user.companyName }}</td>
              <td>{{ user.roleName }}</td>
              <td>{{ user.userName }}</td>
              <td>{{ user.pid }}</td>
              <td>{{ user.email }}</td>
              <td>
                <!-- @click.stop 避免冒泡到 document 的關閉監聽 -->
                <div class="action-dropdown" @click.stop>
                  <button
                    type="button"
                    class="btn action-toggle"
                    :aria-expanded="openMenuKey === user.keyId"
                    @click="toggleMenu(user.keyId)"
                  >
                    請選擇功能 <span class="caret"></span>
                  </button>
                  <ul v-if="openMenuKey === user.keyId" class="action-menu">
                    <li><a @click="closeMenu(); openEdit(user)">編輯</a></li>
                    <!-- 已註記停用（Mark_To_Delete）時，僅提供「啟用」 -->
                    <template v-if="user.levelId === MEMBER_STATUS_MARK_TO_DELETE">
                      <li><a @click="closeMenu(); activate(user)">啟用</a></li>
                    </template>
                    <template v-else>
                      <li><a @click="closeMenu(); sendConfirmation(user)">重送確認信</a></li>
                      <li><a @click="closeMenu(); deactivate(user)">停用</a></li>
                      <li><a @click="closeMenu(); removeUser(user)">刪除</a></li>
                    </template>
                  </ul>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-else-if="searched" class="state-msg">查無資料!!</div>

      <!-- 分頁 -->
      <Pager
        :page="page"
        :total-pages="totalPages"
        :total-count="totalCount"
        @change="goToPage"
      />
    </div>

    <!-- 編輯 / 新增帳號對話框（遷移自舊版 UserProfile/EditUserProfile） -->
    <UserAccountEditDialog
      :visible="editVisible"
      :loading="editLoading"
      :saving="editSaving"
      :error="editError"
      :data="editData"
      :role-options="roleOptions"
      @close="closeEdit"
      @save="saveEdit"
    />
  </div>
</template>

<style scoped>
.user-account-page {
  max-width: 1200px;
  margin: 0 auto;
}
.page-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.25rem;
}
.page-header h1 {
  font-size: 1.5rem;
  color: var(--color-text);
  margin: 0;
}
.org-name {
  margin: 0.35rem 0 0;
  color: var(--color-muted);
  font-size: 0.95rem;
}
.card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 0.75rem;
  padding: 1.5rem;
  margin-bottom: 1.25rem;
}
.query-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1rem;
}
.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}
.form-group label {
  font-size: 0.85rem;
  font-weight: 500;
  color: var(--color-muted);
}
.query-actions {
  margin-top: 1.25rem;
  display: flex;
  justify-content: flex-end;
}
.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.btn.ghost {
  background: transparent;
  border: 1px solid var(--color-border);
  color: var(--color-text);
}
.btn.ghost:hover:not(:disabled) {
  border-color: var(--color-accent);
}
.result-toolbar {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 1rem;
}
.table-wrap {
  overflow-x: auto;
}
.result-table {
  width: 100%;
  border-collapse: collapse;
  color: var(--color-text);
}
.result-table th,
.result-table td {
  padding: 0.6rem 0.75rem;
  text-align: left;
  border-bottom: 1px solid var(--color-border);
  white-space: nowrap;
  font-size: 0.9rem;
}
.result-table thead th {
  color: var(--color-muted);
  font-weight: 600;
}
.result-table tbody tr:hover {
  background: rgba(59, 199, 255, 0.06);
}
/* 管理下拉選單 */
.action-dropdown {
  position: relative;
  display: inline-block;
}
.action-toggle {
  background: var(--color-accent);
  border: 1px solid var(--color-accent);
  color: #fff;
  font-size: 0.85rem;
  padding: 0.35rem 0.7rem;
}
.action-toggle .caret {
  display: inline-block;
  margin-left: 0.35rem;
  border-top: 4px solid currentColor;
  border-right: 4px solid transparent;
  border-left: 4px solid transparent;
  vertical-align: middle;
}
.action-menu {
  position: absolute;
  right: 0;
  z-index: 10;
  min-width: 9.5rem;
  margin: 0.25rem 0 0;
  padding: 0.25rem 0;
  list-style: none;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 0.5rem;
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.25);
}
.action-menu li a {
  display: block;
  padding: 0.45rem 0.9rem;
  font-size: 0.85rem;
  color: var(--color-text);
  white-space: nowrap;
  cursor: pointer;
}
.action-menu li a:hover {
  background: rgba(59, 199, 255, 0.12);
  color: var(--color-accent);
}
.state-msg {
  text-align: center;
  color: var(--color-muted);
  padding: 2.5rem 0;
}
.alert-error {
  color: #ff8585;
  margin-bottom: 1rem;
  white-space: pre-line;
}
</style>
