<template>
  <div class="container-fluid py-3">
    <!-- 面包屑导航 -->
    <div class="mb-3">
      <small class="text-muted">首頁 > 發票作業</small>
    </div>

    <!-- 查詢表單 -->
    <InvoiceQueryForm :form="form" :loading="loading" @search="search" />

    <!-- 查詢結果 -->
    <InvoiceQueryResult
      :results="results"
      :total="total"
      :page="page"
      :page-size="pageSize"
      :total-pages="totalPages"
      :loading="loading"
      :selected-ids="selectedIds"
      :all-selected="allSelected"
      :show-platform-status="form.showPlatformStatus"
      :action-message="actionMessage"
      @print="onBatchPrint"
      @export="onExport"
      @page-change="goToPage"
      @toggle-select="toggleSelect"
      @toggle-select-all="toggleSelectAll"
      @row-action="onRowAction"
      @clear-message="actionMessage = ''"
    />
  </div>
</template>

<script setup lang="ts">
import { useInvoiceQuery } from '@/composables/useInvoiceQuery'
import InvoiceQueryForm from './InvoiceQueryForm.vue'
import InvoiceQueryResult from './InvoiceQueryResult.vue'
import {
  printInvoices,
  exportInvoices,
  applyPOS,
  commitPOS,
  deletePOS,
  previewInvoice,
} from '@/api/invoices'
import type { InvoiceRow } from '@/api/invoices'

const {
  form,
  loading,
  results,
  total,
  page,
  pageSize,
  totalPages,
  selectedIds,
  allSelected,
  actionMessage,
  search,
  goToPage,
  toggleSelect,
  toggleSelectAll,
} = useInvoiceQuery()

async function onBatchPrint(type: string) {
  const ids =
    selectedIds.value.size > 0 ? [...selectedIds.value] : results.value.map((r) => r.id)
  if (ids.length === 0) return
  const res = await printInvoices(ids, type)
  actionMessage.value = res.message
}

async function onExport(format: string) {
  const blob = await exportInvoices({ ...form }, format as 'csv' | 'xlsx')
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `invoices_${Date.now()}.${format}`
  a.click()
  URL.revokeObjectURL(url)
}

async function onRowAction(type: string, row: InvoiceRow) {
  if (type === 'preview') {
    await previewInvoice(row.id)
    return
  }
  if (type === 'applyPOS') {
    actionMessage.value = (await applyPOS(row.id)).message
    return
  }
  if (type === 'commitPOS') {
    actionMessage.value = (await commitPOS(row.id)).message
    return
  }
  if (type === 'deletePOS') {
    actionMessage.value = (await deletePOS(row.id)).message
    return
  }
  if (type.startsWith('print:')) {
    const res = await printInvoices([row.id], type.slice(6))
    actionMessage.value = res.message
  }
}
</script>
