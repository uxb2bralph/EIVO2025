import { createRouter, createWebHistory } from 'vue-router'
import LoginForm from '@/components/LoginForm.vue'
import InvoiceQuery from '@/components/invoice-process/InvoiceQuery.vue'

const routes = [
  { path: '/Account/CbsLogin', component: LoginForm },
  { path: '/', redirect: '/Account/CbsLogin' },
  { path: '/InvoiceQuery', component: InvoiceQuery },
]

export const router = createRouter({
  history: createWebHistory(),
  routes,
})
