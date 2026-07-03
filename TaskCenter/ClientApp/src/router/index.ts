import { createRouter, createWebHistory } from 'vue-router'
import LoginForm from '@/components/LoginForm.vue'
import MainPage from '@/components/MainPage.vue'
import TrackCodeQuery from '@/components/TrackCodeQuery.vue'
import MvcMainPage from '@/components/MvcMainPage.vue'
import DefaultLayout from '@/components/DefaultLayout.vue'
import { useAuthStore } from '@/auth'

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
      {
        path: 'TrackCodeIndex',
        name: 'TrackCodeQuery',
        component: TrackCodeQuery,
      },
      {
        path: 'MvcMainPage',
        name: 'MvcMainPage',
        component: MvcMainPage,
      },
      // ── OrganizationQuery (migrated from OrganizationQueryController) ──
      {
        path: 'OrganizationQuery',
        name: 'OrganizationQuery',
        component: () => import('@/components/OrganizationQueryIndex.vue'),
        alias: ['/OrganizationQuery/Index'],
      },
    ],
  },
]

export const router = createRouter({
  history: createWebHistory(),
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
