<script setup>
import { onMounted, computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/auth'

const auth = useAuthStore()
const router = useRouter()

// Sidebar collapse state
const sidebarOpen = ref(true)

onMounted(async () => {
  if (!auth.isLoggedIn.value) {
    await auth.getMe()
    if (!auth.isLoggedIn.value) {
      router.push('/Login')
    }
  }
})

function handleLogout() {
  auth.logout()
  router.push('/Login')
}

// ─── Role-based menu definitions ────────────────────────────────────────────
// Map roleId → array of menu groups { label, icon, items: [{label, href}] }

const ROLE_SYS = 1
const ROLE_SELLER = 51
const ROLE_BUYER = 52
const ROLE_GUEST = 53
const ROLE_NETWORKSELLER = 54
const ROLE_GOOGLETW = 55
const ROLE_GROUP_MEMBER = 61
const ROLE_RELATIVE_ENTITY = 62
const ROLE_BRANCH_ENTITY = 63
const ROLE_DATA_AUDITOR = 64

const menusByRole = {
  [ROLE_SYS]: [
    {
      label: '系統管理維護', icon: 'fa-cogs',
      items: [
        { label: '電子發票字軌維護', href: '/TrackCode/Index' },
        { label: '電子發票中獎號碼維護', href: '/WinningNumber/Index' },
        { label: '登錄掛號郵件號碼', href: '/Handling/MailTracking' },
      ]
    },
    {
      label: '會員管理維護', icon: 'fa-users',
      items: [
        { label: '使用者帳號管理', href: '/Account/AccountIndex' },
        { label: '營業人資料管理', href: '/OrganizationQuery' },
        { label: '相對營業人資料管理', href: '/BusinessRelationship/MaintainRelationship' },
        { label: '新登錄營業人資料受理', href: '/InvoiceNumberApply/QueryIndex' },
      ]
    },
    {
      label: '發票作業', icon: 'fa-file-text-o',
      items: [
        { label: '電子發票號碼維護', href: '/InvoiceNo/MaintainInvoiceNoInterval' },
        { label: '資料查詢／列印／匯出', href: '/InvoiceProcess/Index' },
        { label: '線上開立發票', href: '/InvoiceBusiness/CreateInvoice' },
        { label: '線上作廢發票', href: '/InvoiceProcess/InquireToCancel' },
        { label: '線上開立折讓證明', href: '/InvoiceProcess/InquireToIssueAllowance' },
        { label: '上期發票空白號碼查詢', href: '/InvoiceNo/VacantNoIndex' },
        { label: '下載MIG檔案', href: '/InvoiceProcess/InquireToMIG' },
        { label: '核准重印發票', href: '/InvoiceProcess/InquireToAuthorize' },
        { label: '註銷發票', href: '/InvoiceProcess/InquireToVoid' },
        { label: '核准註銷發票', href: '/InvoiceProcess/AllowToVoid' },
      ]
    },
    {
      label: '發票通知', icon: 'fa-bell-o',
      items: [
        { label: '重送開立發票通知', href: '/InvoiceProcess/IssuingNotice' },
        { label: '重送發票中獎通知', href: '/InvoiceProcess/InquireToNotifyWinning' },
      ]
    },
    {
      label: '統計報表', icon: 'fa-bar-chart',
      items: [
        { label: '發票明細查詢', href: '/InvoiceQuery/InvoiceReport' },
        { label: '發票統計表', href: '/InvoiceQuery/InvoiceSummary' },
        { label: '中獎統計表', href: '/WinningInvoice/ReportIndex' },
        { label: '捐贈統計表', href: '/DonatedInvoice/ReportIndex' },
        { label: '媒體申報檔匯出', href: '/InvoiceQuery/InvoiceMediaReport' },
        { label: '下載發票月報表', href: '/InvoiceQuery/MonthlyReport' },
      ]
    },
  ],

  [ROLE_SELLER]: [
    {
      label: '系統使用設定', icon: 'fa-wrench',
      items: [
        { label: '帳號管理', href: '/UserProfile/EditMySelf' },
        { label: '營業人資料管理', href: '/UserProfile/EditMyBusiness' },
        { label: '使用者管理', href: '/Account/AccountIndex' },
        { label: '相對營業人資料維護', href: '/BusinessRelationship/MaintainRelationship' },
        { label: '常用品項維護', href: '/ProductCatalog/QueryIndex' },
      ]
    },
    {
      label: '發票開立', icon: 'fa-pencil-square-o',
      items: [
        { label: '電子發票號碼維護', href: '/InvoiceNo/MaintainInvoiceNoInterval' },
        { label: '線上開立發票', href: '/InvoiceBusiness/CreateInvoice' },
        { label: '線上作廢發票', href: '/InvoiceProcess/InquireToCancel' },
        { label: '線上開立折讓證明', href: '/InvoiceProcess/InquireToIssueAllowance' },
        { label: '線上註銷發票', href: '/InvoiceProcess/InquireToVoid' },
        { label: 'A0101接收待確認', href: '/InvoiceProcess/DealReceivedA0101' },
        { label: 'A0301退回待確認', href: '/InvoiceProcess/DealReceivedA0301' },
        { label: 'A0201接收待確認', href: '/InvoiceProcess/DealReceivedA0201' },
        { label: 'B0101接收待確認', href: '/AllowanceProcess/DealReceivedB0101' },
        { label: 'B0201接收待確認', href: '/AllowanceProcess/DealReceivedB0201' },
      ]
    },
    {
      label: '查詢與報表', icon: 'fa-search',
      items: [
        { label: '資料查詢／列印／匯出', href: '/InvoiceProcess/Index' },
        { label: '上期發票空白號碼查詢', href: '/InvoiceNo/VacantNoIndex' },
        { label: '發票媒體申報檔查詢', href: '/InvoiceQuery/InvoiceMediaReport' },
        { label: '下載MIG檔案', href: '/InvoiceProcess/InquireToMIG' },
        { label: '發票統計表', href: '/InvoiceQuery/InvoiceSummary' },
      ]
    },
    {
      label: '訊息通知', icon: 'fa-envelope-o',
      items: [
        { label: '重送開立發票通知', href: '/InvoiceProcess/IssuingNotice' },
        { label: '核准重印發票', href: '/InvoiceProcess/InquireToAuthorize' },
        { label: '工作清單', href: '/ProcessRequest/QueryIndex' },
      ]
    },
  ],

  [ROLE_BUYER]: [
    {
      label: '帳號管理', icon: 'fa-user',
      items: [
        { label: '帳號管理', href: '/UserProfile/EditMySelf' },
      ]
    },
    {
      label: '查詢', icon: 'fa-search',
      items: [
        { label: '資料查詢／列印／匯出', href: '/InvoiceProcess/InquireForIncoming' },
      ]
    },
  ],

  [ROLE_DATA_AUDITOR]: [
    {
      label: '稽核查詢', icon: 'fa-eye',
      items: [
        { label: '資料查詢', href: '/InvoiceAudit/QueryIndex' },
      ]
    },
  ],
}

// ROLE_NETWORKSELLER, ROLE_GOOGLETW and group roles share the Member menu
const memberMenu = [
  {
    label: '帳號設定', icon: 'fa-user',
    items: [
      { label: '帳號管理', href: '/UserProfile/EditMySelf' },
      { label: '營業人資料管理', href: '/UserProfile/EditMyBusiness' },
    ]
  },
  {
    label: '查詢與報表', icon: 'fa-search',
    items: [
      { label: '資料查詢／列印／匯出', href: '/InvoiceProcess/Index' },
      { label: '上期發票空白號碼查詢', href: '/InvoiceNo/VacantNoIndex' },
      { label: '發票媒體申報檔查詢', href: '/InvoiceQuery/InvoiceMediaReport' },
      { label: '下載MIG檔案', href: '/InvoiceProcess/InquireToMIG' },
      { label: '發票統計表', href: '/InvoiceQuery/InvoiceSummary' },
    ]
  },
]
;[ROLE_NETWORKSELLER, ROLE_GOOGLETW, ROLE_GROUP_MEMBER, ROLE_RELATIVE_ENTITY, ROLE_BRANCH_ENTITY]
  .forEach(id => { menusByRole[id] = memberMenu })

const menuGroups = computed(() => {
  const roleId = auth.user.value?.roleId
  return roleId != null ? (menusByRole[roleId] ?? []) : []
})

const userName = computed(() => auth.user.value?.userName ?? auth.user.value?.pid ?? '')
const roleName = computed(() => auth.user.value?.roleName ?? '')

// Expanded state for accordion sidebar
const expanded = ref({})
function toggleGroup(label) {
  expanded.value[label] = !expanded.value[label]
}
</script>

<template>
  <div class="app-shell" :class="{ 'sidebar-collapsed': !sidebarOpen }">
    <!-- ── Sidebar ─────────────────────────────────── -->
    <nav class="sidebar" aria-label="主選單">
      <div class="sidebar-header">
        <a href="/MainPage" class="brand">
          <i class="fa fa-bolt brand-icon"></i>
          <span class="brand-text">電子發票系統</span>
        </a>
        <button class="collapse-btn" @click="sidebarOpen = !sidebarOpen" :aria-label="sidebarOpen ? '收起選單' : '展開選單'">
          <i class="fa" :class="sidebarOpen ? 'fa-angle-left' : 'fa-angle-right'"></i>
        </button>
      </div>

      <!-- Role menu -->
      <ul class="nav-menu">
        <li v-for="group in menuGroups" :key="group.label" class="nav-group">
          <button
            class="nav-group-toggle"
            :class="{ active: expanded[group.label] }"
            @click="toggleGroup(group.label)"
          >
            <i class="fa nav-icon" :class="group.icon"></i>
            <span class="nav-label">{{ group.label }}</span>
            <i class="fa fa-angle-down arrow" :class="{ rotated: expanded[group.label] }"></i>
          </button>
          <transition name="slide">
            <ul v-if="expanded[group.label]" class="nav-sub">
              <li v-for="item in group.items" :key="item.href">
                <a :href="item.href" class="nav-sub-link">
                  <i class="fa fa-hand-o-right sub-icon"></i>
                  {{ item.label }}
                </a>
              </li>
            </ul>
          </transition>
        </li>

        <!-- Fallback: no menu found -->
        <li v-if="menuGroups.length === 0 && auth.isLoggedIn.value" class="nav-empty">
          尚未指派工作選單
        </li>
      </ul>
    </nav>

    <!-- ── Main area ───────────────────────────────── -->
    <div class="main-area">
      <!-- Header -->
      <header class="top-header">
        <button class="mobile-menu-btn" @click="sidebarOpen = !sidebarOpen">
          <i class="fa fa-bars"></i>
        </button>
        <div class="header-right">
          <span class="user-info">
            <i class="fa fa-user-circle-o"></i>
            {{ userName }}
            <small v-if="roleName" class="role-badge">({{ roleName }})</small>
          </span>
          <button class="btn ghost logout-btn" @click="handleLogout">
            <i class="fa fa-sign-out"></i> 登出
          </button>
        </div>
      </header>

      <!-- Page content -->
      <main class="page-content">
        <RouterView />
      </main>
    </div>
  </div>
</template>

<style scoped>
/* ── Shell layout ─────────────────────────────────────── */
.app-shell {
  display: flex;
  min-height: 100vh;
  background: var(--color-bg);
}

/* ── Sidebar ──────────────────────────────────────────── */
.sidebar {
  width: var(--nav-width);
  background: var(--color-nav-bg);
  border-right: 1px solid var(--color-border);
  display: flex;
  flex-direction: column;
  flex-shrink: 0;
  transition: width 0.25s ease;
  overflow: hidden;
}
.app-shell.sidebar-collapsed .sidebar {
  width: 56px;
}
.app-shell.sidebar-collapsed .brand-text,
.app-shell.sidebar-collapsed .nav-label,
.app-shell.sidebar-collapsed .arrow,
.app-shell.sidebar-collapsed .nav-sub {
  display: none;
}

.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1.1rem 1rem;
  border-bottom: 1px solid var(--color-border);
}
.brand {
  display: flex;
  align-items: center;
  gap: 0.6rem;
  text-decoration: none;
  color: var(--color-accent);
  font-weight: 700;
  font-size: 1rem;
  white-space: nowrap;
}
.brand-icon { font-size: 1.2rem; }
.collapse-btn {
  background: transparent;
  border: none;
  color: var(--color-muted);
  cursor: pointer;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  transition: color 0.2s;
}
.collapse-btn:hover { color: var(--color-accent); }

