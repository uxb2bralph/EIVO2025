<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/auth'

const auth = useAuthStore()
const router = useRouter()
const userName = computed(() => auth.user.value?.userName ?? auth.user.value?.pid ?? '使用者')
const roleName = computed(() => auth.user.value?.roleName ?? '')
const roleId   = computed(() => auth.user.value?.roleId)

const today = new Date().toLocaleDateString('zh-TW', {
  year: 'numeric', month: 'long', day: 'numeric', weekday: 'long'
})

// ─── 角色捷徑 ───────────────────────────────────────────────────────────────
// 每張捷徑卡以「路由名稱」指向 SPA 路由（見 router/index.ts），點擊後由 vue-router
// 導頁；部署根路徑（IIS 子應用程式如 /TaskCenter2025/）由 router base 自動帶入，
// 卡片本身不需知道實際子路徑。
// 尚未遷移的功能仍保留卡片，但以「未上線」停用標示（與側邊選單一致）；
// 待該模組加入 routes 後會自動亮起，毋須再改這裡。
const QUICK_LINKS = {
  // SysAdmin
  1: [
    { route: 'TrackCode',           icon: 'fa-key',          label: '字軌維護' },
    { route: 'OrganizationQuery',   icon: 'fa-building-o',   label: '營業人管理' },
    { route: 'InvoiceProcessQuery', icon: 'fa-search',       label: '發票查詢' },
    { route: 'UserAccount',         icon: 'fa-users',        label: '帳號管理' },
  ],
  // Seller
  51: [
    { route: 'CreateInvoice',       icon: 'fa-plus-circle',  label: '開立發票', primary: true },
    { route: 'InvoiceProcessQuery', icon: 'fa-search',       label: '查詢發票' },
    { route: 'InvoiceNoInterval',   icon: 'fa-list-ol',      label: '號碼維護' },
    { route: 'ProcessRequest',      icon: 'fa-tasks',        label: '工作清單' },
  ],
  // Buyer
  52: [
    { route: 'InvoiceProcessQuery', icon: 'fa-search',       label: '查詢發票' },
    { route: 'UserProfile',         icon: 'fa-user',         label: '帳號管理' },
  ],
  // Data auditor
  64: [
    { route: 'InvoiceAudit',        icon: 'fa-eye',          label: '稽核查詢' },
  ],
}
// 一般成員角色共用同一組捷徑
const MEMBER_ROLES = [54, 55, 61, 62, 63]
const MEMBER_LINKS = [
  { route: 'InvoiceProcessQuery',   icon: 'fa-search',       label: '查詢發票' },
  { route: 'UserProfile',           icon: 'fa-user',         label: '帳號管理' },
]

const quickLinks = computed(() => {
  const id = roleId.value
  if (id == null) return []
  const items = QUICK_LINKS[id] ?? (MEMBER_ROLES.includes(id) ? MEMBER_LINKS : [])
  // hasRoute：該模組是否已遷移為 SPA 路由；未遷移者停用顯示。
  return items.map(item => ({ ...item, available: router.hasRoute(item.route) }))
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
    <section class="quick-links" v-if="quickLinks.length">
      <template v-for="item in quickLinks" :key="item.route">
        <router-link
          v-if="item.available"
          :to="{ name: item.route }"
          class="quick-card"
          :class="{ primary: item.primary }"
        >
          <i class="fa" :class="item.icon"></i>
          <span>{{ item.label }}</span>
        </router-link>
        <span v-else class="quick-card is-disabled" aria-disabled="true" title="功能尚未上線">
          <i class="fa" :class="item.icon"></i>
          <span>{{ item.label }}</span>
          <span class="soon-badge">未上線</span>
        </span>
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

.quick-card.is-disabled {
  color: var(--color-muted);
  opacity: 0.5;
  cursor: not-allowed;
}
.quick-card.is-disabled i { color: var(--color-muted); }
.quick-card.is-disabled:hover {
  border-color: var(--color-border);
  background: var(--color-surface);
  transform: none;
}
.soon-badge {
  font-size: 0.6rem;
  line-height: 1;
  padding: 0.15rem 0.35rem;
  border: 1px solid var(--color-border);
  border-radius: 3px;
  color: var(--color-muted);
  background: rgba(255, 255, 255, 0.05);
  white-space: nowrap;
}

/* ── Footer ── */
.page-footer { color: var(--color-muted); font-size: 0.8rem; text-align: center; padding: 1rem 0; }
</style>
