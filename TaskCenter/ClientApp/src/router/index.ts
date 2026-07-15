import { createRouter, createWebHistory } from 'vue-router'
import LoginForm from '@/components/LoginForm.vue'
import MainPage from '@/components/MainPage.vue'
import MvcMainPage from '@/components/MvcMainPage.vue'
import DefaultLayout from '@/components/DefaultLayout.vue'
import { useAuthStore } from '@/auth'
import { appBase } from '@/utils/app-base'

const routes = [
  // ── Standalone pages (no layout wrapper) ────────────
  {
    path: '/Login',
    name: 'Login',
    component: LoginForm,
    alias: ['/Account/CbsLogin'],
    meta: { public: true },
  },

  // ── Pages wrapped in DefaultLayout ──────────────────
  {
    path: '/',
    component: DefaultLayout,
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        name: 'MainPage',
        component: MainPage,
        alias: ['/MainPage', '/Home/MainPage'],
      },
      // ── TrackCode 字軌維護 (migrated from TrackCodeController) ──
      {
        path: 'TrackCode',
        name: 'TrackCode',
        component: () => import('@/components/TrackCodeIndex.vue'),
        alias: ['/TrackCode/Index', '/TrackCodeIndex'],
      },
      // ── WinningNumber 中獎號碼維護 (migrated from WinningNumberController) ──
      {
        path: 'WinningNumber',
        name: 'WinningNumber',
        component: () => import('@/components/WinningNumberIndex.vue'),
        alias: ['/WinningNumber/Index', '/WinningNumberIndex'],
      },
      {
        path: 'MvcMainPage',
        name: 'MvcMainPage',
        component: MvcMainPage,
      },
      // ── PeriodicalExchangeRate 期別匯率維護 (migrated from PeriodicalExchangeRateController) ──
      {
        path: 'PeriodicalExchangeRate',
        name: 'PeriodicalExchangeRate',
        component: () => import('@/components/PeriodicalExchangeRateIndex.vue'),
        alias: ['/PeriodicalExchangeRate/Index', '/PeriodicalExchangeRateIndex'],
      },
      // ── BusinessRelationship 相對營業人資料維護 (migrated from BusinessRelationshipController) ──
      {
        path: 'BusinessRelationship',
        name: 'BusinessRelationship',
        component: () => import('@/components/BusinessRelationshipIndex.vue'),
        alias: ['/BusinessRelationship/MaintainRelationship'],
      },
      // ── InvoiceNumberApply 新登錄營業人資料受理 (migrated from InvoiceNumberApplyController.QueryIndex) ──
      {
        path: 'InvoiceNumberApply',
        name: 'InvoiceNumberApply',
        component: () => import('@/components/InvoiceNumberApplyIndex.vue'),
        alias: ['/InvoiceNumberApply/QueryIndex'],
      },
      // ── InvoiceNoInterval 電子發票號碼維護 (migrated from InvoiceNoController.MaintainInvoiceNoInterval) ──
      {
        path: 'InvoiceNo/MaintainInvoiceNoInterval',
        name: 'InvoiceNoInterval',
        component: () => import('@/components/InvoiceNoIntervalIndex.vue'),
        alias: ['/InvoiceNoInterval'],
      },
      // ── InvoiceProcess 資料查詢／列印／匯出 (migrated from InvoiceProcessController.Index) ──
      {
        path: 'InvoiceProcess/Index',
        name: 'InvoiceProcessQuery',
        component: () => import('@/components/InvoiceProcessIndex.vue'),
        alias: ['/InvoiceProcessIndex'],
      },
      // ── OrganizationQuery (migrated from OrganizationQueryController) ──
      {
        path: 'OrganizationQuery',
        name: 'OrganizationQuery',
        component: () => import('@/components/OrganizationQueryIndex.vue'),
        alias: ['/OrganizationQuery/Index'],
      },
      // ── UserAccount (migrated from AccountController.AccountIndex) ──
      // 由營業人管理頁「管理使用者」導向；org（加密 CompanyID）與 name 以 query 傳入。
      {
        path: 'UserAccount',
        name: 'UserAccount',
        component: () => import('@/components/UserAccountIndex.vue'),
        alias: ['/Account/AccountIndex'],
      },
    ],
  },
]

export const router = createRouter({
  history: createWebHistory(appBase),
  routes,
})

// ── Navigation guard: redirect unauthenticated users to /Login ──
router.beforeEach((to) => {
  const auth = useAuthStore()
  const isPublic = to.meta['public'] === true
  const requiresAuth = to.matched.some(r => !!r.meta['requiresAuth'])
  if (requiresAuth && !auth.isLoggedIn.value) {
    return { path: '/Login', query: { returnUrl: to.fullPath } }
  }
  if (isPublic && auth.isLoggedIn.value && to.path === '/Login') {
    return { path: '/MainPage' }
  }
})