/* ── Nav menu ─────────────────────────────────────────── */
.nav-menu {
  flex: 1;
  overflow-y: auto;
  padding: 0.5rem 0;
  list-style: none;
}
.nav-group { border-bottom: 1px solid var(--color-border); }

.nav-group-toggle {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 0.65rem;
  padding: 0.75rem 1rem;
  background: transparent;
  border: none;
  color: var(--color-text);
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  text-align: left;
  transition: background 0.15s, color 0.15s;
}
.nav-group-toggle:hover,
.nav-group-toggle.active { background: rgba(59, 199, 255, 0.08); color: var(--color-accent); }

.nav-icon { width: 1.1rem; text-align: center; }
.nav-label { flex: 1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.arrow { font-size: 0.75rem; transition: transform 0.2s; }
.arrow.rotated { transform: rotate(180deg); }

.nav-sub {
  list-style: none;
  background: rgba(0, 0, 0, 0.2);
  overflow: hidden;
}
.nav-sub-link {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.55rem 1rem 0.55rem 1.8rem;
  color: var(--color-muted);
  text-decoration: none;
  font-size: 0.8rem;
  transition: color 0.15s, background 0.15s;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.nav-sub-link:hover { color: var(--color-text); background: rgba(59, 199, 255, 0.05); }
.sub-icon { font-size: 0.65rem; opacity: 0.6; flex-shrink: 0; }
.nav-empty { padding: 1rem; color: var(--color-muted); font-size: 0.85rem; }

/* Slide transition */
.slide-enter-active, .slide-leave-active { transition: max-height 0.2s ease; overflow: hidden; }
.slide-enter-from, .slide-leave-to { max-height: 0 !important; }
.slide-enter-to, .slide-leave-from { max-height: 600px; }

/* ── Main area ────────────────────────────────────────── */
.main-area {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

/* ── Header ───────────────────────────────────────────── */
.top-header {
  height: var(--header-height);
  background: var(--color-surface);
  border-bottom: 1px solid var(--color-border);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 1.5rem;
  position: sticky;
  top: 0;
  z-index: 50;
}
.mobile-menu-btn {
  background: transparent;
  border: none;
  color: var(--color-muted);
  font-size: 1.1rem;
  cursor: pointer;
  padding: 0.25rem 0.5rem;
  display: none;
}
@media (max-width: 768px) {
  .mobile-menu-btn { display: block; }
  .sidebar { position: fixed; top: 0; bottom: 0; left: 0; z-index: 200; }
  .app-shell:not(.sidebar-collapsed) .sidebar { transform: translateX(0); }
  .app-shell.sidebar-collapsed .sidebar { transform: translateX(-100%); width: var(--nav-width); }
  .app-shell.sidebar-collapsed .brand-text,
  .app-shell.sidebar-collapsed .nav-label,
  .app-shell.sidebar-collapsed .arrow { display: initial; }
}
.header-right { display: flex; align-items: center; gap: 1rem; }
.user-info { color: var(--color-text); font-size: 0.9rem; display: flex; align-items: center; gap: 0.4rem; }
.role-badge { color: var(--color-muted); }
.logout-btn { font-size: 0.85rem; padding: 0.4rem 0.9rem; }

/* ── Page content ─────────────────────────────────────── */
.page-content {
  flex: 1;
  padding: 1.5rem;
  overflow-y: auto;
}
</style>
