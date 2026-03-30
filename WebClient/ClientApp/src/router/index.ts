import { createRouter, createWebHistory } from 'vue-router'
import LoginForm from '@/components/LoginForm.vue'
import InvoiceQuery from '@/components/invoice-process/InvoiceQuery.vue'
import MainPage from '@/pages/MainPage.vue'
import TrackCodeQuery from '@/components/TrackCodeQuery.vue'
import MvcMainPage from '../components/MvcMainPage.vue'

const routes = [
  // ── Standalone pages (no layout wrapper) ────────────
  {
    path: '/Login',
    name: 'Login',
    component: LoginForm,
    alias: ['/Account/CbsLogin'],
  },

  // ── Pages wrapped in DefaultLayoutWithVerticalNav ────
  {
    path: '/',
    component: () => import('@/layouts/default.vue'),
    children: [
      {
        path: '',
        name: 'MainPage',
        component: MainPage,
        alias: ['/MainPage', '/Home/MainPage'],
      },
      {
        path: 'InvoiceQuery',
        name: 'InvoiceQuery',
        component: InvoiceQuery,
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
        component: () => import('@/components/organization-query/OrganizationQueryIndex.vue'),
        // Aliases keep legacy MVC URLs working while the SPA takes over
        alias: ['/OrganizationQuery/Index'],
      },
    ],
  },
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
})
