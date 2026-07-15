<script setup>
import { onMounted, computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/auth'

const auth = useAuthStore()
const router = useRouter()

// 側邊選單連結分三種狀態：
//  - SPA 路由（已遷移）→ <router-link>，客戶端導航，部署 base 自動帶入。
//  - 外部連結（http/https）→ 一般 <a>，另開分頁。
//  - 其餘（尚未遷移、無對應頁面）→ 停用並標示「未上線」。
// 是否可用以 router 能否解析為準；某模組完成遷移（新增路由）後，選單會自動亮起，毋須改選單設定。
function isExternal(href) {
  return typeof href === 'string' && /^(https?:)?\/\//.test(href)
}
function isSpaRoute(href) {
  if (typeof href !== 'string' || !href.startsWith('/')) return false
  return router.resolve(href).matched.length > 0
}

// Sidebar collapse state
const sidebarOpen = ref(true)

onMounted(() => {
  if (!auth.isLoggedIn.value) {
    router.push('/Login')
  }
})

function handleLogout() {
  auth.logout()
  router.push('/Login')
}

// ─── Role-based menu ────────────────────────────────────────────────────────
// 選單改由後台 appSettings（MenusByRole）設定，登入時依角色回傳並存入 auth store。
// 為每個項目預先算好導航方式（spa=客戶端路由 / 否則伺服器 URL），避免在 template 內重複解析。
const menuGroups = computed(() =>
  (auth.menuGroups.value ?? []).map((group) => ({
    ...group,
    items: (group.items ?? []).map((item) => {
      const external = isExternal(item.href)
      const spa = !external && isSpaRoute(item.href)
      return { ...item, external, spa, available: external || spa }
    }),
  })),
)

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
        <router-link to="/MainPage" class="brand">
          <i class="fa fa-bolt brand-icon"></i>
          <span class="brand-text">電子發票系統</span>
        </router-link>
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
                <router-link v-if="item.spa" :to="item.href" class="nav-sub-link">
                  <i class="fa fa-hand-o-right sub-icon"></i>
                  <span class="nav-sub-label">{{ item.label }}</span>
                </router-link>
                <a
                  v-else-if="item.external"
                  :href="item.href"
                  class="nav-sub-link"
                  target="_blank"
                  rel="noopener"
                >
                  <i class="fa fa-hand-o-right sub-icon"></i>
                  <span class="nav-sub-label">{{ item.label }}</span>
                </a>
                <span
                  v-else
                  class="nav-sub-link is-disabled"
                  aria-disabled="true"
                  title="功能尚未上線"
                >
                  <i class="fa fa-hand-o-right sub-icon"></i>
                  <span class="nav-sub-label">{{ item.label }}</span>
                  <span class="soon-badge">未上線</span>
                </span>
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
.nav-sub-label { overflow: hidden; text-overflow: ellipsis; }

/* 尚未上線：停用外觀，不可點擊 */
.nav-sub-link.is-disabled { color: var(--color-muted); opacity: 0.5; cursor: not-allowed; }
.nav-sub-link.is-disabled:hover { color: var(--color-muted); background: transparent; }
.soon-badge {
  margin-left: auto;
  flex-shrink: 0;
  font-size: 0.6rem;
  line-height: 1;
  padding: 0.15rem 0.35rem;
  border: 1px solid var(--color-border);
  border-radius: 3px;
  color: var(--color-muted);
  background: rgba(255, 255, 255, 0.05);
  white-space: nowrap;
}
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
