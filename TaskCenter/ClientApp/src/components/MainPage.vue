<script setup>
import { computed } from 'vue'
import { useAuthStore } from '@/auth'

const auth = useAuthStore()
const userName = computed(() => auth.user.value?.userName ?? auth.user.value?.pid ?? '使用者')
const roleName = computed(() => auth.user.value?.roleName ?? '')
const roleId   = computed(() => auth.user.value?.roleId)

const today = new Date().toLocaleDateString('zh-TW', {
  year: 'numeric', month: 'long', day: 'numeric', weekday: 'long'
})
</script>

<template>
  <div class="main-page">
    <!-- Welcome card -->
    <section class="welcome-card">
      <div class="welcome-inner">
        <div class="welcome-text">
          <h1 class="welcome-title">
            歡迎回來，<span class="highlight">{{ userName }}</span>
          </h1>
          <p class="welcome-sub">{{ today }}</p>
          <p class="welcome-role" v-if="roleName">
            <span class="role-chip">{{ roleName }}</span>
          </p>
        </div>
        <div class="welcome-icon">
          <i class="fa fa-file-text-o"></i>
        </div>
      </div>
    </section>

    <!-- Quick links -->
    <section class="quick-links" v-if="roleId">
      <!-- SysAdmin shortcuts -->
      <template v-if="roleId === 1">
        <a href="/TrackCode/Index" class="quick-card">
          <i class="fa fa-key"></i>
          <span>字軌維護</span>
        </a>
        <a href="/OrganizationQuery" class="quick-card">
          <i class="fa fa-building-o"></i>
          <span>營業人管理</span>
        </a>
        <a href="/InvoiceProcess/Index" class="quick-card">
          <i class="fa fa-search"></i>
          <span>發票查詢</span>
        </a>
        <a href="/Account/AccountIndex" class="quick-card">
          <i class="fa fa-users"></i>
          <span>帳號管理</span>
        </a>
      </template>

      <!-- Seller shortcuts -->
      <template v-else-if="roleId === 51">
        <a href="/InvoiceBusiness/CreateInvoice" class="quick-card primary">
          <i class="fa fa-plus-circle"></i>
          <span>開立發票</span>
        </a>
        <a href="/InvoiceProcess/Index" class="quick-card">
          <i class="fa fa-search"></i>
          <span>查詢發票</span>
        </a>
        <a href="/InvoiceNo/MaintainInvoiceNoInterval" class="quick-card">
          <i class="fa fa-list-ol"></i>
          <span>號碼維護</span>
        </a>
        <a href="/ProcessRequest/QueryIndex" class="quick-card">
          <i class="fa fa-tasks"></i>
          <span>工作清單</span>
        </a>
      </template>

      <!-- Buyer shortcuts -->
      <template v-else-if="roleId === 52">
        <a href="/InvoiceProcess/InquireForIncoming" class="quick-card">
          <i class="fa fa-search"></i>
          <span>查詢發票</span>
        </a>
        <a href="/UserProfile/EditMySelf" class="quick-card">
          <i class="fa fa-user"></i>
          <span>帳號管理</span>
        </a>
      </template>

      <!-- Member roles -->
      <template v-else-if="[54,55,61,62,63].includes(roleId)">
        <a href="/InvoiceProcess/Index" class="quick-card">
          <i class="fa fa-search"></i>
          <span>查詢發票</span>
        </a>
        <a href="/UserProfile/EditMySelf" class="quick-card">
          <i class="fa fa-user"></i>
          <span>帳號管理</span>
        </a>
      </template>

      <!-- Data auditor -->
      <template v-else-if="roleId === 64">
        <a href="/InvoiceAudit/QueryIndex" class="quick-card">
          <i class="fa fa-eye"></i>
          <span>稽核查詢</span>
        </a>
      </template>
    </section>

    <!-- System info -->
    <footer class="page-footer">
      <span>© 2025 電子發票系統 &middot; Powered by UXB2B</span>
    </footer>
  </div>
</template>

<style scoped>
.main-page {
  max-width: 1100px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

/* ── Welcome card ── */
.welcome-card {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 1rem;
  padding: 2rem;
  backdrop-filter: blur(10px);
}
.welcome-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}
.welcome-title { font-size: 1.6rem; font-weight: 700; color: var(--color-text); }
.highlight { color: var(--color-accent); }
.welcome-sub { color: var(--color-muted); margin-top: 0.4rem; font-size: 0.9rem; }
.welcome-role { margin-top: 0.6rem; }
.role-chip {
  display: inline-block;
  background: rgba(59, 199, 255, 0.12);
  color: var(--color-accent);
  border: 1px solid rgba(59, 199, 255, 0.3);
  border-radius: 999px;
  padding: 0.2rem 0.8rem;
  font-size: 0.8rem;
  font-weight: 600;
}
.welcome-icon { font-size: 3rem; color: rgba(59, 199, 255, 0.2); }

/* ── Quick links ── */
.quick-links {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  gap: 1rem;
}
.quick-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.65rem;
  padding: 1.25rem 1rem;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 0.75rem;
  text-decoration: none;
  color: var(--color-text);
  font-size: 0.9rem;
  font-weight: 500;
  transition: border-color 0.2s, background 0.2s, transform 0.15s;
  cursor: pointer;
}
.quick-card i { font-size: 1.6rem; color: var(--color-accent); }
.quick-card:hover {
  border-color: var(--color-accent);
  background: rgba(59, 199, 255, 0.06);
  transform: translateY(-2px);
}
.quick-card.primary { border-color: rgba(59, 199, 255, 0.4); background: rgba(59, 199, 255, 0.06); }
.quick-card.primary i { color: var(--color-accent-2); }

/* ── Footer ── */
.page-footer { color: var(--color-muted); font-size: 0.8rem; text-align: center; padding: 1rem 0; }
</style>
